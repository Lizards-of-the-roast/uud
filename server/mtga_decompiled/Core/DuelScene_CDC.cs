using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Cards.Parts;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class DuelScene_CDC : BASE_CDC, IEntityView
{
	private const string ReactionRootName = "Reaction Root";

	private Transform _reactionRoot;

	private const string ClickGuardName = "Local Hand UI Click Guard";

	private Transform _clickGuard;

	protected View_CDC_References _cdcReferences;

	private readonly HashSet<PropertyType> _updatedProperties = new HashSet<PropertyType>();

	private Animation _effectAnimation;

	private Animation _reactionAnimation;

	private Dictionary<string, GameObject> _persistEffectPrefabPathToInstanceMap = new Dictionary<string, GameObject>();

	private HashSet<string> _persistentEffectAudioEvents = new HashSet<string>();

	private CardInput _cardInput;

	private readonly HashSet<string> _tmpWantedAbilityVisualPaths = new HashSet<string>();

	private AltAssetReference<AnimationClip> _abilityPersistAnimReference;

	private AssetTracker _assetTracker = new AssetTracker();

	public Transform ArrowRoot => base.PartsRoot;

	public uint PreviousGrpId { get; private set; }

	public ICardDataAdapter RevealOverride { get; private set; }

	public ICardDataAdapter SimplifiedOverride { get; private set; }

	public override ICardDataAdapter VisualModel
	{
		get
		{
			if (RevealOverride != null)
			{
				return RevealOverride;
			}
			if (SimplifiedOverride != null)
			{
				return SimplifiedOverride;
			}
			return base.Model;
		}
	}

	public View_CDC_References CDCReferences
	{
		get
		{
			if (!_cdcReferences)
			{
				_cdcReferences = GetComponent<View_CDC_References>();
			}
			return _cdcReferences;
		}
	}

	public ICardHolder PreviousCardHolder { get; set; }

	public ICardHolder CurrentCardHolder { get; set; }

	public override CardHolderType HolderType
	{
		get
		{
			if (base.HolderTypeOverride.HasValue)
			{
				return base.HolderTypeOverride.Value;
			}
			if (CurrentCardHolder != null && CurrentCardHolder.CardHolderType != CardHolderType.None)
			{
				return CurrentCardHolder.CardHolderType;
			}
			return _holderType;
		}
	}

	public bool IsBeingDestroyed { get; set; }

	public HashSet<PropertyType> UpdatedProperties => _updatedProperties;

	protected Animation EffectAnimation
	{
		get
		{
			if (!_effectAnimation)
			{
				_effectAnimation = base.PartsRoot.GetComponent<Animation>();
				if (!_effectAnimation)
				{
					_effectAnimation = base.PartsRoot.gameObject.AddComponent<Animation>();
					_effectAnimation.enabled = false;
					_effectAnimation.playAutomatically = true;
				}
			}
			return _effectAnimation;
		}
	}

	public Animation ReactionAnimation
	{
		get
		{
			if (!_reactionAnimation)
			{
				_reactionAnimation = _reactionRoot.GetComponent<Animation>();
				if (!_reactionAnimation)
				{
					_reactionAnimation = _reactionRoot.gameObject.AddComponent<Animation>();
					_reactionAnimation.playAutomatically = false;
				}
			}
			return _reactionAnimation;
		}
	}

	public CardInput InputHandler => _cardInput;

	public void Init(ICardDataAdapter model, bool isVisible, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, CardDatabase cardDatabase, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, AssetLookupSystem assetLookupSystem, IClientLocProvider localizationManager, IBILogger biLogger, ResourceErrorMessageManager resourceErrorMessageManager, IVfxProvider vfxProvider, IEntityNameProvider<uint> nameProvider)
	{
		Init(model, isVisible, cardViewBuilder, cardMaterialBuilder, cardDatabase, unityObjectPool, genericObjectPool, assetLookupSystem, localizationManager, biLogger, resourceErrorMessageManager);
		_vfxProvider = vfxProvider ?? NullVfxProvider.Default;
		_entityNameProvider = nameProvider ?? NullIdNameProvider.Default;
		if (_reactionRoot == null)
		{
			_reactionRoot = new GameObject("Reaction Root").transform;
			_reactionRoot.SetParent(base.transform);
			_reactionRoot.ZeroOut();
		}
		if (_clickGuard == null)
		{
			_clickGuard = new GameObject("Local Hand UI Click Guard").transform;
			_clickGuard.SetParent(base.CollisionRoot);
			_clickGuard.ZeroOut();
			Bounds getColliderBounds = base.ActiveScaffold.GetColliderBounds;
			BoxCollider boxCollider = _clickGuard.gameObject.AddOrGetComponent<BoxCollider>();
			boxCollider.center = getColliderBounds.center + base.ActiveScaffold.ClickGuardOffset;
			boxCollider.size = Vector3.Scale(getColliderBounds.size, base.ActiveScaffold.ClickGuardScale);
			_clickGuard.gameObject.UpdateActive(active: false);
		}
		base.PartsRoot.SetParent(_reactionRoot);
		base.PartsRoot.ZeroOut();
		_cardInput = GetComponent<CardInput>();
	}

	internal override void OnDestroy()
	{
		_assetTracker.Cleanup();
		base.OnDestroy();
	}

	public override void UpdateVisibility(bool shouldBeVisible)
	{
		base.UpdateVisibility(shouldBeVisible);
		if (!shouldBeVisible)
		{
			ClearEffects(skipLoopingAnimations: true);
		}
	}

	public override void UpdateHighlight(HighlightType highlightType)
	{
		UpdateHighlight(highlightType, fireCallbacks: true);
	}

	public void UpdateHighlight(HighlightType highlightType, bool fireCallbacks)
	{
		if (CurrentHighlight() != highlightType)
		{
			base.UpdateHighlight(highlightType);
		}
	}

	public void ApplyRevealOverride(ICardDataAdapter revealOverride)
	{
		ClearEffects(skipLoopingAnimations: false);
		PreviousGrpId = revealOverride.GrpId;
		RevealOverride = revealOverride;
		base.ModelOverride = new ModelOverride(revealOverride);
		base.IsDirty = true;
	}

	public void RemoveRevealOverride()
	{
		RevealOverride = null;
		ClearOverrides();
		base.IsDirty = true;
	}

	public void ClearDeadVFX()
	{
		ClearEffects(skipLoopingAnimations: true);
	}

	protected override void ClearEffects(bool skipLoopingAnimations)
	{
		List<string> list = _genericObjectPool.PopObject<List<string>>();
		list.Clear();
		foreach (KeyValuePair<string, GameObject> item in _persistEffectPrefabPathToInstanceMap)
		{
			if (!skipLoopingAnimations || !LoopingAnimationManager.IsLoopingInstance(item.Value))
			{
				_unityObjectPool.PushObject(item.Value);
				list.Add(item.Key);
			}
		}
		foreach (string item2 in list)
		{
			_persistEffectPrefabPathToInstanceMap.Remove(item2);
		}
		list.Clear();
		_genericObjectPool.PushObject(list, tryClear: false);
		base.ClearEffects(skipLoopingAnimations);
	}

	public override void Teardown()
	{
		PreviousGrpId = 0u;
		RevealOverride = null;
		StopAllCoroutines();
		_updatedProperties.Clear();
		TrySetManaCostSortingOrder(0);
		UpdateTopCardRelevantVisuals(display: false);
		UpdateCounterVisibility(display: false);
		foreach (GameObject value in _persistEffectPrefabPathToInstanceMap.Values)
		{
			_unityObjectPool.PushObject(value);
		}
		_persistEffectPrefabPathToInstanceMap.Clear();
		_persistentEffectAudioEvents.Clear();
		PreviousCardHolder = new NoCardHolder();
		CurrentCardHolder = new NoCardHolder();
		if ((bool)_effectAnimation)
		{
			_effectAnimation.Stop();
			_effectAnimation.enabled = false;
			_effectAnimation.clip = null;
		}
		if ((bool)_cardInput)
		{
			SetInputEnabled(enabled: true);
			_cardInput.Teardown();
		}
		base.GetCurrentGameState = null;
		base.GetCurrentInteraction = null;
		base.Teardown();
	}

	public void SetInputEnabled(bool enabled)
	{
		if ((bool)_cardInput)
		{
			_cardInput.enabled = enabled;
		}
	}

	public void SetUIClickGuardEnabled(bool enabled)
	{
		_clickGuard.gameObject.UpdateActive(enabled);
	}

	public void UpdateTopCardRelevantVisuals(bool display)
	{
		display &= base.IsVisible;
		if (base.Model == null)
		{
			return;
		}
		CDCPart_Icons cDCPart_Icons = FindPart<CDCPart_Icons>(AnchorPointType.Icons);
		if ((bool)cDCPart_Icons)
		{
			cDCPart_Icons.SetVisible(display);
		}
		CDCPart_PTBox cDCPart_PTBox = FindPart<CDCPart_PTBox>(AnchorPointType.PowerToughness);
		if ((bool)cDCPart_PTBox)
		{
			cDCPart_PTBox.SetVisible(display || base.Model.ZoneType == ZoneType.Exile);
		}
		CDCPart_Loyalty cDCPart_Loyalty = FindPart<CDCPart_Loyalty>(AnchorPointType.Loyalty);
		if ((bool)cDCPart_Loyalty)
		{
			cDCPart_Loyalty.SetVisible(display || base.Model.ZoneType == ZoneType.Exile);
		}
		CDCPart_Defense cDCPart_Defense = FindPart<CDCPart_Defense>(AnchorPointType.Defense);
		if ((bool)cDCPart_Defense)
		{
			cDCPart_Defense.SetVisible(display || base.Model.ZoneType == ZoneType.Exile);
		}
		CDCPart_SummoningSickness cDCPart_SummoningSickness = FindPart<CDCPart_SummoningSickness>(AnchorPointType.SummoningSickness);
		if ((bool)cDCPart_SummoningSickness)
		{
			cDCPart_SummoningSickness.SetVisible(display);
		}
		CDCPart cDCPart = FindPart<CDCPart>(AnchorPointType.OrnamentalPart);
		if ((bool)cDCPart)
		{
			cDCPart.gameObject.UpdateActive(display);
		}
		foreach (CDCPart_LinkedFace item in FindLinkedFaceParts())
		{
			if ((bool)item && item.LinkedFaceCDC is DuelScene_CDC duelScene_CDC)
			{
				duelScene_CDC.UpdateTopCardRelevantVisuals(display);
			}
		}
	}

	public bool TrySetManaCostSortingOrder(int sortingOrder)
	{
		if ((ActivePartsMap.TryGetValue(AnchorPointType.TitleContent, out var value) && (bool)value) || (ActivePartsMap.TryGetValue(AnchorPointType.TitleBar, out var value2) && (bool)value2 && value2.DynamicPartsMap.TryGetValue(AnchorPointType.TitleContent, out value) && (bool)value))
		{
			CDCManaCostFiller cDCManaCostFiller = (CDCManaCostFiller)value.ManagedFillers.FirstOrDefault((CDCFillerBase filler) => filler is CDCManaCostFiller);
			if ((bool)cDCManaCostFiller)
			{
				cDCManaCostFiller.SetRendererOrder(sortingOrder);
				return true;
			}
		}
		return false;
	}

	public void HandleAddedAbility(uint abilityId)
	{
		if (base.Model == null || base.Model.Zone == null || (base.Model.Zone.Type != ZoneType.Battlefield && base.Model.Zone.Type != ZoneType.Command) || !base.IsVisible || IsBeingDestroyed)
		{
			return;
		}
		AbilityPrintingData abilityPrintingById = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(abilityId);
		if (abilityPrintingById == null)
		{
			return;
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(base.Model);
		_assetLookupSystem.Blackboard.CardHolderType = CurrentCardHolder.CardHolderType;
		_assetLookupSystem.Blackboard.Ability = abilityPrintingById;
		_assetLookupSystem.Blackboard.UpdatedProperties = _updatedProperties;
		AssetLookupTree<GainVfx> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<GainVfx>();
		AssetLookupTree<GainSfx> assetLookupTree2 = _assetLookupSystem.TreeLoader.LoadTree<GainSfx>();
		GainVfx payload = assetLookupTree.GetPayload(_assetLookupSystem.Blackboard);
		GainSfx payload2 = assetLookupTree2.GetPayload(_assetLookupSystem.Blackboard);
		if (payload2 != null)
		{
			AudioManager.PlayAudio(payload2.SfxData.AudioEvents, base.gameObject);
		}
		if (payload != null)
		{
			foreach (VfxData vfxData in payload.VfxDatas)
			{
				_vfxProvider.PlayVFX(vfxData, base.Model, base.Model.Instance, base.EffectsRoot);
			}
		}
		if (ActivePartsMap.TryGetValue(AnchorPointType.Icons, out var value) && value is CDCPart_Icons_Battlefield cDCPart_Icons_Battlefield)
		{
			cDCPart_Icons_Battlefield.QueueBadgePlump(abilityPrintingById.Id);
			base.IsDirty = true;
		}
	}

	public float UpdateAbilityVisuals<T, U>(bool display, bool animate, float? animationAlignerIn) where T : PersistVFX where U : PersistSfx
	{
		float result = 0f;
		if ((!base.TargetVisibility || IsBeingDestroyed || (base.Model.Zone != null && base.Model.Zone.Type != ZoneType.Battlefield)) && (!base.Model.Instance.IsTemporary || base.Model.Zone.Type != ZoneType.Hand))
		{
			display = false;
			animate = false;
		}
		_tmpWantedAbilityVisualPaths.Clear();
		if (display)
		{
			PlayPersistVFX<T>();
		}
		else
		{
			ClearPersistVFXPrefabMap();
		}
		_tmpWantedAbilityVisualPaths.Clear();
		if (display)
		{
			PlayPersistSFX<U>();
		}
		_persistentEffectAudioEvents.IntersectWith(_tmpWantedAbilityVisualPaths);
		bool flag = true;
		if (animate)
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(base.Model);
			AnimationClip animationClip = null;
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<AbilityPersistAnimation> loadedTree))
			{
				AbilityPersistAnimation payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					if (_abilityPersistAnimReference != null && payload.ClipRef.RelativePath != _abilityPersistAnimReference.RelativePath && (bool)EffectAnimation && EffectAnimation.clip != null)
					{
						animationClip = EffectAnimation.clip;
					}
					else
					{
						_abilityPersistAnimReference = payload.ClipRef;
						animationClip = AssetLoader.AcquireAndTrackAsset(_assetTracker, "abilityPersistAnim", _abilityPersistAnimReference);
					}
				}
			}
			if (animationClip != null)
			{
				flag = false;
				if (!EffectAnimation.isPlaying || EffectAnimation.clip != animationClip)
				{
					EffectAnimation.AddClip(animationClip, animationClip.name);
					EffectAnimation.clip = animationClip;
					EffectAnimation.Play(animationClip.name);
				}
				EffectAnimation.enabled = true;
				if (animationAlignerIn.HasValue)
				{
					EffectAnimation[animationClip.name].normalizedTime = animationAlignerIn.Value;
				}
				else
				{
					result = EffectAnimation[animationClip.name].normalizedTime;
				}
			}
		}
		if (flag)
		{
			_assetTracker.Cleanup();
			EffectAnimation.Stop();
			EffectAnimation.clip = null;
			EffectAnimation.enabled = false;
		}
		if (!EffectAnimation.isPlaying)
		{
			base.PartsRoot.transform.localPosition = Vector3.zero;
			base.PartsRoot.transform.localEulerAngles = (base.Model.IsDisplayedFaceDown ? new Vector3(0f, 180f, 0f) : Vector3.zero);
		}
		_tmpWantedAbilityVisualPaths.Clear();
		return result;
	}

	public void PlayPersistVFX<T>(bool clearPersistVFXPrefabMap = true) where T : PersistVFX
	{
		AssetLookupTree<T> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<T>();
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(base.Model);
		_assetLookupSystem.Blackboard.CardHolderType = HolderType;
		HashSet<T> hashSet = _genericObjectPool.PopObject<HashSet<T>>();
		if (assetLookupTree.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet))
		{
			foreach (T vfxPayload in hashSet)
			{
				foreach (VfxData vfxData in vfxPayload.VfxDatas)
				{
					foreach (AltAssetReference<GameObject> allPrefab in vfxData.PrefabData.AllPrefabs)
					{
						string relativePath = allPrefab.RelativePath;
						if (_tmpWantedAbilityVisualPaths.Contains(relativePath))
						{
							continue;
						}
						_tmpWantedAbilityVisualPaths.Add(relativePath);
						GameObject value = null;
						if (!_persistEffectPrefabPathToInstanceMap.TryGetValue(relativePath, out value) || value == null)
						{
							value = (_persistEffectPrefabPathToInstanceMap[relativePath] = _unityObjectPool.PopObject(relativePath));
						}
						Transform transform = value.transform;
						base.EffectsRoot.gameObject.UpdateActive(active: true);
						value.UpdateActive(active: true);
						transform.SetParent(base.EffectsRoot);
						transform.ZeroOut();
						if (vfxPayload.AnchorPointType != AnchorPointType.Invalid)
						{
							CDCSmartAnchor cDCSmartAnchor = base.ActiveScaffold.AllAnchorPoints.Find((CDCSmartAnchor x) => x.AnchorType == vfxPayload.AnchorPointType);
							if (cDCSmartAnchor != null)
							{
								Transform transform2 = cDCSmartAnchor.transform;
								transform.localPosition = transform2.localPosition;
								transform.localRotation = transform2.localRotation;
								transform.localScale = transform2.localScale;
							}
						}
						OffsetData offset = vfxData.Offset;
						transform.localPosition += offset.PositionOffset;
						transform.localEulerAngles += offset.RotationOffset;
						transform.localScale = offset.ScaleMultiplier;
						if (vfxData.SpaceData.ReverseIfOpponent && !base.Model.Controller.IsLocalPlayer)
						{
							transform.localEulerAngles += new Vector3(0f, 0f, 180f);
						}
					}
				}
			}
			hashSet.Clear();
			_genericObjectPool.PushObject(hashSet, tryClear: false);
		}
		if (clearPersistVFXPrefabMap)
		{
			ClearPersistVFXPrefabMap();
		}
	}

	public void ClearPersistVFXPrefabMap()
	{
		if (_persistEffectPrefabPathToInstanceMap.Count <= 0)
		{
			return;
		}
		List<string> list = _genericObjectPool.PopObject<List<string>>();
		list.Clear();
		list.AddRange(_persistEffectPrefabPathToInstanceMap.Keys);
		foreach (string item in list)
		{
			if (!_tmpWantedAbilityVisualPaths.Contains(item))
			{
				if (_persistEffectPrefabPathToInstanceMap.TryGetValue(item, out var value))
				{
					_unityObjectPool.PushObject(value);
				}
				_persistEffectPrefabPathToInstanceMap.Remove(item);
			}
		}
		list.Clear();
		_genericObjectPool.PushObject(list, tryClear: false);
		_tmpWantedAbilityVisualPaths.Clear();
	}

	public void PlayPersistSFX<U>() where U : PersistSfx
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(base.Model);
		_assetLookupSystem.Blackboard.CardHolderType = CurrentCardHolder.CardHolderType;
		AssetLookupTree<U> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<U>();
		HashSet<U> hashSet = _genericObjectPool.PopObject<HashSet<U>>();
		if (assetLookupTree.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet))
		{
			foreach (U item in hashSet)
			{
				foreach (AudioEvent audioEvent in item.SfxData.AudioEvents)
				{
					if (!_tmpWantedAbilityVisualPaths.Contains(audioEvent.WwiseEventName))
					{
						_tmpWantedAbilityVisualPaths.Add(audioEvent.WwiseEventName);
						if (!_persistentEffectAudioEvents.Contains(audioEvent.WwiseEventName))
						{
							_persistentEffectAudioEvents.Add(audioEvent.WwiseEventName);
							AudioManager.PlayAudio(audioEvent, base.EffectsRoot.gameObject);
						}
					}
				}
			}
		}
		hashSet.Clear();
		_genericObjectPool.PushObject(hashSet, tryClear: false);
	}

	public void PlayReactionAnimation(CardReactionEnum reactionType)
	{
		if (reactionType == CardReactionEnum.None || !base.IsVisible || !_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ReactionAnimation> loadedTree))
		{
			return;
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(base.Model);
		_assetLookupSystem.Blackboard.CardReactionType = reactionType;
		_assetLookupSystem.Blackboard.CardHolder = CurrentCardHolder;
		_assetLookupSystem.Blackboard.CardHolderType = CurrentCardHolder.CardHolderType;
		ReactionAnimation payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
		if (payload == null)
		{
			return;
		}
		AnimationClip objectData = AssetLoader.GetObjectData(payload.ClipRef);
		if (!objectData)
		{
			return;
		}
		if (!ReactionAnimation.GetClip(objectData.name))
		{
			ReactionAnimation.AddClip(objectData, objectData.name);
		}
		ReactionAnimation.clip = objectData;
		if (payload.UseRandomSpeed)
		{
			ReactionAnimation[objectData.name].speed = Random.Range(payload.MinSpeed, payload.MaxSpeed);
		}
		else
		{
			ReactionAnimation[objectData.name].speed = payload.Speed;
		}
		if (payload.UseRandomStartTime)
		{
			ReactionAnimation[objectData.name].normalizedTime = Random.Range(payload.MinStartTime, payload.MaxStartTime);
		}
		if (payload.Weight < 1f)
		{
			ReactionAnimation[objectData.name].weight = payload.Weight;
			ReactionAnimation[objectData.name].layer = 1;
			ReactionAnimation[objectData.name].enabled = true;
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.CardReactionType = CardReactionEnum.Idle;
			ReactionAnimation payload2 = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				AnimationClip objectData2 = AssetLoader.GetObjectData(payload2.ClipRef);
				if ((bool)objectData2)
				{
					if (!ReactionAnimation.GetClip(objectData2.name))
					{
						ReactionAnimation.AddClip(objectData2, objectData2.name);
					}
					ReactionAnimation.clip = objectData2;
					ReactionAnimation[objectData2.name].layer = 0;
					ReactionAnimation[objectData2.name].speed = objectData2.length / objectData.length;
					ReactionAnimation[objectData2.name].enabled = true;
				}
			}
		}
		ReactionAnimation.Play();
	}

	public void UpdateCombatIcons(CombatStateData combatState)
	{
		CDCPart_Icons_Battlefield cDCPart_Icons_Battlefield = FindPart<CDCPart_Icons_Battlefield>(AnchorPointType.Icons);
		if ((bool)cDCPart_Icons_Battlefield)
		{
			cDCPart_Icons_Battlefield.SetCombatIcons(combatState);
		}
	}

	public void ClearCombatIcons()
	{
		CDCPart_Icons_Battlefield cDCPart_Icons_Battlefield = FindPart<CDCPart_Icons_Battlefield>(AnchorPointType.Icons);
		if ((bool)cDCPart_Icons_Battlefield)
		{
			cDCPart_Icons_Battlefield.CleanupCombatIcons();
		}
	}

	public void UpdateCombatWarningIcon(bool enabled)
	{
		CDCPart_Icons_Battlefield cDCPart_Icons_Battlefield = FindPart<CDCPart_Icons_Battlefield>(AnchorPointType.Icons);
		if ((bool)cDCPart_Icons_Battlefield)
		{
			cDCPart_Icons_Battlefield.UpdateCombatWarningIcon(enabled);
		}
	}

	public void SetOpponentHoverState(bool isMousedOver)
	{
		if (base.IsVisible)
		{
			CDCPart_Highlights cDCPart_Highlights = FindPart<CDCPart_Highlights>(AnchorPointType.Highlights);
			if ((bool)cDCPart_Highlights)
			{
				cDCPart_Highlights.OpponentHighlight = isMousedOver;
			}
		}
	}

	public void AddUpdatedProperty(PropertyType property)
	{
		_updatedProperties.Add(property);
	}

	public override void SetModel(ICardDataAdapter model, bool updateVisuals = true, CardHolderType cardHolderType = CardHolderType.None)
	{
		if (model != null && model.GrpId != 0)
		{
			PreviousGrpId = model.GrpId;
		}
		UpdateStateChangeVFX(base.Model, model);
		base.SetModel(model, updateVisuals, cardHolderType);
		if (updateVisuals && CurrentCardHolder != null && CurrentCardHolder.CardHolderType == CardHolderType.Battlefield)
		{
			CurrentCardHolder.LayoutNow();
		}
	}

	public override void PreCardUpdated()
	{
		base.PreCardUpdated();
		CheckSimplifyOverride(base.Model);
		if (CurrentCardHolder != null)
		{
			CurrentCardHolder.OnCardUpdated(this);
		}
	}

	public override void PostCardUpdated()
	{
		base.PostCardUpdated();
		PerformPropertyUpdateFX();
		_updatedProperties.Clear();
		SetDimmedState(base.IsDimmed);
	}

	private void PerformPropertyUpdateFX()
	{
		if (_updatedProperties.Count == 0 || !_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PropertyUpdateVFX> loadedTree) || !_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<PropertyUpdateSFX> loadedTree2))
		{
			return;
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(base.Model);
		_assetLookupSystem.Blackboard.CardHolder = CurrentCardHolder;
		_assetLookupSystem.Blackboard.CardHolderType = CurrentCardHolder?.CardHolderType ?? CardHolderType.None;
		_assetLookupSystem.Blackboard.UpdatedProperties = _updatedProperties;
		HashSet<PropertyUpdateVFX> hashSet = _genericObjectPool.PopObject<HashSet<PropertyUpdateVFX>>();
		if (loadedTree.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet))
		{
			foreach (PropertyUpdateVFX item in hashSet)
			{
				foreach (VfxData vfxData in item.VfxDatas)
				{
					_vfxProvider.PlayAnchoredVFX(vfxData, item.AnchorPointType, base.ActiveScaffold, base.Model, base.Model.Instance, base.EffectsRoot);
				}
			}
		}
		hashSet.Clear();
		_genericObjectPool.PushObject(hashSet, tryClear: false);
		HashSet<PropertyUpdateSFX> hashSet2 = _genericObjectPool.PopObject<HashSet<PropertyUpdateSFX>>();
		if (loadedTree2.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet2))
		{
			foreach (PropertyUpdateSFX item2 in hashSet2)
			{
				AudioManager.PlayAudio(item2.SfxData.AudioEvents, base.EffectsRoot.gameObject);
			}
		}
		hashSet2.Clear();
		_genericObjectPool.PushObject(hashSet2, tryClear: false);
		_assetLookupSystem.Blackboard.Clear();
	}

	private void UpdateStateChangeVFX(ICardDataAdapter oldModel, ICardDataAdapter newModel)
	{
		if (base.Root != null && CDCReferences.HealVFX != null && CurrentCardHolder != null && CurrentCardHolder.CardHolderType == CardHolderType.Battlefield && oldModel != null && oldModel.Damage != 0 && newModel != null && newModel.Damage == 0 && newModel.Zone != null && newModel.Zone.Type == ZoneType.Battlefield && (newModel.ObjectType == GameObjectType.Card || newModel.ObjectType == GameObjectType.Token) && newModel.CardTypes.Contains(CardType.Creature))
		{
			GameObject obj = _unityObjectPool.PopObject(CDCReferences.HealVFX.gameObject);
			obj.transform.parent = base.EffectsRoot;
			obj.transform.localPosition = CDCReferences.HealVFXPosOffset;
			obj.transform.localEulerAngles = CDCReferences.HealVFXRotationOffset;
			obj.transform.localScale = Vector3.one;
		}
	}

	private void CheckSimplifyOverride(ICardDataAdapter model)
	{
		SimplifiedOverride = null;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ForceSimplified> loadedTree))
		{
			IBlackboard blackboard = _assetLookupSystem.Blackboard;
			blackboard.Clear();
			blackboard.SetCardDataExtensive(model);
			ForceSimplified payload = loadedTree.GetPayload(blackboard);
			if (payload != null && payload.UseSimplifiedOverride)
			{
				SimplifiedOverride = CardSimplifier.Simplify(CardSimplifier.Context.ModelOverride, model, _cardDatabase.CardDataProvider, _cardDatabase.AbilityDataProvider, keepArtId: true);
			}
		}
	}
}
