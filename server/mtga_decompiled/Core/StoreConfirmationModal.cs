using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Assets.Core.Meta.MainNavigation.Store.Utils;
using Core.Meta.MainNavigation.Store;
using Core.Meta.MainNavigation.Store.Utils;
using Core.Meta.Shared;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.GeneralUtilities.AdvancedButton;
using Wizards.MDN.Store;
using Wizards.Mtga;
using Wizards.Mtga.UI;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

public class StoreConfirmationModal : MonoBehaviour
{
	[Serializable]
	public struct PurchaseButton
	{
		public CustomButton Button;

		public TextMeshProUGUI Label;
	}

	public delegate void PurchaseConfirmationDelgate(StoreItem item, Client_PurchaseCurrencyType type);

	[Header("StoreItem")]
	[SerializeField]
	private Transform _storeItemContainer;

	[SerializeField]
	private float _maxStoreItemWidth = 700f;

	[Header("Product List")]
	[SerializeField]
	private Transform _productListLayoutElement;

	[SerializeField]
	private Transform _productListContainer;

	[SerializeField]
	private Transform _unownedCardTileContainer;

	[SerializeField]
	private Transform _ownedCardTileContainer;

	[SerializeField]
	private ListMetaCardView_Expanding _cardTilePrefab;

	[SerializeField]
	private ListMetaCardHolder_Expanding _cardHolder;

	[SerializeField]
	private Transform _cosmeticTileContainer;

	[SerializeField]
	private StoreConfirmationCosmeticTile _cosmeticTilePrefab;

	[SerializeField]
	private Localize _label;

	[SerializeField]
	private Localize _descriptionForcedTooltip;

	[SerializeField]
	private Localize _ownedCardsDescription;

	[SerializeField]
	private GameObject _ownedUnownedDescription;

	[SerializeField]
	private GameObject _itemListTextPrefab;

	[Header("Buttons")]
	[SerializeField]
	private PurchaseButton _buttonGemPurchase;

	[SerializeField]
	private PurchaseButton _buttonCoinPurchase;

	[SerializeField]
	private PurchaseButton _buttonCashPurchase;

	[SerializeField]
	private PurchaseButton _buttonFreePurchase;

	[SerializeField]
	private AdvancedButton _buttonDeckInspect;

	[SerializeField]
	private Image _customTokenIcon;

	private PurchaseConfirmationDelgate _PurchaseConfirmationDelgate;

	private StoreItem _storeItem;

	private StoreItemBase _itemWidget;

	private Dictionary<Client_PurchaseCurrencyType, PurchaseButton> _purchaseTypeButtonMap;

	private Dictionary<(uint, bool, string), ListMetaCardView_Expanding> _cardTiles;

	private List<StoreConfirmationCosmeticTile> _cosmeticTiles;

	private CardDatabase _cardDatabase;

	private IGreLocProvider _greLocalizationManager;

	private IClientLocProvider _clientLocalizationProvider;

	private AssetLookupSystem _assetLookupSystem;

	private AssetLoader.AssetTracker<Sprite> _prizeWallTokenImageSpriteTracker;

	private StoreTabType _tabOfOrigin;

	private CardViewBuilder _cardViewBuilder;

	private Localize _itemListText;

	[SerializeField]
	private FrameSpriteData[] _frameSpritesData;

	private static readonly int Disabled = Animator.StringToHash("Disabled");

	public event System.Action OnOpened;

	public event System.Action OnClosed;

	public void Initialize(CardDatabase cardDatabase, IGreLocProvider greLocalizationManager, IClientLocProvider clientLocalizationProvider, CardViewBuilder cardViewBuilder, AssetLookupSystem assetLookupSystem)
	{
		_cardDatabase = cardDatabase;
		_greLocalizationManager = greLocalizationManager;
		_clientLocalizationProvider = clientLocalizationProvider;
		_assetLookupSystem = assetLookupSystem;
		_cardViewBuilder = cardViewBuilder;
		_purchaseTypeButtonMap = new Dictionary<Client_PurchaseCurrencyType, PurchaseButton>();
		_purchaseTypeButtonMap.Add(Client_PurchaseCurrencyType.Gem, _buttonGemPurchase);
		_purchaseTypeButtonMap.Add(Client_PurchaseCurrencyType.Gold, _buttonCoinPurchase);
		_purchaseTypeButtonMap.Add(Client_PurchaseCurrencyType.RMT, _buttonCashPurchase);
		_purchaseTypeButtonMap.Add(Client_PurchaseCurrencyType.None, _buttonFreePurchase);
		_purchaseTypeButtonMap.Add(Client_PurchaseCurrencyType.CustomToken, _buttonFreePurchase);
		foreach (KeyValuePair<Client_PurchaseCurrencyType, PurchaseButton> kvp in _purchaseTypeButtonMap)
		{
			if (kvp.Value.Button != null)
			{
				kvp.Value.Button.OnClick.AddListener(delegate
				{
					OnButtonClicked(kvp.Key);
				});
			}
		}
		_cardTiles = new Dictionary<(uint, bool, string), ListMetaCardView_Expanding>();
		_cosmeticTiles = new List<StoreConfirmationCosmeticTile>();
	}

	private void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_customTokenIcon, _prizeWallTokenImageSpriteTracker);
	}

	public void SetStoreItem(StoreItem storeItem, StoreItemBase itemWidget, Client_PurchaseCurrencyType? currencyType, PurchaseConfirmationDelgate purchaseConfirmation, StoreTabType tabOfOrigin = StoreTabType.None)
	{
		_PurchaseConfirmationDelgate = purchaseConfirmation;
		base.gameObject.UpdateActive(active: true);
		SetItem(storeItem, itemWidget);
		SetPurchaseButtons(currencyType);
		SetProductList();
		_itemWidget.ResetTags();
		this.OnOpened?.Invoke();
		_tabOfOrigin = tabOfOrigin;
	}

	public void Close()
	{
		base.gameObject.UpdateActive(active: false);
		if (_itemWidget != null)
		{
			_itemWidget.RestorePreviousWidget(_itemWidget._storeItem.ListingType == EListingType.PrizeWall && _tabOfOrigin == StoreTabType.PrizeWall);
			_itemWidget.SetDailyDealOverride(_itemWidget._previousWidgetInfo.DailyDealTrimActive);
			RectTransform component = _itemWidget.GetComponent<RectTransform>();
			if (component != null)
			{
				component.sizeDelta = _itemWidget._previousWidgetInfo.Size;
			}
			_itemWidget._previousWidgetInfo = null;
			_itemWidget.SetAllowInput(allowInput: true);
			_itemWidget.SetAllowHover(allowHover: true);
			_itemWidget.SetPurchaseButtons(_storeItem, _assetLookupSystem);
			_itemWidget.ResetTags();
		}
		foreach (KeyValuePair<(uint, bool, string), ListMetaCardView_Expanding> cardTile in _cardTiles)
		{
			UnityEngine.Object.Destroy(cardTile.Value.gameObject);
		}
		_cardTiles.Clear();
		foreach (StoreConfirmationCosmeticTile cosmeticTile in _cosmeticTiles)
		{
			UnityEngine.Object.Destroy(cosmeticTile.gameObject);
		}
		_cosmeticTiles.Clear();
		_storeItem = null;
		_itemWidget = null;
		_PurchaseConfirmationDelgate = null;
		this.OnClosed?.Invoke();
	}

	private void SetItem(StoreItem storeItem, StoreItemBase itemWidget)
	{
		_storeItem = storeItem;
		_itemWidget = itemWidget;
		_itemWidget.SetPreviousWidgetInfo();
		_itemWidget._storeItem = _storeItem;
		Transform obj = _itemWidget.transform;
		obj.SetParent(_storeItemContainer);
		obj.localPosition = Vector3.zero;
		obj.SetAsFirstSibling();
		RectTransform component = _itemWidget.GetComponent<RectTransform>();
		if (component != null && component.sizeDelta.x > _maxStoreItemWidth)
		{
			component.sizeDelta = new Vector2(_maxStoreItemWidth, component.sizeDelta.y);
		}
		_itemWidget.SetAllowInput(allowInput: false);
		_itemWidget.SetAllowHover(allowHover: false, lockedInHover: true);
		_itemWidget.SetPurchaseButtons(null, _assetLookupSystem);
		_itemWidget.HideBrowseButton();
		_itemWidget.HideTooltip();
	}

	private void SetPurchaseButtons(Client_PurchaseCurrencyType? currencyType)
	{
		foreach (KeyValuePair<Client_PurchaseCurrencyType, PurchaseButton> item in _purchaseTypeButtonMap)
		{
			item.Deconstruct(out var _, out var value);
			value.Button.gameObject.UpdateActive(active: false);
		}
		foreach (StoreItemBase.PurchaseButtonSpecification spec in PurchaseCostUtils.TransformPurchaseOptions(_storeItem))
		{
			if (!_purchaseTypeButtonMap.TryGetValue(spec.CurrencyType, out var value2))
			{
				continue;
			}
			value2.Button.gameObject.UpdateActive(!currencyType.HasValue || spec.CurrencyType == currencyType.Value);
			value2.Label.SetText(spec.Text);
			value2.Button.GetComponent<Animator>().SetBool(Disabled, !spec.Enabled);
			value2.Button.Interactable = spec.Enabled;
			if (spec.CurrencyType == Client_PurchaseCurrencyType.CustomToken || spec.CurrencyType == Client_PurchaseCurrencyType.None)
			{
				value2.Button.OnClick.RemoveAllListeners();
				value2.Button.OnClick.AddListener(delegate
				{
					OnButtonClicked(spec.CurrencyType);
				});
			}
			if (spec.CurrencyType == Client_PurchaseCurrencyType.CustomToken)
			{
				if (_prizeWallTokenImageSpriteTracker == null)
				{
					_prizeWallTokenImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("PrizeWallTokenImageSprite");
				}
				AssetLoaderUtils.TrySetSprite(_customTokenIcon, _prizeWallTokenImageSpriteTracker, PrizeWallUtils.GetTokenImagePath(_assetLookupSystem, spec.CurrencyId));
			}
			if (_customTokenIcon != null)
			{
				_customTokenIcon.gameObject.SetActive(spec.CurrencyType == Client_PurchaseCurrencyType.CustomToken);
			}
		}
	}

	private void OnButtonClicked(Client_PurchaseCurrencyType currencyType)
	{
		_PurchaseConfirmationDelgate?.Invoke(_storeItem, currencyType);
		Close();
	}

	private void SetProductList()
	{
		bool flag = _storeItem.Skus.Select((Sku sku) => CardTileStoreCore.ExpectedTileCountForTreasureType(sku.TreasureItem.TreasureType)).Sum() > 1;
		if (_unownedCardTileContainer != null)
		{
			_unownedCardTileContainer.gameObject.SetActive(value: false);
		}
		if (_ownedCardTileContainer != null)
		{
			_ownedCardTileContainer.gameObject.SetActive(value: false);
		}
		if (_cardHolder.gameObject != null)
		{
			_cardHolder.gameObject.SetActive(value: false);
		}
		if (_cosmeticTileContainer != null)
		{
			_cosmeticTileContainer.gameObject.SetActive(value: false);
		}
		if (_descriptionForcedTooltip != null)
		{
			_descriptionForcedTooltip.gameObject.SetActive(value: false);
		}
		if (_ownedCardsDescription != null)
		{
			_ownedCardsDescription.gameObject.SetActive(value: false);
		}
		if (_ownedUnownedDescription != null)
		{
			_ownedUnownedDescription.gameObject.SetActive(value: false);
		}
		if (!flag && string.IsNullOrEmpty(_itemWidget._tooltipTrigger.LocString.mTerm))
		{
			_productListLayoutElement.gameObject.SetActive(value: false);
			return;
		}
		_productListLayoutElement.gameObject.SetActive(value: true);
		if (_itemListText == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_itemListTextPrefab, _productListContainer);
			_itemListText = gameObject.GetComponent<Localize>();
		}
		_itemListText.SetText(_itemWidget._tooltipTrigger.LocString.mTerm);
		_itemListText.gameObject.SetActive(!flag);
		if (!flag)
		{
			return;
		}
		if (_unownedCardTileContainer != null)
		{
			_unownedCardTileContainer.gameObject.SetActive(value: true);
		}
		if (_ownedCardTileContainer != null)
		{
			_ownedCardTileContainer.gameObject.SetActive(value: true);
		}
		if (_cardHolder != null)
		{
			_cardHolder.gameObject.SetActive(value: true);
		}
		if (_cosmeticTileContainer != null)
		{
			_cosmeticTileContainer.gameObject.SetActive(value: true);
		}
		if (_descriptionForcedTooltip != null)
		{
			_descriptionForcedTooltip.gameObject.SetActive(value: true);
		}
		if (_ownedCardsDescription != null)
		{
			_ownedCardsDescription.gameObject.SetActive(value: true);
		}
		if (_ownedUnownedDescription != null)
		{
			_ownedUnownedDescription.gameObject.SetActive(value: true);
		}
		if (_buttonDeckInspect != null)
		{
			_buttonDeckInspect.gameObject.SetActive(value: false);
			_buttonDeckInspect.onClick.RemoveAllListeners();
		}
		SetLabelText(_itemWidget._label.Text.TextTarget.locKey);
		SetForcedDescriptionFromTooltip(_itemWidget._tooltipTrigger.LocString.mTerm);
		StoreItemDisplay componentInChildren = _itemWidget.GetComponentInChildren<StoreItemDisplay>();
		List<CardDataForTile> cardDatas;
		if (!(componentInChildren is StoreDisplayCardViewBundle storeDisplayCardViewBundle))
		{
			if (componentInChildren is StoreDisplayPreconDeck storeDisplayPreconDeck)
			{
				cardDatas = storeDisplayPreconDeck.CardData;
				if (_buttonDeckInspect != null)
				{
					_buttonDeckInspect.gameObject.SetActive(value: true);
					_buttonDeckInspect.onClick.AddListener(storeDisplayPreconDeck.CallDeckBoxBuilder);
				}
			}
			else
			{
				cardDatas = new List<CardDataForTile>();
			}
		}
		else
		{
			cardDatas = CardTileStoreCore.CardDataForViews(storeDisplayCardViewBundle.BundleCardViews, _storeItem.Skus);
		}
		List<Sku> artStyles = _storeItem.Skus.Where((Sku sku) => sku.TreasureItem.TreasureType == TreasureType.ArtStyle).ToList();
		IncludeArtStylesAsTiles(cardDatas, artStyles);
		bool isProratedBundle = _storeItem.IsProratedBundle;
		bool flag2 = SetCardTiles(cardDatas, isProratedBundle);
		SetOwnedProrationListText("MainNav/Store/Decks/Proration_Description", isProratedBundle && flag2);
	}

	private bool SetCardTiles(List<CardDataForTile> cardDatas, bool isProratedBundle)
	{
		_cardHolder.EnsureInit(_cardDatabase, _cardViewBuilder);
		_cardHolder.RolloverZoomView = Pantry.Get<ICardRolloverZoom>();
		InventoryManager inventoryManager = Pantry.Get<InventoryManager>();
		CosmeticsProvider cosmeticsProvider = Pantry.Get<CosmeticsProvider>();
		ICardDatabaseAdapter cardDb = Pantry.Get<ICardDatabaseAdapter>();
		ITitleCountManager titleCountManager = Pantry.Get<ITitleCountManager>();
		Dictionary<(string, string), CardTileStoreViewModel> viewModelsHash = CardTileStoreCore.ViewModelsForStoreItem(cardDatas, inventoryManager, cosmeticsProvider, titleCountManager);
		bool result = false;
		foreach (CardDataForTile item in CardSorter.Sort(cardDatas, cardDb, SortType.LandLast, SortType.CMCWithXLast, SortType.ColorOrder, SortType.ManaCostDifficulty, SortType.Title))
		{
			item.Deconstruct(out var card, out var quantity, out var _);
			CardData cardData = card;
			quantity = cardData.GrpId;
			CardTileStoreViewModel cardTileStoreViewModel = CardTileStoreCore.ViewModelForID(quantity.ToString(), cardData.SkinCode, viewModelsHash);
			if (isProratedBundle)
			{
				if (cardTileStoreViewModel.UnownedCount > 0 && !_cardTiles.ContainsKey((cardData.GrpId, false, cardData.SkinCode)))
				{
					ListMetaCardView_Expanding listMetaCardView_Expanding = CreateCardTile(cardTileStoreViewModel, cardTileStoreViewModel.UnownedCount, isOwned: false);
					_cardTiles.Add((cardData.GrpId, false, cardData.SkinCode), listMetaCardView_Expanding);
					listMetaCardView_Expanding.SetQuantity(cardTileStoreViewModel.UnownedCount);
				}
				if (cardTileStoreViewModel.OwnedCount > 0 && !_cardTiles.ContainsKey((cardData.GrpId, true, cardData.SkinCode)))
				{
					ListMetaCardView_Expanding listMetaCardView_Expanding2 = CreateCardTile(cardTileStoreViewModel, cardTileStoreViewModel.OwnedCount, isOwned: true);
					_cardTiles.Add((cardData.GrpId, true, cardData.SkinCode), listMetaCardView_Expanding2);
					listMetaCardView_Expanding2.SetQuantity(cardTileStoreViewModel.OwnedCount);
					result = true;
				}
			}
			else
			{
				int num = cardTileStoreViewModel.UnownedCount + cardTileStoreViewModel.OwnedCount;
				if (num > 0 && !_cardTiles.ContainsKey((cardData.GrpId, false, cardData.SkinCode)))
				{
					ListMetaCardView_Expanding listMetaCardView_Expanding3 = CreateCardTile(cardTileStoreViewModel, cardTileStoreViewModel.UnownedCount, isOwned: false);
					_cardTiles.Add((cardData.GrpId, false, cardData.SkinCode), listMetaCardView_Expanding3);
					listMetaCardView_Expanding3.SetQuantity(num);
				}
			}
		}
		return result;
	}

	private void IncludeArtStylesAsTiles(List<CardDataForTile> cardDatas, List<Sku> artStyles)
	{
		foreach (Sku artStyle in artStyles)
		{
			CardData cardData = MaybeCardDataFromArtStyle(artStyle);
			if (cardData != null)
			{
				cardDatas.Add(new CardDataForTile(cardData, 1u, isArtStyle: true));
			}
		}
	}

	private CardData MaybeCardDataFromArtStyle(Sku style)
	{
		if (!style.Id.Contains("_"))
		{
			return null;
		}
		string[] array = style.Id.Split("_", 2);
		if (!uint.TryParse(array[0], out var result))
		{
			return null;
		}
		string skinCode = array[1];
		IReadOnlyList<CardPrintingData> printingsByArtId = _cardDatabase.DatabaseUtilities.GetPrintingsByArtId(result);
		if (printingsByArtId.Count < 1)
		{
			return null;
		}
		return CardDataExtensions.CreateSkinCard(printingsByArtId[0].GrpId, _cardDatabase, skinCode);
	}

	private ListMetaCardView_Expanding CreateCardTile(CardTileStoreViewModel viewModel, int quantity, bool isOwned)
	{
		ListMetaCardView_Expanding listMetaCardView_Expanding = UnityEngine.Object.Instantiate(_cardTilePrefab, isOwned ? _ownedCardTileContainer : _unownedCardTileContainer);
		listMetaCardView_Expanding.gameObject.SetActive(value: false);
		listMetaCardView_Expanding.Card = viewModel.CardData;
		if (viewModel.CardData.IsDisplayedFaceDown && !string.IsNullOrEmpty(viewModel.CardData.SleeveCode))
		{
			MTGALocalizedString mTGALocalizedString = CosmeticsUtils.LocKeyForSleeveId(viewModel.CardData.SleeveCode);
			listMetaCardView_Expanding.SetName(mTGALocalizedString);
		}
		else
		{
			string localizedText = _greLocalizationManager.GetLocalizedText(viewModel.CardData.TitleId);
			string text = (viewModel.IsArtStyle ? _clientLocalizationProvider.GetLocalizedText("BattlePass/Rewards/CardStyle", ("CardName", localizedText)) : localizedText);
			listMetaCardView_Expanding.SetName(text);
		}
		if (viewModel.IsArtStyle)
		{
			listMetaCardView_Expanding.SetStyleMode();
		}
		else
		{
			string text2 = string.Empty;
			if (viewModel.CardData.CardTypes.Count != 1 || viewModel.CardData.CardTypes[0] != CardType.Land)
			{
				text2 = viewModel.CardData.OldSchoolManaText;
			}
			listMetaCardView_Expanding.SetManaCost(ManaUtilities.ConvertManaSymbols(text2));
			listMetaCardView_Expanding.SetQuantity(quantity);
		}
		FrameSpriteData frameSpriteData = (string.IsNullOrEmpty(viewModel.CardData.IsFakeStyleCard ? "DA" : viewModel.CardData.SkinCode) ? _frameSpritesData[0] : _frameSpritesData[1]);
		listMetaCardView_Expanding.SetFrameSprite(ListMetaCardHolder_Expanding.GetFrameSprite(viewModel.CardData, frameSpriteData));
		listMetaCardView_Expanding.SetNameColor(frameSpriteData.TitleTextColor);
		listMetaCardView_Expanding.SendDragEventsUp = true;
		listMetaCardView_Expanding.TagButton.Interactable = false;
		listMetaCardView_Expanding.gameObject.SetActive(value: true);
		listMetaCardView_Expanding.Holder = _cardHolder;
		return listMetaCardView_Expanding;
	}

	private void SetLabelText(MTGALocalizedString text)
	{
		if (_label != null && text != null)
		{
			_label.gameObject.UpdateActive(!string.IsNullOrEmpty(text.Key));
			_label.SetText(text);
		}
	}

	public void SetOwnedProrationListText(MTGALocalizedString text, bool hasOwnedProrationItems)
	{
		if (_ownedCardsDescription != null && text != null)
		{
			_ownedCardsDescription.gameObject.UpdateActive(!string.IsNullOrEmpty(text.Key) && hasOwnedProrationItems);
			_ownedCardsDescription.SetText(text);
		}
	}

	private void SetForcedDescriptionFromTooltip(MTGALocalizedString text)
	{
		if (text != null)
		{
			if (!_itemWidget._forceToolTipDescriptionOnConfirmation)
			{
				_descriptionForcedTooltip.gameObject.SetActive(value: false);
				return;
			}
			_descriptionForcedTooltip.gameObject.UpdateActive(!string.IsNullOrEmpty(text.Key));
			_descriptionForcedTooltip.SetText(text);
		}
	}
}
