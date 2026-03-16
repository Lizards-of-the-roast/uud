using System;
using System.Collections.Generic;
using Core.Code.Decks;
using Core.Meta.Cards.Views;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class PagesMetaCardView : CDCMetaCardView
{
	public enum Tint
	{
		None,
		Red,
		Grey
	}

	public enum QuantityDisplayStyle
	{
		Pips,
		Infinity,
		Number,
		None
	}

	public enum PipsDisplayStyle
	{
		Card,
		Skin,
		Fake_Card
	}

	public enum ExpandedDisplayStyle
	{
		Solo,
		Stacked,
		Expanded_First,
		Expanded_Mid,
		Expanded_Last
	}

	[SerializeField]
	private RectTransform _pipsParentTransform;

	[SerializeField]
	private MetaCardPip _pipPrefab;

	[SerializeField]
	private TMP_Text _quantityText;

	[SerializeField]
	private Transform _cardParent;

	[SerializeField]
	private GameObject _companionFrame;

	[SerializeField]
	private GameObject _commanderFrame;

	[SerializeField]
	private GameObject _preferredPrintingTagSpawner;

	[SerializeField]
	private TAG_PreferredPrinting _TAG_PreferredPrinting;

	private readonly List<MetaCardPip> _quantityPips = new List<MetaCardPip>();

	private MetaCardPip _infinityPip;

	private RectTransform _rectTransform;

	private IPreferredPrintingDataProvider _preferredPrintingDataProvider;

	private PagesMetaCardViewDisplayInformation _lastDisplayInfo = new PagesMetaCardViewDisplayInformation();

	public Action<PagesMetaCardView> OnPrefPrintExpansionToggled { get; set; }

	public Action<PagesMetaCardView, bool> OnPreferredPrintingToggleClicked { get; set; }

	public RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public Collider CardCollider => _cardCollider;

	private IPreferredPrintingDataProvider PreferredPrintingDataProvider => _preferredPrintingDataProvider ?? (_preferredPrintingDataProvider = Pantry.Get<IPreferredPrintingDataProvider>());

	protected override bool UsesOutsideCdcZone => true;

	public uint TitleId => _lastDisplayInfo.Card?.TitleId ?? 0;

	private void Awake()
	{
		_pipsParentTransform.gameObject.UpdateActive(active: true);
		_quantityText.gameObject.UpdateActive(active: false);
		for (int i = 0; i < 4; i++)
		{
			MetaCardPip metaCardPip = UnityEngine.Object.Instantiate(_pipPrefab, _pipsParentTransform, worldPositionStays: true);
			metaCardPip.transform.localPosition = Vector3.zero;
			metaCardPip.transform.localRotation = Quaternion.Euler(Vector3.zero);
			_quantityPips.Add(metaCardPip);
		}
		_infinityPip = UnityEngine.Object.Instantiate(_pipPrefab, _pipsParentTransform, worldPositionStays: true);
		_infinityPip.transform.localPosition = Vector3.zero;
		_infinityPip.transform.localRotation = Quaternion.Euler(Vector3.zero);
		_infinityPip.SetSpriteType(MetaCardPip.SpriteType.Infinity);
		_infinityPip.SetToolTip("MainNav/Collection/Unlimited");
	}

	public override void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		base.Init(cardDatabase, cardViewBuilder);
		_cardView.transform.SetParent(_cardParent, worldPositionStays: false);
	}

	protected override bool IsOutsideCardCDC(PointerEventData eventData)
	{
		if (eventData == null)
		{
			return true;
		}
		GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
		if (!gameObject)
		{
			return true;
		}
		foreach (MetaCardPip quantityPip in _quantityPips)
		{
			if (quantityPip.gameObject == gameObject)
			{
				return true;
			}
		}
		Transform parent = gameObject.transform;
		while (parent != null)
		{
			if (_TAG_PreferredPrinting.gameObject == parent.gameObject)
			{
				return true;
			}
			parent = parent.parent;
		}
		return false;
	}

	public bool GetIsPreferredPrinting(uint titleId, uint grpId, string styleCode)
	{
		PreferredPrintingWithStyle preferredPrintingForTitleId = PreferredPrintingDataProvider.GetPreferredPrintingForTitleId((int)titleId);
		if (preferredPrintingForTitleId != null && preferredPrintingForTitleId.printingGrpId == grpId)
		{
			return preferredPrintingForTitleId.styleCode == styleCode;
		}
		return false;
	}

	public bool GetIsExpandable()
	{
		return _lastDisplayInfo.ExpandedStyle == ExpandedDisplayStyle.Stacked;
	}

	public bool GetIsExpanded()
	{
		if (_lastDisplayInfo.ExpandedStyle != ExpandedDisplayStyle.Solo)
		{
			return _lastDisplayInfo.ExpandedStyle != ExpandedDisplayStyle.Stacked;
		}
		return false;
	}

	public bool GetIsSolo(PagesMetaCardViewDisplayInformation displayInformation)
	{
		return displayInformation.ExpandedStyle == ExpandedDisplayStyle.Solo;
	}

	public ExpandedDisplayStyle GetLastDisplayInfoStyle()
	{
		return (_lastDisplayInfo?.ExpandedStyle).Value;
	}

	private uint GetDisplayInfoTitleId(PagesMetaCardViewDisplayInformation displayInformation)
	{
		if (displayInformation.Card == null)
		{
			return 0u;
		}
		return displayInformation.Card.TitleId;
	}

	public uint GetGrpId(PagesMetaCardViewDisplayInformation displayInformation = null)
	{
		if (displayInformation == null)
		{
			displayInformation = _lastDisplayInfo;
		}
		if (displayInformation.Card == null)
		{
			return 0u;
		}
		return displayInformation.Card.GrpId;
	}

	public void ForceCollapseAnimation()
	{
		_TAG_PreferredPrinting.StartOffscreenCollapsedAnimation();
	}

	public bool UpdateDisplayInfo(PagesMetaCardViewDisplayInformation displayInfo, int waitFrames = 0, bool forceCardRefresh = false)
	{
		bool flag = _lastDisplayInfo.Card != displayInfo.Card || _lastDisplayInfo.Skin != displayInfo.Skin || _lastDisplayInfo.UsedPrintingCount != displayInfo.UsedPrintingCount || _lastDisplayInfo.ExpandedStyle != displayInfo.ExpandedStyle || forceCardRefresh;
		if (flag || _lastDisplayInfo.RemainingTitleCount != displayInfo.RemainingTitleCount || _lastDisplayInfo.UnownedPrintingCount != displayInfo.UnownedPrintingCount || _lastDisplayInfo.AvailablePrintingCount != displayInfo.AvailablePrintingCount || _lastDisplayInfo.AvailableTitleCount != displayInfo.AvailableTitleCount || _lastDisplayInfo.UsedPrintingCount != displayInfo.UsedPrintingCount || _lastDisplayInfo.UsedTitleCount != displayInfo.UsedTitleCount || _lastDisplayInfo.Max != displayInfo.Max || _lastDisplayInfo.QuantityStyle != displayInfo.QuantityStyle || _lastDisplayInfo.PipsStyle != displayInfo.PipsStyle || _lastDisplayInfo.ExpandedStyle != displayInfo.ExpandedStyle)
		{
			_ = 1;
		}
		else
			_ = _lastDisplayInfo.Tint != displayInfo.Tint;
		ActivateFirstTag(isFirst: false);
		ActivateFactionTag(isFaction: false, "");
		if (displayInfo.Card != null && displayInfo.UseNewTag)
		{
			UpdateNumberNew(displayInfo.Card.GrpId);
		}
		if (displayInfo.Card != null && displayInfo.UseFactionTag)
		{
			ActivateFactionTag(isFaction: true, displayInfo.FactionTag);
		}
		bool activeSelf = _tagObject.activeSelf;
		bool isSelectable = displayInfo.AvailablePrintingCount != 0;
		bool isSolo = GetIsSolo(displayInfo);
		if (!isSolo && !(_TAG_PreferredPrinting == null))
		{
			if (!_TAG_PreferredPrinting.GetAreClickHandlersSet())
			{
				_TAG_PreferredPrinting.SetClickHandlers(OnPrefPrintExpansionToggled, OnPreferredPrintingToggleClicked, this);
			}
			_TAG_PreferredPrinting.SetViewData(displayInfo.ExpandedStyle, !string.IsNullOrEmpty(displayInfo.Skin), GetIsPreferredPrinting(GetDisplayInfoTitleId(displayInfo), GetGrpId(displayInfo), displayInfo.Skin), isSelectable, displayInfo.PoolContainsNewCards, displayInfo.IsCollapsing);
			if (displayInfo.PoolContainsNewCards)
			{
				ActivateFirstTag(isFirst: true);
			}
		}
		displayInfo.IsCollapsing = false;
		_preferredPrintingTagSpawner.SetActive(!isSolo);
		_TAG_PreferredPrinting.gameObject.SetActive(!isSolo);
		if (true || activeSelf != _tagObject.activeSelf)
		{
			if (displayInfo.UseCustomAutoLandsToggleObject)
			{
				_cardParent.gameObject.UpdateActive(active: false);
				_pipsParentTransform.gameObject.UpdateActive(active: false);
				_quantityText.gameObject.UpdateActive(active: false);
				base.Card = null;
				base.VisualCard = null;
				Pantry.Get<DeckBuilderCardFilterProvider>().ReparentAutoSuggestLand(base.transform);
			}
			else
			{
				_cardParent.gameObject.UpdateActive(active: true);
				if (flag)
				{
					CardData cardData = CardDataExtensions.CreateSkinCard(displayInfo.Card.GrpId, _cardDatabase, displayInfo.Skin);
					cardData.IsFakeStyleCard = !string.IsNullOrEmpty(displayInfo.Skin) && !displayInfo.IsSideboardingOrLimited && displayInfo.ExpandedStyle != ExpandedDisplayStyle.Stacked;
					SetDataAndWait(cardData, CardHolderType.None, (uint)waitFrames);
				}
				SetQuantity(displayInfo, _cardNumberNew);
			}
		}
		_lastDisplayInfo = displayInfo.GetCopy();
		return true;
	}

	public void ShowCommanderFrame(bool isCommander)
	{
		if (_commanderFrame != null)
		{
			_commanderFrame.SetActive(isCommander);
		}
	}

	public void ShowCompanionFrame(bool isCompanion)
	{
		if (_companionFrame != null)
		{
			_companionFrame.SetActive(isCompanion);
		}
	}

	private void SetQuantity(PagesMetaCardViewDisplayInformation displayInfo, int numNew)
	{
		Color? dimmed = displayInfo.Tint switch
		{
			Tint.None => null, 
			Tint.Grey => CDCMetaCardView.GRAY_OUT_COLOR_VALUE, 
			Tint.Red => CDCMetaCardView.BANNED_COLOR_VALUE, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		_cardView.SetDimmed(dimmed);
		_pipsParentTransform.gameObject.UpdateActive(displayInfo.QuantityStyle != QuantityDisplayStyle.None && displayInfo.QuantityStyle != QuantityDisplayStyle.Number);
		_quantityText.gameObject.UpdateActive(displayInfo.QuantityStyle == QuantityDisplayStyle.Number);
		for (int i = 0; i < _quantityPips.Count; i++)
		{
			_quantityPips[i].gameObject.UpdateActive(displayInfo.QuantityStyle == QuantityDisplayStyle.Pips && i < displayInfo.Max);
		}
		_infinityPip.gameObject.UpdateActive(displayInfo.QuantityStyle == QuantityDisplayStyle.Infinity);
		if (displayInfo.QuantityStyle == QuantityDisplayStyle.Number)
		{
			uint num = ((displayInfo.AvailablePrintingCount != 0) ? displayInfo.AvailableTitleCount : 0u);
			_quantityText.SetText($"x{num}");
		}
		else
		{
			if (displayInfo.QuantityStyle != QuantityDisplayStyle.Pips)
			{
				return;
			}
			for (int j = 0; j < Mathf.Min(_quantityPips.Count, displayInfo.Max); j++)
			{
				MetaCardPip.SpriteType spriteType;
				string toolTip;
				if (displayInfo.PipsStyle == PipsDisplayStyle.Fake_Card)
				{
					spriteType = MetaCardPip.SpriteType.Collected_InDeck_Collapsed;
					toolTip = "MainNav/General/Empty_String";
				}
				else
				{
					if (displayInfo.ExpandedStyle != ExpandedDisplayStyle.Stacked && displayInfo.ExpandedStyle != ExpandedDisplayStyle.Solo)
					{
						(spriteType, toolTip) = GetPipSpriteForExpanded(displayInfo, j, displayInfo.UsedPrintingCount, displayInfo.AvailablePrintingCount, displayInfo.AvailableTitleCount, displayInfo.RemainingTitleCount, displayInfo.PipsStyle);
					}
					else
					{
						(spriteType, toolTip) = GetPipSpriteForCollapsed(displayInfo, j, displayInfo.RemainingTitleCount);
					}
					int availablePrintingCount = (int)displayInfo.AvailablePrintingCount;
					_quantityPips[j].SetNew(availablePrintingCount > j && availablePrintingCount - j <= numNew);
				}
				_quantityPips[j].SetSpriteType(spriteType);
				_quantityPips[j].SetToolTip(toolTip);
			}
		}
	}

	private (MetaCardPip.SpriteType, string) GetPipSpriteForExpanded(PagesMetaCardViewDisplayInformation displayInfo, int pipIndex, uint usedPrintingCount, uint availablePrintingCount, uint availableTitleCount, uint remainingTitleCount, PipsDisplayStyle pipStyle)
	{
		MetaCardPip.SpriteType item = MetaCardPip.SpriteType.NotCollected_InInventory;
		string item2 = "MainNav/Collection/ToolTip_Unowned";
		usedPrintingCount = Math.Min(usedPrintingCount, 4u);
		if (availablePrintingCount == 0 || (pipIndex < usedPrintingCount && pipIndex >= usedPrintingCount - displayInfo.UnownedPrintingCount))
		{
			item = MetaCardPip.SpriteType.NotCollected_InDeck;
			item2 = "MainNav/Collection/ToolTip_UnownedInDeck";
		}
		else if (pipIndex < usedPrintingCount)
		{
			if (pipStyle == PipsDisplayStyle.Skin)
			{
				item = MetaCardPip.SpriteType.Collected_Skin;
				item2 = "MainNav/Collection/ToolTip_StyleInDeck";
			}
			else
			{
				item = MetaCardPip.SpriteType.Collected_InDeck_Expanded;
				item2 = "MainNav/Collection/ToolTip_CardInDeck";
			}
		}
		else if (pipIndex >= usedPrintingCount && pipIndex < usedPrintingCount + remainingTitleCount)
		{
			if (pipStyle == PipsDisplayStyle.Skin)
			{
				item = MetaCardPip.SpriteType.NotCollected_Skin;
				item2 = "MainNav/Collection/ToolTip_OwnedStyle";
			}
			else
			{
				item = MetaCardPip.SpriteType.Collected_InInventory_Expanded;
				item2 = "MainNav/Collection/ToolTip_OwnedCard";
			}
		}
		if (pipIndex >= availableTitleCount && pipIndex >= usedPrintingCount)
		{
			item = MetaCardPip.SpriteType.NotCollected_InInventory;
			item2 = "MainNav/Collection/ToolTip_Unowned";
		}
		return (item, item2);
	}

	private (MetaCardPip.SpriteType, string) GetPipSpriteForCollapsed(PagesMetaCardViewDisplayInformation displayInfo, int pipIndex, uint availableTitleCount)
	{
		uint usedTitleCount = displayInfo.UsedTitleCount;
		MetaCardPip.SpriteType item = MetaCardPip.SpriteType.NotCollected_InInventory;
		string item2 = "MainNav/Collection/ToolTip_Unowned";
		if (pipIndex < usedTitleCount && pipIndex < displayInfo.AvailableTitleCount)
		{
			item = MetaCardPip.SpriteType.Collected_InDeck_Collapsed;
			item2 = "MainNav/Collection/ToolTip_InDeck";
		}
		else if (pipIndex >= usedTitleCount && pipIndex < usedTitleCount + availableTitleCount)
		{
			item = MetaCardPip.SpriteType.Collected_InInventory_Collapsed;
			item2 = "MainNav/Collection/ToolTip_Owned";
		}
		else if (pipIndex >= availableTitleCount && pipIndex < usedTitleCount)
		{
			item = MetaCardPip.SpriteType.NotCollected_InDeck;
			item2 = "MainNav/Collection/ToolTip_UnownedInDeck";
		}
		else if (pipIndex >= availableTitleCount && pipIndex >= usedTitleCount)
		{
			item = MetaCardPip.SpriteType.NotCollected_InInventory;
			item2 = "MainNav/Collection/ToolTip_Unowned";
		}
		return (item, item2);
	}

	public void ExpandClicked()
	{
		OnPrefPrintExpansionToggled?.Invoke(this);
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (_preferredPrintingTagSpawner != null)
		{
			_preferredPrintingTagSpawner.SetActive(value: false);
		}
		_TAG_PreferredPrinting = null;
		_preferredPrintingTagSpawner = null;
	}

	public void OnSpawnerInstantiate(GameObject preferredPrintingTag)
	{
	}
}
