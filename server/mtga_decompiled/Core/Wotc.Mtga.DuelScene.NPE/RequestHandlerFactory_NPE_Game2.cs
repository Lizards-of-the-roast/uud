using System;
using System.Collections.Generic;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.NPE;

public class RequestHandlerFactory_NPE_Game2 : RequestHandlerFactory<BaseUserRequest>, IDisposable
{
	private class SelectTargetsHandler : BaseUserRequestHandler<SelectTargetsRequest>
	{
		private readonly MtgGameState _gameState;

		public SelectTargetsHandler(SelectTargetsRequest request, MtgGameState gameState)
			: base(request)
		{
			_gameState = gameState;
		}

		public override void HandleRequest()
		{
			if (CanSubmit(_request.TargetSelections))
			{
				_request.SubmitTargets();
				return;
			}
			foreach (TargetSelection targetSelection in _request.TargetSelections)
			{
				if (!targetSelection.CanSubmit() && TryGetTargetToUpdate(targetSelection.SelectableTargets(), _gameState, out var result))
				{
					_request.UpdateTarget(result, targetSelection.TargetIdx);
					break;
				}
			}
		}

		private static bool CanSubmit(IReadOnlyList<TargetSelection> targetSelections)
		{
			foreach (TargetSelection targetSelection in targetSelections)
			{
				if (!targetSelection.CanSubmit())
				{
					return false;
				}
			}
			return true;
		}

		private static bool TryGetTargetToUpdate(IEnumerable<Target> targets, MtgGameState gameState, out Target result)
		{
			result = null;
			foreach (Target target2 in targets)
			{
				if (target2.Highlight == Wotc.Mtgo.Gre.External.Messaging.HighlightType.Hot)
				{
					if (gameState.TryGetPlayer(target2.TargetInstanceId, out var _))
					{
						result = target2;
						return true;
					}
					Target target = result;
					if (target == null || target.Highlight != Wotc.Mtgo.Gre.External.Messaging.HighlightType.Hot)
					{
						result = target2;
					}
				}
			}
			return result != null;
		}
	}

	private class DeclareBlockersHandler : BaseUserRequestHandler<DeclareBlockersRequest>
	{
		private readonly struct Attacker
		{
			public readonly uint InstanceId;

			public readonly int Power;

			public readonly bool IsBlocked;

			public readonly bool BlockerSurvives;

			public readonly bool AttackerSurvivesBlock;

			private readonly bool _multiBlockable;

			private readonly bool _isLethal;

			public bool UnblockedLethalAttacker
			{
				get
				{
					if (_isLethal)
					{
						return !IsBlocked;
					}
					return false;
				}
			}

			public bool MultiBlockToKill
			{
				get
				{
					if (_multiBlockable)
					{
						return !AttackerSurvivesBlock;
					}
					return false;
				}
			}

			public bool BlockerSurvivesSingleBlock
			{
				get
				{
					if (BlockerSurvives)
					{
						return !IsBlocked;
					}
					return false;
				}
			}

			public bool AttackerKilledFromSingleBlock
			{
				get
				{
					if (!AttackerSurvivesBlock)
					{
						return !IsBlocked;
					}
					return false;
				}
			}

			public Attacker(uint instanceId, int power, bool isBlocked, bool blockerSurvives, bool attackerSurvivesBlock, bool multiBlockable, bool isLethal)
			{
				InstanceId = instanceId;
				Power = power;
				IsBlocked = isBlocked;
				BlockerSurvives = blockerSurvives;
				AttackerSurvivesBlock = attackerSurvivesBlock;
				_multiBlockable = multiBlockable;
				_isLethal = isLethal;
			}
		}

		private class BlockPriorityComparer : IComparer<Attacker>
		{
			public int Compare(Attacker x, Attacker y)
			{
				int num = y.UnblockedLethalAttacker.CompareTo(x.UnblockedLethalAttacker);
				if (num != 0)
				{
					return num;
				}
				num = BlockerSurvivesSoloKill(y).CompareTo(BlockerSurvivesSoloKill(x));
				if (num != 0)
				{
					return num;
				}
				num = y.MultiBlockToKill.CompareTo(x.MultiBlockToKill);
				if (num != 0)
				{
					return num;
				}
				num = y.BlockerSurvivesSingleBlock.CompareTo(x.BlockerSurvivesSingleBlock);
				if (num != 0)
				{
					return num;
				}
				num = y.AttackerKilledFromSingleBlock.CompareTo(x.AttackerKilledFromSingleBlock);
				if (num != 0)
				{
					return num;
				}
				num = x.IsBlocked.CompareTo(y.IsBlocked);
				if (num != 0)
				{
					return num;
				}
				num = y.Power.CompareTo(x.Power);
				if (num != 0)
				{
					return num;
				}
				return x.InstanceId.CompareTo(y.InstanceId);
			}

			private bool BlockerSurvivesSoloKill(Attacker attacker)
			{
				if (attacker.BlockerSurvives && !attacker.AttackerSurvivesBlock)
				{
					return !attacker.IsBlocked;
				}
				return false;
			}
		}

		private static readonly IComparer<Attacker> AttackerComparer = new BlockPriorityComparer();

		private readonly IObjectPool _objectPool;

		private readonly MtgGameState _gameState;

		public DeclareBlockersHandler(DeclareBlockersRequest request, IObjectPool objectPool, MtgGameState gameState)
			: base(request)
		{
			_objectPool = objectPool;
			_gameState = gameState;
		}

		public override void HandleRequest()
		{
			if (TryGetBlockerToUpdate(_request.AllBlockers, out var result))
			{
				uint idToBlock = GetIdToBlock(result, _request.AllBlockers, _objectPool, _gameState);
				result.SelectedAttackerInstanceIds.Add(idToBlock);
				_request.UpdateBlockers(result);
			}
			else
			{
				_request.SubmitBlockers();
			}
		}

		private bool TryGetBlockerToUpdate(IEnumerable<Blocker> blockers, out Blocker result)
		{
			result = null;
			foreach (Blocker blocker in blockers)
			{
				if (blocker.SelectedAttackerInstanceIds.Count <= 0)
				{
					result = blocker;
					break;
				}
			}
			return result != null;
		}

		private static (int power, int toughness) GetPowerAndToughness(uint instanceId, MtgGameState gameState)
		{
			if (!gameState.TryGetCard(instanceId, out var card))
			{
				return (power: 0, toughness: 0);
			}
			return (power: card.Power.Value, toughness: card.Toughness.Value);
		}

		private static uint GetIdToBlock(Blocker blocker, IReadOnlyList<Blocker> allBlockers, IObjectPool objectPool, MtgGameState gameState)
		{
			if (blocker.AttackerInstanceIds.Count == 1)
			{
				return blocker.AttackerInstanceIds[0];
			}
			(int power, int toughness) powerAndToughness = GetPowerAndToughness(blocker.BlockerInstanceId, gameState);
			int item = powerAndToughness.power;
			int item2 = powerAndToughness.toughness;
			List<Attacker> list = objectPool.PopObject<List<Attacker>>();
			foreach (uint attackerInstanceId in blocker.AttackerInstanceIds)
			{
				list.Add(ToAttacker(attackerInstanceId, item, item2, allBlockers, gameState));
			}
			list.Sort(AttackerComparer);
			uint instanceId = list[0].InstanceId;
			list.Clear();
			objectPool.PushObject(list, tryClear: false);
			return instanceId;
		}

		private static bool IsLethalAttacker(uint instanceId, int attackerPower, MtgGameState gameState)
		{
			if (!gameState.AttackInfo.TryGetValue(instanceId, out var value))
			{
				return false;
			}
			if (!gameState.TryGetPlayer(value.TargetId, out var player))
			{
				return false;
			}
			return player.LifeTotal <= attackerPower;
		}

		private static Attacker ToAttacker(uint instanceId, int blockerPower, int blockerToughness, IEnumerable<Blocker> allBlockers, MtgGameState gameState)
		{
			(int power, int toughness) powerAndToughness = GetPowerAndToughness(instanceId, gameState);
			int item = powerAndToughness.power;
			int item2 = powerAndToughness.toughness;
			bool flag = false;
			int num = item;
			int num2 = 0;
			bool isLethal = IsLethalAttacker(instanceId, item, gameState);
			foreach (Blocker allBlocker in allBlockers)
			{
				if (allBlocker.SelectedAttackerInstanceIds.Contains(instanceId))
				{
					(int, int) powerAndToughness2 = GetPowerAndToughness(allBlocker.BlockerInstanceId, gameState);
					flag = true;
					isLethal = false;
					num2 += powerAndToughness2.Item1;
					num -= powerAndToughness2.Item2;
				}
			}
			bool multiBlockable = flag && num2 < item2;
			bool blockerSurvives = num < blockerToughness;
			bool attackerSurvivesBlock = item2 - (num2 + blockerPower) > 0;
			return new Attacker(instanceId, item, flag, blockerSurvives, attackerSurvivesBlock, multiBlockable, isLethal);
		}
	}

	private class AttackWithAllHandler : BaseUserRequestHandler<DeclareAttackerRequest>
	{
		public AttackWithAllHandler(DeclareAttackerRequest request)
			: base(request)
		{
		}

		public override void HandleRequest()
		{
			if (_request.DeclaredAttackers.Count > 0)
			{
				_request.SubmitAttackers();
			}
			else
			{
				_request.DeclareAllAttackers(_request.Attackers[0].LegalDamageRecipients[0]);
			}
		}
	}

	private class AutoSubmitSelectNHandler : BaseUserRequestHandler<SelectNRequest>
	{
		public AutoSubmitSelectNHandler(SelectNRequest request)
			: base(request)
		{
		}

		public override void HandleRequest()
		{
			_request.SubmitSelection(_request.Ids[0]);
		}
	}

	private class ChooseSelfAsStartingPlayerHandler : BaseUserRequestHandler<ChooseStartingPlayerRequest>
	{
		private readonly MtgGameState _gameState;

		public ChooseSelfAsStartingPlayerHandler(ChooseStartingPlayerRequest request, MtgGameState gameState)
			: base(request)
		{
			_gameState = gameState;
		}

		public override void HandleRequest()
		{
			_request.ChooseStartingPlayer(GetStartingPlayerId(_request.SeatIds, _gameState));
		}

		private static uint GetStartingPlayerId(IReadOnlyList<uint> ids, MtgGameState gameState)
		{
			foreach (uint id in ids)
			{
				if (gameState.TryGetPlayer(id, out var player) && player.IsLocalPlayer)
				{
					return id;
				}
			}
			return ids[0];
		}
	}

	private readonly List<Wotc.Mtgo.Gre.External.Messaging.Action> _actionsToTake;

	private readonly RequestHandlerFactory<BaseUserRequest> _defaultHandler;

	private readonly IObjectPool _objectPool;

	private readonly float _blockerDelayInMs;

	private MtgGameState _gameState;

	public RequestHandlerFactory_NPE_Game2(RequestHandlerFactory<BaseUserRequest> defaultHandler, IObjectPool objectPool, float blockerDelayInMs)
	{
		_actionsToTake = new List<Wotc.Mtgo.Gre.External.Messaging.Action>(NPEActions.GetGame2Actions());
		_defaultHandler = defaultHandler;
		_objectPool = objectPool ?? new ObjectPool();
		_blockerDelayInMs = blockerDelayInMs;
	}

	public override BaseUserRequestHandler GetHandlerForRequest(BaseUserRequest request)
	{
		if (!(request is ActionsAvailableRequest request2))
		{
			if (!(request is DeclareAttackerRequest request3))
			{
				if (!(request is DeclareBlockersRequest declareBlockers))
				{
					if (!(request is SelectTargetsRequest request4))
					{
						if (!(request is SelectNRequest request5))
						{
							if (request is ChooseStartingPlayerRequest request6)
							{
								return new ChooseSelfAsStartingPlayerHandler(request6, _gameState);
							}
							return _defaultHandler.GetHandlerForRequest(request);
						}
						return new AutoSubmitSelectNHandler(request5);
					}
					return new SelectTargetsHandler(request4, _gameState);
				}
				return GetDeclareBlockersHandler(declareBlockers, _objectPool, _gameState, _blockerDelayInMs);
			}
			return new AttackWithAllHandler(request3);
		}
		return new ActionsAvailableQueueHandler(request2, _actionsToTake);
	}

	private static BaseUserRequestHandler GetDeclareBlockersHandler(DeclareBlockersRequest declareBlockers, IObjectPool objectPool, MtgGameState gameState, float blockerDelayInMs)
	{
		BaseUserRequestHandler baseUserRequestHandler = new DeclareBlockersHandler(declareBlockers, objectPool, gameState);
		if (!(blockerDelayInMs > 0f))
		{
			return baseUserRequestHandler;
		}
		return new DelayedRequestHandler(baseUserRequestHandler, blockerDelayInMs);
	}

	public override void SetGameState(MtgGameState state)
	{
		_gameState = state;
		_defaultHandler.SetGameState(state);
	}

	public void Dispose()
	{
		_gameState = null;
		if (_defaultHandler is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}
