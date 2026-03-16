using System;
using System.Collections.Generic;
using System.Text;
using AssetLookupTree;
using AssetLookupTree.Payloads.Combat;
using GreClient.Rules;
using MovementSystem;
using UnityEngine;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class CombatFrame : UXEvent
{
	private class DamageBranch
	{
		private readonly GameManager _gameManager;

		private readonly ICardViewProvider _cardViewProvider;

		private readonly IUXEventGrouper _zoneTransferGrouper;

		private readonly IBattlefieldCardHolder _battlefield;

		private readonly ISplineMovementSystem _splineMovementSystem;

		private UXEventDamageDealt _damageEvent;

		private List<UXEvent> _leafEvents = new List<UXEvent>();

		private DamageBranch _nextBranch;

		private List<UXEvent> _runningEvents = new List<UXEvent>();

		public int BranchDepth
		{
			get
			{
				if (_nextBranch != null)
				{
					return _nextBranch.BranchDepth + 1;
				}
				return 1;
			}
		}

		public List<uint> InvolvedIds
		{
			get
			{
				List<uint> list = new List<uint>();
				list.Add(_damageEvent.Source.InstanceId);
				if (!list.Contains(_damageEvent.Target.InstanceId))
				{
					list.Add(_damageEvent.Target.InstanceId);
				}
				if (_nextBranch != null)
				{
					foreach (uint involvedId in _nextBranch.InvolvedIds)
					{
						if (!list.Contains(involvedId))
						{
							list.Add(involvedId);
						}
					}
				}
				return list;
			}
		}

		public DamageBranch(UXEventDamageDealt dmgEvt, IBattlefieldCardHolder battlefield, GameManager gameManager, ICardViewProvider cardViewProvider, IUXEventGrouper zoneTransferGrouper)
		{
			_damageEvent = dmgEvt;
			_battlefield = battlefield;
			_gameManager = gameManager;
			_cardViewProvider = cardViewProvider;
			_zoneTransferGrouper = zoneTransferGrouper ?? NullUXEventGrouper.Default;
			_splineMovementSystem = gameManager.SplineMovementSystem;
		}

		public void AddBranch(UXEventDamageDealt dmgEvt)
		{
			if (_nextBranch != null)
			{
				_nextBranch.AddBranch(dmgEvt);
			}
			else
			{
				_nextBranch = new DamageBranch(dmgEvt, _battlefield, _gameManager, _cardViewProvider, _zoneTransferGrouper);
			}
		}

		public void AddLeaf(UXEvent evt)
		{
			ZoneTransferUXEvent zoneTransferUXEvent = evt as ZoneTransferUXEvent;
			LifeTotalUpdateUXEvent lifeTotalUpdateUXEvent = evt as LifeTotalUpdateUXEvent;
			CountersChangedUXEvent countersChangedUXEvent = evt as CountersChangedUXEvent;
			UpdateCardModelUXEvent updateCardModelUXEvent = evt as UpdateCardModelUXEvent;
			uint item;
			if (zoneTransferUXEvent != null)
			{
				item = zoneTransferUXEvent.OldId;
			}
			else if (lifeTotalUpdateUXEvent != null)
			{
				item = lifeTotalUpdateUXEvent.AffectorId;
			}
			else if (countersChangedUXEvent != null)
			{
				item = countersChangedUXEvent.AffectedId;
			}
			else
			{
				if (updateCardModelUXEvent == null)
				{
					Debug.LogError("Unhandled Leaf UX Event " + evt.ToString());
					return;
				}
				item = updateCardModelUXEvent.NewInstance.InstanceId;
			}
			if (_nextBranch != null && _nextBranch.InvolvedIds.Contains(item))
			{
				_nextBranch.AddLeaf(evt);
			}
			else
			{
				_leafEvents.Add(evt);
			}
		}

		public void Execute(System.Action onHit)
		{
			_damageEvent.OnHit += delegate
			{
				onHit?.Invoke();
				uint sourceId = _damageEvent.Source.InstanceId;
				bool flag = _leafEvents.FindAll((UXEvent x) => x is ZoneTransferUXEvent).ConvertAll((UXEvent x) => x as ZoneTransferUXEvent).Exists((ZoneTransferUXEvent x) => x.OldId == sourceId);
				_zoneTransferGrouper.GroupEvents(0, ref _leafEvents);
				foreach (UXEvent leafEvent in _leafEvents)
				{
					leafEvent.Execute();
					_runningEvents.Add(leafEvent);
				}
				DuelScene_CDC sourceCDC;
				if (_nextBranch != null)
				{
					_damageEvent.Complete();
					_nextBranch._damageEvent.CanHitPageArrow = _damageEvent.CanHitPageArrow;
					_nextBranch.Execute(null);
				}
				else if (!flag && (_damageEvent.DamageType == DamageType.Combat || _damageEvent.DamageType == DamageType.Fight) && _cardViewProvider.TryGetCardView(sourceId, out sourceCDC))
				{
					SplineReturn splineReturn = null;
					AssetLookupSystem assetLookupSystem = _gameManager.AssetLookupSystem;
					assetLookupSystem.Blackboard.Clear();
					assetLookupSystem.Blackboard.SetCardDataExtensive(sourceCDC.Model);
					assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Battlefield;
					assetLookupSystem.Blackboard.DamageRecipientEntity = _damageEvent.DamageInfo?.TargetEntity;
					if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<SplineReturn> loadedTree))
					{
						splineReturn = loadedTree.GetPayload(assetLookupSystem.Blackboard);
					}
					SplineMovementData splineMovementData = ((splineReturn != null) ? AssetLoader.GetObjectData(splineReturn.SplineDataRef) : null);
					if (splineMovementData == null)
					{
						splineMovementData = ScriptableObject.CreateInstance<SplineMovementData>();
						splineMovementData.Spline = SplineData.Parabolic;
					}
					IdealPoint layoutEndpoint = _battlefield.GetLayoutEndpoint(sourceCDC);
					_splineMovementSystem.AddTemporaryGoal(sourceCDC.Root, layoutEndpoint, allowInteractions: false, splineMovementData, new SplineEventData(new SplineEventCallback(1f, delegate
					{
						StopAttachmentsFromFollowing(sourceCDC);
						_damageEvent.Complete();
						_splineMovementSystem.RemoveTemporaryGoal(sourceCDC.Root);
						AudioManager.PlayAudio(WwiseEvents.sfx_combat_attacker_return, sourceCDC.gameObject);
					})));
					IBattlefieldStack stackForCard = _battlefield.GetStackForCard(sourceCDC);
					if (stackForCard != null && stackForCard.HasAttachmentOrExile)
					{
						CardLayoutData cardLayoutData = _battlefield.PreviousLayoutData.Find((CardLayoutData x) => x.Card == sourceCDC);
						if (cardLayoutData != null)
						{
							Quaternion quaternion = _battlefield.Transform.rotation * cardLayoutData.Rotation;
							Matrix4x4 inverse = Matrix4x4.TRS(cardLayoutData.Position, quaternion, cardLayoutData.Scale).inverse;
							foreach (DuelScene_CDC cardView in GetAttachmentsForStack(stackForCard))
							{
								CardLayoutData cardLayoutData2 = _battlefield.PreviousLayoutData.Find((CardLayoutData x) => x.Card == cardView);
								if (cardLayoutData2 != null)
								{
									Vector3 positionOffset = inverse.MultiplyPoint3x4(cardLayoutData2.Position);
									Quaternion rotationOffset = _battlefield.Transform.rotation * cardLayoutData2.Rotation * Quaternion.Inverse(quaternion);
									Vector3 scaleOffset = new Vector3(cardLayoutData2.Scale.x / cardLayoutData.Scale.x, cardLayoutData2.Scale.y / cardLayoutData.Scale.y, cardLayoutData2.Scale.z / cardLayoutData.Scale.z);
									_splineMovementSystem.AddFollowTransform(sourceCDC.Root, cardLayoutData2.Card.Root, positionOffset, rotationOffset, scaleOffset);
								}
							}
						}
					}
				}
				else
				{
					_damageEvent.Complete();
				}
			};
			_damageEvent.Execute();
			_runningEvents.Add(_damageEvent);
			void StopAttachmentsFromFollowing(DuelScene_CDC source)
			{
				IBattlefieldStack stackForCard = _battlefield.GetStackForCard(source);
				if (stackForCard != null && stackForCard.HasAttachmentOrExile)
				{
					foreach (DuelScene_CDC cardView in GetAttachmentsForStack(stackForCard))
					{
						if (!(cardView == null))
						{
							_splineMovementSystem.RemoveFollowTransform(cardView.Root);
							_splineMovementSystem.AddPermanentGoal(cardView.Root, _battlefield.GetLayoutEndpoint(_battlefield.PreviousLayoutData.Find((CardLayoutData x) => x.Card == cardView)));
						}
					}
				}
			}
		}

		private List<DuelScene_CDC> GetAttachmentsForStack(IBattlefieldStack sourceStack)
		{
			return sourceStack.StackedCards;
		}

		public void ExecuteAll()
		{
			_damageEvent.OnHit += delegate
			{
				_damageEvent.Complete();
				_zoneTransferGrouper.GroupEvents(0, ref _leafEvents);
				foreach (UXEvent leafEvent in _leafEvents)
				{
					leafEvent.Execute();
					_runningEvents.Add(leafEvent);
				}
			};
			_damageEvent.PageToTarget = false;
			_damageEvent.Execute();
			_runningEvents.Add(_damageEvent);
			if (_nextBranch != null)
			{
				_nextBranch.ExecuteAll();
			}
		}

		public void Update(float dt)
		{
			for (int i = 0; i < _runningEvents.Count; i++)
			{
				UXEvent uXEvent = _runningEvents[i];
				if (uXEvent.IsComplete)
				{
					_runningEvents.RemoveAt(i);
					i--;
				}
				else
				{
					uXEvent.Update(dt);
				}
			}
			if (_damageEvent.IsComplete && _nextBranch != null)
			{
				_nextBranch.Update(dt);
			}
		}

		public bool HasCompleted()
		{
			if (!_damageEvent.IsComplete)
			{
				return false;
			}
			foreach (UXEvent leafEvent in _leafEvents)
			{
				if (!leafEvent.IsComplete)
				{
					return false;
				}
			}
			if (_nextBranch != null && !_nextBranch.HasCompleted())
			{
				return false;
			}
			return true;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(_damageEvent.ToString());
			if (_leafEvents.Count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("[LEAFS] ");
				for (int i = 0; i < _leafEvents.Count; i++)
				{
					stringBuilder.Append(_leafEvents[i].ToString());
					if (i + 1 < _leafEvents.Count)
					{
						stringBuilder.Append(" | ");
					}
				}
				if (_nextBranch != null)
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("[NEXT BRANCH] " + _nextBranch.ToString());
				}
			}
			return stringBuilder.ToString();
		}
	}

	private readonly GameManager _gameManager;

	private readonly ICardViewProvider _cardViewProvider;

	private readonly IUXEventGrouper _zoneTransferGrouper;

	private readonly CardHolderReference<IBattlefieldCardHolder> _battlefield;

	private List<DamageBranch> _branches = new List<DamageBranch>();

	private List<DamageBranch> _runningBranches = new List<DamageBranch>();

	private bool _simultaneousPlayback;

	public DamageType DamageType { get; private set; }

	public int OpponentDamageDealt { get; private set; }

	public override bool IsBlocking => true;

	public CombatFrame(List<UXEvent> events, GameManager gameManager, ICardViewProvider cardViewProvider, IUXEventGrouper zoneTransferGrouper)
	{
		_gameManager = gameManager;
		_cardViewProvider = cardViewProvider;
		_zoneTransferGrouper = zoneTransferGrouper ?? NullUXEventGrouper.Default;
		_battlefield = CardHolderReference<IBattlefieldCardHolder>.Battlefield(gameManager.CardHolderManager);
		DamageType = DamageType.None;
		List<UXEventDamageDealt> attacks = new List<UXEventDamageDealt>();
		List<UXEventDamageDealt> list = new List<UXEventDamageDealt>();
		List<ZoneTransferUXEvent> list2 = new List<ZoneTransferUXEvent>();
		List<LifeTotalUpdateUXEvent> list3 = new List<LifeTotalUpdateUXEvent>();
		List<UpdateCardModelUXEvent> list4 = new List<UpdateCardModelUXEvent>();
		List<CountersChangedUXEvent> list5 = new List<CountersChangedUXEvent>();
		foreach (UXEvent @event in events)
		{
			if (!(@event is UXEventDamageDealt uXEventDamageDealt))
			{
				if (!(@event is ZoneTransferUXEvent item))
				{
					if (!(@event is LifeTotalUpdateUXEvent item2))
					{
						if (!(@event is UpdateCardModelUXEvent updateCardModelUXEvent))
						{
							if (@event is CountersChangedUXEvent item3)
							{
								list5.Add(item3);
							}
						}
						else if (updateCardModelUXEvent.Property == PropertyType.Damage)
						{
							list4.Add(updateCardModelUXEvent);
						}
					}
					else
					{
						list3.Add(item2);
					}
				}
				else
				{
					list2.Add(item);
				}
				continue;
			}
			if (DamageType == DamageType.None)
			{
				DamageType = uXEventDamageDealt.DamageType;
			}
			if (uXEventDamageDealt.Target is MtgPlayer { IsLocalPlayer: false })
			{
				OpponentDamageDealt += uXEventDamageDealt.Amount;
			}
			if (uXEventDamageDealt.DamageType != DamageType.Combat || !(uXEventDamageDealt.Target is MtgCardInstance) || !(uXEventDamageDealt.Source is MtgCardInstance mtgCardInstance) || mtgCardInstance.Power.Value != 0 || uXEventDamageDealt.Amount != 0)
			{
				if (DamageType == DamageType.Combat && uXEventDamageDealt.IsBlockDamage)
				{
					list.Add(uXEventDamageDealt);
				}
				else
				{
					attacks.Add(uXEventDamageDealt);
				}
			}
		}
		if (DamageType == DamageType.Combat)
		{
			attacks.Sort(delegate(UXEventDamageDealt x, UXEventDamageDealt y)
			{
				MtgEntity target = x.Target;
				MtgEntity target2 = y.Target;
				bool flag = target is MtgPlayer;
				bool flag2 = target2 is MtgPlayer;
				if (flag || flag2)
				{
					return flag.CompareTo(flag2);
				}
				if (target is MtgCardInstance mtgCardInstance2 && target2 is MtgCardInstance mtgCardInstance3)
				{
					if (mtgCardInstance2.CardTypes.Contains(CardType.Creature) && mtgCardInstance3.CardTypes.Contains(CardType.Creature))
					{
						bool flag3 = attacks.Exists(x.Source.InstanceId, (UXEventDamageDealt attack, uint sourceId) => attack.Target.InstanceId == sourceId);
						bool flag4 = attacks.Exists(y.Source.InstanceId, (UXEventDamageDealt attack, uint sourceId) => attack.Target.InstanceId == sourceId);
						if (flag3 != flag4)
						{
							return flag3.CompareTo(flag4);
						}
						if (x.Source is MtgCardInstance mtgCardInstance4 && mtgCardInstance4.BlockedByIds.Count > 0)
						{
							int num7 = mtgCardInstance4.BlockedByIds.IndexOf(target.InstanceId);
							int value7 = mtgCardInstance4.BlockedByIds.IndexOf(target2.InstanceId);
							return num7.CompareTo(value7);
						}
						return 0;
					}
					bool value8 = mtgCardInstance2.CardTypes.Contains(CardType.Creature);
					int num8 = mtgCardInstance3.CardTypes.Contains(CardType.Creature).CompareTo(value8);
					if (num8 != 0)
					{
						return num8;
					}
					bool value9 = mtgCardInstance2.CardTypes.Contains(CardType.Battle);
					num8 = mtgCardInstance3.CardTypes.Contains(CardType.Battle).CompareTo(value9);
					if (num8 != 0)
					{
						return num8;
					}
					bool value10 = mtgCardInstance2.CardTypes.Contains(CardType.Planeswalker);
					return mtgCardInstance3.CardTypes.Contains(CardType.Planeswalker).CompareTo(value10);
				}
				return 0;
			});
			list.Sort(delegate(UXEventDamageDealt x, UXEventDamageDealt y)
			{
				if (x.Target is MtgCardInstance mtgCardInstance2 && mtgCardInstance2.BlockedByIds.Count > 0)
				{
					int num7 = mtgCardInstance2.BlockedByIds.IndexOf(x.Source.InstanceId);
					int value7 = mtgCardInstance2.BlockedByIds.IndexOf(y.Source.InstanceId);
					return num7.CompareTo(value7);
				}
				return 0;
			});
		}
		Dictionary<uint, DamageBranch> dictionary = new Dictionary<uint, DamageBranch>();
		Dictionary<uint, DamageBranch> dictionary2 = new Dictionary<uint, DamageBranch>();
		for (int num = 0; num < attacks.Count; num++)
		{
			UXEventDamageDealt uXEventDamageDealt2 = attacks[num];
			DamageBranch value = null;
			uint instanceId = uXEventDamageDealt2.Source.InstanceId;
			if (dictionary.TryGetValue(instanceId, out value))
			{
				value.AddBranch(uXEventDamageDealt2);
			}
			else
			{
				value = new DamageBranch(uXEventDamageDealt2, _battlefield.Get(), _gameManager, _cardViewProvider, _zoneTransferGrouper);
				dictionary.Add(instanceId, value);
				_branches.Add(value);
			}
			dictionary2[uXEventDamageDealt2.Target.InstanceId] = value;
		}
		if (DamageType == DamageType.Combat)
		{
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				UXEventDamageDealt uXEventDamageDealt3 = list[num2];
				uint instanceId2 = uXEventDamageDealt3.Source.InstanceId;
				uint instanceId3 = uXEventDamageDealt3.Target.InstanceId;
				DamageBranch value2 = null;
				if (!dictionary2.ContainsKey(instanceId2))
				{
					UXEventDamageDealt dmgEvt = new UXEventDamageDealt(uXEventDamageDealt3.Target, uXEventDamageDealt3.Source, uXEventDamageDealt3.Amount, uXEventDamageDealt3.DamageType, uXEventDamageDealt3.IsBlockDamage, gameManager);
					if (dictionary.TryGetValue(instanceId3, out value2))
					{
						value2.AddBranch(dmgEvt);
						dictionary2[instanceId2] = value2;
					}
					else
					{
						value2 = (dictionary2[instanceId2] = (dictionary[instanceId3] = new DamageBranch(dmgEvt, _battlefield.Get(), _gameManager, _cardViewProvider, _zoneTransferGrouper)));
						_branches.Add(value2);
					}
				}
			}
		}
		for (int num3 = 0; num3 < list2.Count; num3++)
		{
			ZoneTransferUXEvent zoneTransferUXEvent = list2[num3];
			DamageBranch value3 = null;
			if (!dictionary.TryGetValue(zoneTransferUXEvent.OldId, out value3) && !dictionary2.TryGetValue(zoneTransferUXEvent.OldId, out value3) && !dictionary.TryGetValue(zoneTransferUXEvent.Instigator?.InstanceId ?? 0, out value3) && !dictionary2.TryGetValue(zoneTransferUXEvent.Instigator?.InstanceId ?? 0, out value3))
			{
				Debug.LogError("Unhandled ZoneTransferUXEvent in CombatFrame: " + zoneTransferUXEvent);
			}
			value3?.AddLeaf(zoneTransferUXEvent);
		}
		for (int num4 = 0; num4 < list5.Count; num4++)
		{
			CountersChangedUXEvent countersChangedUXEvent = list5[num4];
			uint instanceId4 = countersChangedUXEvent.Affected.InstanceId;
			if (dictionary.TryGetValue(instanceId4, out var value4) || dictionary2.TryGetValue(instanceId4, out value4))
			{
				value4.AddLeaf(countersChangedUXEvent);
			}
		}
		for (int num5 = 0; num5 < list3.Count; num5++)
		{
			LifeTotalUpdateUXEvent lifeTotalUpdateUXEvent = list3[num5];
			if (dictionary.TryGetValue(lifeTotalUpdateUXEvent.AffectorId, out var value5) || dictionary2.TryGetValue(lifeTotalUpdateUXEvent.AffectorId, out value5))
			{
				value5.AddLeaf(lifeTotalUpdateUXEvent);
			}
		}
		for (int num6 = 0; num6 < list4.Count; num6++)
		{
			UpdateCardModelUXEvent updateCardModelUXEvent2 = list4[num6];
			if (dictionary.TryGetValue(updateCardModelUXEvent2.AffectorId, out var value6) || dictionary2.TryGetValue(updateCardModelUXEvent2.AffectorId, out value6))
			{
				value6.AddLeaf(updateCardModelUXEvent2);
			}
		}
		_simultaneousPlayback = _branches.Count == 1 && _branches[0].BranchDepth >= 4 && DamageType == DamageType.Direct;
	}

	public override bool CanExecute(List<UXEvent> currentlyRunningEvents)
	{
		return !currentlyRunningEvents.Exists((UXEvent x) => x.HasWeight);
	}

	public override void Execute()
	{
		_battlefield.Get().LayoutLocked = true;
		if (_simultaneousPlayback)
		{
			DamageBranch damageBranch = _branches[0];
			damageBranch.ExecuteAll();
			_runningBranches.Add(damageBranch);
		}
		else
		{
			PlayBranch(0);
		}
	}

	private void PlayBranch(int branchIndex)
	{
		if (branchIndex < _branches.Count)
		{
			DamageBranch damageBranch = _branches[branchIndex];
			damageBranch.Execute(delegate
			{
				PlayBranch(branchIndex + 1);
			});
			_runningBranches.Add(damageBranch);
			_timeOutTarget += (float)damageBranch.BranchDepth * 1.1f;
		}
	}

	public override void Update(float dt)
	{
		base.Update(dt);
		for (int num = _runningBranches.Count - 1; num >= 0; num--)
		{
			if (_runningBranches[num].HasCompleted())
			{
				_runningBranches.RemoveAt(num);
			}
		}
		for (int i = 0; i < _runningBranches.Count; i++)
		{
			_runningBranches[i].Update(dt);
		}
		foreach (DamageBranch branch in _branches)
		{
			if (!branch.HasCompleted())
			{
				return;
			}
		}
		Complete();
	}

	protected override void Cleanup()
	{
		base.Cleanup();
		_battlefield.Get().LayoutLocked = false;
		_battlefield.ClearCache();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _branches.Count; i++)
		{
			for (int j = 0; j < i; j++)
			{
				stringBuilder.AppendLine("-");
			}
			stringBuilder.Append(_branches[i].ToString());
		}
		return stringBuilder.ToString();
	}
}
