using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.ZoneTransfer;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ZoneTransferGroup : UXEvent
{
	public readonly List<ZoneTransferUXEvent> _zoneTransfers = new List<ZoneTransferUXEvent>();

	private readonly Queue<ZoneTransferUXEvent> _pendingEvents = new Queue<ZoneTransferUXEvent>();

	private readonly List<ZoneTransferUXEvent> _runningEvents = new List<ZoneTransferUXEvent>();

	private readonly IObjectPool _objectPool;

	private readonly IVfxProvider _vfxProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly GameManager _gameManager;

	private ICardDataAdapter _transferInstigatorDataAdapter;

	private readonly List<(ZoneTransferReason, MtgZone)> _reasonZonePairs = new List<(ZoneTransferReason, MtgZone)>();

	private bool _activeStagger;

	private float _staggerTimer;

	private float _targetStaggerDuration;

	private const uint MAX_ACTIVE_CARD_CREATIONS = 3u;

	private readonly float MAX_STAGGER_TIME = 1.5f;

	private const string TOSTRING_NO_EVENTS = "ZoneTransferGroup: No Events";

	private const string TOSTRING_HEADER = "ZoneTransferGroup";

	private const string TOSTRING_PENDING_EVENTS = "Pending Events:";

	private const string TOSTRING_RUNNING_EVENTS = "Running Events:";

	private const string TOSTRING_TAB = "     ";

	public override bool IsBlocking => true;

	public ZoneTransferGroup(ZoneTransferUXEvent zoneTransferEvent)
	{
		Enqueue(zoneTransferEvent);
		_canTimeOut = false;
	}

	public ZoneTransferGroup(ZoneTransferUXEvent zoneTransferEvent, IVfxProvider vfxProvider, AssetLookupSystem assetLookupSystem, GameManager gameManager)
		: this(zoneTransferEvent)
	{
		_vfxProvider = vfxProvider ?? NullVfxProvider.Default;
		_assetLookupSystem = assetLookupSystem;
		_gameManager = gameManager;
	}

	public void Enqueue(ZoneTransferUXEvent zte)
	{
		_zoneTransfers.Add(zte);
		_pendingEvents.Enqueue(zte);
	}

	public override void Execute()
	{
		InitializeVfxData();
		PlayStartFx();
		DequeueAndExecutePendingTransfer();
		if (_pendingEvents.Count == 0 && _runningEvents.TrueForAll((ZoneTransferUXEvent x) => x.IsComplete))
		{
			Complete();
		}
	}

	public override bool CanExecute(List<UXEvent> currentlyRunningEvents)
	{
		for (int i = 0; i < currentlyRunningEvents.Count; i++)
		{
			UXEvent uXEvent = currentlyRunningEvents[i];
			if (uXEvent.HasWeight)
			{
				return false;
			}
			if (uXEvent is ManaProducedUXEvent)
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		if (_pendingEvents.Count == 0 && _runningEvents.Count == 0)
		{
			return "ZoneTransferGroup: No Events";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("ZoneTransferGroup");
		stringBuilder.AppendLine();
		appendEventCollectionToSB(stringBuilder, "Pending Events:", _pendingEvents);
		appendEventCollectionToSB(stringBuilder, "Running Events:", _runningEvents);
		return stringBuilder.ToString();
		static void appendEventCollectionToSB(StringBuilder sb, string header, IReadOnlyCollection<UXEvent> events)
		{
			if (events.Count > 0)
			{
				sb.Append("     ");
				sb.Append(header);
				foreach (ZoneTransferUXEvent @event in events)
				{
					sb.AppendLine();
					sb.Append("     ");
					sb.Append("     ");
					sb.Append(@event.ToString());
				}
			}
		}
	}

	public override void Update(float dt)
	{
		base.Update(dt);
		if (!_activeStagger)
		{
			int num = 0;
			while (_pendingEvents.Count > 0)
			{
				ZoneTransferReason reason = _pendingEvents.Peek().Reason;
				if (TryGetStaggerDuration(reason, out var staggerTime))
				{
					_activeStagger = true;
					float num2 = staggerTime * (float)_zoneTransfers.Count;
					if (num2 > MAX_STAGGER_TIME)
					{
						float num3 = (num2 - MAX_STAGGER_TIME) / (float)_zoneTransfers.Count;
						staggerTime -= num3;
					}
					_targetStaggerDuration = staggerTime;
					break;
				}
				if (reason == ZoneTransferReason.CardCreated && (long)num >= 3L)
				{
					break;
				}
				DequeueAndExecutePendingTransfer();
				if (reason == ZoneTransferReason.CardCreated)
				{
					num++;
				}
			}
		}
		if (_activeStagger)
		{
			_staggerTimer += dt;
			bool num4 = _staggerTimer > _targetStaggerDuration;
			if (num4)
			{
				_staggerTimer -= _targetStaggerDuration;
			}
			if (num4)
			{
				_activeStagger = false;
				DequeueAndExecutePendingTransfer();
			}
		}
		_runningEvents.ForEach(delegate(ZoneTransferUXEvent x)
		{
			x.Update(dt);
		});
		for (int num5 = _runningEvents.Count - 1; num5 >= 0; num5--)
		{
			if (_runningEvents[num5].IsComplete)
			{
				_runningEvents.RemoveAt(num5);
			}
		}
		if (_pendingEvents.Count == 0 && _runningEvents.Count == 0)
		{
			Complete();
		}
	}

	protected override void OnComplete()
	{
		PlayEndFx();
	}

	private void DequeueAndExecutePendingTransfer()
	{
		ZoneTransferUXEvent zoneTransferUXEvent = _pendingEvents.Dequeue();
		_runningEvents.Add(zoneTransferUXEvent);
		zoneTransferUXEvent.Execute();
	}

	public override IEnumerable<uint> GetInvolvedIds()
	{
		foreach (ZoneTransferUXEvent zoneTransfer in _zoneTransfers)
		{
			foreach (uint involvedId in zoneTransfer.GetInvolvedIds())
			{
				yield return involvedId;
			}
		}
	}

	private void InitializeVfxData()
	{
		MtgCardInstance mtgCardInstance = null;
		foreach (ZoneTransferUXEvent zoneTransfer in _zoneTransfers)
		{
			if (mtgCardInstance == null)
			{
				mtgCardInstance = zoneTransfer.Instigator;
			}
			else if (mtgCardInstance != zoneTransfer.Instigator)
			{
				mtgCardInstance = null;
				break;
			}
		}
		if (mtgCardInstance != null)
		{
			if (mtgCardInstance.ObjectType != GameObjectType.Ability && mtgCardInstance.Children.Count > 0)
			{
				MtgCardInstance mtgCardInstance2 = mtgCardInstance.Children.Find((MtgCardInstance x) => x.ObjectType == GameObjectType.Ability);
				if (mtgCardInstance2 != null)
				{
					mtgCardInstance = mtgCardInstance2;
				}
			}
			_transferInstigatorDataAdapter = CardDataExtensions.CreateWithDatabase(mtgCardInstance, _gameManager.CardDatabase);
		}
		foreach (ZoneTransferUXEvent zte in _zoneTransfers)
		{
			if (zte.FromZone != null && !_reasonZonePairs.Exists(((ZoneTransferReason, MtgZone) x) => x.Item1 == zte.Reason && x.Item2 == zte.FromZone))
			{
				_reasonZonePairs.Add((zte.Reason, zte.FromZone));
			}
		}
	}

	private void FillBlackboard()
	{
		IBlackboard blackboard = _assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.InDuelScene = true;
		blackboard.InWrapper = false;
		blackboard.ActiveResolution = _gameManager.ActiveResolutionEffect;
		blackboard.Interaction = _gameManager.CurrentInteraction;
		blackboard.BattlefieldId = BattlefieldUtil.BattlefieldId;
		blackboard.SetCardDataExtensive(_transferInstigatorDataAdapter);
		if (_transferInstigatorDataAdapter == null)
		{
			return;
		}
		if (_transferInstigatorDataAdapter.ObjectType == GameObjectType.Ability)
		{
			blackboard.Ability = _gameManager.CardDatabase.AbilityDataProvider.GetAbilityPrintingById(_transferInstigatorDataAdapter.GrpId);
			return;
		}
		if (_transferInstigatorDataAdapter.Children.Count > 0)
		{
			MtgCardInstance mtgCardInstance = _transferInstigatorDataAdapter.Children.Find(GameObjectType.Ability, (MtgCardInstance x, GameObjectType t) => x.ObjectType == t);
			if (mtgCardInstance != null)
			{
				blackboard.Ability = _gameManager.CardDatabase.AbilityDataProvider.GetAbilityPrintingById(mtgCardInstance.GrpId);
				return;
			}
		}
		blackboard.Ability = _transferInstigatorDataAdapter.Abilities.FirstOrDefault((AbilityPrintingData x) => x.Category == AbilityCategory.Spell);
	}

	private bool TryGetStaggerDuration(ZoneTransferReason reason, out float staggerTime)
	{
		staggerTime = 0f;
		FillBlackboard();
		_assetLookupSystem.Blackboard.ZoneTransferReason = reason;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ZoneTransfer_GroupStagger> loadedTree))
		{
			ZoneTransfer_GroupStagger payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				staggerTime = payload.Stagger;
				return true;
			}
		}
		return false;
	}

	private void PlayStartFx()
	{
		FillBlackboard();
		foreach (var (zoneTransferReason, fromZone) in _reasonZonePairs)
		{
			_assetLookupSystem.Blackboard.FromZone = fromZone;
			_assetLookupSystem.Blackboard.ZoneTransferReason = zoneTransferReason;
			if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ZoneTransfer_GroupEffect_Start> loadedTree))
			{
				continue;
			}
			ZoneTransfer_GroupEffect_Start payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload == null)
			{
				continue;
			}
			GameObject audioRoot = null;
			foreach (VfxData vfxData in payload.VfxDatas)
			{
				audioRoot = PlayVfx(vfxData);
			}
			PlaySfx(payload.SfxData, audioRoot);
		}
	}

	private void PlayEndFx()
	{
		FillBlackboard();
		foreach (var (zoneTransferReason, fromZone) in _reasonZonePairs)
		{
			_assetLookupSystem.Blackboard.FromZone = fromZone;
			_assetLookupSystem.Blackboard.ZoneTransferReason = zoneTransferReason;
			if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ZoneTransfer_GroupEffect_End> loadedTree))
			{
				continue;
			}
			ZoneTransfer_GroupEffect_End payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload == null)
			{
				continue;
			}
			GameObject audioRoot = null;
			foreach (VfxData vfxData in payload.VfxDatas)
			{
				audioRoot = PlayVfx(vfxData);
			}
			PlaySfx(payload.SfxData, audioRoot);
		}
	}

	private GameObject PlayVfx(VfxData vfxData)
	{
		return _vfxProvider.PlayVFX(vfxData, _transferInstigatorDataAdapter);
	}

	private void PlaySfx(SfxData sfxData, GameObject audioRoot)
	{
		if (sfxData != null && sfxData.AudioEvents?.Count > 0)
		{
			audioRoot = audioRoot ?? AudioManager.Default;
			AudioManager.PlayAudio(sfxData.AudioEvents, audioRoot);
		}
	}
}
