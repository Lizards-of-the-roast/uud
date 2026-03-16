using System;
using System.Collections.Generic;
using System.IO;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using Assets.Core.Shared.Code.Utilities;
using GreClient.CardData;
using GreClient.Rules;
using MovementSystem;
using Pooling;
using Unity.VisualScripting;
using UnityEngine;
using Wotc.Mtga.Duel;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.VFX;

namespace Wotc.Mtga.DuelScene.VFX;

public class VfxProvider : IVfxProvider
{
	private readonly IEntityViewProvider _entityViewProvider;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IUnityObjectPool _unityPool;

	private readonly IObjectPool _genericPool;

	private readonly ISplineMovementSystem _splineMovementSystem;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly CardMaterialBuilder _cardMaterialBuilder;

	private readonly TopCardOnlySpaceConverterCollectionUtility _topCardSpaceConverterUtility = new TopCardOnlySpaceConverterCollectionUtility();

	private readonly DefaultSpaceConverterCollectionUtility _defaultSpaceConverterUtility = new DefaultSpaceConverterCollectionUtility();

	private readonly IReadOnlyDictionary<RelativeSpace, ISpaceConverter> _spaceConverters;

	private readonly IReadOnlyCollection<AudioEvent> _flyingWooshAudio = (IReadOnlyCollection<AudioEvent>)(object)new AudioEvent[1]
	{
		new AudioEvent(WwiseEvents.sfx_basicloc_place_flying_whoosh.EventName)
	};

	private readonly IReadOnlyCollection<AudioEvent> _flyingImpactAudio = (IReadOnlyCollection<AudioEvent>)(object)new AudioEvent[1]
	{
		new AudioEvent(WwiseEvents.sfx_basicloc_place_impact_flying.EventName)
	};

	private readonly IReadOnlyCollection<AudioEvent> _wooshAudio = (IReadOnlyCollection<AudioEvent>)(object)new AudioEvent[1]
	{
		new AudioEvent(WwiseEvents.sfx_basicloc_place_whoosh.EventName)
	};

	private readonly IReadOnlyCollection<AudioEvent> _impactAudio = (IReadOnlyCollection<AudioEvent>)(object)new AudioEvent[1]
	{
		new AudioEvent(WwiseEvents.sfx_basicloc_place_impact.EventName)
	};

	private readonly CardVfxDupeHandler _cardVfxDupeHandler = new CardVfxDupeHandler();

	public VfxProvider(IObjectPool objPool, IUnityObjectPool unityPool, CardMaterialBuilder cardMaterialBuilder, AssetLookupSystem assetLookupSystem, ISplineMovementSystem splineMovementSystem, IGameStateProvider gameStateProvider, IEntityViewProvider entityViewProvider, IReadOnlyDictionary<RelativeSpace, ISpaceConverter> spaceConverters)
	{
		_unityPool = unityPool ?? NullUnityObjectPool.Default;
		_genericPool = objPool ?? NullObjectPool.Default;
		_cardMaterialBuilder = cardMaterialBuilder;
		_assetLookupSystem = assetLookupSystem;
		_splineMovementSystem = splineMovementSystem;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_entityViewProvider = entityViewProvider ?? NullEntityViewProvider.Default;
		_spaceConverters = spaceConverters ?? DictionaryExtensions.Empty<RelativeSpace, ISpaceConverter>();
	}

	public IReadOnlyCollection<SplineEvent> GenerateEtbSplineEvents(DuelScene_CDC cardView, IBattlefieldStack stack, bool playHitEffects, Transform localSpaceOverride = null)
	{
		GameObject gameObject = cardView.Root.gameObject;
		List<SplineEvent> list = new List<SplineEvent>();
		List<(IReadOnlyCollection<VfxData>, IReadOnlyCollection<SfxData>)> list2 = _genericPool.PopObject<List<(IReadOnlyCollection<VfxData>, IReadOnlyCollection<SfxData>)>>();
		LoadEtbDataFromAlt(cardView, out var shouldForceSubTypeAudio, list2);
		foreach (var item4 in list2)
		{
			IReadOnlyCollection<VfxData> item = item4.Item1;
			IReadOnlyCollection<SfxData> item2 = item4.Item2;
			float? num = null;
			foreach (VfxData item5 in item)
			{
				Transform item3 = localSpaceOverride;
				if (stack == null || (stack.HasAttachmentOrExile && !item5.PlayOnAttachmentStack))
				{
					continue;
				}
				if (item5.PrefabData.StartTime >= 0.8f)
				{
					if (!playHitEffects)
					{
						continue;
					}
					item3 = stack.StackParent.EffectsRoot;
				}
				num = Mathf.Clamp01(item5.PrefabData.StartTime);
				list.Add(new SplineEventCallbackWithParams<(ICardDataAdapter, MtgEntity, VfxData, Transform, IVfxProvider, IBattlefieldStack)>(num.Value, (cardView.Model, cardView.Model.Instance, item5, item3, this, stack), delegate(float time, (ICardDataAdapter effectContext, MtgEntity spaceContext, VfxData vfx, Transform overrideLocal, IVfxProvider vfxProvider, IBattlefieldStack stackRoot) callbackParams)
				{
					if (time < 0.8f || callbackParams.stackRoot == null || (callbackParams.stackRoot.StackParent != null && callbackParams.stackRoot.StackParent.IsVisible))
					{
						callbackParams.vfxProvider.PlayVFX(callbackParams.vfx, callbackParams.effectContext, callbackParams.spaceContext, callbackParams.overrideLocal);
					}
				}));
			}
			if (!num.HasValue && item.Count != 0)
			{
				continue;
			}
			foreach (SfxData item6 in item2)
			{
				list.Add(new SplineEventAudio(num.GetValueOrDefault(), item6.AudioEvents, gameObject));
			}
		}
		if (shouldForceSubTypeAudio && stack != null && !stack.HasAttachmentOrExile)
		{
			list.Add(new SplineEventCardAudioETB(1f, cardView));
		}
		if (cardView.Model.Abilities.Exists((AbilityPrintingData x) => x.Id == 8))
		{
			list.Add(new SplineEventAudio(0f, _flyingWooshAudio, gameObject));
			list.Add(new SplineEventAudio(1f, _flyingImpactAudio, gameObject));
		}
		else
		{
			list.Add(new SplineEventAudio(0f, _wooshAudio, gameObject));
			list.Add(new SplineEventAudio(1f, _impactAudio, gameObject));
		}
		return list;
	}

	public void GenerateEtbTriggerEvents(DuelScene_CDC cardView, List<SplineEvent> events, IBattlefieldStack stack, Transform localSpaceOverride = null)
	{
		List<IReadOnlyCollection<VfxData>> list = _genericPool.PopObject<List<IReadOnlyCollection<VfxData>>>();
		LoadEtbTriggerDataFromAlt(cardView, list);
		foreach (IReadOnlyCollection<VfxData> item in list)
		{
			float? num = null;
			foreach (VfxData item2 in item)
			{
				if (stack.HasAttachmentOrExile && !item2.PlayOnAttachmentStack)
				{
					continue;
				}
				events.Add(new SplineEventCallbackWithParams<(ICardDataAdapter, MtgEntity, VfxData, Transform, IVfxProvider, IBattlefieldStack)>(new float?(Mathf.Clamp01(item2.PrefabData.StartTime)).Value, (cardView.Model, cardView.Model.Instance, item2, localSpaceOverride, this, stack), delegate(float time, (ICardDataAdapter effectContext, MtgEntity spaceContext, VfxData vfx, Transform overrideLocal, IVfxProvider vfxProvider, IBattlefieldStack stackRoot) callbackParams)
				{
					if (time < 0.8f || callbackParams.stackRoot == null || (callbackParams.stackRoot.StackParent != null && callbackParams.stackRoot.StackParent.IsVisible))
					{
						callbackParams.vfxProvider.PlayVFX(callbackParams.vfx, callbackParams.effectContext, callbackParams.spaceContext, callbackParams.overrideLocal);
					}
				}));
			}
		}
		list.Clear();
		_genericPool.PushObject(list, tryClear: false);
	}

	private void LoadEtbDataFromAlt(DuelScene_CDC cardView, out bool shouldForceSubTypeAudio, List<(IReadOnlyCollection<VfxData> vfxDatas, IReadOnlyCollection<SfxData> sfxDatas)> etbEffects)
	{
		shouldForceSubTypeAudio = true;
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(cardView.Model);
		_assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Battlefield;
		_assetLookupSystem.Blackboard.ZonePair = new ZonePair(cardView.PreviousCardHolder, cardView.CurrentCardHolder);
		if (cardView.PreviousCardHolder is ZoneCardHolderBase zoneCardHolderBase)
		{
			_assetLookupSystem.Blackboard.FromZone = zoneCardHolderBase.GetZone;
		}
		if (cardView.CurrentCardHolder is ZoneCardHolderBase zoneCardHolderBase2)
		{
			_assetLookupSystem.Blackboard.ToZone = zoneCardHolderBase2.GetZone;
		}
		Dictionary<string, (List<VfxData>, List<SfxData>)> dictionary = _genericPool.PopObject<Dictionary<string, (List<VfxData>, List<SfxData>)>>();
		HashSet<EtbVFX> hashSet = _genericPool.PopObject<HashSet<EtbVFX>>();
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EtbVFX> loadedTree) && loadedTree.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet))
		{
			cardView.EffectsRoot.gameObject.UpdateActive(active: true);
			foreach (EtbVFX item in hashSet)
			{
				string key = string.Join(string.Empty, item.Layers);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = (dictionary[key] = (new List<VfxData>(3), new List<SfxData>(3)));
				}
				value.Item1.AddRange(item.VfxDatas);
			}
		}
		hashSet.Clear();
		_genericPool.PushObject(hashSet, tryClear: false);
		HashSet<EtbSFX> hashSet2 = _genericPool.PopObject<HashSet<EtbSFX>>();
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EtbSFX> loadedTree2) && loadedTree2.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet2))
		{
			foreach (EtbSFX item2 in hashSet2)
			{
				string key2 = string.Join(string.Empty, item2.Layers);
				if (!dictionary.TryGetValue(key2, out var value2))
				{
					value2 = (dictionary[key2] = (new List<VfxData>(3), new List<SfxData>(3)));
				}
				value2.Item2.Add(item2.SfxData);
				shouldForceSubTypeAudio &= item2.ForceSubtype;
			}
		}
		hashSet2.Clear();
		_genericPool.PushObject(hashSet2, tryClear: false);
		etbEffects.Clear();
		foreach (KeyValuePair<string, (List<VfxData>, List<SfxData>)> item3 in dictionary)
		{
			(List<VfxData>, List<SfxData>) value3 = item3.Value;
			etbEffects.Add((value3.Item1, value3.Item2));
		}
		dictionary.Clear();
		_genericPool.PushObject(dictionary, tryClear: false);
		_assetLookupSystem.Blackboard.Clear();
	}

	private void LoadEtbTriggerDataFromAlt(DuelScene_CDC cardView, List<IReadOnlyCollection<VfxData>> etbEffects)
	{
		AssetLookupSystem assetLookupSystem = _assetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(cardView.Model);
		assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Battlefield;
		assetLookupSystem.Blackboard.ZonePair = new ZonePair(cardView.PreviousCardHolder, cardView.CurrentCardHolder);
		assetLookupSystem.Blackboard.GREPlayerNum = cardView.Model.ControllerNum;
		if (cardView.PreviousCardHolder is ZoneCardHolderBase zoneCardHolderBase)
		{
			assetLookupSystem.Blackboard.FromZone = zoneCardHolderBase.GetZone;
		}
		if (cardView.CurrentCardHolder is ZoneCardHolderBase zoneCardHolderBase2)
		{
			assetLookupSystem.Blackboard.ToZone = zoneCardHolderBase2.GetZone;
		}
		Dictionary<string, List<VfxData>> dictionary = _genericPool.PopObject<Dictionary<string, List<VfxData>>>();
		HashSet<EtbTriggerVFX> hashSet = _genericPool.PopObject<HashSet<EtbTriggerVFX>>();
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<EtbTriggerVFX> loadedTree) && loadedTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet))
		{
			foreach (EtbTriggerVFX item in hashSet)
			{
				string key = string.Join(string.Empty, item.Layers);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = (dictionary[key] = new List<VfxData>(3));
				}
				value.AddRange(item.VfxDatas);
			}
		}
		hashSet.Clear();
		_genericPool.PushObject(hashSet, tryClear: false);
		foreach (KeyValuePair<string, List<VfxData>> item2 in dictionary)
		{
			etbEffects.Add(item2.Value);
		}
		dictionary.Clear();
		_genericPool.PushObject(dictionary, tryClear: false);
		assetLookupSystem.Blackboard.Clear();
	}

	public GameObject PlayVFX(VfxData vfxData, ICardDataAdapter effectContext, MtgEntity spaceContext = null, Transform localSpaceOverride = null, string prefabPath = null)
	{
		return PlayVFX(vfxData, effectContext, spaceContext, localSpaceOverride, AnchorPointType.Invalid, null, prefabPath);
	}

	public GameObject PlayAnchoredVFX(VfxData vfxData, AnchorPointType anchorType, ScaffoldingBase scaffold, ICardDataAdapter effectContext, MtgEntity spaceContext = null, Transform localSpaceOverride = null)
	{
		return PlayVFX(vfxData, effectContext, spaceContext, localSpaceOverride, anchorType, scaffold);
	}

	private GameObject PlayVFX(VfxData vfxData, ICardDataAdapter effectContext, MtgEntity spaceContext, Transform localSpaceOverride, AnchorPointType anchorType, ScaffoldingBase scaffold, string prefabPath = null)
	{
		if (vfxData == null)
		{
			Debug.LogError("VfxProvider.PlayVFX() -> Invalid parameter! 'vfxData' is null.");
			return null;
		}
		if (spaceContext == null && effectContext != null)
		{
			spaceContext = effectContext.Instance;
		}
		IEntityView entityView = ((spaceContext != null) ? _entityViewProvider.GetEntity(spaceContext.InstanceId) : null);
		if (!localSpaceOverride && entityView != null)
		{
			localSpaceOverride = entityView.EffectsRoot;
		}
		GameObject result = null;
		foreach (Transform item in ResolveSpaceIntoTransforms(vfxData.SpaceData.Space, spaceContext, localSpaceOverride, vfxData.PlayOnStackChildren))
		{
			switch (vfxData.ActivationType)
			{
			case VfxActivationType.LoopingStart:
				if (LoopingAnimationManager.IsLoopRunning(item, vfxData.LoopingKey))
				{
					continue;
				}
				break;
			case VfxActivationType.LoopingEnd:
				LoopingAnimationManager.RemoveLoopingEffect(item, vfxData.LoopingKey);
				continue;
			case VfxActivationType.OneShotLoopEnd:
				if (!LoopingAnimationManager.IsLoopRunning(item, vfxData.LoopingKey))
				{
					continue;
				}
				break;
			}
			if (prefabPath == null)
			{
				prefabPath = vfxData.PrefabData.RandomPrefabPath;
			}
			if (!vfxData.IgnoreDedupe)
			{
				if (_cardVfxDupeHandler.IsTransformDuplicate(item, prefabPath))
				{
					continue;
				}
				switch (vfxData.TurnWideDataOptions)
				{
				case TurnWideDataOptions.World:
					if (_cardVfxDupeHandler.IsTurnDuplicate(_gameStateProvider.CurrentGameState, prefabPath))
					{
						continue;
					}
					break;
				case TurnWideDataOptions.Instance:
					if (effectContext != null && _cardVfxDupeHandler.IsTurnDuplicateForInstance(effectContext.InstanceId, _gameStateProvider.CurrentGameState, prefabPath))
					{
						continue;
					}
					break;
				}
			}
			GameObject gameObject = _unityPool.PopObject(prefabPath);
			if (!gameObject)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(prefabPath);
				throw new NullReferenceException("Failed to instantiate prefab (\"" + fileNameWithoutExtension + "\") for VfxPayload:\n" + prefabPath);
			}
			Transform transform = gameObject.transform;
			Vector3 lossyScale = transform.lossyScale;
			SetVFXPosition(transform, vfxData, lossyScale, item, anchorType, scaffold);
			if (vfxData.SpaceData.ReverseIfOpponent && effectContext != null && !effectContext.Controller.IsLocalPlayer)
			{
				transform.eulerAngles += new Vector3(0f, 180f, 0f);
			}
			float? cleanupTime = vfxData.PrefabData.CleanupAfterTime;
			if (vfxData.PrefabData.SkipSelfCleanup || (vfxData.ActivationType == VfxActivationType.LoopingStart && LoopingAnimationManager.AddLoopingEffect(item, vfxData.LoopingKey, vfxData.CanSurviveZoneTransfer, gameObject)))
			{
				cleanupTime = null;
				gameObject.AddOrGetComponent<AkStopAllOnDisable>();
			}
			HandleSpawnedInstance(effectContext, gameObject, cleanupTime, vfxData);
			result = gameObject;
			if (vfxData.ActivationType == VfxActivationType.OneShotLoopEnd)
			{
				LoopingAnimationManager.RemoveLoopingEffect(item, vfxData.LoopingKey);
			}
		}
		return result;
	}

	private void SetVFXPosition(Transform instanceTran, VfxData vfxData, Vector3 lossyScale, Transform vfxOriginRoot, AnchorPointType anchorType = AnchorPointType.Invalid, ScaffoldingBase scaffold = null)
	{
		instanceTran.SetParent(vfxOriginRoot);
		Transform transform = null;
		if (anchorType != AnchorPointType.Invalid && scaffold != null)
		{
			transform = scaffold.AllAnchorPoints.Find(anchorType, (CDCSmartAnchor smartAnchor, AnchorPointType anchorPointType) => smartAnchor.AnchorType == anchorPointType).transform;
		}
		Vector3 localPosition = (transform ? transform.localPosition : Vector3.zero);
		if (vfxData.AddParentZPositionToOffset)
		{
			localPosition += new Vector3(0f, 0f, instanceTran.localPosition.z);
		}
		localPosition += vfxData.Offset.PositionOffset;
		instanceTran.localPosition = localPosition;
		Vector3 vector = ((!transform) ? Vector3.zero : (vfxData.Offset.RotationIsWorld ? transform.eulerAngles : transform.localEulerAngles));
		vector += vfxData.Offset.RotationOffset;
		if (!vfxData.Offset.RotationIsWorld)
		{
			instanceTran.localEulerAngles = vector;
		}
		else
		{
			instanceTran.eulerAngles = vector;
		}
		Vector3 scaleMultiplier = vfxData.Offset.ScaleMultiplier;
		if (!vfxData.Offset.ScaleIsWorld)
		{
			instanceTran.localScale = scaleMultiplier;
		}
		else
		{
			instanceTran.localScale = new Vector3(scaleMultiplier.x / lossyScale.x, scaleMultiplier.y / lossyScale.y, scaleMultiplier.z / lossyScale.z);
		}
		if (!vfxData.ParentToSpace)
		{
			instanceTran.parent = null;
		}
	}

	private void HandleSpawnedInstance(ICardDataAdapter relevantCard, GameObject instance, float? cleanupTime, VfxData vfxData = null)
	{
		SelfCleanup selfCleanup = instance.GetComponent<SelfCleanup>();
		if (!cleanupTime.HasValue)
		{
			if ((bool)selfCleanup)
			{
				UnityEngine.Object.Destroy(selfCleanup);
			}
		}
		else
		{
			if (!selfCleanup)
			{
				selfCleanup = instance.AddComponent<SelfCleanup>();
			}
			selfCleanup.SetLifetime(cleanupTime.Value);
		}
		if (vfxData != null && vfxData.HideIfNotTopOfStack)
		{
			instance.AddComponent<VfxOptionsComponent>().HideIfNotTopOfStack = vfxData.HideIfNotTopOfStack;
		}
		bool flag = vfxData?.HasDelay(_assetLookupSystem, relevantCard) ?? false;
		if (flag)
		{
			instance.transform.parent.AddComponent<DelayVFXCallback>().StartDelay(vfxData.DelayData?.Time ?? 0f, instance, delegate(DelayVFXCallback obj)
			{
				UnityEngine.Object.Destroy(obj);
			});
		}
		if (relevantCard != null)
		{
			ApplyCardArtToVFX component = instance.GetComponent<ApplyCardArtToVFX>();
			if ((bool)component)
			{
				component.Apply(relevantCard.ImageAssetPath, _cardMaterialBuilder.TextureLoader, _cardMaterialBuilder.CropDatabase);
			}
		}
		if (relevantCard != null)
		{
			ApplyMaterialReplacementsToVFX component2 = instance.GetComponent<ApplyMaterialReplacementsToVFX>();
			if ((bool)component2)
			{
				component2.Apply(relevantCard, _assetLookupSystem);
			}
		}
		AutomaticProjectile component3 = instance.GetComponent<AutomaticProjectile>();
		if ((bool)component3)
		{
			PlayAutoProjectile(component3, relevantCard);
		}
		if (!instance.activeSelf && !flag)
		{
			instance.SetActive(value: true);
		}
	}

	private void PlayAutoProjectile(AutomaticProjectile autoProjectile, ICardDataAdapter relevantCard)
	{
		if (!autoProjectile)
		{
			throw new NullReferenceException("AutoProjectile passed into PayloadBase_VFX_Utils.PlayAutoProjectile() was null!");
		}
		if (autoProjectile.ProjectilePrefab == null && autoProjectile.HitPrefab == null)
		{
			Debug.LogErrorFormat("AutomaticProjectile on object \"{0}\" had neither a ProjectilePrefab nor HitPrefab, so it's not doing anything.", autoProjectile);
			return;
		}
		SplineMovementData splineMovementData = (autoProjectile.Spline ? autoProjectile.Spline : null);
		if (!splineMovementData)
		{
			splineMovementData = ScriptableObject.CreateInstance<SplineMovementData>();
			splineMovementData.Spline = SplineData.Parabolic;
		}
		foreach (Transform item in ResolveSpaceIntoTransforms(autoProjectile.TargetSpace, relevantCard?.Instance))
		{
			Transform projectileTransform = new GameObject("Auto Projectile: " + autoProjectile.name).transform;
			projectileTransform.position = autoProjectile.transform.position;
			SplineEventData splineEventData = new SplineEventData();
			if (autoProjectile.ProjectilePrefab != null)
			{
				splineEventData.Events.Add(new SplineEventCallback(autoProjectile.ProjectilePrefab.StartTime, delegate
				{
					SpawnEffectFromPrefab(autoProjectile.ProjectilePrefab, projectileTransform, null);
				}));
			}
			if (autoProjectile.HitPrefab != null)
			{
				Transform cachedTargetTransform = item;
				splineEventData.Events.Add(new SplineEventCallback(autoProjectile.HitPrefab.StartTime, delegate
				{
					SpawnEffectFromPrefab(autoProjectile.HitPrefab, cachedTargetTransform, autoProjectile.HitOffsetData);
				}));
			}
			splineEventData.Events.Add(new SplineEventCallbackWithParams<Transform>(1f, projectileTransform, delegate(float _, Transform projTran)
			{
				projTran.gameObject.AddOrGetComponent<SelfCleanup>().SetLifetime(0.1f, SelfCleanup.CleanupType.Destroy, onlyWhenChildless: true);
			}));
			IdealPoint endPoint = new IdealPoint(item);
			if (autoProjectile.HitOffsetData != null)
			{
				endPoint.Position += autoProjectile.HitOffsetData.PositionOffset;
				endPoint.Rotation *= Quaternion.Euler(autoProjectile.HitOffsetData.RotationOffset);
				endPoint.Scale = Vector3.Scale(endPoint.Scale, autoProjectile.HitOffsetData.ScaleMultiplier);
			}
			_splineMovementSystem.AddTemporaryGoal(projectileTransform, endPoint, allowInteractions: false, splineMovementData, splineEventData);
		}
		void SpawnEffectFromPrefab(VfxPrefabData prefabData, Transform parent, OffsetData offsets)
		{
			string randomPrefabPath = prefabData.RandomPrefabPath;
			if (string.IsNullOrEmpty(randomPrefabPath))
			{
				Debug.LogErrorFormat("Failed to create AutoProjectile component because the provided VfxPrefabData had a null Prefab Path.");
			}
			else
			{
				GameObject gameObject = _unityPool.PopObject(randomPrefabPath);
				if (!gameObject)
				{
					Debug.LogErrorFormat("Failed to create instance of AutoProjectile component prefab \"{0}\".", Path.GetFileNameWithoutExtension(randomPrefabPath));
				}
				else
				{
					Transform transform = gameObject.transform;
					Vector3 lossyScale = parent.lossyScale;
					transform.SetParent(parent);
					if (offsets != null)
					{
						transform.localPosition = offsets.PositionOffset;
						if (!offsets.RotationIsWorld)
						{
							transform.localEulerAngles = offsets.RotationOffset;
						}
						else
						{
							transform.eulerAngles = offsets.RotationOffset;
						}
						if (!offsets.ScaleIsWorld)
						{
							transform.localScale = offsets.ScaleMultiplier;
						}
						else
						{
							transform.localScale = new Vector3(offsets.ScaleMultiplier.x / lossyScale.x, offsets.ScaleMultiplier.y / lossyScale.y, offsets.ScaleMultiplier.z / lossyScale.z);
						}
					}
					else
					{
						transform.ZeroOut();
					}
					HandleSpawnedInstance(relevantCard, gameObject, prefabData.CleanupAfterTime);
				}
			}
		}
	}

	public IEnumerable<Transform> ResolveSpaceIntoTransforms(RelativeSpace space, MtgEntity spaceContext, Transform localSpaceOverride = null, bool resolveForStackChildren = false)
	{
		IEntityView entityView = ((spaceContext != null) ? _entityViewProvider.GetEntity(spaceContext.InstanceId) : null);
		Transform transform = entityView?.EffectsRoot;
		if (!localSpaceOverride)
		{
			if (entityView is DuelScene_CDC { CurrentCardHolder: IBattlefieldCardHolder currentCardHolder } duelScene_CDC)
			{
				IBattlefieldStack stackForCard = currentCardHolder.GetStackForCard(duelScene_CDC);
				if (stackForCard != null && !stackForCard.HasAttachmentOrExile && !resolveForStackChildren && (bool)stackForCard.StackParent)
				{
					localSpaceOverride = stackForCard.StackParent.EffectsRoot;
					goto IL_00d4;
				}
			}
			if ((bool)transform)
			{
				localSpaceOverride = transform;
			}
		}
		goto IL_00d4;
		IL_00d4:
		HashSet<Transform> rootList = _genericPool.PopObject<HashSet<Transform>>();
		if (_spaceConverters.TryGetValue(space, out var value))
		{
			ISpaceConverter spaceConverter = value;
			Transform localTransform = localSpaceOverride;
			ISpaceConverterCollectionUtility spaceConverterUtility;
			if (!resolveForStackChildren)
			{
				ISpaceConverterCollectionUtility topCardSpaceConverterUtility = _topCardSpaceConverterUtility;
				spaceConverterUtility = topCardSpaceConverterUtility;
			}
			else
			{
				ISpaceConverterCollectionUtility topCardSpaceConverterUtility = _defaultSpaceConverterUtility;
				spaceConverterUtility = topCardSpaceConverterUtility;
			}
			spaceConverter.PopulateSet(localTransform, spaceContext, rootList, spaceConverterUtility);
			rootList.Remove(null);
			foreach (Transform item in rootList)
			{
				if ((bool)item)
				{
					yield return item;
				}
			}
		}
		rootList.Clear();
		_genericPool.PushObject(rootList, tryClear: false);
	}
}
