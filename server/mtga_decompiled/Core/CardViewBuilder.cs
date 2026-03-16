using System;
using System.Collections.Generic;
using System.IO;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card.Parts;
using AssetLookupTree.Payloads.CardViewBuilder;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.CardParts;
using Wotc.Mtga.CardParts.Utils;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class CardViewBuilder : IDisposable
{
	private const string CDC_NAME_FORMAT = "CDC #{0}";

	private AltAssetReference<DuelScene_CDC> _duelSceneCDCPrefabRef;

	private AltAssetReference<Meta_CDC> _metaCDCPrefabRef;

	private AltAssetReference<CDCMetaCardView> _metaCardViewPrefabRef;

	private AltAssetReference<ScaffoldingBase> _defaultScaffoldPrefabRef;

	private CardDatabase _cardDatabase;

	private IUnityObjectPool _unityPool;

	private IObjectPool _genericPool;

	private CardMaterialBuilder _cardMaterialBuilder;

	private MatchManager _matchManager;

	private AssetLookupSystem _assetLookupSystem;

	private IClientLocProvider _localizationManager;

	private IBILogger _biLogger;

	private ResourceErrorMessageManager _resourceErrorMessageManager;

	private CardColorCaches _cardColorCaches;

	private readonly AssetCache<ScaffoldingBase> _scaffoldingCache = new AssetCache<ScaffoldingBase>();

	public AssetLookupSystem AssetLookupSystem => _assetLookupSystem;

	public CardMaterialBuilder CardMaterialBuilder => _cardMaterialBuilder;

	public event Action<BASE_CDC> onCardUpdated;

	public event Action<BASE_CDC> onCardCreated;

	public event Action<BASE_CDC> preCardDestroyEvent;

	public CardViewBuilder(CardDatabase cdb, IUnityObjectPool unityPool, IObjectPool genericPool, CardMaterialBuilder cardMaterialBuilder, AssetLookupSystem assetLookupSystem, IClientLocProvider localizationManager, MatchManager matchManager, IBILogger biLogger, ResourceErrorMessageManager resourceErrorMessageManager, CardColorCaches cardColorCaches)
	{
		_cardDatabase = cdb;
		_unityPool = unityPool ?? NullUnityObjectPool.Default;
		_genericPool = genericPool;
		_cardMaterialBuilder = cardMaterialBuilder;
		_assetLookupSystem = assetLookupSystem;
		_localizationManager = localizationManager;
		_matchManager = matchManager;
		_biLogger = biLogger;
		_resourceErrorMessageManager = resourceErrorMessageManager;
		_cardColorCaches = cardColorCaches;
		AssetLookupTree<Prefabs> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<Prefabs>();
		if (assetLookupTree != null)
		{
			Prefabs payload = assetLookupTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				_duelSceneCDCPrefabRef = payload.DuelSceneCdcPrefabRef;
				_metaCDCPrefabRef = payload.MetaCdcPrefabRef;
				_metaCardViewPrefabRef = payload.MetaCardViewPrefabRef;
				_defaultScaffoldPrefabRef = payload.DefaultScaffoldPrefabRef;
			}
		}
	}

	public DuelScene_CDC CreateDuelSceneCdc(ICardDataAdapter data, Func<MtgGameState> getCurrentGameState, Func<WorkflowBase> getCurrentInteraction, IVfxProvider vfxProvider, IEntityNameProvider<uint> entityNameProvider = null, bool isVisible = false)
	{
		return (DuelScene_CDC)CreateCDC(data, isMeta: false, isVisible, getCurrentGameState, getCurrentInteraction, vfxProvider, entityNameProvider);
	}

	public Meta_CDC CreateMetaCdc(ICardDataAdapter data, Transform parent = null)
	{
		Meta_CDC meta_CDC = (Meta_CDC)CreateCDC(data, isMeta: true, isVisible: true);
		if ((bool)parent)
		{
			meta_CDC.transform.parent = parent;
			meta_CDC.transform.ZeroOut();
		}
		return meta_CDC;
	}

	public CDCMetaCardView CreateCDCMetaCardView(CardData data, Transform parent = null)
	{
		CDCMetaCardView component = _unityPool.PopObject(_metaCardViewPrefabRef.RelativePath).GetComponent<CDCMetaCardView>();
		component.transform.SetParent(parent, worldPositionStays: true);
		component.transform.ZeroOut();
		if (data != null)
		{
			component.InitWithData(data, _cardDatabase, this);
		}
		return component;
	}

	private BASE_CDC CreateCDC(ICardDataAdapter data, bool isMeta = false, bool isVisible = false, Func<MtgGameState> getCurrentGameState = null, Func<WorkflowBase> getCurrentInteraction = null, IVfxProvider vfxProvider = null, IEntityNameProvider<uint> nameProvider = null)
	{
		BASE_CDC component = _unityPool.PopObject(isMeta ? _metaCDCPrefabRef.RelativePath : _duelSceneCDCPrefabRef.RelativePath).GetComponent<BASE_CDC>();
		component.gameObject.name = $"CDC #{data.InstanceId}";
		component.InitializeScaffolding(_defaultScaffoldPrefabRef.RelativePath, _scaffoldingCache);
		if (!(component is DuelScene_CDC duelScene_CDC))
		{
			if (component is Meta_CDC meta_CDC)
			{
				meta_CDC.Init(data, isVisible, this, _cardMaterialBuilder, _cardDatabase, _unityPool, _genericPool, _assetLookupSystem, _localizationManager, _biLogger, _resourceErrorMessageManager);
			}
		}
		else
		{
			duelScene_CDC.Init(data, isVisible, this, _cardMaterialBuilder, _cardDatabase, _unityPool, _genericPool, _assetLookupSystem, _localizationManager, _biLogger, _resourceErrorMessageManager, vfxProvider, nameProvider);
		}
		component.enabled = true;
		if (!component.gameObject.activeSelf)
		{
			Debug.LogErrorFormat("CDC instance for card \"{0}\" pulled from pool as inactive. This should never happen.", component.gameObject.name);
			component.gameObject.SetActive(value: true);
		}
		if (!isMeta && component is DuelScene_CDC duelScene_CDC2)
		{
			duelScene_CDC2.GetCurrentGameState = getCurrentGameState;
			duelScene_CDC2.GetCurrentInteraction = getCurrentInteraction;
			ICardHolder previousCardHolder = (duelScene_CDC2.CurrentCardHolder = new NoCardHolder());
			duelScene_CDC2.PreviousCardHolder = previousCardHolder;
		}
		component.ImmediateUpdate();
		this.onCardCreated?.Invoke(component);
		return component;
	}

	public void SendCardUpdatedEvent(BASE_CDC cdc)
	{
		this.onCardUpdated?.Invoke(cdc);
	}

	private void DestroyObj(GameObject obj)
	{
		if (!obj.activeSelf)
		{
			obj.SetActive(value: true);
		}
		_unityPool?.PushObject(obj);
	}

	public void DestroyCDC(BASE_CDC cdc)
	{
		if ((bool)cdc)
		{
			cdc.ClearOverrides();
			this.preCardDestroyEvent?.Invoke(cdc);
			cdc.Teardown();
			DestroyObj(cdc.gameObject);
		}
	}

	public void UpdateSleeveCode(ICardDataAdapter model)
	{
		if (_matchManager != null && model != null)
		{
			UpdateSleeveCode(_matchManager, model.Instance);
		}
	}

	private void UpdateSleeveCode(IPlayerInfoProvider playerInfoProvider, MtgCardInstance instance)
	{
		if (instance != null && string.IsNullOrEmpty(instance.SleeveCode) && !instance.CardTypes.Contains(CardType.Dungeon))
		{
			instance.SleeveCode = playerInfoProvider.GetSleeveForPlayer(instance.Owner?.InstanceId ?? 0);
		}
	}

	public void UpdateParts(ICardDataAdapter model, Transform parent, List<CDCSmartAnchor> anchors, List<string> partsThatShouldBeActive, Dictionary<string, CDCPart> activeParts, AssetLookupSystem assetLookupSystem, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, ICardDatabaseAdapter cardDatabase, CardMaterialBuilder cardMaterialBuilder, IClientLocProvider localizationManager, Func<MtgGameState> getCurrentGameState, Func<WorkflowBase> getCurrentInteraction, IVfxProvider vfxProvider, IEntityNameProvider<uint> entityNameProvider, CDCViewMetadata viewMetadata, CardHolderType holderType, HashSet<PropertyType> changedProps, Dictionary<AnchorPointType, CDCPart> previousParts)
	{
		foreach (CDCSmartAnchor anchor in anchors)
		{
			AnchorPointType anchorType = anchor.AnchorType;
			string text = FindPartPrefabPath(model, holderType, anchor, assetLookupSystem, viewMetadata);
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			CDCPart cDCPart = null;
			if (!activeParts.ContainsKey(text) || !activeParts[text])
			{
				GameObject gameObject = unityObjectPool?.PopObject(text);
				if (!gameObject)
				{
					Debug.LogError("Client.CDC.Creation: Part could not be instantiated \t" + Path.GetFileNameWithoutExtension(text));
					continue;
				}
				cDCPart = gameObject.GetComponent<CDCPart>();
				InitPart(cDCPart, this, cardDatabase, unityObjectPool, genericObjectPool, cardMaterialBuilder, assetLookupSystem, localizationManager, getCurrentGameState, getCurrentInteraction, vfxProvider, entityNameProvider, _cardColorCaches);
				activeParts[text] = cDCPart;
			}
			else
			{
				cDCPart = activeParts[text];
			}
			partsThatShouldBeActive.Add(text);
			cDCPart.transform.SetParent(parent);
			cDCPart.gameObject.SetLayer(parent.gameObject.layer);
			RectTransform rectTransform = cDCPart.transform as RectTransform;
			if (rectTransform == null)
			{
				rectTransform = cDCPart.gameObject.AddComponent<RectTransform>();
			}
			RectTransform rectTransform2 = anchor.transform as RectTransform;
			if (rectTransform2 == null)
			{
				continue;
			}
			rectTransform.anchoredPosition = rectTransform2.anchoredPosition;
			rectTransform.pivot = rectTransform2.pivot;
			rectTransform.anchorMin = rectTransform2.anchorMin;
			rectTransform.anchorMax = rectTransform2.anchorMax;
			rectTransform.offsetMin = rectTransform2.offsetMin;
			rectTransform.offsetMax = rectTransform2.offsetMax;
			rectTransform.localPosition = rectTransform2.localPosition;
			rectTransform.localEulerAngles = rectTransform2.localEulerAngles;
			rectTransform.localScale = rectTransform2.localScale;
			if (anchor.UsePartBounds)
			{
				Rect rect = default(Rect);
				if (AssetLoader.GetObjectData<CDCPart>(text).transform is RectTransform rectTransform3)
				{
					rect = rectTransform3.rect;
				}
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.width);
				rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.height);
			}
			foreach (SmartAnchorRelationship anchorRelationship in anchor.AnchorRelationships)
			{
				if (!previousParts.TryGetValue(anchorRelationship.AnchorType, out var value) || value == null)
				{
					continue;
				}
				Rect relativeRect = RectTransformUtils.GetRelativeRect(rectTransform, rectTransform.localPosition);
				Rect rect2 = value.GetRect();
				if (!relativeRect.Intersects(rect2, out var _))
				{
					continue;
				}
				switch (anchorRelationship.Relationship)
				{
				case SmartAnchorRelationship.RelationshipType.Affects_Bottom:
					if (relativeRect.yMin < rect2.yMax - anchorRelationship.OverlapAffordance)
					{
						Vector2 offsetMin2 = rectTransform.offsetMin;
						offsetMin2 = new Vector2(offsetMin2.x, offsetMin2.y + (rect2.yMax - anchorRelationship.OverlapAffordance - relativeRect.yMin));
						rectTransform.offsetMin = offsetMin2;
					}
					break;
				case SmartAnchorRelationship.RelationshipType.Affects_Top:
					if (relativeRect.yMax > rect2.yMin + anchorRelationship.OverlapAffordance)
					{
						Vector2 offsetMax2 = rectTransform.offsetMax;
						offsetMax2 = new Vector2(offsetMax2.x, offsetMax2.y - (relativeRect.yMax - (rect2.yMin + anchorRelationship.OverlapAffordance)));
						rectTransform.offsetMax = offsetMax2;
					}
					break;
				case SmartAnchorRelationship.RelationshipType.Affects_Left:
					if (relativeRect.xMin < rect2.xMax - anchorRelationship.OverlapAffordance)
					{
						Vector2 offsetMin = rectTransform.offsetMin;
						offsetMin = new Vector2(offsetMin.x + (rect2.xMax - anchorRelationship.OverlapAffordance - relativeRect.xMin), offsetMin.y);
						rectTransform.offsetMin = offsetMin;
					}
					break;
				case SmartAnchorRelationship.RelationshipType.Affects_Right:
					if (relativeRect.xMax > rect2.xMin + anchorRelationship.OverlapAffordance)
					{
						Vector2 offsetMax = rectTransform.offsetMax;
						offsetMax = new Vector2(offsetMax.x - (relativeRect.xMax - (rect2.xMin + anchorRelationship.OverlapAffordance)), offsetMax.y);
						rectTransform.offsetMax = offsetMax;
					}
					break;
				}
			}
			UpdatePart(cDCPart, model, holderType, viewMetadata, changedProps);
			previousParts[anchorType] = cDCPart;
			if (cDCPart is CDCPart_LinkedFace cDCPart_LinkedFace)
			{
				cDCPart_LinkedFace.LinkedFaceCDC.IsMousedOver = viewMetadata.IsMouseOver;
			}
			cDCPart.enabled = true;
			if (!cDCPart.gameObject.activeSelf)
			{
				cDCPart.gameObject.SetActive(value: true);
			}
		}
	}

	private static void InitPart(CDCPart part, CardViewBuilder cardViewBuilder, ICardDatabaseAdapter cardDatabase, IUnityObjectPool unityObjectPool, IObjectPool genericObjectPool, CardMaterialBuilder cardMaterialBuilder, AssetLookupSystem assetLookupSystem, IClientLocProvider localizationManager, Func<MtgGameState> getCurrentGameState, Func<WorkflowBase> getCurrentInteraction, IVfxProvider vfxProvider, IEntityNameProvider<uint> entityNameProvider, CardColorCaches cardColorCaches)
	{
		part.GetCurrentGameState = getCurrentGameState;
		part.GetCurrentInteraction = getCurrentInteraction;
		part.VfxProvider = vfxProvider;
		part.EntityNameProvider = entityNameProvider;
		part.Init(cardViewBuilder, cardDatabase, unityObjectPool, cardMaterialBuilder, assetLookupSystem, localizationManager, genericObjectPool, cardColorCaches);
	}

	private static void UpdatePart(CDCPart part, ICardDataAdapter model, CardHolderType holderType, CDCViewMetadata viewMetadata, HashSet<PropertyType> changedProps)
	{
		part.UpdateFields(model, holderType, viewMetadata, changedProps);
	}

	private static string FindPartPrefabPath(ICardDataAdapter model, CardHolderType cardHolderType, CDCSmartAnchor anchor, AssetLookupSystem assetLookupSystem, CDCViewMetadata viewMetadata)
	{
		string result = string.Empty;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		assetLookupSystem.Blackboard.SetCdcViewMetadata(viewMetadata);
		assetLookupSystem.Blackboard.CardHolderType = cardHolderType;
		assetLookupSystem.Blackboard.Language = Languages.CurrentLanguage;
		AltAssetReference<CDCPart> altAssetReference = null;
		switch (anchor.AnchorType)
		{
		case AnchorPointType.CardBase:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<CardBasePart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.TextBox:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<TextBoxPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.Highlights:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<HighlightsPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.Icons:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<IconsPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.Counters:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<CountersPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.Loyalty:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<LoyaltyPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.Defense:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<DefensePart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.PowerToughness:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<PowerToughnessPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.LinkedFaceRoot_0:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<LinkedFacePartRoot_0>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.LinkedFaceRoot_1:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<LinkedFacePartRoot_1>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.TitleBar:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<TitleBarPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.TitleContent:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<TitleContentPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.TypeLine:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<TypeLinePart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.ArtistCredit:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<ArtistCreditPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.FaceSymbol:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<FaceSymbolPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.ExpansionSymbol:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<ExpansionSymbolPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.AnimatedCardback:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<AnimatedCardbackPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.Guildmark:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<GuildmarkPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.CastableStank:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<CastableStankPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.SubTypeSymbol:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<SubTypeSymbolPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.ArtInFrame:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<ArtInFramePart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.ManaCost:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<ManaCostPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.OrnamentalPart:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<OrnamentalPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.SubTypeText:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<SubTypeText>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.AdventureOmissionLeftPage:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<AdventureOmissionLeftPage>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.AdventureOmissionRightPage:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<AdventureOmissionRightPage>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.SummoningSickness:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<SummoningSickness>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		case AnchorPointType.Omission:
			altAssetReference = assetLookupSystem.TreeLoader.LoadTree<OmissionPart>(returnNewTree: false)?.GetPayload(assetLookupSystem.Blackboard)?.PartRef;
			break;
		default:
			Debug.LogError($"Unhandled anchor type: {anchor.AnchorType}");
			break;
		}
		if (altAssetReference != null)
		{
			result = altAssetReference.RelativePath;
		}
		assetLookupSystem.Blackboard.Clear();
		return result;
	}

	public void Dispose()
	{
		_scaffoldingCache.Dispose();
	}
}
