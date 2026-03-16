using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Decks;
using DG.Tweening;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Arena.DeckValidation.Core.Models;
using Wizards.Arena.Enums.Card;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Format;
using Wizards.Mtga.Inventory;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.DuelScene.Examine;
using Wotc.Mtga.DuelScene.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class CardViewerController : PopupBase, IScrollHandler, IEventSystemHandler
{
	[SerializeField]
	private CustomButton _craftButton;

	[SerializeField]
	private CustomButton _cancelButton;

	[SerializeField]
	private CustomButton _cosmeticGemsButton;

	[SerializeField]
	private CustomButton _cosmeticGoldButton;

	[SerializeField]
	private CustomButton _selectButton;

	[SerializeField]
	private CustomButton _storeButton;

	[SerializeField]
	private GameObject _transactionBlocker;

	[SerializeField]
	private CardSelector _cardSelectorPrefab;

	[SerializeField]
	private MetaCardHolder _cardHolder;

	[SerializeField]
	private MetaCardHolder _bonusCardHolder;

	[SerializeField]
	private ViewSimplifiedButton _cardStyleStateToggle;

	[Header("Craft")]
	[SerializeField]
	private Transform _locatorCraftCard;

	[SerializeField]
	private Transform _locatorCraftBonusCard;

	[SerializeField]
	private GameObject[] _wildcards;

	[SerializeField]
	private GameObject _ContentCraft;

	[SerializeField]
	private GameObject _CraftPipContainer;

	[SerializeField]
	private CustomButton[] _CraftPips;

	[SerializeField]
	private Sprite _CraftPipUnowned;

	[SerializeField]
	private Sprite _CraftPipOwned;

	[SerializeField]
	private Image[] _PendingCraftPips;

	[SerializeField]
	private Transform _wildcardTotalParent;

	[SerializeField]
	private TMP_Text _wildcardTotalLabel;

	[SerializeField]
	private TMP_Text _subtitleLabel;

	[SerializeField]
	private TMP_Text _subtitleRedLabel;

	[SerializeField]
	private TMP_Text _craftCountLabel;

	[SerializeField]
	private Image _redLabelBackground;

	[SerializeField]
	private GameObject _craftIntroParticles;

	[SerializeField]
	private GameObject _craftVFXCommon;

	[SerializeField]
	private GameObject _craftVFXUncommon;

	[SerializeField]
	private GameObject _craftVFXRare;

	[SerializeField]
	private GameObject _craftVFXMythicRare;

	[Header("Currency")]
	[SerializeField]
	private GameObject _currencyGold;

	[SerializeField]
	private TMP_Text _currencyGoldLabel;

	[SerializeField]
	private GameObject _currencyGems;

	[SerializeField]
	private TMP_Text _currencyGemsLabel;

	[Header("Cosmetic")]
	[SerializeField]
	private RectTransform _cosmeticCardParent;

	[SerializeField]
	private TMP_Text _cosmeticGemsButtonLabel;

	[SerializeField]
	private TMP_Text _cosmeticGoldButtonLabel;

	[SerializeField]
	private GameObject _cosmeticIntroParticles;

	[SerializeField]
	private GameObject _cosmeticPurchaseParticles;

	private Coroutine _cosmeticPurchaseVFXRoutine;

	[SerializeField]
	private GameObject _ContentCosmetics;

	[SerializeField]
	private Animator _AnimatorRoot;

	[Header("Cosmetics Carousel")]
	[SerializeField]
	private float _selectorWidthInLayout = 360f;

	[SerializeField]
	private float _selectorMoveDuration = 0.35f;

	[SerializeField]
	private Ease _selectorMoveEase = Ease.OutBack;

	private bool _craftMode = true;

	private int _collectedQuantity;

	private int _quantityToCraft;

	private int _requestedQuantity;

	private CDCMetaCardView _craftCardInstance;

	private CardData _craftCardData;

	private CDCMetaCardView _craftBonusCardInstance;

	private CardData _craftBonusCardData;

	private readonly List<ArtStyleEntry> _skins = new List<ArtStyleEntry>();

	private readonly List<CardSelector> _cosmeticSelectors = new List<CardSelector>();

	private CardPrintingData _printingData;

	private CardRarity _wildcardCraftingRarity;

	private CardData _wildcardData;

	private List<Meta_CDC> _wildcardInstances;

	private Meta_CDC _wildcardTotalInstance;

	private int _wildcardTotal;

	private int _currentSelector;

	private CosmeticsProvider _cosmetics;

	private CardDatabase _cardDatabase;

	private IModelConverter _modelConverter = NullConverter.Default;

	private CardViewBuilder _cardViewBuilder;

	private IClientLocProvider _locProvider;

	private ISetMetadataProvider _setMetadataProvider;

	private ITitleCountManager _titleCountManager;

	private DeckBuilderModelProvider _modelProvider = Pantry.Get<DeckBuilderModelProvider>();

	private Action<string> _onSelect;

	private Action<Action> _onNav;

	private static readonly int _select = Animator.StringToHash("Select");

	private static readonly int _craft = Animator.StringToHash("Craft");

	private static readonly int _pop = Animator.StringToHash("Pop");

	private static readonly int _menu = Animator.StringToHash("Menu");

	private const int MAX_PIPS = 4;

	private bool UsingCardStyleStateToggle => _cardStyleStateToggle;

	private InventoryManager Inv => WrapperController.Instance.InventoryManager;

	protected override void Awake()
	{
		base.Awake();
		_cancelButton.OnClick.AddListener(OnCancelClicked);
		_craftButton.OnClick.AddListener(OnCraftClicked);
		_cosmeticGemsButton.OnClick.AddListener(OnCosmeticGemsClicked);
		_cosmeticGoldButton.OnClick.AddListener(OnCosmeticGoldClicked);
		_selectButton.OnClick.AddListener(OnSelectClicked);
		_storeButton.OnClick.AddListener(OnStoreClicked);
		_craftButton.OnMouseover.AddListener(OnGenericHover);
		_cosmeticGemsButton.OnMouseover.AddListener(OnGenericHover);
		_cosmeticGoldButton.OnMouseover.AddListener(OnGenericHover);
		_selectButton.OnMouseover.AddListener(OnGenericHover);
		_storeButton.OnMouseover.AddListener(OnGenericHover);
		for (int i = 0; i < _CraftPips.Length; i++)
		{
			int x = i;
			_CraftPips[x].OnClick.AddListener(delegate
			{
				Unity_OnCraftPipClicked(x);
			});
		}
		_cosmeticCardParent.DestroyChildren();
		if (UsingCardStyleStateToggle)
		{
			_cardStyleStateToggle.Clicked += OnCardStylesButtonClicked;
			_cardStyleStateToggle.gameObject.UpdateActive(active: false);
		}
		Languages.LanguageChangedSignal.Listeners += OnLanguageChanged;
	}

	protected void OnDestroy()
	{
		Languages.LanguageChangedSignal.Listeners -= OnLanguageChanged;
	}

	protected override void Show()
	{
		base.Show();
		_modelProvider = Pantry.Get<DeckBuilderModelProvider>();
		Inv.OnRedeemWildcardResponse += InventoryManager_OnRedeemWildcardsResponse;
		Inv.OnPurchaseSkinResponse += InventoryManager_OnPurchaseSkinResponse;
		Inv.InventoryUpdated += InventoryManager_InventoryUpdated;
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_in", AudioManager.Default);
		Refresh_Currency();
	}

	protected override void Hide()
	{
		base.Hide();
		_cardHolder.RolloverZoomView.Close();
		if (UsingCardStyleStateToggle)
		{
			_cardStyleStateToggle.Clear();
		}
		Inv.OnRedeemWildcardResponse -= InventoryManager_OnRedeemWildcardsResponse;
		Inv.OnPurchaseSkinResponse -= InventoryManager_OnPurchaseSkinResponse;
		Inv.InventoryUpdated -= InventoryManager_InventoryUpdated;
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_out", AudioManager.Default);
		_cosmeticPurchaseParticles.SetActive(value: false);
	}

	public void Setup(bool craftMode, uint grpid, string craftSkin, int quantityToCraft, Action<string> onSelect, Action<Action> onNav, ICardRolloverZoom zoomHandler, CosmeticsProvider cosmetics, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, IClientLocProvider locProvider, ISetMetadataProvider setMetadataProvider, ITitleCountManager titleCountManager, uint artId = 0u)
	{
		_craftMode = craftMode;
		_quantityToCraft = quantityToCraft;
		_onSelect = onSelect;
		_onNav = onNav;
		_cosmetics = cosmetics;
		_cardDatabase = cardDatabase;
		_modelConverter = new ModelConverter(cardDatabase);
		_cardViewBuilder = cardViewBuilder;
		_locProvider = locProvider;
		_setMetadataProvider = setMetadataProvider;
		_titleCountManager = titleCountManager;
		_cardHolder.RolloverZoomView = zoomHandler;
		_cardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		_bonusCardHolder.RolloverZoomView = zoomHandler;
		_bonusCardHolder.EnsureInit(cardDatabase, cardViewBuilder);
		_transactionBlocker.SetActive(value: false);
		ActivateCraftVFX(CardRarity.None);
		_printingData = _cardDatabase.CardDataProvider.GetCardPrintingById(grpid);
		_wildcardCraftingRarity = CraftingUtilities.GetWildCardRarityForCrafting(_cardDatabase, _printingData.TitleId);
		uint wildcardGrpId = GetWildcardGrpId(_wildcardCraftingRarity);
		CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(wildcardGrpId);
		_wildcardData = new CardData(null, cardPrintingById);
		CardViewBuilder cardViewBuilder2 = WrapperController.Instance.CardViewBuilder;
		if (_wildcardInstances == null)
		{
			_wildcardInstances = new List<Meta_CDC>
			{
				cardViewBuilder2.CreateMetaCdc(_wildcardData, _wildcards[0].transform),
				cardViewBuilder2.CreateMetaCdc(_wildcardData, _wildcards[1].transform),
				cardViewBuilder2.CreateMetaCdc(_wildcardData, _wildcards[2].transform),
				cardViewBuilder2.CreateMetaCdc(_wildcardData, _wildcards[3].transform)
			};
		}
		else
		{
			for (int i = 0; i < 4; i++)
			{
				_wildcardInstances[i].SetModel(_wildcardData);
			}
		}
		if (_wildcardTotalInstance == null)
		{
			_wildcardTotalInstance = cardViewBuilder2.CreateMetaCdc(_wildcardData, _wildcardTotalParent);
		}
		else
		{
			_wildcardTotalInstance.SetModel(_wildcardData);
		}
		_craftCardData = CardDataExtensions.CreateSkinCard(grpid, _cardDatabase, craftSkin);
		if (_craftCardData.Instance == null)
		{
			_craftCardData = new CardData(_craftCardData.Printing?.CreateInstance(), _craftCardData.Printing);
		}
		_craftBonusCardData = ((_craftCardData.RebalancedCardLink != 0) ? CardDataExtensions.CreateSkinCard(_craftCardData.RebalancedCardLink, _cardDatabase, craftSkin) : null);
		if (_craftBonusCardData != null && _craftBonusCardData.Instance == null)
		{
			_craftBonusCardData = new CardData(_craftBonusCardData.Printing?.CreateInstance(), _craftBonusCardData.Printing);
		}
		SetCraftCardData(_craftCardData, _craftBonusCardData);
		if (UsingCardStyleStateToggle)
		{
			_cardStyleStateToggle.Init(locProvider, cardViewBuilder.AssetLookupSystem, _craftCardInstance.CardView);
			_cardStyleStateToggle.CurrentState = _cardStyleStateToggle.DefaultState;
			_cardStyleStateToggle.SetButtonText();
			_cardStyleStateToggle.ButtonCheckmarkOn(active: false);
			_cardStyleStateToggle.UpdateSourceModel(_craftCardData, CardHolderType.CardViewer);
			_cardStyleStateToggle.SetupToggle();
		}
		ArtStyleEntry artStyleEntry = null;
		if (!string.IsNullOrEmpty(craftSkin) && WrapperController.Instance.Store.CardSkinCatalog.TryGetSkins(_printingData.ArtId, out var skinList))
		{
			artStyleEntry = skinList.FirstOrDefault((ArtStyleEntry x) => !string.IsNullOrEmpty(x.Variant) && string.Equals(x.Variant, craftSkin, StringComparison.InvariantCultureIgnoreCase));
		}
		_skins.Clear();
		_skins.Add(artStyleEntry);
		CardData cardData = CardDataExtensions.CreateSkinCard(grpid, _cardDatabase, artStyleEntry?.Variant);
		cardData.IsFakeStyleCard = !string.IsNullOrEmpty(artStyleEntry?.Variant);
		CardSelector cardSelector;
		if (_cosmeticSelectors.Count == 0)
		{
			cardSelector = UnityEngine.Object.Instantiate(_cardSelectorPrefab, _cosmeticCardParent);
			cardSelector.CardView.Init(_cardDatabase, _cardViewBuilder);
			cardSelector.CardView.SetData(cardData, CardHolderType.CardViewer);
			cardSelector.CardView.Holder = _cardHolder;
			CDCMetaCardView cardView = cardSelector.CardView;
			cardView.OnClicked = (Action<MetaCardView>)Delegate.Combine(cardView.OnClicked, new Action<MetaCardView>(CosmeticSelector_OnClick));
			_cosmeticSelectors.Add(cardSelector);
		}
		else
		{
			cardSelector = _cosmeticSelectors[0];
			cardSelector.CardView.SetData(cardData, CardHolderType.CardViewer);
		}
		cardSelector.CardSkin = artStyleEntry;
		cardSelector.GemPrice = artStyleEntry?.StoreItem?.PurchaseOptions?.FirstOrDefault((Client_PurchaseOption po) => po.CurrencyType == Client_PurchaseCurrencyType.Gem)?.Price;
		cardSelector.GoldPrice = artStyleEntry?.StoreItem?.PurchaseOptions?.FirstOrDefault((Client_PurchaseOption po) => po.CurrencyType == Client_PurchaseCurrencyType.Gold)?.Price;
		cardSelector.gameObject.SetActive(value: true);
		_currentSelector = 0;
		DOTween.Kill(_cosmeticCardParent);
		_cosmeticCardParent.DOAnchorPosX(0f, 0f);
		if (craftSkin == null)
		{
			OpenCraftMode();
		}
		else
		{
			OpenCosmeticMode(craftSkin, artId);
		}
		UpdateIntroParticles(craftMode);
	}

	private void SetCraftCardData(ICardDataAdapter craftCardData, ICardDataAdapter craftBonusCardData)
	{
		CardData data = (craftCardData as CardData) ?? ((craftCardData != null) ? new CardData(craftCardData.Instance, craftCardData.Printing) : null);
		if (_cosmeticSelectors.Count > 0)
		{
			CDCMetaCardView cardView = _cosmeticSelectors[0].CardView;
			cardView.Init(_cardDatabase, _cardViewBuilder);
			cardView.Holder = _cardHolder;
			cardView.SetData(data, CardHolderType.CardViewer);
			cardView.CardView.ImmediateUpdate();
		}
		if (_craftCardInstance == null)
		{
			_craftCardInstance = _cardViewBuilder.CreateCDCMetaCardView(null, _locatorCraftCard);
			_craftCardInstance.Init(_cardDatabase, _cardViewBuilder);
			_craftCardInstance.Holder = _cardHolder;
		}
		_craftCardInstance.SetData(data, CardHolderType.CardViewer);
		_craftCardInstance.CardView.ImmediateUpdate();
		_locatorCraftBonusCard.parent.gameObject.SetActive(craftBonusCardData != null);
		if (craftBonusCardData != null)
		{
			CardData data2 = (craftBonusCardData as CardData) ?? new CardData(craftBonusCardData.Instance, craftBonusCardData.Printing);
			if (_craftBonusCardInstance == null)
			{
				_craftBonusCardInstance = _cardViewBuilder.CreateCDCMetaCardView(data2, _locatorCraftBonusCard);
				_craftBonusCardInstance.Holder = _bonusCardHolder;
			}
			else
			{
				_craftBonusCardInstance.SetData(data2, CardHolderType.CardViewer);
			}
		}
	}

	private int GetMaxCraftLimit()
	{
		_titleCountManager.OwnedTitleCounts.TryGetValue(_printingData.TitleId, out var value);
		int num = (int)_printingData.MaxCollected - value;
		if (num > 0)
		{
			return num;
		}
		if (_collectedQuantity == 0)
		{
			return 1;
		}
		return 0;
	}

	private void OpenCraftMode()
	{
		_ContentCraft.SetActive(value: true);
		_ContentCosmetics.SetActive(value: false);
		_cosmeticGemsButton.gameObject.SetActive(value: false);
		_cosmeticGoldButton.gameObject.SetActive(value: false);
		_CraftPipContainer.SetActive(value: true);
		_selectButton.gameObject.SetActive(value: false);
		_storeButton.gameObject.SetActive(value: false);
		_AnimatorRoot.SetInteger(_menu, 0);
		Inv.Cards.TryGetValue(_printingData.GrpId, out _collectedQuantity);
		_titleCountManager.OwnedTitleCounts.TryGetValue(_printingData.TitleId, out var value);
		for (int i = 0; i < 4; i++)
		{
			Sprite sprite = ((i < value) ? _CraftPipOwned : _CraftPipUnowned);
			_CraftPips[i].GetComponentInChildren<Image>().sprite = sprite;
			_CraftPips[i].Interactable = i >= value;
			_PendingCraftPips[i].gameObject.SetActive(value: false);
		}
		_requestedQuantity = value + _quantityToCraft;
		_wildcardTotal = GetWildcardTotalForCraftPopup();
		string localizedText = _locProvider.GetLocalizedText("MainNav/General/XQuantityLabel", ("quantity", _wildcardTotal.ToString("N0")));
		_wildcardTotalLabel.text = localizedText;
		RefreshCraftMode();
		_AnimatorRoot.SetTrigger("BasicLands", _printingData.IsBasicLand);
	}

	private void RefreshCraftMode()
	{
		_titleCountManager.OwnedTitleCounts.TryGetValue(_printingData.TitleId, out var value);
		int num = Math.Max(0, Math.Min(_requestedQuantity, 4) - value);
		if (value >= 4 && _collectedQuantity == 0)
		{
			num = 1;
		}
		for (int i = 0; i < 4; i++)
		{
			bool flag = i < num;
			_wildcardInstances[i].gameObject.SetActive(flag);
			if (flag)
			{
				if (i < _wildcardTotal)
				{
					_wildcardInstances[i].SetDimmed(null);
				}
				else
				{
					_wildcardInstances[i].SetDimmed(CDCMetaCardView.GRAY_OUT_COLOR_VALUE);
				}
			}
		}
		for (int j = value; j < 4; j++)
		{
			bool active = j < _requestedQuantity;
			_PendingCraftPips[j].gameObject.SetActive(active);
		}
		string text = null;
		List<string> list = new List<string>();
		bool active2 = false;
		bool flag2 = false;
		bool interactable = false;
		int maxCraftLimit = GetMaxCraftLimit();
		if (_printingData.IsBasicLand)
		{
			list.Add(_locProvider.GetLocalizedText("SystemMessage/System_Invalid_Redemption_Text"));
		}
		else if (!CardUtilities.IsCardCraftable(_printingData))
		{
			active2 = true;
			list.Add(_locProvider.GetLocalizedText("SystemMessage/System_Invalid_Redemption_Text"));
		}
		else if (!_setMetadataProvider.IsSetPublished(_printingData.ExpansionCode))
		{
			active2 = true;
			list.Add(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Invalid_Redemption_Unreleased_Text"));
		}
		else if (maxCraftLimit > 0)
		{
			active2 = true;
			flag2 = true;
			CheckForBanned(list);
			string warningText = DeckFormatUtils.GetWarningText(WrapperController.Instance.FormatManager.GetCardTitleAvailability(_printingData.TitleId, WrapperController.Instance.FormatManager.GetDefaultFormat()));
			if (!string.IsNullOrEmpty(warningText))
			{
				list.Add(warningText);
			}
			string rarityLoc = Utils.GetRarityLoc(_wildcardCraftingRarity);
			if (num > _wildcardTotal)
			{
				list.Add(_locProvider.GetLocalizedText("MainNav/DeckBuilder/RedeemWildcard_DontHaveWildcard", ("localizedRarity", rarityLoc)));
			}
			else
			{
				interactable = true;
				text = ((num != 1) ? Languages.ActiveLocProvider.GetLocalizedText("MainNav/CardViewer/Crafting/WildcardsConsumed_Plural", ("quantity", num.ToString("N0")), ("rarity", rarityLoc)) : Languages.ActiveLocProvider.GetLocalizedText("MainNav/CardViewer/Crafting/WildcardsConsumed_Singular", ("rarity", rarityLoc)));
			}
			if (_wildcardCraftingRarity != _printingData.Rarity)
			{
				list.Add(Languages.ActiveLocProvider.GetLocalizedText("MainNav/CardViewer/Crafting/WildcardsConsumed_DifferentWildcard", ("rarity", rarityLoc)));
			}
			if (value >= 4 && _collectedQuantity == 0)
			{
				list.Add(Languages.ActiveLocProvider.GetLocalizedText("MainNav/CardViewer/Crafting/CanCraftOnlyOne"));
			}
		}
		else
		{
			active2 = true;
		}
		_subtitleLabel.gameObject.UpdateActive(text != null);
		if (text != null)
		{
			_subtitleLabel.text = text;
		}
		_subtitleRedLabel.gameObject.UpdateActive(list.Count > 0);
		if (_redLabelBackground != null)
		{
			_redLabelBackground.gameObject.SetActive(list.Count > 0);
		}
		if (list.Count > 0)
		{
			_subtitleRedLabel.text = string.Join(Environment.NewLine, list.ToArray());
		}
		_CraftPipContainer.SetActive(active2);
		_craftButton.gameObject.SetActive(flag2);
		if (flag2)
		{
			_craftButton.Interactable = interactable;
		}
		_craftCountLabel.gameObject.SetActive(num > 0 && maxCraftLimit > 0);
		if (num > 0 && maxCraftLimit > 0)
		{
			string localizedText = _locProvider.GetLocalizedText("MainNav/General/XQuantityLabel", ("quantity", num.ToString("N0")));
			_craftCountLabel.text = localizedText;
		}
	}

	private void OpenCosmeticMode(string skinCode = null, uint artId = 0u)
	{
		_ContentCosmetics.SetActive(value: true);
		_ContentCraft.SetActive(value: false);
		_craftButton.gameObject.SetActive(value: false);
		_CraftPipContainer.SetActive(value: false);
		_subtitleRedLabel.gameObject.SetActive(value: false);
		if (_redLabelBackground != null)
		{
			_redLabelBackground.gameObject.SetActive(value: false);
		}
		_craftCountLabel.gameObject.SetActive(value: false);
		_AnimatorRoot.SetInteger(_menu, 1);
		if (!_cosmetics.TryGetOwnedArtStyles((artId == 0) ? _printingData.ArtId : artId, out var ownedStyles))
		{
			ownedStyles = new List<string>();
		}
		int num = -1;
		for (int i = 0; i < _skins.Count; i++)
		{
			ArtStyleEntry cardSkin = _cosmeticSelectors[i].CardSkin;
			bool flag = cardSkin == null || ownedStyles.Contains(cardSkin.Variant);
			_cosmeticSelectors[i].Collected = flag;
			_cosmeticSelectors[i].ShowPip(cardSkin != null);
			if (cardSkin != null && !string.IsNullOrEmpty(skinCode) && cardSkin.Variant == skinCode)
			{
				num = i;
			}
			int? gemPrice = _cosmeticSelectors[i].GemPrice;
			bool flag2 = gemPrice.HasValue && !flag;
			_cosmeticSelectors[i].CostGemsParent.SetActive(flag2);
			if (flag2)
			{
				_cosmeticSelectors[i].CostGemsLabel.text = gemPrice.Value.ToString("N0");
			}
			int? goldPrice = _cosmeticSelectors[i].GoldPrice;
			bool flag3 = goldPrice.HasValue && !flag;
			_cosmeticSelectors[i].CostGoldParent.SetActive(flag3);
			if (flag3)
			{
				_cosmeticSelectors[i].CostGoldLabel.text = goldPrice.Value.ToString("N0");
			}
		}
		if (num > -1)
		{
			SetCurrentSelector(num);
		}
		RefreshCosmeticMode();
	}

	private void RefreshCosmeticMode()
	{
		for (int i = 0; i < _cosmeticSelectors.Count; i++)
		{
			_cosmeticSelectors[i].Animator.SetBool(_select, i == _currentSelector);
			_cosmeticSelectors[i].CardView.AllowRollOver = false;
		}
		string text = null;
		List<string> list = new List<string>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool active = false;
		CardSelector cardSelector = _cosmeticSelectors[_currentSelector];
		cardSelector.CardView.AllowRollOver = true;
		if (cardSelector.Collected)
		{
			flag = _onSelect != null;
		}
		else if (cardSelector.GemPrice.HasValue || cardSelector.GoldPrice.HasValue)
		{
			flag2 = cardSelector.GemPrice.HasValue;
			if (flag2)
			{
				_cosmeticGemsButtonLabel.text = cardSelector.GemPrice.Value.ToString("N0");
			}
			flag3 = cardSelector.GoldPrice.HasValue;
			if (flag3)
			{
				_cosmeticGoldButtonLabel.text = cardSelector.GoldPrice.Value.ToString("N0");
				_cosmeticGoldButton.Interactable = cardSelector.GoldPrice.Value <= Inv.Inventory.gold;
			}
			CheckForBanned(list);
			CheckForUnowned(list);
			string warningText = DeckFormatUtils.GetWarningText(WrapperController.Instance.FormatManager.GetCardArtAvailability(_printingData.ArtId, WrapperController.Instance.FormatManager.GetDefaultFormat()));
			if (!string.IsNullOrEmpty(warningText))
			{
				list.Add(warningText);
			}
		}
		else if (cardSelector.CardSkin.Source.HasFlag(AcquisitionFlags.BattlePass) || cardSelector.CardSkin.StoreSection == EStoreSection.ProgressionTracks)
		{
			active = true;
			list.Add(_locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/CantPurchaseStyle_BattlePass"));
		}
		else if (cardSelector.CardSkin.Source.HasFlag(AcquisitionFlags.CodeRedemption))
		{
			list.Add(_locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/CantPurchaseStyle_Code"));
		}
		else if (cardSelector.CardSkin.Source.HasFlag(AcquisitionFlags.Event))
		{
			list.Add(_locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/CantPurchaseStyle_Event"));
		}
		else if (cardSelector.CardSkin.Source.HasFlag(AcquisitionFlags.SeasonReward))
		{
			list.Add(_locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/CantPurchaseStyle_SeasonReward"));
		}
		else if (cardSelector.CardSkin.StoreItem != null || cardSelector.CardSkin.StoreSection != EStoreSection.None)
		{
			active = true;
			list.Add(_locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/CantPurchaseTreatment_Bundle"));
		}
		else
		{
			list.Add(_locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/CantPurchaseTreatment_Other"));
		}
		_subtitleLabel.gameObject.UpdateActive(text != null);
		if (text != null)
		{
			_subtitleLabel.text = text;
		}
		_subtitleRedLabel.gameObject.UpdateActive(list.Count > 0);
		if (_redLabelBackground != null)
		{
			_redLabelBackground.gameObject.SetActive(list.Count > 0);
		}
		if (list.Count > 0)
		{
			_subtitleRedLabel.text = string.Join(Environment.NewLine, list.ToArray());
		}
		_cosmeticGemsButton.gameObject.SetActive(flag2);
		_cosmeticGoldButton.gameObject.SetActive(flag3);
		_storeButton.gameObject.SetActive(active);
		bool flag4 = _modelProvider.Model.GetQuantityInWholeDeckByTitle(_printingData.TitleId) != 0;
		flag = flag && flag4;
		_selectButton.gameObject.SetActive(flag);
	}

	private void CheckForUnowned(List<string> errors)
	{
		_titleCountManager.OwnedTitleCounts.TryGetValue(_printingData.TitleId, out var value);
		if (value == 0)
		{
			errors.Add(_locProvider.GetLocalizedText("MainNav/Store/Crafting_Unowned_Skin_Message"));
		}
	}

	private void CheckForBanned(List<string> errors)
	{
		List<DeckFormat> activeFormats = FormatUtilitiesClient.GetActiveFormats(WrapperController.Instance.FormatManager, WrapperController.Instance.EventManager);
		string[] bannedFormatsName = FormatUtilitiesClient.GetBannedFormatsName(_printingData.TitleId, activeFormats);
		if (bannedFormatsName.Length != 0)
		{
			errors.Add(_locProvider.GetLocalizedText("AbilityHanger/SpecialHangers/Banned_Body", ("formats", string.Join(", ", bannedFormatsName))));
		}
	}

	private uint GetWildcardGrpId(CardRarity rarity)
	{
		return rarity switch
		{
			CardRarity.MythicRare => 6u, 
			CardRarity.Rare => 7u, 
			CardRarity.Uncommon => 8u, 
			_ => 9u, 
		};
	}

	private int GetWildcardTotalForCraftPopup()
	{
		if (Inv.Inventory == null)
		{
			return 0;
		}
		return _wildcardCraftingRarity switch
		{
			CardRarity.MythicRare => Inv.Inventory.wcMythic, 
			CardRarity.Rare => Inv.Inventory.wcRare, 
			CardRarity.Uncommon => Inv.Inventory.wcUncommon, 
			_ => Inv.Inventory.wcCommon, 
		};
	}

	private void ActivateCraftVFX(CardRarity rarity)
	{
		_craftVFXCommon.SetActive(rarity == CardRarity.Common);
		_craftVFXUncommon.SetActive(rarity == CardRarity.Uncommon);
		_craftVFXRare.SetActive(rarity == CardRarity.Rare);
		_craftVFXMythicRare.SetActive(rarity == CardRarity.MythicRare);
	}

	private void OnGenericHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	private void CosmeticSelector_OnClick(MetaCardView selector)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_choose", base.gameObject);
		SetCurrentSelector(_cosmeticSelectors.FindIndex((CardSelector x) => x.CardView == selector));
	}

	private void SetCurrentSelector(int currentSelector)
	{
		_currentSelector = currentSelector;
		DOTween.Kill(_cosmeticCardParent);
		_cosmeticCardParent.DOAnchorPosX((float)(-_currentSelector) * _selectorWidthInLayout, _selectorMoveDuration).SetEase(_selectorMoveEase).SetTarget(_cosmeticCardParent);
		RefreshCosmeticMode();
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (!_craftMode)
		{
			if (eventData.scrollDelta.y < 0f && _currentSelector < _skins.Count - 1)
			{
				SetCurrentSelector(_currentSelector + 1);
			}
			if (eventData.scrollDelta.y > 0f && _currentSelector > 0)
			{
				SetCurrentSelector(_currentSelector - 1);
			}
		}
	}

	public override void OnEnter()
	{
		if (_craftMode)
		{
			if (_craftButton.gameObject.activeSelf && _craftButton.Interactable)
			{
				OnCraftClicked();
			}
		}
		else if (_cosmeticGoldButton.gameObject.activeSelf && _cosmeticGoldButton.Interactable)
		{
			OnCosmeticGoldClicked();
		}
		else if (_cosmeticGemsButton.gameObject.activeSelf && _cosmeticGemsButton.Interactable)
		{
			OnCosmeticGemsClicked();
		}
		else if (_selectButton.gameObject.activeSelf && _selectButton.Interactable)
		{
			OnSelectClicked();
		}
		else if (_storeButton.gameObject.activeSelf && _storeButton.Interactable)
		{
			OnStoreClicked();
		}
	}

	public override void OnEscape()
	{
		OnCancelClicked();
	}

	private void OnCancelClicked()
	{
		if (!_transactionBlocker.activeSelf)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_cancel, base.gameObject);
			Activate(activate: false);
		}
	}

	private void OnCraftClicked()
	{
		if (_transactionBlocker.activeSelf)
		{
			return;
		}
		_transactionBlocker.SetActive(value: true);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		_titleCountManager.OwnedTitleCounts.TryGetValue(_printingData.TitleId, out var value);
		int num = Math.Max(0, Math.Min(_requestedQuantity, 4) - value);
		if (num == 0 && _collectedQuantity == 0 && value >= _printingData.MaxCollected)
		{
			num = 1;
		}
		WildcardBulkRequest request = new WildcardBulkRequest(1);
		request.bulkRequest.Add(new CardAndQuantity((int)_printingData.GrpId, num));
		SetAvailability cardTitleAvailability = WrapperController.Instance.FormatManager.GetCardTitleAvailability(_printingData.TitleId, WrapperController.Instance.FormatManager.GetDefaultFormat());
		bool hasCardsBannedInDeckFormat = WrapperController.Instance.FormatManager.GetDefaultFormat().IsCardBanned(_printingData.TitleId);
		bool willBeOverRestrictedLimit = false;
		if (WrapperController.Instance.CurrentContentType == NavContentType.DeckBuilder)
		{
			DeckFormat deckFormat = (WrapperController.Instance.SceneLoader.CurrentNavContent as WrapperDeckBuilder)?.GetContextFormat();
			if (deckFormat != null)
			{
				Dictionary<uint, Quota> restrictedTitleIds = deckFormat.RestrictedTitleIds;
				if (restrictedTitleIds != null && restrictedTitleIds.Count > 0 && deckFormat.RestrictedTitleIds.TryGetValue(_printingData.TitleId, out var value2))
				{
					willBeOverRestrictedLimit = _collectedQuantity + num > value2.Max;
				}
			}
		}
		Inv.HandlePurchaseAvailabilityWarnings(RotationWarningContext.CraftViewer, cardTitleAvailability, hasCardsBannedInDeckFormat, _locProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/Warning_Craft"), Purchase, Cancel, willBeOverRestrictedLimit);
		void Cancel()
		{
			_transactionBlocker.SetActive(value: false);
		}
		void Purchase()
		{
			StartCoroutine(Inv.Coroutine_RedeemWildcards(request));
		}
	}

	private void InventoryManager_OnRedeemWildcardsResponse(bool success)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (success && _printingData != null)
		{
			_AnimatorRoot.SetBool(_craft, value: true);
			switch (_printingData.Rarity)
			{
			case CardRarity.Land:
				if (_printingData.IsCraftableRarityLand)
				{
					ActivateCraftVFX(CardRarity.Common);
					AudioManager.PlayAudio("sfx_ui_new_crafting_common", base.gameObject);
				}
				break;
			case CardRarity.Common:
				ActivateCraftVFX(_printingData.Rarity);
				AudioManager.PlayAudio("sfx_ui_new_crafting_common", base.gameObject);
				break;
			case CardRarity.Uncommon:
				ActivateCraftVFX(_printingData.Rarity);
				AudioManager.PlayAudio("sfx_ui_new_crafting_uncommon", base.gameObject);
				break;
			case CardRarity.Rare:
				ActivateCraftVFX(_printingData.Rarity);
				AudioManager.PlayAudio("sfx_ui_new_crafting_rare", base.gameObject);
				break;
			case CardRarity.MythicRare:
				ActivateCraftVFX(_printingData.Rarity);
				AudioManager.PlayAudio("sfx_ui_new_crafting_mythic", base.gameObject);
				break;
			}
		}
		else
		{
			_transactionBlocker.SetActive(value: false);
		}
	}

	public void OnCraftAnimationUpdateView()
	{
		_quantityToCraft = 1;
		OpenCraftMode();
	}

	public void OnCraftAnimationComplete()
	{
		_transactionBlocker.SetActive(value: false);
	}

	private void OnCosmeticGemsClicked()
	{
		if (_transactionBlocker.activeSelf)
		{
			return;
		}
		_transactionBlocker.SetActive(value: true);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		CardSelector cardSelector = _cosmeticSelectors[_currentSelector];
		int? gemPrice = cardSelector.GemPrice;
		if (gemPrice.GetValueOrDefault() > Inv.Inventory.gems)
		{
			Action nav = delegate
			{
				SceneLoader.GetSceneLoader().GoToStore(StoreTabType.Gems, "buy gems from card viewer");
			};
			SceneLoader.GetSceneLoader().SystemMessages.ShowMessage(_locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/NotEnoughGems_Title"), _locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/NotEnoughGems_Treatment_Message"), _locProvider.GetLocalizedText("MainNav/DeckBuilder/Cancel_Button"), delegate
			{
				_transactionBlocker.SetActive(value: false);
			}, _locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/NotEnoughGems_OKButton"), delegate
			{
				_onNav?.Invoke(nav);
				Activate(activate: false);
			});
		}
		else
		{
			StartCoroutine(TryPurchaseStyleYield(cardSelector.CardSkin, gemPrice, Client_PurchaseCurrencyType.Gem, "MainNav/CardViewer/Cosmetics/ConfirmPurchase_Treatment_Message"));
		}
	}

	private void OnCosmeticGoldClicked()
	{
		if (!_transactionBlocker.activeSelf)
		{
			_transactionBlocker.SetActive(value: true);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			CardSelector cardSelector = _cosmeticSelectors[_currentSelector];
			int? goldPrice = cardSelector.GoldPrice;
			StartCoroutine(TryPurchaseStyleYield(cardSelector.CardSkin, goldPrice, Client_PurchaseCurrencyType.Gold, "MainNav/CardViewer/Cosmetics/ConfirmPurchase_Treatment_Gold_Message"));
		}
	}

	private IEnumerator TryPurchaseStyleYield(ArtStyleEntry cardSkin, int? price, Client_PurchaseCurrencyType currency, string confirmMessageKey)
	{
		if (WrapperController.Instance.Store.StoreStatus.DisabledTags.Contains(EProductTag.CardStyle))
		{
			SceneLoader.GetSceneLoader().SystemMessages.ShowMessage(_locProvider.GetLocalizedText("MainNav/Store/Store_Unavailable_Title"), _locProvider.GetLocalizedText("MainNav/Store/Store_Unavailable"), _locProvider.GetLocalizedText("MainNav/Store/OK"), delegate
			{
				_transactionBlocker.SetActive(value: false);
			});
			yield break;
		}
		string message = _locProvider.GetLocalizedText(confirmMessageKey, ("quantity", price.GetValueOrDefault().ToString("N0")));
		SetAvailability cardArtAvailability = WrapperController.Instance.FormatManager.GetCardArtAvailability(_printingData.ArtId, WrapperController.Instance.FormatManager.GetDefaultFormat());
		bool hasCardsBannedInDeckFormat = WrapperController.Instance.FormatManager.GetDefaultFormat().IsCardBanned(_printingData.TitleId);
		Inv.HandlePurchaseAvailabilityWarnings(RotationWarningContext.StyleViewer, cardArtAvailability, hasCardsBannedInDeckFormat, _locProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/Warning_Purchase"), delegate
		{
			SceneLoader.GetSceneLoader().SystemMessages.ShowMessage(_locProvider.GetLocalizedText("MainNav/Store/Confirm_Purchase_Title"), message, _locProvider.GetLocalizedText("MainNav/DeckBuilder/Cancel_Button"), Cancel, _locProvider.GetLocalizedText("MainNav/CardViewer/Cosmetics/ConfirmPurchase_OKButton"), delegate
			{
				((Action)delegate
				{
					StartCoroutine(Inv.Coroutine_PurchaseSkin(cardSkin, _printingData.GrpId, currency));
				})();
			});
		}, Cancel);
		void Cancel()
		{
			_transactionBlocker.SetActive(value: false);
		}
	}

	private void InventoryManager_OnPurchaseSkinResponse(uint grpId, string variant, bool success)
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		if (success)
		{
			OpenCosmeticMode();
			if (_cosmeticPurchaseVFXRoutine != null)
			{
				StopCoroutine(_cosmeticPurchaseVFXRoutine);
				_cosmeticPurchaseParticles.SetActive(value: false);
			}
			_cosmeticPurchaseVFXRoutine = StartCoroutine(Coroutine_CosmeticPurchaseParticles());
		}
		_transactionBlocker.SetActive(value: false);
	}

	private void InventoryManager_InventoryUpdated()
	{
		Refresh_Currency();
	}

	private void Refresh_Currency()
	{
		if (Inv != null)
		{
			ClientPlayerInventory inventory = Inv.Inventory;
			_currencyGoldLabel.text = ((inventory.gold >= int.MaxValue) ? $"{int.MaxValue:N0}+" : inventory.gold.ToString("N0"));
			_currencyGemsLabel.text = ((inventory.gems >= int.MaxValue) ? $"{int.MaxValue:N0}+" : inventory.gems.ToString("N0"));
		}
	}

	private IEnumerator Coroutine_CosmeticPurchaseParticles()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_new_crafting_depthart_unlocked, base.gameObject);
		_cosmeticPurchaseParticles.SetActive(value: true);
		yield return new WaitForSeconds(2f);
		_cosmeticPurchaseParticles.SetActive(value: false);
		_cosmeticPurchaseVFXRoutine = null;
	}

	private void OnSelectClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		AudioManager.PlayAudio("sfx_ui_main_card_cosmetic_picker_select", base.gameObject);
		_onSelect?.Invoke(_cosmeticSelectors[_currentSelector].CardSkin?.Variant);
		Activate(activate: false);
	}

	private void OnStoreClicked()
	{
		CardSelector cardSelector = _cosmeticSelectors[_currentSelector];
		StoreTabType targetStore = StoreTabType.Bundles;
		if (cardSelector.CardSkin.Source.HasFlag(AcquisitionFlags.BattlePass))
		{
			targetStore = StoreTabType.Featured;
		}
		else if (cardSelector.CardSkin.StoreItem != null)
		{
			targetStore = cardSelector.CardSkin.StoreSection switch
			{
				EStoreSection.Packs => StoreTabType.Packs, 
				EStoreSection.Sale => StoreTabType.DailyDeals, 
				EStoreSection.Bundles => StoreTabType.Bundles, 
				EStoreSection.ProgressionTracks => StoreTabType.Featured, 
				EStoreSection.Cosmetics => StoreTabType.Cosmetics, 
				_ => targetStore, 
			};
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
		_onNav?.Invoke(Nav);
		Activate(activate: false);
		void Nav()
		{
			SceneLoader.GetSceneLoader().GoToStore(targetStore, "Card viewer store redirect");
		}
	}

	private void SetRequestedQuantity(int newRequested)
	{
		_titleCountManager.OwnedTitleCounts.TryGetValue(_printingData.TitleId, out var value);
		newRequested = Mathf.Clamp(newRequested, value + 1, 4);
		if (_requestedQuantity != newRequested)
		{
			_requestedQuantity = newRequested;
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			_AnimatorRoot.SetTrigger(_pop);
			RefreshCraftMode();
		}
	}

	private void Unity_OnCraftPipClicked(int pipIndex)
	{
		SetRequestedQuantity(pipIndex + 1);
	}

	public void Unity_OnCraftIncrease()
	{
		SetRequestedQuantity(_requestedQuantity + 1);
	}

	public void Unity_OnCraftDecrease()
	{
		SetRequestedQuantity(_requestedQuantity - 1);
	}

	private void UpdateIntroParticles(bool craftMode)
	{
		_craftIntroParticles.SetActive(craftMode);
		_cosmeticIntroParticles.SetActive(!craftMode);
	}

	private void OnCardStylesButtonClicked()
	{
		ExamineState examineState = _cardStyleStateToggle.FindNextExamineState();
		_cardStyleStateToggle.CurrentState = examineState;
		ICardDataAdapter cardDataAdapter = _modelConverter.ConvertModel(_craftCardData, examineState);
		ICardDataAdapter craftBonusCardData = _modelConverter.ConvertModel(_craftBonusCardData, examineState);
		if (cardDataAdapter != null)
		{
			SetCraftCardData(cardDataAdapter, craftBonusCardData);
			_cardStyleStateToggle.SetButtonText();
			_cardStyleStateToggle.ButtonCheckmarkOn(_cardStyleStateToggle.CurrentState != _cardStyleStateToggle.DefaultState);
		}
		else
		{
			_cardStyleStateToggle.gameObject.UpdateActive(active: false);
		}
	}

	public void OnLanguageChanged()
	{
		if (UsingCardStyleStateToggle)
		{
			_cardStyleStateToggle.SetButtonText();
		}
	}
}
