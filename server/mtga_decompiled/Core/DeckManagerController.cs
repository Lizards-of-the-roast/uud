using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Decks;
using Core.Code.Promises;
using Core.Shared.Code.CardFilters;
using Core.Shared.Code.Providers;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Decks.DeckValidation;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.DeckValidation.Client;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Arena.Enums.Deck;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.MDN.DeckManager;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Format;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

public class DeckManagerController : NavContentController
{
	private class DeckBucket
	{
		public readonly string LocalizedName;

		public readonly string CreatesFormat;

		public readonly string[] FormatNames;

		public readonly List<Client_Deck> Decks = new List<Client_Deck>();

		public DeckBucket(string localizedName, string createsFormat, params string[] formatNames)
		{
			LocalizedName = localizedName;
			CreatesFormat = createsFormat;
			FormatNames = formatNames;
		}
	}

	private class SortOption
	{
		public string LocKey;

		public DeckViewSortType SortType;
	}

	[SerializeField]
	private DeckFilterBar _deckColorFilterPrefab;

	[SerializeField]
	private Transform _deckFilterBarPrefab;

	[SerializeField]
	private Transform _deckSelectorParent;

	[SerializeField]
	private TMP_Text _deckCountText;

	[SerializeField]
	private CustomButton _deckDetailsButton;

	[SerializeField]
	private CustomButton _importDeckButton;

	[SerializeField]
	private CustomButton _exportDeckButton;

	[SerializeField]
	private CustomButton _deleteDeckButton;

	[SerializeField]
	private CustomButton _cloneDeckButton;

	[SerializeField]
	private CustomButton _favoriteDeckButton;

	[SerializeField]
	private CustomButton _craftAllButton;

	[SerializeField]
	private CustomButton _editDeckButton;

	[SerializeField]
	private CustomButton _collectionButton;

	[SerializeField]
	private TMP_Dropdown _deckBucketDropdown;

	[SerializeField]
	private CustomButton _deckBucketButton;

	[SerializeField]
	private TMP_Dropdown _deckOrderDropdown;

	[SerializeField]
	private CustomButton _deckOrderButton;

	[SerializeField]
	private Transform _popupRoot;

	private string _textFilter = "";

	private List<SortOption> _sortOptions = new List<SortOption>();

	private DeckViewSelector _deckSelectorInstance;

	private Coroutine _loadDecksCoroutine;

	private Coroutine _favoriteDeckCoroutine;

	private AssetLookupSystem _assetLookupSystem;

	private IBILogger _logger;

	private DecksManager _decksManager;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private FormatManager _formatManager;

	private DeckDetailsPopup _detailsPopup;

	private EventManager _eventManager;

	private CosmeticsProvider _cosmeticsProvider;

	private List<Client_Deck> _decks;

	private List<DeckViewInfo> _deckViewInfos;

	private Client_Deck _selectedDeck;

	private IPreferredPrintingDataProvider _preferredPrintingDataProvider;

	private ISetMetadataProvider _setMetadataProvider;

	private Promise<List<Client_Deck>> _getDecksRequest;

	private int _selectedBucket;

	private List<DeckBucket> _deckBuckets = new List<DeckBucket>();

	public override NavContentType NavContentType => NavContentType.DeckListViewer;

	public override bool IsReadyToShow => _getDecksRequest?.IsDone ?? false;

	private void Small_OnMouseover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	private void Big_OnMouseover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public DeckInfo GetSelectedDeckInfo()
	{
		if (_selectedDeck != null)
		{
			return DeckServiceWrapperHelpers.ToAzureModel(_selectedDeck);
		}
		return null;
	}

	private void Awake()
	{
		_deckColorFilterPrefab.FilterValueChanged += HeaderColorFilterValueChanged;
		_deckColorFilterPrefab.SearchTextChanged += Header_SearchTextChanged;
		_editDeckButton.OnMouseover.AddListener(Big_OnMouseover);
		_collectionButton.OnMouseover.AddListener(Big_OnMouseover);
		_deckDetailsButton.OnMouseover.AddListener(Small_OnMouseover);
		_deleteDeckButton.OnMouseover.AddListener(Small_OnMouseover);
		_exportDeckButton.OnMouseover.AddListener(Small_OnMouseover);
		_importDeckButton.OnMouseover.AddListener(Small_OnMouseover);
		_cloneDeckButton.OnMouseover.AddListener(Small_OnMouseover);
		_favoriteDeckButton.OnMouseover.AddListener(Small_OnMouseover);
		_craftAllButton.OnMouseover.AddListener(Small_OnMouseover);
		_deckOrderButton.OnMouseover.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
		});
		_deckOrderButton.OnClick.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_filter_toggle, AudioManager.Default);
		});
		_deckBucketButton.OnMouseover.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
		});
		_deckBucketButton.OnClick.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_filter_toggle, AudioManager.Default);
		});
		_preferredPrintingDataProvider = Pantry.Get<IPreferredPrintingDataProvider>();
		_sortOptions = new List<SortOption>
		{
			new SortOption
			{
				LocKey = "MainNav/DeckManager/DeckManager_SortBy_LastModified",
				SortType = DeckViewSortType.LastModified
			},
			new SortOption
			{
				LocKey = "MainNav/DeckManager/DeckManager_SortBy_LastPlayed",
				SortType = DeckViewSortType.LastPlayed
			},
			new SortOption
			{
				LocKey = "MainNav/DeckManager/DeckManager_SortBy_Alphabetical",
				SortType = DeckViewSortType.Alphabetical
			}
		};
		_deckOrderDropdown.options = _sortOptions.Select((SortOption so) => new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText(so.LocKey))).ToList();
		_deckOrderDropdown.value = 0;
		_deckOrderDropdown.onValueChanged.AddListener(DeckSortingDropdown_OnValueChanged);
	}

	private void OnEnable()
	{
		SceneLoader.GetSceneLoader().GetDeckBuilder()?.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		_deckColorFilterPrefab.FilterValueChanged -= HeaderColorFilterValueChanged;
		_deckColorFilterPrefab.SearchTextChanged -= Header_SearchTextChanged;
		_editDeckButton.OnMouseover.RemoveListener(Big_OnMouseover);
		_collectionButton.OnMouseover.RemoveListener(Big_OnMouseover);
		_deckDetailsButton.OnMouseover.RemoveListener(Small_OnMouseover);
		_deleteDeckButton.OnMouseover.RemoveListener(Small_OnMouseover);
		_exportDeckButton.OnMouseover.RemoveListener(Small_OnMouseover);
		_importDeckButton.OnMouseover.RemoveListener(Small_OnMouseover);
		_cloneDeckButton.OnMouseover.RemoveListener(Small_OnMouseover);
		_favoriteDeckButton.OnMouseover.RemoveListener(Small_OnMouseover);
		_craftAllButton.OnMouseover.RemoveListener(Small_OnMouseover);
		_deckOrderButton.OnMouseover.RemoveAllListeners();
		_deckOrderButton.OnClick.RemoveAllListeners();
		_deckBucketButton.OnMouseover.RemoveAllListeners();
		_deckBucketButton.OnClick.RemoveAllListeners();
		_deckSelectorInstance.ClearDecks();
		if (_loadDecksCoroutine != null)
		{
			StopCoroutine(_loadDecksCoroutine);
			_loadDecksCoroutine = null;
		}
		_getDecksRequest = null;
		Languages.LanguageChangedSignal.Listeners -= OnLocalize;
		DestroyInstantiatedControls();
	}

	public void Init(AssetLookupSystem assetLookupSystem, IBILogger logger, DecksManager decksManager, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, IClientLocProvider locManager, CosmeticsProvider cosmeticsProvider, AvatarCatalog avatarCatalog, PetCatalog petCatalog, ICardRolloverZoom zoomHandler, IEmoteDataProvider emoteDataProvider, IUnityObjectPool unityObjectPool, StoreManager storeManager, FormatManager formatManager, EventManager eventManager, ISetMetadataProvider setMetadataProvider)
	{
		_assetLookupSystem = assetLookupSystem;
		_logger = logger;
		_decksManager = decksManager;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_formatManager = formatManager;
		_cosmeticsProvider = cosmeticsProvider;
		_eventManager = eventManager;
		_setMetadataProvider = setMetadataProvider;
		string prefabPath = _assetLookupSystem.GetPrefabPath<DeckDetailsPrefab, DeckDetailsPopup>();
		_detailsPopup = AssetLoader.Instantiate<DeckDetailsPopup>(prefabPath, _popupRoot);
		_detailsPopup.Init(locManager, cosmeticsProvider, avatarCatalog, petCatalog, _assetLookupSystem, _decksManager, zoomHandler, _logger, _cardDatabase, _cardViewBuilder, emoteDataProvider, unityObjectPool, storeManager, isReadOnly: false);
		_detailsPopup.Activate(activate: false);
		_detailsPopup.OnAvatarSelected += DetailsPopup_OnAvatarSelected;
		_detailsPopup.OnSleeveSelected += DetailsPopup_OnSleeveSelected;
		_detailsPopup.OnPetSelected += DetailsPopup_OnPetSelected;
		_detailsPopup.OnDefaultCosmeticSelected += DetailsPopup_OnDefaultCosmeticSelected;
		_detailsPopup.OnEmotesSelected += DetailsPopup_OnEmotesSelected;
		_detailsPopup.OnNameChanged += DetailsPopup_OnNameChanged;
	}

	public override void Activate(bool active)
	{
		CreateInstantiatedControls();
		if (active)
		{
			_editDeckButton.gameObject.SetActive(value: true);
			_editDeckButton.Interactable = true;
			_collectionButton.gameObject.SetActive(value: true);
			_collectionButton.Interactable = true;
			_deckDetailsButton.gameObject.SetActive(value: true);
			_importDeckButton.gameObject.SetActive(value: true);
			_favoriteDeckButton.gameObject.SetActive(value: true);
			_craftAllButton.gameObject.SetActive(value: true);
			_exportDeckButton.gameObject.SetActive(value: true);
			_deleteDeckButton.gameObject.SetActive(value: true);
			_cloneDeckButton.gameObject.SetActive(value: true);
			OnLocalize();
			_cloneDeckButton.gameObject.SetActive(value: true);
			Languages.LanguageChangedSignal.Listeners += OnLocalize;
			_detailsPopup.gameObject.SetActive(value: false);
			_detailsPopup.SetDeckDetailsRequested(addListener: true);
			HandleCachedDeck();
			return;
		}
		_editDeckButton.gameObject.SetActive(value: false);
		_editDeckButton.Interactable = false;
		_collectionButton.gameObject.SetActive(value: false);
		_collectionButton.Interactable = false;
		_deckDetailsButton.gameObject.SetActive(value: false);
		_importDeckButton.gameObject.SetActive(value: false);
		_favoriteDeckButton.gameObject.SetActive(value: false);
		_craftAllButton.gameObject.SetActive(value: false);
		_exportDeckButton.gameObject.SetActive(value: false);
		_deleteDeckButton.gameObject.SetActive(value: false);
		_cloneDeckButton.gameObject.SetActive(value: false);
		Languages.LanguageChangedSignal.Listeners -= OnLocalize;
		_detailsPopup.SetDeckDetailsRequested(addListener: false);
		_deckSelectorInstance.ClearDecks();
		if (_loadDecksCoroutine != null)
		{
			StopCoroutine(_loadDecksCoroutine);
			_loadDecksCoroutine = null;
		}
		_getDecksRequest = null;
	}

	private void OnLocalize()
	{
		_favoriteDeckButton.SetText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/DeckManager_Top_Favorite"), warnOnMissingTextComponent: false);
		_deckOrderDropdown.onValueChanged.RemoveListener(DeckSortingDropdown_OnValueChanged);
		_deckOrderDropdown.options = _sortOptions.Select((SortOption so) => new TMP_Dropdown.OptionData(Languages.ActiveLocProvider.GetLocalizedText(so.LocKey))).ToList();
		_deckOrderDropdown.onValueChanged.AddListener(DeckSortingDropdown_OnValueChanged);
		_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, _selectedDeck?.Id));
	}

	private void CreateInstantiatedControls()
	{
		if (_deckSelectorInstance == null)
		{
			_assetLookupSystem.Blackboard.Clear();
			DeckViewSelectorPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<DeckViewSelectorPrefab>().GetPayload(_assetLookupSystem.Blackboard);
			_deckSelectorInstance = AssetLoader.Instantiate(payload.Prefab, _deckSelectorParent);
			_deckSelectorInstance.Initialize(DeckView_OnClick, DeckView_OnDoubleClick, DeckView_OnNameChanged);
			_deckSelectorInstance.SetSort(DeckViewSortType.LastModified);
			_deckSelectorInstance.GetComponent<RectTransform>().StretchToParent();
		}
		if (!base.gameObject)
		{
			return;
		}
		ScrollRect componentInChildren = _deckSelectorInstance.GetComponentInChildren<ScrollRect>();
		if (!componentInChildren)
		{
			return;
		}
		ScrollFade[] componentsInChildren = _deckFilterBarPrefab.GetComponentsInChildren<ScrollFade>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ScrollView = componentInChildren;
		}
		componentsInChildren = _deckColorFilterPrefab.GetComponentsInChildren<ScrollFade>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ScrollView = componentInChildren;
		}
		if (_deckCountText != null)
		{
			ScrollFade component = _deckCountText.GetComponent<ScrollFade>();
			if (component != null)
			{
				component.ScrollView = componentInChildren;
			}
		}
	}

	private void DestroyInstantiatedControls()
	{
		_deckOrderDropdown.onValueChanged.RemoveListener(DeckSortingDropdown_OnValueChanged);
	}

	private void HandleCachedDeck()
	{
		if (WrapperDeckBuilder.HasCachedDeck())
		{
			SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Resume_Edit_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Resume_Edit_Message"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Resume_Edit_Discard"), WrapperDeckBuilder.ClearCachedDeck, Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Resume_Edit_Resume"), OnResumeDraft);
		}
		void OnResumeDraft()
		{
			if (WrapperDeckBuilder.TryRestoreDeckFromCache(out var result, out var isFirstEdit))
			{
				Edit_GoToDeckBuilder(result, DeckBuilderMode.DeckBuilding, isFirstEdit);
			}
		}
	}

	private void OnCreateDeck()
	{
		if (!_decksManager.ShowDeckLimitError())
		{
			string createsFormat = _deckBuckets[_selectedBucket].CreatesFormat;
			Client_Deck deck = _formatManager.GetSafeFormat(createsFormat).NewDeck(_decksManager);
			New_GoToDeckBuilder(deck, FormatUtilities.IsAmbiguous(createsFormat));
		}
	}

	private void New_GoToDeckBuilder(Client_Deck deck, bool isAmbiguous)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_done, base.gameObject);
		DeckBuilderContext context = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(deck), null, sideboarding: false, firstEdit: true, DeckBuilderMode.DeckBuilding, isAmbiguous, default(Guid), null, null, null, cachingEnabled: true);
		SceneLoader.GetSceneLoader().GoToDeckBuilder(context);
	}

	private void CraftAll_OnClick()
	{
		if (_selectedDeck == null)
		{
			return;
		}
		InventoryManager inv = WrapperController.Instance.InventoryManager;
		if (inv.CurrentPurchaseMode == InventoryPurchaseMode.None)
		{
			IEnumerator routine = _decksManager.GetFullDeck(_selectedDeck.Id).ThenOnMainThreadIfSuccess(delegate(Client_Deck result)
			{
				CraftAll_OnFullDeckSuccess(result, _cardDatabase, inv, CraftAll_PurchaseCallback);
			}).AsCoroutine()
				.WithLoadingIndicator();
			StartCoroutine(routine);
		}
	}

	private void CraftAll_OnFullDeckSuccess(Client_Deck deck, CardDatabase cardDB, InventoryManager inv, System.Action purchaseCallback)
	{
		DeckInfo deckInfo = new DeckInfo();
		deckInfo.UpdateWith(deck.Contents);
		deckInfo.format = deck.Summary.Format;
		DeckBuilderModel deckBuilderModel = new DeckBuilderModel(cardDB, deckInfo, WrapperController.Instance.InventoryManager.Cards.ToDictionary((KeyValuePair<uint, int> x) => x.Key, (KeyValuePair<uint, int> x) => (uint)x.Value), isConstructed: true, isSideboarding: false, (uint)_formatManager.GetDefaultFormat().MaxCardsByTitle);
		DeckBuilderUtilities.CraftAll(_cardDatabase, deckBuilderModel.GetCardsNeededToFinishDeck(), inv, _formatManager, _cardDatabase.GreLocProvider, SystemMessageManager.Instance, deckBuilderModel, _setMetadataProvider, purchaseCallback);
	}

	private void CraftAll_PurchaseCallback()
	{
		_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, _selectedDeck?.Id));
	}

	private void DeckDetails_OnClick()
	{
		ShowDeckDetails(_selectedDeck);
	}

	private void ShowDeckDetails(Client_Deck deckInfo)
	{
		DeckViewInfo deckViewInfo = _deckViewInfos.Find(deckInfo.Id, (DeckViewInfo dvi, Guid paramDeckInfoId) => dvi.deckId == paramDeckInfoId);
		List<CardPrintingQuantity> deck = deckInfo.Contents.Piles[EDeckPile.Main].Select((Client_DeckCard dc) => new CardPrintingQuantity
		{
			Printing = _cardDatabase.CardDataProvider.GetCardPrintingById(dc.Id),
			Quantity = dc.Quantity
		}).ToList();
		_detailsPopup.SetDeck(deck, _cardDatabase.GreLocProvider);
		_detailsPopup.SetDeckName(deckInfo.Summary.Name);
		DeckBuilderMode deckBuilderMode = (deckViewInfo.IsNetDeck ? DeckBuilderMode.ReadOnly : DeckBuilderMode.DeckBuilding);
		bool flag = deckBuilderMode == DeckBuilderMode.ReadOnly;
		DeckBuilderContext context = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(_selectedDeck), null, sideboarding: false, firstEdit: false, deckBuilderMode, ambiguousFormat: false, default(Guid), null, null, null, cachingEnabled: true);
		Pantry.Get<DeckBuilderContextProvider>().Context = context;
		_detailsPopup.SetCosmeticsData(deckInfo, flag);
		_detailsPopup.SetDeckBoxSleeve(deckInfo.Summary.CardBack, _cardDatabase, _cardViewBuilder);
		DeckFormat safeFormat = WrapperController.Instance.FormatManager.GetSafeFormat(deckInfo.Summary.Format);
		List<DeckFormat> availableFormats = DeckBuilderWidgetUtilities.GetAvailableFormats(_formatManager.GetAllFormats(), _eventManager.EventContexts, safeFormat);
		_detailsPopup.SetSelectableFormat(safeFormat, availableFormats);
		_detailsPopup.Activate(activate: true);
		_detailsPopup.SetInteractable(!flag);
	}

	private void Favorite_OnClick()
	{
		if (_favoriteDeckCoroutine != null)
		{
			StopCoroutine(_favoriteDeckCoroutine);
			_favoriteDeckCoroutine = null;
		}
		_favoriteDeckCoroutine = StartCoroutine(Coroutine_FavoriteDeck());
	}

	private IEnumerator Coroutine_FavoriteDeck()
	{
		if (_selectedDeck != null)
		{
			Promise<Client_Deck> request = _decksManager.GetFullDeck(_selectedDeck.Id);
			yield return request.AsCoroutine();
			if (request.Successful)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
				WrapperDeckUtilities.setFavorite(_selectedDeck, !_selectedDeck.Summary.IsFavorite);
				SelectDeckBucket(_deckBucketDropdown.value, _selectedDeck.Id);
				_favoriteDeckCoroutine = null;
			}
		}
	}

	private void Collection_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		SceneLoader.GetSceneLoader().GoToDeckBuilder(new DeckBuilderContext());
	}

	private void Delete_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		if (_selectedDeck != null)
		{
			Guid idCapture = _selectedDeck.Id;
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/DeckManager_MessageTitle_DeleteDeck"), string.Format(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/DeckManager_MessageContent_DeleteDeck"), _selectedDeck.Summary.Name), showCancel: true, delegate
			{
				SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
				_decksManager.DeleteDeck(idCapture).ThenOnMainThread(OnDeckDeleteComplete);
			}, null, 30);
		}
	}

	private void Import_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		if (_decksManager.ShowDeckLimitError())
		{
			return;
		}
		string systemCopyBuffer = GUIUtility.systemCopyBuffer;
		if (WrapperDeckUtilities.TryImportDeck(systemCopyBuffer, _cardDatabase, _setMetadataProvider, Languages.ActiveLocProvider, WrapperController.Instance.InventoryManager.Cards, Languages.CurrentLanguage, out var deck, out var errorMessage))
		{
			deck.Summary.DeckId = Guid.NewGuid();
			deck = WrapperDeckUtilities.UpdateDeckWithPreferredPrintings(deck, _preferredPrintingDataProvider, _cardDatabase, _cosmeticsProvider);
			Dictionary<EDeckPile, List<Client_DeckCard>> piles = deck.Contents.Piles;
			if (piles[EDeckPile.Companions].Count > 0)
			{
				bool flag = false;
				foreach (Client_DeckCard item in piles[EDeckPile.Sideboard])
				{
					if (item.Id == piles[EDeckPile.Companions][0].Id)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					piles[EDeckPile.Sideboard].Add(new Client_DeckCard(piles[EDeckPile.Companions][0].Id, 1u));
				}
			}
			deck.Summary.Name = WrapperDeckUtilities.GetUniqueName(deck.Summary.Name, Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Imported_Deck"), _decksManager.GetAllDeckNames());
			if (piles[EDeckPile.CommandZone].Count > 0)
			{
				deck.Summary.DeckTileId = piles[EDeckPile.CommandZone][0].Id;
			}
			else if (piles[EDeckPile.Companions].Count > 0)
			{
				deck.Summary.DeckTileId = piles[EDeckPile.Companions][0].Id;
			}
			else if (piles[EDeckPile.Main].Count > 0)
			{
				deck.Summary.DeckTileId = piles[EDeckPile.Main][0].Id;
			}
			else
			{
				deck.Summary.DeckTileId = piles[EDeckPile.Sideboard][0].Id;
			}
			deck.Summary.Format = _formatManager.GetDefaultFormat().FormatName;
			StartCoroutine(Coroutine_CreateDeck(deck, Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Import_Error_Title"), isImport: true, 0, DeckActionType.Imported));
		}
		else
		{
			string text = Languages.ActiveLocProvider.GetLocalizedTextForLanguage(errorMessage.Key, "en-US", errorMessage.Parameters.AsTuples());
			if (!string.IsNullOrEmpty(systemCopyBuffer))
			{
				text = text.Replace(systemCopyBuffer, "[ClipBoardContent]");
			}
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Import_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Import_Error_Text") + errorMessage, showCancel: false, null, null, -1, text);
			Debug.LogErrorFormat("DeckManager.ImportDeck: Failed for {0}", text);
		}
	}

	private void Export_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		if (_selectedDeck != null)
		{
			StartCoroutine(Coroutine_ExportDeck(_selectedDeck));
		}
	}

	private IEnumerator Coroutine_ExportDeck(Client_Deck deck)
	{
		Promise<Client_Deck> request = _decksManager.GetFullDeck(deck.Id);
		yield return request.AsCoroutine();
		if (request.Successful)
		{
			DeckInfo deckInfo = DeckServiceWrapperHelpers.ToAzureModel(request.Result);
			string text = (GUIUtility.systemCopyBuffer = WrapperDeckUtilities.ToExportString(deckInfo, Languages.ActiveLocProvider, _cardDatabase));
			DeckAction payload = new DeckAction
			{
				EventTime = DateTime.UtcNow,
				DeckId = deckInfo.id,
				DeckDetails = new DeckDetails
				{
					MainDeck = deckInfo.mainDeck?.Select((CardInDeck c) => c.Id).ToList(),
					SideBoard = deckInfo.sideboard?.Select((CardInDeck c) => c.Id).ToList(),
					CommandZoneGrpIds = deckInfo.commandZone?.Select((CardInDeck c) => c.Id).ToList(),
					Format = deckInfo.format,
					CardBack = deckInfo.cardBack,
					CardSkins = deckInfo.cardSkins?.Select((CardSkin c) => new CardSkinData(c.GrpId, c.CCV)).ToList()
				},
				ActionType = DeckActionType.Exported,
				CosmeticOnlyUpdate = _favoriteDeckButton,
				DeckAttributesChanged = false,
				FavoriteToggle = deckInfo.IsFavorite
			};
			_logger.Send(ClientBusinessEventType.DeckAction, payload);
			Debug.LogFormat("Exporting deck data to clipboard: {0}{1}", Environment.NewLine, text);
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Export_Deck_Text", ("deckName", deckInfo.name));
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Export_Deck_Title"), localizedText);
		}
	}

	private void Clone_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
		if (_selectedDeck != null && !_decksManager.ShowDeckLimitError())
		{
			StartCoroutine(Coroutine_CloneDeck(_selectedDeck));
		}
	}

	private IEnumerator Coroutine_CloneDeck(Client_Deck deck)
	{
		Promise<Client_Deck> request = _decksManager.GetFullDeck(deck.Id);
		yield return request.AsCoroutine();
		if (request.Successful)
		{
			Client_Deck client_Deck = new Client_Deck(request.Result);
			client_Deck.Summary.DeckId = Guid.NewGuid();
			client_Deck.Summary.Name = WrapperDeckUtilities.GetUniqueName(client_Deck.Summary.Name, _decksManager.GetAllDeckNames());
			client_Deck.Summary.Description = "";
			client_Deck.Summary.LastUpdated = DateTime.Now;
			StartCoroutine(Coroutine_CreateDeck(client_Deck, Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Clone_Error_Title"), isImport: false, _selectedBucket, DeckActionType.Cloned));
		}
	}

	private void Edit_OnClick()
	{
		if (_selectedDeck != null)
		{
			DeckBuilderMode dbm = (_deckViewInfos.Find(_selectedDeck.Id, (DeckViewInfo dvi, Guid paramSelectedDeckId) => dvi.deckId == paramSelectedDeckId).IsNetDeck ? DeckBuilderMode.ReadOnly : DeckBuilderMode.DeckBuilding);
			Edit_GoToDeckBuilder(DeckServiceWrapperHelpers.ToAzureModel(_selectedDeck), dbm);
		}
	}

	private void Edit_GoToDeckBuilder(DeckInfo deck, DeckBuilderMode dbm, bool isFirstEdit = false)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_done, base.gameObject);
		DeckBuilderContext context = new DeckBuilderContext(deck, null, sideboarding: false, isFirstEdit, dbm, ambiguousFormat: false, default(Guid), null, null, null, cachingEnabled: true);
		SceneLoader.GetSceneLoader().GoToDeckBuilder(context);
	}

	private void DeckView_OnClick(DeckViewInfo deckViewInfo)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deck_select, base.gameObject);
		if (deckViewInfo != null)
		{
			_selectedDeck = _decks.Find(deckViewInfo.deckId, (Client_Deck d, Guid paramDeckViewDeckId) => d.Id == paramDeckViewDeckId);
		}
		UpdateSelectedDeckView(deckViewInfo);
	}

	private void DeckView_OnDoubleClick(DeckViewInfo deckViewInfo)
	{
		Edit_OnClick();
	}

	private void OnDeckDeleteComplete(Promise<bool> request)
	{
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
		if (!request.Successful)
		{
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Deletion_Failure_Text"));
		}
		else
		{
			_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, null));
		}
	}

	private IEnumerator Coroutine_LoadDecks(int selectedBucket, Guid? selectedDeckId)
	{
		_getDecksRequest = _decksManager.GetAllDecks();
		if (!_getDecksRequest.IsDone)
		{
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
			yield return _getDecksRequest.AsCoroutine();
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
		}
		CreateDeckBuckets(_getDecksRequest.Result);
		SelectDeckBucket(selectedBucket, selectedDeckId);
		_loadDecksCoroutine = null;
	}

	private void DetailsPopup_OnFormatSelected(DeckFormat selectedFormat)
	{
		StartCoroutine(Coroutine_UpdateDeckFormat(selectedFormat));
	}

	private IEnumerator Coroutine_UpdateDeckFormat(DeckFormat selectedFormat)
	{
		Client_Deck deckInfo = _selectedDeck;
		string oldFormatName = deckInfo.Summary.Format;
		string formatName = selectedFormat.FormatName;
		if (oldFormatName != formatName)
		{
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
			deckInfo.Summary.Format = formatName;
			Promise<Client_DeckSummary> updateRequest = _decksManager.UpdateDeck(deckInfo, DeckActionType.Updated);
			yield return updateRequest.AsCoroutine();
			if (updateRequest.Successful)
			{
				_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, deckInfo.Id));
			}
			else
			{
				deckInfo.Summary.Format = oldFormatName;
				DeckFormat safeFormat = WrapperController.Instance.FormatManager.GetSafeFormat(deckInfo.Summary.Format);
				List<DeckFormat> availableFormats = DeckBuilderWidgetUtilities.GetAvailableFormats(_formatManager.GetAllFormats(), _eventManager.EventContexts, safeFormat);
				_detailsPopup.SetSelectableFormat(safeFormat, availableFormats);
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Update_Failure_Text"));
			}
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
		}
	}

	private void DetailsPopup_OnAvatarSelected(AvatarSelection selectedAvatar)
	{
		StartCoroutine(Coroutine_UpdateDeckAvatar(selectedAvatar.Id));
	}

	private IEnumerator Coroutine_UpdateDeckAvatar(string selectedAvatar)
	{
		Client_Deck deckInfo = _selectedDeck;
		string oldAvatar = deckInfo.Summary.Avatar;
		string text = selectedAvatar;
		if (text == _cosmeticsProvider.PlayerAvatarSelection)
		{
			text = null;
		}
		if (oldAvatar != text)
		{
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
			deckInfo.Summary.Avatar = text;
			Promise<Client_DeckSummary> updateRequest = _decksManager.UpdateDeck(deckInfo, DeckActionType.Updated);
			yield return updateRequest.AsCoroutine();
			if (updateRequest.Successful)
			{
				_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, _selectedDeck?.Id));
			}
			else
			{
				deckInfo.Summary.Avatar = oldAvatar;
				_detailsPopup.SetCosmeticsData(deckInfo);
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Update_Failure_Text"));
			}
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
		}
	}

	private void DetailsPopup_OnEmotesSelected(List<string> selectedEmotes)
	{
		StartCoroutine(Coroutine_UpdateDeckEmotes(selectedEmotes));
	}

	private IEnumerator Coroutine_UpdateDeckEmotes(List<string> selectedEmotes)
	{
		Client_Deck deckInfo = _selectedDeck;
		_ = deckInfo.Summary.Pet;
		List<string> oldEmotes = deckInfo.Summary.Emotes;
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
		deckInfo.Summary.Emotes = selectedEmotes;
		Promise<Client_DeckSummary> updateRequest = _decksManager.UpdateDeck(deckInfo, DeckActionType.Updated);
		yield return updateRequest.AsCoroutine();
		if (updateRequest.Successful)
		{
			_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, _selectedDeck?.Id));
		}
		else
		{
			deckInfo.Summary.Emotes = oldEmotes;
			_detailsPopup.SetCosmeticsData(deckInfo);
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Update_Failure_Text"));
		}
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
	}

	private void DetailsPopup_OnPetSelected(PetEntry selectedPetEntry)
	{
		StartCoroutine(Coroutine_UpdateDeckPet(selectedPetEntry));
	}

	private IEnumerator Coroutine_UpdateDeckPet(PetEntry selectedPetEntry)
	{
		string obj = ((selectedPetEntry == null) ? null : (selectedPetEntry.Name + "." + selectedPetEntry.Variant));
		Client_Deck deckInfo = _selectedDeck;
		string oldPet = deckInfo.Summary.Pet;
		string text = obj;
		if (oldPet != text)
		{
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
			deckInfo.Summary.Pet = text;
			Promise<Client_DeckSummary> updateRequest = _decksManager.UpdateDeck(deckInfo, DeckActionType.Updated);
			yield return updateRequest.AsCoroutine();
			if (updateRequest.Successful)
			{
				_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, _selectedDeck?.Id));
			}
			else
			{
				deckInfo.Summary.Pet = oldPet;
				_detailsPopup.SetCosmeticsData(deckInfo);
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Update_Failure_Text"));
			}
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
		}
	}

	private void DetailsPopup_OnSleeveSelected(string selectedSleeve)
	{
		StartCoroutine(Coroutine_UpdateDeckSleeve(selectedSleeve));
	}

	private IEnumerator Coroutine_UpdateDeckSleeve(string selectedSleeve)
	{
		Client_Deck deckInfo = _selectedDeck;
		string oldSleeve = deckInfo.Summary.CardBack;
		string text = selectedSleeve;
		if (text == _decksManager?.GetDefaultSleeve())
		{
			text = null;
		}
		if (oldSleeve != text)
		{
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
			deckInfo.Summary.CardBack = text;
			Promise<Client_DeckSummary> updateRequest = _decksManager.UpdateDeck(deckInfo, DeckActionType.Updated);
			yield return updateRequest.AsCoroutine();
			if (updateRequest.Successful)
			{
				_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, _selectedDeck?.Id));
			}
			else
			{
				deckInfo.Summary.CardBack = oldSleeve;
				_detailsPopup.SetDeckBoxSleeve(oldSleeve, _cardDatabase, _cardViewBuilder);
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Update_Failure_Text"));
			}
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
		}
	}

	private void DetailsPopup_OnDefaultCosmeticSelected(CosmeticType cosmeticType)
	{
		_loadDecksCoroutine = StartCoroutine(Coroutine_DetailsPopup_OnDefaultCosmeticSelected());
	}

	private IEnumerator Coroutine_DetailsPopup_OnDefaultCosmeticSelected()
	{
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
		_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, _selectedDeck?.Id));
		while (_loadDecksCoroutine != null)
		{
			yield return null;
		}
		_detailsPopup.SetCosmeticsData(_selectedDeck);
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
	}

	private void DetailsPopup_OnNameChanged(string newValue)
	{
		DeckViewInfo deckViewInfo = _deckViewInfos.Find(_selectedDeck.Id, (DeckViewInfo dvi, Guid paramSelectedDeckId) => dvi.deckId == paramSelectedDeckId);
		DeckView_OnNameChanged(deckViewInfo, newValue);
	}

	private void DeckView_OnNameChanged(DeckViewInfo deckViewInfo, string newValue)
	{
		StartCoroutine(Coroutine_UpdateDeckName(deckViewInfo, newValue));
	}

	private IEnumerator Coroutine_UpdateDeckName(DeckViewInfo deckViewInfo, string newValue)
	{
		Client_Deck deck = _decks.Find(deckViewInfo.deckId, (Client_Deck d, Guid deckViewInfoDeckId) => d.Id == deckViewInfoDeckId);
		string oldValue = deck.Summary.Name;
		if (DeckValidationUtils.ValidateDeckNameWithSystemMessages(deck.Id, newValue, delegate
		{
			SelectDeckBucket(_deckBucketDropdown.value, _selectedDeck?.Id);
			_detailsPopup.SetDeckName(oldValue);
		}, delegate(string adjustedName)
		{
			SelectDeckBucket(_deckBucketDropdown.value, _selectedDeck?.Id);
			_detailsPopup.SetDeckName(adjustedName);
			_detailsPopup.FocusDeckNameInput();
			StartCoroutine(Coroutine_UpdateDeckName(deckViewInfo, adjustedName));
		}, isConstructed: true) && newValue != oldValue)
		{
			Action<Client_Deck> handleUpdateFailure = delegate
			{
				SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
				SelectDeckBucket(_deckBucketDropdown.value, _selectedDeck?.Id);
				_detailsPopup.SetDeckName(oldValue);
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Update_Failure_Text"));
			};
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
			deck.Summary.Name = newValue;
			Promise<Client_DeckSummary> updateRequest = _decksManager.UpdateDeck(deck, DeckActionType.Updated);
			yield return updateRequest.AsCoroutine();
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
			if (updateRequest.Successful)
			{
				_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(_selectedBucket, _selectedDeck?.Id));
			}
			else
			{
				handleUpdateFailure(deck);
			}
		}
	}

	public void Back()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, base.gameObject);
		throw new NotImplementedException();
	}

	private void UpdateSelectedDeckView(DeckViewInfo deckViewInfo)
	{
		bool flag = deckViewInfo != null;
		DeckDisplayInfo deckDisplayInfo = null;
		if (flag)
		{
			deckDisplayInfo = deckViewInfo.GetValidationForFormat(null);
		}
		bool flag2 = flag && deckViewInfo.IsNetDeck;
		bool active = flag && deckDisplayInfo.IsCraftable;
		_editDeckButton.Interactable = flag;
		_deleteDeckButton.Interactable = flag && !flag2;
		_deckDetailsButton.Interactable = flag;
		_importDeckButton.Interactable = true;
		_exportDeckButton.Interactable = flag;
		_cloneDeckButton.Interactable = flag;
		_favoriteDeckButton.Interactable = flag && !deckViewInfo.IsNetDeck;
		_craftAllButton.gameObject.SetActive(active);
		_detailsPopup.SetInteractable(!flag2);
		string key = (flag2 ? "MainNav/DeckManager/Button_View" : "MainNav/DeckManager/Button_Edit");
		_editDeckButton.SetText(Languages.ActiveLocProvider.GetLocalizedText(key));
		if (flag)
		{
			string key2 = (deckViewInfo.isFavorite ? "MainNav/DeckManager/DeckManager_Top_Unfavorite" : "MainNav/DeckManager/DeckManager_Top_Favorite");
			_favoriteDeckButton.SetText(Languages.ActiveLocProvider.GetLocalizedText(key2), warnOnMissingTextComponent: false);
		}
	}

	private IEnumerator Coroutine_CreateDeck(Client_Deck deck, string errorTitle, bool isImport, int selectedBucket, DeckActionType action)
	{
		if (isImport)
		{
			Client_Deck client_Deck = new Client_Deck(deck);
			if (DeckFormatUtil.RemoveUnpublishedCards(client_Deck, _setMetadataProvider, _cardDatabase))
			{
				deck.Summary.Format = DeckFormatUtil.GetBestFormat(client_Deck, _formatManager, _cardDatabase);
			}
			else
			{
				deck.Summary.Format = DeckFormatUtil.GetBestFormat(deck, _formatManager, _cardDatabase);
			}
		}
		ClientSideDeckValidationResult clientSideDeckValidationResult = DeckValidationHelper.CalculateIsDeckLegal(_formatManager.GetSafeFormat(deck.Summary.Format), deck, _cardDatabase, Pantry.Get<IEmergencyCardBansProvider>(), Pantry.Get<ISetMetadataProvider>(), Pantry.Get<CosmeticsProvider>(), Pantry.Get<DesignerMetadataProvider>());
		if (!clientSideDeckValidationResult.CanSave)
		{
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_InvalidDeck");
			string message = string.Format(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageContent_InvalidDeck_CantSave").Replace("\\r\\n", Environment.NewLine), clientSideDeckValidationResult.GetInvalidReasons(Languages.ActiveLocProvider), Languages.ActiveLocProvider);
			SystemMessageManager.ShowSystemMessage(localizedText, message, showCancel: false, delegate
			{
			});
			yield break;
		}
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
		Promise<Client_DeckSummary> createRequest = _decksManager.CreateDeck(deck, action.ToString());
		yield return createRequest.AsCoroutine();
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
		if (createRequest.Successful)
		{
			_loadDecksCoroutine = StartCoroutine(Coroutine_LoadDecks(selectedBucket, createRequest.Result.DeckId));
			if (isImport)
			{
				SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Import_Deck_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Import_Deck_Text", ("name", deck.Summary.Name)));
			}
		}
		else
		{
			SystemMessageManager.ShowSystemMessage(errorTitle, Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Creation_Failure_Text"));
		}
	}

	private void DeckBucketDropdown_OnValueChanged(int value)
	{
		SelectDeckBucket(value, null);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
	}

	private void DeckSortingDropdown_OnValueChanged(int value)
	{
		SelectDeckBucket(_deckBucketDropdown.value, null);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
	}

	private void HeaderColorFilterValueChanged(CardFilterType filter, bool value)
	{
		SelectDeckBucket(_deckBucketDropdown.value, null);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
	}

	private void Header_SearchTextChanged(string value)
	{
		if (_textFilter != value)
		{
			_textFilter = value;
			SelectDeckBucket(_deckBucketDropdown.value, null);
		}
	}

	private void SelectDeckBucket(int index, Guid? selectedDeckId)
	{
		if (index >= _deckBuckets.Count)
		{
			index = 0;
		}
		_deckBucketDropdown.onValueChanged.RemoveListener(DeckBucketDropdown_OnValueChanged);
		_deckBucketDropdown.value = index;
		_deckBucketDropdown.onValueChanged.AddListener(DeckBucketDropdown_OnValueChanged);
		_selectedBucket = index;
		_deckSelectorInstance.ClearDecks();
		List<ManaColor> filterColors = _deckColorFilterPrefab.GetFilterColors();
		_decks = new List<Client_Deck>();
		foreach (Client_Deck deck in _deckBuckets[index].Decks)
		{
			bool isLimited = FormatUtilities.IsLimited(WrapperController.Instance.FormatManager.GetSafeFormat(deck.Summary.Format).FormatType);
			HashSet<ManaColor> manaColors = deck.GetDeckColors(_cardDatabase.CardDataProvider, isLimited);
			if (filterColors.All((ManaColor manaColor) => manaColors.Contains(manaColor)) && deck.Summary.Name.ToUpper().Contains(_textFilter.ToUpper()))
			{
				_decks.Add(deck);
			}
		}
		if (_deckCountText != null)
		{
			int num = 0;
			foreach (Client_Deck deck2 in _deckBuckets[0].Decks)
			{
				if (!deck2.Summary.IsNetDeck)
				{
					num++;
				}
			}
			int num2 = 0;
			foreach (Client_Deck deck3 in _decks)
			{
				if (!deck3.Summary.IsNetDeck)
				{
					num2++;
				}
			}
			int deckLimit = _decksManager.GetDeckLimit();
			string text = string.Empty;
			if (num2 > deckLimit)
			{
				text = "#FF5555";
			}
			else if (num2 < num)
			{
				text = "#5555FF";
			}
			string arg = (string.IsNullOrEmpty(text) ? num2.ToString() : $"<color={text}>{num2}</color>");
			_deckCountText.text = $"{arg}/{deckLimit}";
		}
		DeckViewBuilder deckViewBuilder = Pantry.Get<DeckViewBuilder>();
		_deckViewInfos = deckViewBuilder.CreateDeckViewInfos(_decks);
		_deckSelectorInstance.SetSort(_sortOptions[_deckOrderDropdown.value].SortType);
		_deckSelectorInstance.SetDecks(_deckViewInfos, allowUnownedCards: false);
		_deckSelectorInstance.SetFormat(null, allowUnownedCards: false);
		_deckSelectorInstance.ShowCreateDeckButton(OnCreateDeck);
		_selectedDeck = _decks.FirstOrDefault(delegate(Client_Deck d)
		{
			Guid id = d.Id;
			Guid? guid = selectedDeckId;
			return id == guid;
		});
		DeckViewInfo deckViewInfo = null;
		if (_selectedDeck != null)
		{
			deckViewInfo = _deckViewInfos.Find(_selectedDeck.Id, (DeckViewInfo dvi, Guid paramSelectedDeckId) => dvi.deckId == paramSelectedDeckId);
			_deckSelectorInstance.SelectDeck(deckViewInfo);
		}
		UpdateSelectedDeckView(deckViewInfo);
	}

	private void CreateDeckBuckets(List<Client_Deck> decks)
	{
		DeckBucket deckBucket = new DeckBucket(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/AllDecks"), "Ambiguous");
		_deckBuckets = new List<DeckBucket>
		{
			deckBucket,
			new DeckBucket(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Standard"), "Standard", "Standard", "TraditionalStandard"),
			new DeckBucket(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Alchemy"), "Alchemy", "Alchemy", "TraditionalAlchemy"),
			new DeckBucket(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Explorer"), "Explorer", "Explorer", "TraditionalExplorer"),
			new DeckBucket(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Historic"), "Historic", "Historic", "TraditionalHistoric"),
			new DeckBucket(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/Timeless"), "Timeless", "Timeless", "TraditionalTimeless"),
			new DeckBucket(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckFormat/BrawlAny"), "Brawl", "Brawl", "HistoricBrawl", "DirectGameBrawlRebalanced", "DirectGameBrawl")
		};
		_deckBuckets.Sort(delegate(DeckBucket a, DeckBucket b)
		{
			DeckFormat safeFormat = _formatManager.GetSafeFormat(a.CreatesFormat);
			DeckFormat safeFormat2 = _formatManager.GetSafeFormat(b.CreatesFormat);
			return FormatUtilitiesClient.FormatSortOrderComparator(safeFormat, safeFormat2);
		});
		DeckBucket deckBucket2 = new DeckBucket(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/OtherDecks"), "Ambiguous");
		if (decks != null)
		{
			foreach (Client_Deck deck in decks)
			{
				DeckFormat format = _formatManager.GetSafeFormat(deck.Summary.Format);
				(_deckBuckets.FirstOrDefault((DeckBucket b) => ((IReadOnlyCollection<string>)(object)b.FormatNames).Contains(format.FormatName)) ?? deckBucket2).Decks.Add(deck);
				deckBucket.Decks.Add(deck);
			}
		}
		if (deckBucket2.Decks.Count > 0)
		{
			_deckBuckets.Add(deckBucket2);
		}
		_deckBucketDropdown.options = _deckBuckets.Select((DeckBucket b) => new TMP_Dropdown.OptionData(b.LocalizedName)).ToList();
	}
}
