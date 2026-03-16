using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Extractors.UI;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

[ExecuteInEditMode]
public class BASE_CDC : MonoBehaviour
{
	private const string CDC_NAME_FORMAT = "CDC #{0}";

	private const string PARTS_ROOT_NAME = "Parts Root";

	private const string EFFECTS_ROOT_NAME = "Effects Root";

	public readonly Dictionary<string, CDCPart> ActiveParts = new Dictionary<string, CDCPart>();

	public readonly Dictionary<AnchorPointType, CDCPart> ActivePartsMap = new Dictionary<AnchorPointType, CDCPart>(15, new AnchorPointTypeComparer());

	private TooltipTrigger _tooltipTrigger;

	private ICardDataAdapter _model;

	private ICardDataAdapter _overridenModel;

	private ModelOverride _modelOverride;

	private bool _isMousedOver;

	private bool _isExaminedCard;

	protected CardHolderType _holderType;

	private CardHolderType? _holderTypeOverride;

	private uint _updateDelayFrames;

	protected CardViewBuilder _cardViewBuilder;

	protected CardMaterialBuilder _cardMaterialBuilder;

	protected ICardDatabaseAdapter _cardDatabase = NullCardDatabaseAdapter.Default;

	protected IUnityObjectPool _unityObjectPool;

	protected IObjectPool _genericObjectPool;

	protected IBILogger _biLogger = NullBILogger.Default;

	protected ResourceErrorMessageManager _resourceErrorMessageManager;

	protected AssetCache<ScaffoldingBase> _scaffoldingCache;

	protected AssetLookupSystem _assetLookupSystem;

	protected IClientLocProvider _localizationManager;

	protected HashSet<InfoHanger> infoHangerPayloadsCache = new HashSet<InfoHanger>();

	protected CDCPart_Highlights _cachedHighlightPart;

	protected IVfxProvider _vfxProvider = NullVfxProvider.Default;

	protected IEntityNameProvider<uint> _entityNameProvider = NullIdNameProvider.Default;

	private bool _initialized;

	private HighlightType _mostRecentHighlight;

	private static readonly IEnumerable<AnchorPointType> _linkedFaceAnchors = new AnchorPointType[2]
	{
		AnchorPointType.LinkedFaceRoot_0,
		AnchorPointType.LinkedFaceRoot_1
	};

	public ScaffoldingBase ActiveScaffold { get; set; }

	public ICardDataAdapter Model
	{
		get
		{
			return _overridenModel ?? _model;
		}
		private set
		{
			if (_model != value)
			{
				_model = value;
				_overridenModel = _modelOverride?.GetOverriddenModel(_model);
			}
		}
	}

	public ModelOverride ModelOverride
	{
		get
		{
			return _modelOverride;
		}
		set
		{
			if ((_modelOverride == null && value != null) || (_modelOverride != null && value == null) || (_modelOverride != null && value != null && (_modelOverride.ObjectTypeOverride != value.ObjectTypeOverride || _modelOverride.ZoneTypeOverride != value.ZoneTypeOverride || _modelOverride.GrpIdOverride != value.GrpIdOverride || _modelOverride.IsFaceDownOverride != value.IsFaceDownOverride)))
			{
				_modelOverride = value;
				_overridenModel = _modelOverride?.GetOverriddenModel(_model);
				IsDirty = true;
			}
		}
	}

	public virtual ICardDataAdapter VisualModel => Model;

	public bool IsMousedOver
	{
		get
		{
			return _isMousedOver;
		}
		set
		{
			IsDirty |= _isMousedOver != value;
			_isMousedOver = value;
		}
	}

	public bool IsHoverCopy { get; set; }

	public bool IsExaminedCard
	{
		get
		{
			return _isExaminedCard;
		}
		set
		{
			IsDirty |= _isExaminedCard != value;
			_isExaminedCard = value;
		}
	}

	public uint InstanceId => Model?.InstanceId ?? 0;

	public Transform Root { get; protected set; }

	public Transform PartsRoot { get; protected set; }

	public Transform CollisionRoot { get; protected set; }

	public BoxCollider Collider { get; protected set; }

	public Transform EffectsRoot { get; protected set; }

	public IEnumerable<CDCPart> GetActiveParts => ActiveParts.Values;

	public bool IsDimmed { get; protected set; }

	public bool IsVisible
	{
		get
		{
			if (PartsRoot != null)
			{
				return PartsRoot.gameObject.activeSelf;
			}
			return false;
		}
	}

	public bool TargetVisibility { get; private set; }

	public bool IsDirty { get; set; }

	public virtual CardHolderType HolderType
	{
		get
		{
			return _holderTypeOverride ?? _holderType;
		}
		protected set
		{
			_holderType = value;
		}
	}

	public CardHolderType? HolderTypeOverride
	{
		get
		{
			return _holderTypeOverride;
		}
		set
		{
			if (_holderTypeOverride != value)
			{
				_holderTypeOverride = value;
				IsDirty = true;
			}
		}
	}

	public Func<MtgGameState> GetCurrentGameState { get; set; }

	public Func<WorkflowBase> GetCurrentInteraction { get; set; }

	public event System.Action OnPreCardUpdated;

	public event System.Action OnPostCardUpdated;

	protected void Init(ICardDataAdapter model, bool isVisible, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, ICardDatabaseAdapter cardDatabase, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, AssetLookupSystem assetLookupSystem, IClientLocProvider localizationManager, IBILogger biLogger, ResourceErrorMessageManager resourceErrorMessageManager)
	{
		_initialized = true;
		_cardViewBuilder = cardViewBuilder;
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_cardMaterialBuilder = cardMaterialBuilder;
		_unityObjectPool = unityObjectPool;
		_genericObjectPool = genericObjectPool;
		_assetLookupSystem = assetLookupSystem;
		_localizationManager = localizationManager;
		_biLogger = biLogger ?? NullBILogger.Default;
		_resourceErrorMessageManager = resourceErrorMessageManager;
		Model = model;
		Root = base.transform;
		if (PartsRoot == null)
		{
			PartsRoot = new GameObject("Parts Root").transform;
		}
		if (Collider == null)
		{
			Collider = GetComponentInChildren<BoxCollider>();
		}
		if (CollisionRoot == null)
		{
			CollisionRoot = Collider.transform;
		}
		PartsRoot.parent = base.transform;
		PartsRoot.ZeroOut();
		PartsRoot.gameObject.SetActive(isVisible);
		if (EffectsRoot == null)
		{
			EffectsRoot = new GameObject("Effects Root").transform;
		}
		EffectsRoot.parent = PartsRoot.transform;
		EffectsRoot.ZeroOut();
		Collider.enabled = true;
		CollisionRoot.ZeroOut();
		IsDimmed = false;
		foreach (object value in EnumHelper.GetValues(typeof(CDCAnchorType)))
		{
			ActivePartsMap[(AnchorPointType)value] = null;
		}
		TargetVisibility = isVisible;
		IsDirty = true;
	}

	public void ClearOverrides()
	{
		IsDirty |= _modelOverride != null;
		_modelOverride = null;
		IsDirty |= _overridenModel != null;
		_overridenModel = null;
		IsDirty |= _isMousedOver;
		_isMousedOver = false;
		IsDirty |= _holderTypeOverride.HasValue;
		_holderTypeOverride = null;
	}

	public virtual void Teardown()
	{
		this.OnPreCardUpdated = null;
		this.OnPostCardUpdated = null;
		UpdateVisibility(shouldBeVisible: true);
		RemoveTooltip();
		ClearOverrides();
		PartsRoot.ZeroOut();
		CollisionRoot.ZeroOut();
		CollisionRoot.gameObject.SetActive(value: true);
		_cachedHighlightPart = null;
		_mostRecentHighlight = HighlightType.None;
		ActiveScaffold = null;
		foreach (CDCPart value in ActiveParts.Values)
		{
			if ((bool)value)
			{
				value.HandleCleanup();
				if (_unityObjectPool != null)
				{
					_unityObjectPool.PushObject(value.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(value.gameObject);
				}
			}
		}
		ActiveParts.Clear();
		foreach (Transform item in PartsRoot)
		{
			if (!(item == EffectsRoot))
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		ClearEffects(skipLoopingAnimations: false);
		Model = null;
		HolderType = CardHolderType.Invalid;
		IsHoverCopy = false;
		_isMousedOver = false;
		_isExaminedCard = false;
	}

	public virtual void UpdateVisibility(bool shouldBeVisible)
	{
		if (shouldBeVisible)
		{
			PrepareLoopingEffects();
		}
		Collider.enabled = true;
		TargetVisibility = shouldBeVisible;
	}

	public virtual void UpdateVisuals()
	{
		IsDirty = true;
	}

	public IEnumerable<InfoHanger> GetInfoHangerPayloads()
	{
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<InfoHanger> loadedTree))
		{
			ICardDataAdapter model = Model;
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
			_assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Invalid;
			loadedTree.GetPayloadLayered(_assetLookupSystem.Blackboard, infoHangerPayloadsCache);
			return infoHangerPayloadsCache;
		}
		infoHangerPayloadsCache.Clear();
		return infoHangerPayloadsCache;
	}

	public void GetSleeveFXPayload(ICardDataAdapter cardData, CardHolderType cardHolderType, out SleeveFX sleeveFXPayload, out string prefabFilePath)
	{
		sleeveFXPayload = null;
		prefabFilePath = null;
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(cardData);
		_assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<SleeveFX> loadedTree))
		{
			sleeveFXPayload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (sleeveFXPayload != null)
			{
				prefabFilePath = sleeveFXPayload.PrefabRef.RelativePath;
			}
		}
		_assetLookupSystem.Blackboard.Clear();
	}

	public virtual void SetModel(ICardDataAdapter data, bool updateVisuals = true, CardHolderType cardHolderType = CardHolderType.None)
	{
		Model = data;
		HolderType = cardHolderType;
		if (updateVisuals)
		{
			IsDirty = true;
		}
	}

	public virtual void SetDimmedState(bool isDimmed)
	{
		if (isDimmed != IsDimmed)
		{
			IsDimmed = isDimmed;
			IsDirty = true;
		}
	}

	public void SetUpdateDelay(uint framesToDelay)
	{
		_updateDelayFrames = framesToDelay;
	}

	private void OnDisable()
	{
		if ((bool)EffectsRoot)
		{
			ClearEffects(skipLoopingAnimations: true);
		}
	}

	private void LateUpdate()
	{
		if (_updateDelayFrames != 0)
		{
			_updateDelayFrames--;
			return;
		}
		if (IsVisible != TargetVisibility || (TargetVisibility && (ActiveScaffold == null || ActiveParts.Count == 0)) || (!TargetVisibility && ActiveParts.Count > 0))
		{
			if (PartsRoot.gameObject.activeSelf != TargetVisibility)
			{
				PartsRoot.gameObject.SetActive(TargetVisibility);
			}
			if (!TargetVisibility)
			{
				ClearEffects(skipLoopingAnimations: true);
			}
			ImmediateUpdate();
		}
		if (IsDirty)
		{
			ImmediateUpdate();
			IsDirty = false;
		}
	}

	public void ImmediateUpdate()
	{
		PreCardUpdated();
		InternalUpdate(VisualModel);
		PostCardUpdated();
		_cardViewBuilder.SendCardUpdatedEvent(this);
		IsDirty = false;
	}

	private void InternalUpdate(ICardDataAdapter model)
	{
		if (model != null)
		{
			CardHolderType holderType = HolderType;
			if (model.Zone == null || model.Zone.Type != ZoneType.Limbo || holderType != CardHolderType.None || (model.ObjectType != GameObjectType.Card && model.ObjectType != GameObjectType.Token))
			{
				base.gameObject.name = $"CDC #{model.InstanceId}";
				UpdateScaffold(model, holderType);
				UpdateParts(model, holderType);
				_assetLookupSystem.Blackboard.Clear();
				PartsRoot.transform.localEulerAngles = new Vector3(0f, model.IsDisplayedFaceDown ? 180 : 0, 0f);
			}
		}
	}

	private void UpdateScaffold(ICardDataAdapter model, CardHolderType cardHolderType)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.GameState = GetCurrentGameState?.Invoke();
		_assetLookupSystem.Blackboard.Interaction = GetCurrentInteraction?.Invoke();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		_assetLookupSystem.Blackboard.SetCdcViewMetadata(new CDCViewMetadata(this));
		_assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		if (model.ObjectType == GameObjectType.Ability)
		{
			_assetLookupSystem.Blackboard.Ability = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(model.GrpId);
		}
		string scaffolding = string.Empty;
		AltAssetReference<ScaffoldingBase> altAssetReference = _assetLookupSystem.TreeLoader.LoadTree<Scaffold>(returnNewTree: false)?.GetPayload(_assetLookupSystem.Blackboard)?.ScaffoldRef;
		if (altAssetReference != null)
		{
			scaffolding = altAssetReference.RelativePath;
		}
		_assetLookupSystem.Blackboard.Clear();
		SetScaffolding(scaffolding);
		if (!ActiveScaffold)
		{
			CDCCreationError cDCCreationError = new CDCCreationError
			{
				Message = "Error finding scaffold for card",
				Title = _cardDatabase.GreLocProvider.GetLocalizedText(_assetLookupSystem.Blackboard.CardData.TitleId),
				GrpId = _assetLookupSystem.Blackboard.CardData.GrpId.ToString(),
				CardHolder = _assetLookupSystem.Blackboard.CardHolderType.ToString(),
				EventTime = DateTime.UtcNow
			};
			_biLogger.Send(ClientBusinessEventType.CDCCreationError, cDCCreationError);
			_resourceErrorMessageManager.ShowError("CDC Creation", cDCCreationError.Message, ("Title", cDCCreationError.Title), ("GrpId", cDCCreationError.GrpId), ("Card Holder", cDCCreationError.CardHolder));
		}
		else
		{
			Bounds getColliderBounds = ActiveScaffold.GetColliderBounds;
			Collider.size = getColliderBounds.size;
			Collider.center = getColliderBounds.center;
		}
	}

	private void UpdateParts(ICardDataAdapter model, CardHolderType holderType)
	{
		List<string> list = _genericObjectPool.PopObject<List<string>>();
		foreach (object value in EnumHelper.GetValues(typeof(AnchorPointType)))
		{
			ActivePartsMap[(AnchorPointType)value] = null;
		}
		if (TargetVisibility)
		{
			List<CDCSmartAnchor> allAnchorPoints = ActiveScaffold.AllAnchorPoints;
			_cardViewBuilder.UpdateSleeveCode(model);
			DuelScene_CDC duelScene_CDC = this as DuelScene_CDC;
			HashSet<PropertyType> changedProps = ((duelScene_CDC == null) ? null : duelScene_CDC.UpdatedProperties);
			_cardViewBuilder.UpdateParts(model, PartsRoot, allAnchorPoints, list, ActiveParts, _assetLookupSystem, _unityObjectPool, _genericObjectPool, _cardDatabase, _cardMaterialBuilder, _localizationManager, GetCurrentGameState, GetCurrentInteraction, _vfxProvider, _entityNameProvider, new CDCViewMetadata(this is Meta_CDC, IsDimmed, _isMousedOver, IsHoverCopy, _isExaminedCard), holderType, changedProps, ActivePartsMap);
		}
		if (ActiveParts.Count > 0)
		{
			List<string> list2 = _genericObjectPool.PopObject<List<string>>() ?? new List<string>(10);
			list2.Clear();
			list2.AddRange(ActiveParts.Keys);
			foreach (string item in list2)
			{
				if (!list.Contains(item))
				{
					CDCPart cDCPart = ActiveParts[item];
					ActiveParts.Remove(item);
					if ((bool)cDCPart)
					{
						cDCPart.HandleCleanup();
						DestroyObj(cDCPart.gameObject);
					}
				}
			}
			list2.Clear();
			_genericObjectPool.PushObject(list2, tryClear: false);
		}
		list.Clear();
		_genericObjectPool.PushObject(list, tryClear: false);
		_cachedHighlightPart = FindPart<CDCPart_Highlights>(AnchorPointType.Highlights);
	}

	public HighlightType CurrentHighlight()
	{
		if (!_cachedHighlightPart)
		{
			return _mostRecentHighlight;
		}
		return _cachedHighlightPart.CurrentHighlight;
	}

	public virtual void UpdateHighlight(HighlightType highlightType)
	{
		_mostRecentHighlight = highlightType;
		if ((bool)_cachedHighlightPart)
		{
			_cachedHighlightPart.CurrentHighlight = highlightType;
		}
	}

	private void DestroyObj(GameObject obj)
	{
		if (Application.isPlaying)
		{
			if (!obj.activeSelf)
			{
				obj.SetActive(value: true);
			}
			_unityObjectPool?.PushObject(obj);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	internal virtual void OnDestroy()
	{
		this.OnPreCardUpdated = null;
		this.OnPostCardUpdated = null;
	}

	public virtual void PreCardUpdated()
	{
		this.OnPreCardUpdated?.Invoke();
	}

	public virtual void PostCardUpdated()
	{
		this.OnPostCardUpdated?.Invoke();
		UpdateHighlight(_mostRecentHighlight);
	}

	protected virtual void ClearEffects(bool skipLoopingAnimations)
	{
		if (!skipLoopingAnimations)
		{
			LoopingAnimationManager.RemoveAllLoopingEffects(EffectsRoot);
		}
		for (int num = EffectsRoot.childCount - 1; num >= 0; num--)
		{
			Transform child = EffectsRoot.GetChild(num);
			if (!skipLoopingAnimations || !LoopingAnimationManager.IsLoopingInstance(child.gameObject))
			{
				if (_unityObjectPool != null)
				{
					_unityObjectPool.PushObject(child.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(child.gameObject);
				}
			}
		}
	}

	private void PrepareLoopingEffects()
	{
		foreach (Transform item in EffectsRoot)
		{
			if (LoopingAnimationManager.IsLoopingInstance(item.gameObject))
			{
				DelayVFX component = item.GetComponent<DelayVFX>();
				if (component != null)
				{
					component.SkipNextDelay();
				}
			}
		}
	}

	public void SetTooltipData(TooltipData tooltipData)
	{
		if (_tooltipTrigger == null)
		{
			if (PlatformUtils.IsHandheld())
			{
				_tooltipTrigger = base.gameObject.AddComponent<TouchTooltipTrigger>();
			}
			else
			{
				_tooltipTrigger = base.gameObject.AddComponent<TooltipTrigger>();
			}
		}
		_tooltipTrigger.tooltipContext = TooltipContext.MiniCDC;
		_tooltipTrigger.TooltipData = tooltipData;
	}

	public void RemoveTooltip()
	{
		if (_tooltipTrigger != null)
		{
			UnityEngine.Object.Destroy(_tooltipTrigger);
			_tooltipTrigger = null;
		}
	}

	public void InitializeScaffolding(string scaffoldPath, AssetCache<ScaffoldingBase> cache)
	{
		_scaffoldingCache = cache;
		SetScaffolding(scaffoldPath);
	}

	public void SetScaffolding(string newScaffoldPath)
	{
		ActiveScaffold = _scaffoldingCache.Get(newScaffoldPath);
	}

	public void UpdateCounterVisibility(bool display)
	{
		display &= IsVisible;
		CDCPart_Counters cDCPart_Counters = FindPart<CDCPart_Counters>(AnchorPointType.Counters);
		if ((bool)cDCPart_Counters)
		{
			cDCPart_Counters.SetVisible(display);
		}
	}

	public T FindPart<T>(AnchorPointType anchorType) where T : CDCPart
	{
		if (anchorType != AnchorPointType.Invalid)
		{
			if (ActivePartsMap.TryGetValue(anchorType, out var value))
			{
				T val = FindPartNested(value);
				if (val != null)
				{
					return val;
				}
			}
			return null;
		}
		foreach (CDCPart value2 in ActiveParts.Values)
		{
			T val2 = FindPartNested(value2);
			if (val2 != null)
			{
				return val2;
			}
		}
		return null;
		T FindPartNested(CDCPart searchPart)
		{
			if (!searchPart)
			{
				return null;
			}
			if (searchPart is T result)
			{
				return result;
			}
			foreach (CDCPart nestedCdcPart in searchPart.NestedCdcParts)
			{
				T val3 = FindPartNested(nestedCdcPart);
				if (val3 != null)
				{
					return val3;
				}
			}
			if (searchPart is CDCPart_LinkedFace cDCPart_LinkedFace && (bool)cDCPart_LinkedFace.LinkedFaceCDC)
			{
				T val4 = cDCPart_LinkedFace.LinkedFaceCDC.FindPart<T>(anchorType);
				if (val4 != null)
				{
					return val4;
				}
			}
			return null;
		}
	}

	public IEnumerable<T> FindParts<T>(IEnumerable<AnchorPointType> anchorPoints) where T : CDCPart
	{
		foreach (AnchorPointType anchorPoint in anchorPoints)
		{
			T val = FindPart<T>(anchorPoint);
			if ((object)val != null)
			{
				yield return val;
			}
		}
	}

	public IEnumerable<CDCPart_LinkedFace> FindLinkedFaceParts()
	{
		return FindParts<CDCPart_LinkedFace>(_linkedFaceAnchors);
	}

	public IEnumerable<T> FindAllParts<T>(AnchorPointType anchorType) where T : CDCPart
	{
		if (anchorType != AnchorPointType.Invalid)
		{
			if (!ActivePartsMap.TryGetValue(anchorType, out var value))
			{
				yield break;
			}
			foreach (T item in FindPartsNested(value))
			{
				yield return item;
			}
			yield break;
		}
		foreach (CDCPart value2 in ActiveParts.Values)
		{
			foreach (T item2 in FindPartsNested(value2))
			{
				yield return item2;
			}
		}
		IEnumerable<T> FindPartsNested(CDCPart searchPart)
		{
			if ((bool)searchPart)
			{
				if (searchPart is T val)
				{
					yield return val;
				}
				foreach (CDCPart nestedCdcPart in searchPart.NestedCdcParts)
				{
					foreach (T item3 in FindPartsNested(nestedCdcPart))
					{
						yield return item3;
					}
				}
				if (searchPart is CDCPart_LinkedFace cDCPart_LinkedFace && (bool)cDCPart_LinkedFace.LinkedFaceCDC)
				{
					foreach (T item4 in cDCPart_LinkedFace.LinkedFaceCDC.FindAllParts<T>(anchorType))
					{
						yield return item4;
					}
				}
			}
		}
	}

	public T FindFiller<T, U>(U fillerType, AnchorPointType anchorType = AnchorPointType.Invalid) where T : CDCFillerBase where U : Enum
	{
		if (anchorType != AnchorPointType.Invalid)
		{
			if (ActivePartsMap.TryGetValue(anchorType, out var value))
			{
				return FindFillerNested(value, fillerType);
			}
			return null;
		}
		foreach (CDCPart value2 in ActiveParts.Values)
		{
			T val = FindFillerNested(value2, fillerType);
			if ((bool)val)
			{
				return val;
			}
		}
		return null;
		static T FindFillerNested(CDCPart searchPart, U searchFillerType)
		{
			if (!searchPart)
			{
				return null;
			}
			int hashCode = searchFillerType.GetHashCode();
			foreach (CDCFillerBase managedFiller in searchPart.ManagedFillers)
			{
				if (managedFiller is T result && managedFiller.RawFieldType.Equals(hashCode))
				{
					return result;
				}
			}
			foreach (CDCPart nestedCdcPart in searchPart.NestedCdcParts)
			{
				T val2 = FindFillerNested(nestedCdcPart, searchFillerType);
				if ((bool)val2)
				{
					return val2;
				}
			}
			return null;
		}
	}

	public V FindPartWithFiller<T, U, V>(U fillerType, out T filler, AnchorPointType anchorType = AnchorPointType.Invalid) where T : CDCFillerBase where U : Enum where V : CDCPart
	{
		filler = null;
		if (anchorType != AnchorPointType.Invalid)
		{
			if (ActivePartsMap.TryGetValue(anchorType, out var value))
			{
				return FindPartNested(value, fillerType, out filler);
			}
			return null;
		}
		foreach (CDCPart value2 in ActiveParts.Values)
		{
			V val = FindPartNested(value2, fillerType, out filler);
			if ((bool)val)
			{
				return val;
			}
		}
		return null;
		static V FindPartNested(CDCPart searchPart, U searchFillerType, out T foundFiller)
		{
			foundFiller = null;
			if (!searchPart)
			{
				return null;
			}
			if (searchPart is V val2)
			{
				int hashCode = searchFillerType.GetHashCode();
				foreach (CDCFillerBase managedFiller in val2.ManagedFillers)
				{
					if (managedFiller is T val3 && managedFiller.RawFieldType.Equals(hashCode))
					{
						foundFiller = val3;
						return val2;
					}
				}
			}
			foreach (CDCPart nestedCdcPart in searchPart.NestedCdcParts)
			{
				V val4 = FindPartNested(nestedCdcPart, searchFillerType, out foundFiller);
				if ((bool)val4)
				{
					return val4;
				}
			}
			foreach (CDCPart value3 in searchPart.DynamicPartsMap.Values)
			{
				V val5 = FindPartNested(value3, searchFillerType, out foundFiller);
				if ((bool)val5)
				{
					return val5;
				}
			}
			return null;
		}
	}
}
