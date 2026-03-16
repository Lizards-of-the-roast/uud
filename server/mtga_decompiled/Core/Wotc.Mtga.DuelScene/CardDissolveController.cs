using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Card;
using Core.Shared.Code.Utilities;
using GreClient.CardData;
using GreClient.Rules;
using MovementSystem;
using UnityEngine;
using Wotc.Mtga.Duel;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene;

public class CardDissolveController : ICardDissolveController, IUpdate, IDisposable
{
	private class DissolveData
	{
		public enum DissolveState
		{
			Pending,
			Dissolving,
			Dissolved,
			Rebuilding,
			Rebuilt
		}

		public DissolveState State;

		public float Progress;

		public float DissolveSpeed = 1f;

		public float HideEffectsThreshold = 1f;

		public string RampPath;

		public string NoisePath;

		public readonly HashSet<MaterialReplacementData> MatReplacementDatas = new HashSet<MaterialReplacementData>();

		public float PendingWaitSeconds;

		public float DissolvedWaitSeconds = 1f;

		public bool SkipMovementHold;

		public Action OnDissolved;
	}

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IVfxProvider _vfxProvider;

	private readonly CardViewBuilder _cardViewBuilder;

	private readonly ISplineMovementSystem _splineMovementSystem;

	private readonly Dictionary<DuelScene_CDC, DissolveData> _dissolvingCardMap = new Dictionary<DuelScene_CDC, DissolveData>(10);

	private readonly Dictionary<DuelScene_CDC, DissolveData> _cardsToAdd = new Dictionary<DuelScene_CDC, DissolveData>();

	private readonly HashSet<DuelScene_CDC> _cardsToRemove = new HashSet<DuelScene_CDC>();

	public CardDissolveController(AssetLookupSystem assetLookupSystem, IVfxProvider vfxProvider, CardViewBuilder cardViewBuilder, ISplineMovementSystem splineMovementSystem)
	{
		_assetLookupSystem = assetLookupSystem;
		_vfxProvider = vfxProvider;
		_cardViewBuilder = cardViewBuilder;
		_splineMovementSystem = splineMovementSystem;
		_cardViewBuilder.onCardUpdated += OnCardUpdated;
		_cardViewBuilder.preCardDestroyEvent += OnCardDestroy;
	}

	public void Dispose()
	{
		_cardViewBuilder.onCardUpdated -= OnCardUpdated;
		_cardViewBuilder.preCardDestroyEvent -= OnCardDestroy;
	}

	public void OnUpdate(float deltaTime)
	{
		foreach (KeyValuePair<DuelScene_CDC, DissolveData> item in _cardsToAdd)
		{
			_dissolvingCardMap[item.Key] = item.Value;
		}
		_cardsToAdd.Clear();
		foreach (DuelScene_CDC item2 in _cardsToRemove)
		{
			if (_dissolvingCardMap.ContainsKey(item2))
			{
				DissolveData dissolveData = _dissolvingCardMap[item2];
				if (dissolveData.State == DissolveData.DissolveState.Dissolved && (bool)item2 && (bool)item2.EffectsRoot)
				{
					item2.EffectsRoot.gameObject.UpdateActive(active: true);
				}
				foreach (MaterialReplacementData matReplacementData in dissolveData.MatReplacementDatas)
				{
					matReplacementData.Instance?.Cleanup();
				}
			}
			_dissolvingCardMap.Remove(item2);
		}
		_cardsToRemove.Clear();
		foreach (KeyValuePair<DuelScene_CDC, DissolveData> item3 in _dissolvingCardMap)
		{
			DuelScene_CDC key = item3.Key;
			DissolveData value = item3.Value;
			switch (value.State)
			{
			case DissolveData.DissolveState.Pending:
				value.Progress += deltaTime;
				if (!(value.Progress >= value.PendingWaitSeconds))
				{
					break;
				}
				value.Progress = 0f;
				value.State = DissolveData.DissolveState.Dissolving;
				foreach (CDCPart value2 in key.ActiveParts.Values)
				{
					value2.GenerateDissolveMaterial(key.VisualModel, key.CurrentCardHolder.CardHolderType, value.RampPath, value.NoisePath, value.MatReplacementDatas);
					value2.SetDestroyed(destroyed: true);
				}
				break;
			case DissolveData.DissolveState.Dissolving:
				value.Progress += deltaTime * value.DissolveSpeed;
				foreach (MaterialReplacementData matReplacementData2 in value.MatReplacementDatas)
				{
					matReplacementData2.Instance?.SetFloat(ShaderPropertyIds.DissolveAmountPropId, value.Progress);
					matReplacementData2.UpdatePropertyBlocks();
				}
				if (value.Progress >= value.HideEffectsThreshold && key.EffectsRoot.gameObject.activeSelf)
				{
					key.EffectsRoot.gameObject.UpdateActive(active: false);
				}
				if (!(value.Progress >= 1f))
				{
					break;
				}
				value.OnDissolved?.Invoke();
				value.Progress = 0f;
				value.State = DissolveData.DissolveState.Dissolved;
				foreach (CDCPart value3 in key.ActiveParts.Values)
				{
					value3.EnableRenderers(enabled: false);
				}
				break;
			case DissolveData.DissolveState.Dissolved:
				value.Progress += deltaTime;
				if (!(value.Progress >= value.DissolvedWaitSeconds) || (!value.SkipMovementHold && !(_splineMovementSystem.GetProgress(key.Root) >= 1f)))
				{
					break;
				}
				value.Progress = 0f;
				value.State = DissolveData.DissolveState.Rebuilding;
				key.EffectsRoot.gameObject.UpdateActive(active: true);
				foreach (CDCPart value4 in key.ActiveParts.Values)
				{
					value4.EnableRenderers(enabled: true);
				}
				break;
			case DissolveData.DissolveState.Rebuilding:
				value.Progress += deltaTime * value.DissolveSpeed * 2f;
				foreach (MaterialReplacementData matReplacementData3 in value.MatReplacementDatas)
				{
					matReplacementData3.Instance?.SetFloat(ShaderPropertyIds.DissolveAmountPropId, 1f - value.Progress);
					matReplacementData3.UpdatePropertyBlocks();
				}
				if (value.Progress >= 1f)
				{
					value.Progress = 0f;
					value.State = DissolveData.DissolveState.Rebuilt;
				}
				break;
			case DissolveData.DissolveState.Rebuilt:
				key.IsBeingDestroyed = false;
				key.CDCReferences.InputCollider.enabled = true;
				foreach (CDCPart value5 in key.ActiveParts.Values)
				{
					value5.CleanupDissolveMaterial();
					value5.SetDestroyed(destroyed: false);
				}
				_cardsToRemove.Add(key);
				key.UpdateVisuals();
				break;
			}
		}
	}

	private void OnCardUpdated(BASE_CDC cardView)
	{
		if (!cardView || !(cardView is DuelScene_CDC duelScene_CDC) || !_dissolvingCardMap.TryGetValue(duelScene_CDC, out var value) || value.State == DissolveData.DissolveState.Rebuilt)
		{
			return;
		}
		foreach (CDCPart value2 in duelScene_CDC.ActiveParts.Values)
		{
			value2.GenerateDissolveMaterial(duelScene_CDC.VisualModel, duelScene_CDC.CurrentCardHolder.CardHolderType, value.RampPath, value.NoisePath, value.MatReplacementDatas);
			value2.SetDestroyed(destroyed: true);
			if (value.State == DissolveData.DissolveState.Dissolved)
			{
				value2.EnableRenderers(enabled: false);
			}
		}
	}

	private void OnCardDestroy(BASE_CDC cardView)
	{
		if ((bool)cardView && cardView is DuelScene_CDC item)
		{
			_cardsToRemove.Add(item);
		}
	}

	public void DissolveCard(DuelScene_CDC cardView, Action onComplete, ZoneTransferReason reason, CardData responsibleCard, MtgZone fromZone, MtgZone toZone)
	{
		if (!cardView)
		{
			return;
		}
		CardData cardData = (CardData)cardView.VisualModel;
		Transform root = cardView.Root;
		GameObject gameObject = root.gameObject;
		CardHolderType holderType = cardView.HolderType;
		_splineMovementSystem.AddTemporaryGoal(root, new IdealPoint(root));
		cardView.CDCReferences.InputCollider.enabled = false;
		LoopingAnimationManager.RemoveAllLoopingEffects(cardView.EffectsRoot);
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(responsibleCard);
		_assetLookupSystem.Blackboard.SetAbilityDataFromChild(responsibleCard);
		_assetLookupSystem.Blackboard.CardHolderType = holderType;
		_assetLookupSystem.Blackboard.ZoneTransferReason = reason;
		_assetLookupSystem.Blackboard.SupplementalCardData[SupplementalKey.Target] = cardData;
		_assetLookupSystem.Blackboard.ZonePair = new ZonePair(fromZone, toZone);
		if (cardView.PreviousCardHolder is ZoneCardHolderBase zoneCardHolderBase)
		{
			_assetLookupSystem.Blackboard.FromZone = zoneCardHolderBase.GetZone;
		}
		if (cardView.CurrentCardHolder is ZoneCardHolderBase zoneCardHolderBase2)
		{
			_assetLookupSystem.Blackboard.ToZone = zoneCardHolderBase2.GetZone;
		}
		AssetLookupTree<DeathPayload_Data> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<DeathPayload_Data>();
		AssetLookupTree<DeathPayload_Textures> assetLookupTree2 = _assetLookupSystem.TreeLoader.LoadTree<DeathPayload_Textures>();
		AssetLookupTree<DeathSFX> assetLookupTree3 = _assetLookupSystem.TreeLoader.LoadTree<DeathSFX>();
		AssetLookupTree<DeathVFX> assetLookupTree4 = _assetLookupSystem.TreeLoader.LoadTree<DeathVFX>();
		DeathPayload_Data payload = assetLookupTree.GetPayload(_assetLookupSystem.Blackboard);
		DeathPayload_Textures payload2 = assetLookupTree2.GetPayload(_assetLookupSystem.Blackboard);
		DeathSFX payload3 = assetLookupTree3.GetPayload(_assetLookupSystem.Blackboard);
		DeathVFX payload4 = assetLookupTree4.GetPayload(_assetLookupSystem.Blackboard);
		string rampPath = null;
		string noisePath = null;
		if (payload2 != null)
		{
			rampPath = payload2.RampTexRef.RelativePath;
			noisePath = payload2.NoiseTexRef.RelativePath;
		}
		float pendingWaitSeconds = 0f;
		float dissolveSpeed = 1f;
		float dissolvedWaitSeconds = 1f;
		float hideEffectsThreshold = 1f;
		bool skipMovementHold = false;
		if (payload != null)
		{
			pendingWaitSeconds = payload.DissolveDelay;
			dissolveSpeed = payload.DissolveSpeed;
			hideEffectsThreshold = payload.HideEffectsTime;
			dissolvedWaitSeconds = payload.DissolveWait;
			skipMovementHold = payload.BypassMovementWait;
		}
		_cardsToAdd[cardView] = new DissolveData
		{
			RampPath = rampPath,
			NoisePath = noisePath,
			State = DissolveData.DissolveState.Pending,
			Progress = 0f,
			DissolveSpeed = dissolveSpeed,
			HideEffectsThreshold = hideEffectsThreshold,
			DissolvedWaitSeconds = dissolvedWaitSeconds,
			PendingWaitSeconds = pendingWaitSeconds,
			SkipMovementHold = skipMovementHold,
			OnDissolved = onComplete
		};
		if (payload4 != null)
		{
			foreach (VfxData vfxData2 in payload4.VfxDatas)
			{
				foreach (string item in vfxData2.PrefabData.AllPrefabPaths())
				{
					_vfxProvider.PlayVFX(vfxData2, cardData, null, null, item);
				}
			}
		}
		if (payload3 != null)
		{
			AudioManager.PlayAudio(payload3.SfxData.AudioEvents, gameObject);
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_combat_creature_deth_gen, gameObject);
		if (responsibleCard == null || (responsibleCard != null && cardData.InstanceId != responsibleCard.InstanceId))
		{
			_assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
			AssetLookupTree<DeathPayload_SFX_Victim> assetLookupTree5 = _assetLookupSystem.TreeLoader.LoadTree<DeathPayload_SFX_Victim>();
			AssetLookupTree<DeathPayload_VFX_Victim> assetLookupTree6 = _assetLookupSystem.TreeLoader.LoadTree<DeathPayload_VFX_Victim>();
			DeathPayload_SFX_Victim payload5 = assetLookupTree5.GetPayload(_assetLookupSystem.Blackboard);
			DeathPayload_VFX_Victim payload6 = assetLookupTree6.GetPayload(_assetLookupSystem.Blackboard);
			if (payload5 != null)
			{
				AudioManager.PlayAudio(payload5.SfxData.AudioEvents, gameObject);
			}
			if (payload6 != null)
			{
				VfxData vfxData = payload6.VfxData;
				_vfxProvider.PlayVFX(vfxData, cardData);
			}
		}
		_assetLookupSystem.Blackboard.Clear();
	}
}
