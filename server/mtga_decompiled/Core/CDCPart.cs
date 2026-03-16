using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.CardParts.Utils;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

[ExecuteAlways]
[DisallowMultipleComponent]
public class CDCPart : MonoBehaviour
{
	[Serializable]
	public struct FillerLinkedFace
	{
		public bool UseLinkedFaceForFillers;

		public int FaceIndex;

		public bool IgnoreInstance;
	}

	protected ICardDataAdapter _cachedModel;

	protected CardHolderType _cachedCardHolderType = CardHolderType.None;

	protected Phase _cachedPhase;

	protected CDCViewMetadata _cachedViewMetadata;

	protected bool _cachedDestroyed;

	protected readonly HashSet<PropertyType> _cachedChangedProps = new HashSet<PropertyType>();

	private uint _pendingFieldFillerUpdate = uint.MaxValue;

	private HashSet<CDCFillerBase> _managedfillers = new HashSet<CDCFillerBase>();

	protected CDCMaterialFiller _materialFiller;

	private readonly Dictionary<int, List<CDCFillerBase>> _managedFillersByPriority = new Dictionary<int, List<CDCFillerBase>>();

	private HashSet<CDCFillerBase> _knownFillers = new HashSet<CDCFillerBase>();

	private readonly List<int> _knownFillerPriorities = new List<int>();

	private readonly List<CDCPart> _nestedCdcParts = new List<CDCPart>();

	public readonly Dictionary<string, CDCPart> _dynamicParts = new Dictionary<string, CDCPart>(15);

	public readonly Dictionary<AnchorPointType, CDCPart> _dynamicPartsMap = new Dictionary<AnchorPointType, CDCPart>(15, new AnchorPointTypeComparer());

	private bool _hasBeenInit;

	protected ICardDatabaseAdapter _cardDatabase = NullCardDatabaseAdapter.Default;

	protected CardViewBuilder _cardViewBuilder;

	protected IUnityObjectPool _unityObjectPool;

	protected CardMaterialBuilder _cardMaterialBuilder;

	protected IClientLocProvider _localizationManager;

	protected AssetLookupSystem _assetLookupSystem;

	protected IObjectPool _genericObjectPool;

	protected CardColorCaches _cardColorCaches;

	protected RectTransform _rectTransform;

	protected float _originalWidth;

	protected float _originalHeight;

	protected AssetTracker _assetTracker = new AssetTracker();

	[SerializeField]
	private FillerLinkedFace _fillerLinkedFace;

	public IReadOnlyCollection<CDCPart> NestedCdcParts => _nestedCdcParts;

	public IReadOnlyCollection<CDCFillerBase> ManagedFillers => _managedfillers;

	public IReadOnlyDictionary<AnchorPointType, CDCPart> DynamicPartsMap => _dynamicPartsMap;

	public Func<MtgGameState> GetCurrentGameState { protected get; set; }

	public Func<WorkflowBase> GetCurrentInteraction { protected get; set; }

	public IVfxProvider VfxProvider { protected get; set; }

	public IEntityNameProvider<uint> EntityNameProvider { protected get; set; }

	public event Action<ICardDataAdapter, CardHolderType> onPartUpdated;

	public void Init(CardViewBuilder cardViewBuilder, ICardDatabaseAdapter cardDatabase, IUnityObjectPool unityObjectPool, CardMaterialBuilder cardMaterialBuilder, AssetLookupSystem assetLookupSystem, IClientLocProvider localizationManager, IObjectPool genericObjectPool, CardColorCaches cardColorCaches)
	{
		_cardDatabase = cardDatabase ?? NullCardDatabaseAdapter.Default;
		_cardViewBuilder = cardViewBuilder;
		_unityObjectPool = unityObjectPool;
		_cardMaterialBuilder = cardMaterialBuilder;
		_assetLookupSystem = assetLookupSystem;
		_localizationManager = localizationManager;
		_genericObjectPool = genericObjectPool;
		_cardColorCaches = cardColorCaches;
		if (!_hasBeenInit)
		{
			_hasBeenInit = true;
			HashSet<int> hashSet = new HashSet<int>();
			_managedfillers = CardViewUtilities.GatherCDCFillers(base.gameObject);
			_materialFiller = base.gameObject.GetComponent<CDCMaterialFiller>();
			foreach (CDCFillerBase managedfiller in _managedfillers)
			{
				managedfiller.Init(_cardDatabase, _assetLookupSystem, _cardMaterialBuilder, _unityObjectPool, _cardColorCaches);
				_knownFillers.Add(managedfiller);
				hashSet.Add(managedfiller.Priority);
				if (!_managedFillersByPriority.TryGetValue(managedfiller.Priority, out var value))
				{
					value = (_managedFillersByPriority[managedfiller.Priority] = new List<CDCFillerBase>(3));
				}
				value.Add(managedfiller);
			}
			if (_materialFiller != null)
			{
				_materialFiller.Init(_cardMaterialBuilder, _cardDatabase);
			}
			int num = 0;
			CDCPart[] componentsInChildren = base.gameObject.GetComponentsInChildren<CDCPart>();
			foreach (CDCPart cDCPart in componentsInChildren)
			{
				if (cDCPart.gameObject != base.gameObject)
				{
					_nestedCdcParts.Add(cDCPart);
					cDCPart.GetCurrentGameState = GetCurrentGameState;
					cDCPart.GetCurrentInteraction = GetCurrentInteraction;
					cDCPart.VfxProvider = VfxProvider;
					cDCPart.EntityNameProvider = EntityNameProvider;
					cDCPart.Init(_cardViewBuilder, _cardDatabase, _unityObjectPool, _cardMaterialBuilder, _assetLookupSystem, _localizationManager, _genericObjectPool, _cardColorCaches);
				}
				else
				{
					num++;
				}
			}
			if (num > 1)
			{
				Debug.LogWarning($"GameObject named [{base.gameObject.name}] has more than one CdcPart (or derivative) attached!");
			}
			foreach (CDCPart nestedCdcPart in _nestedCdcParts)
			{
				_knownFillers.UnionWith(nestedCdcPart._knownFillers);
				hashSet.UnionWith(nestedCdcPart._knownFillerPriorities);
			}
			_knownFillerPriorities.AddRange(hashSet);
			_knownFillerPriorities.Sort((int lhs, int rhs) => rhs.CompareTo(lhs));
			OnInit();
		}
		else
		{
			foreach (CDCPart nestedCdcPart2 in _nestedCdcParts)
			{
				nestedCdcPart2.GetCurrentGameState = GetCurrentGameState;
				nestedCdcPart2.GetCurrentInteraction = GetCurrentInteraction;
				nestedCdcPart2.Init(_cardViewBuilder, _cardDatabase, _unityObjectPool, _cardMaterialBuilder, _assetLookupSystem, _localizationManager, _genericObjectPool, _cardColorCaches);
			}
			foreach (CDCFillerBase managedfiller2 in _managedfillers)
			{
				managedfiller2.Init(_cardDatabase, _assetLookupSystem, _cardMaterialBuilder, _unityObjectPool, _cardColorCaches);
			}
			if (_materialFiller != null)
			{
				_materialFiller.Init(_cardMaterialBuilder, _cardDatabase);
			}
		}
		_rectTransform = base.transform as RectTransform;
		if ((bool)_rectTransform)
		{
			Rect rect = _rectTransform.rect;
			_originalWidth = rect.width;
			_originalHeight = rect.height;
		}
	}

	private void LateUpdate()
	{
		UpdatePendingFields();
		OnLateUpdate();
	}

	protected virtual void OnLateUpdate()
	{
	}

	public void UpdatePendingFields(bool force = false)
	{
		if (force)
		{
			_pendingFieldFillerUpdate = 0u;
		}
		if (_pendingFieldFillerUpdate >= _knownFillerPriorities.Count)
		{
			return;
		}
		int key = _knownFillerPriorities[(int)_pendingFieldFillerUpdate];
		if (_managedFillersByPriority.TryGetValue(key, out var value))
		{
			foreach (CDCFillerBase item in value)
			{
				item.Init(_cardDatabase, _assetLookupSystem, _cardMaterialBuilder, _unityObjectPool, _cardColorCaches);
				item.UpdateField(_cachedModel, _cachedCardHolderType, _knownFillers, _cachedViewMetadata, GetCurrentGameState?.Invoke(), GetCurrentInteraction?.Invoke());
				item.SetDestroyed(_cachedDestroyed);
			}
		}
		_pendingFieldFillerUpdate++;
	}

	public void GenerateDissolveMaterial(ICardDataAdapter model, CardHolderType cardHolderType, string rampTexturePath, string noiseTexturePath, HashSet<MaterialReplacementData> dissolveMaterials)
	{
		if (_materialFiller != null)
		{
			_materialFiller.GenerateDissolveMaterial(model, cardHolderType, GetCurrentGameState, rampTexturePath, noiseTexturePath, dissolveMaterials);
		}
		foreach (CDCPart nestedCdcPart in _nestedCdcParts)
		{
			nestedCdcPart.GenerateDissolveMaterial(model, cardHolderType, rampTexturePath, noiseTexturePath, dissolveMaterials);
		}
		foreach (CDCPart value in _dynamicParts.Values)
		{
			value.GenerateDissolveMaterial(model, cardHolderType, rampTexturePath, noiseTexturePath, dissolveMaterials);
		}
	}

	public void CleanupDissolveMaterial()
	{
		if (_materialFiller != null)
		{
			_materialFiller.CleanupDissolveMaterial();
		}
		foreach (CDCPart nestedCdcPart in _nestedCdcParts)
		{
			nestedCdcPart.CleanupDissolveMaterial();
		}
		foreach (CDCPart value in _dynamicParts.Values)
		{
			value.CleanupDissolveMaterial();
		}
	}

	public void SetDestroyed(bool destroyed)
	{
		_cachedDestroyed = destroyed;
		foreach (CDCFillerBase managedfiller in _managedfillers)
		{
			managedfiller.SetDestroyed(destroyed);
		}
		foreach (CDCPart nestedCdcPart in _nestedCdcParts)
		{
			nestedCdcPart.SetDestroyed(destroyed);
		}
		foreach (CDCPart value in _dynamicParts.Values)
		{
			value.SetDestroyed(destroyed);
		}
		HandleDestructionInternal();
	}

	public void EnableRenderers(bool enabled)
	{
		if (_materialFiller != null)
		{
			_materialFiller.EnableRenderers(enabled);
		}
		foreach (CDCPart nestedCdcPart in _nestedCdcParts)
		{
			nestedCdcPart.EnableRenderers(enabled);
		}
		foreach (CDCPart value in _dynamicParts.Values)
		{
			value.EnableRenderers(enabled);
		}
		HandleEnableRenderersInternal(enabled);
	}

	public void UpdateFields(ICardDataAdapter model, CardHolderType cardHolderType, CDCViewMetadata viewMetadata, HashSet<PropertyType> changedProps)
	{
		_cachedModel = model;
		_cachedCardHolderType = cardHolderType;
		_cachedViewMetadata = viewMetadata;
		if (changedProps != null)
		{
			_cachedChangedProps.UnionWith(changedProps);
		}
		if (_materialFiller != null)
		{
			_materialFiller.UpdateMaterials(GetFillerUpdateModel(model, _fillerLinkedFace), cardHolderType, GetCurrentGameState, viewMetadata.IsDimmed, viewMetadata.IsMouseOver);
		}
		foreach (CDCPart nestedCdcPart in _nestedCdcParts)
		{
			nestedCdcPart.UpdateFields(GetFillerUpdateModel(model, nestedCdcPart._fillerLinkedFace), cardHolderType, viewMetadata, changedProps);
		}
		UpdateDynamicParts();
		HandleUpdateInternal();
		_cachedChangedProps.Clear();
		UpdateShadowSettingsFromTransformType(base.transform, viewMetadata.IsMeta);
		UpdatePendingFields(force: true);
		this.onPartUpdated?.Invoke(model, cardHolderType);
	}

	public virtual Rect GetRect()
	{
		RectTransform obj = base.transform as RectTransform;
		return RectTransformUtils.GetRelativeRect(obj, obj.localPosition);
	}

	private ICardDataAdapter GetFillerUpdateModel(ICardDataAdapter model, FillerLinkedFace linkedFaceData)
	{
		if (!linkedFaceData.UseLinkedFaceForFillers)
		{
			return model;
		}
		return model.GetLinkedFaceAtIndex(linkedFaceData.FaceIndex, linkedFaceData.IgnoreInstance, _cardDatabase.CardDataProvider);
	}

	private void UpdateDynamicParts()
	{
		_dynamicPartsMap.Clear();
		List<string> list = _genericObjectPool.PopObject<List<string>>();
		Dictionary<Transform, List<CDCSmartAnchor>> anchorsByParent = GetAnchorsByParent();
		foreach (Transform key in anchorsByParent.Keys)
		{
			List<CDCSmartAnchor> list2 = anchorsByParent[key];
			list2.Sort((CDCSmartAnchor x, CDCSmartAnchor y) => x.LayoutPriority.CompareTo(y.LayoutPriority));
			_cardViewBuilder.UpdateParts(_cachedModel, key, list2, list, _dynamicParts, _assetLookupSystem, _unityObjectPool, _genericObjectPool, _cardDatabase, _cardMaterialBuilder, _localizationManager, GetCurrentGameState, GetCurrentInteraction, VfxProvider, EntityNameProvider, _cachedViewMetadata, _cachedCardHolderType, _cachedChangedProps, _dynamicPartsMap);
		}
		if (_dynamicParts.Count > 0)
		{
			foreach (string item in (IEnumerable<string>)_dynamicParts.Keys.Except(list).ToList())
			{
				CDCPart cDCPart = _dynamicParts[item];
				_dynamicParts.Remove(item);
				if ((bool)cDCPart)
				{
					cDCPart.HandleCleanup();
					DestroyObj(cDCPart.gameObject);
				}
			}
		}
		list.Clear();
		_genericObjectPool.PushObject(list, tryClear: false);
		foreach (List<CDCSmartAnchor> value in anchorsByParent.Values)
		{
			value.Clear();
			_genericObjectPool.PushObject(value, tryClear: false);
		}
		anchorsByParent.Clear();
		_genericObjectPool.PushObject(anchorsByParent, tryClear: false);
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

	private Dictionary<Transform, List<CDCSmartAnchor>> GetAnchorsByParent()
	{
		Dictionary<Transform, List<CDCSmartAnchor>> dictionary = _genericObjectPool.PopObject<Dictionary<Transform, List<CDCSmartAnchor>>>();
		AddAnchors(base.transform, dictionary);
		return dictionary;
		void AddAnchors(Transform t, Dictionary<Transform, List<CDCSmartAnchor>> anchorDictionary)
		{
			for (int i = 0; i < t.childCount; i++)
			{
				Transform child = t.GetChild(i);
				if (!child.GetComponent<CDCPart>())
				{
					CDCSmartAnchor component = child.GetComponent<CDCSmartAnchor>();
					if ((bool)component)
					{
						List<CDCSmartAnchor> value = null;
						Transform parent = component.transform.parent;
						if (!anchorDictionary.TryGetValue(parent, out value))
						{
							value = _genericObjectPool.PopObject<List<CDCSmartAnchor>>();
							anchorDictionary.Add(parent, value);
						}
						value.Add(component);
					}
					AddAnchors(child, anchorDictionary);
				}
			}
		}
	}

	public void OnPhaseUpdate(Phase phase)
	{
		_cachedPhase = phase;
		HandlePhaseUpdateInternal();
		foreach (CDCPart nestedCdcPart in _nestedCdcParts)
		{
			nestedCdcPart.OnPhaseUpdate(phase);
		}
		foreach (CDCPart value in _dynamicParts.Values)
		{
			value.OnPhaseUpdate(phase);
		}
		UpdatePendingFields(force: true);
	}

	public static void UpdateShadowSettingsFromTransformType(Transform t, bool isMeta)
	{
		if (!isMeta)
		{
			return;
		}
		if (t.GetComponent<MeshFilter>() != null)
		{
			MeshRenderer component = t.GetComponent<MeshRenderer>();
			if ((bool)component)
			{
				component.receiveShadows = false;
			}
		}
		for (int i = 0; i < t.childCount; i++)
		{
			UpdateShadowSettingsFromTransformType(t.GetChild(i), isMeta);
		}
	}

	public virtual void HandleCleanup()
	{
		HandleCleanup_Internal(destroyed: false);
	}

	private void HandleCleanup_Internal(bool destroyed)
	{
		SetDestroyed(destroyed);
		EnableRenderers(!destroyed);
		if ((bool)_rectTransform && !destroyed)
		{
			_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _originalWidth);
			_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _originalHeight);
		}
		foreach (CDCFillerBase managedfiller in _managedfillers)
		{
			managedfiller.Cleanup();
		}
		if (_materialFiller != null)
		{
			_materialFiller.Cleanup();
		}
		foreach (CDCPart nestedCdcPart in _nestedCdcParts)
		{
			nestedCdcPart.HandleCleanup();
		}
		foreach (CDCPart value in _dynamicParts.Values)
		{
			value.HandleCleanup();
			DestroyObj(value.gameObject);
		}
		_assetTracker.Cleanup();
		_dynamicParts.Clear();
		_dynamicPartsMap.Clear();
		GetCurrentGameState = null;
		GetCurrentInteraction = null;
		VfxProvider = null;
		EntityNameProvider = NullIdNameProvider.Default;
	}

	protected virtual void OnDestroy()
	{
		HandleCleanup_Internal(destroyed: true);
	}

	protected virtual void OnInit()
	{
	}

	protected virtual void HandleDestructionInternal()
	{
	}

	protected virtual void HandleUpdateInternal()
	{
	}

	protected virtual void HandlePhaseUpdateInternal()
	{
	}

	protected virtual void HandleEnableRenderersInternal(bool enabled)
	{
	}
}
