using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Decks;
using Core.Code.Input;
using Core.Meta.MainNavigation.Store;
using Core.Meta.Shared;
using Core.Shared.Code;
using EventPage.Components.NetworkModels;
using GreClient.CardData;
using JetBrains.Annotations;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using SharedClientCore.SharedClientCore.Code.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Models;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.PreferredPrinting;
using Wizards.Unification.Models.Draft;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftContentController : NavContentController, IAltViewActionHandler
{
	private const float SecondsBeforeAutoPickHangarDisplay = 5f;

	private const float SecondsBetweenClicksForDoubleClick = 0.5f;

	[SerializeField]
	private NavContentLoadingView _loadingView;

	[SerializeField]
	private DraftPackHolder _draftPackHolder;

	[Header("List View Parameters")]
	[SerializeField]
	private DraftDeckView _deckListViewPrefab;

	[SerializeField]
	private Transform _deckListViewParent;

	[Header("Column View Parameters")]
	[SerializeField]
	private DraftDeckView _deckColumnViewPrefab;

	[SerializeField]
	private Transform _deckColumnViewParent;

	[Header("Prefab Parameters")]
	[SerializeField]
	private TableDraftPopupView _tableDraftPopupViewPrefab;

	[SerializeField]
	private DraftVaultProgress _vaultProgressPrefab;

	[Header("Other Parameters")]
	[SerializeField]
	private GameObject _sparksParticles;

	[SerializeField]
	private bool _isForceVertical;

	private DraftHeaderView[] _draftHeaderViews;

	private DraftDeckView _deckListView;

	private DraftDeckView _deckColumnView;

	private DraftDeckView _activeDeckView;

	private SceneLoader _sceneLoader;

	private IAccountClient _accountClient;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private FormatManager _formatManager;

	private InventoryManager _inventoryManager;

	private ITitleCountManager _titleCountManager;

	private IActionSystem _actionSystem;

	private CosmeticsProvider _cosmeticsProvider;

	private IPreferredPrintingDataProvider _preferredPrintingDataProvider;

	public Action<List<DraftPackCardView>> OnCardPicked;

	private bool _active;

	private Camera _camera;

	private CardCollection _packCollection;

	private DraftPackCardView _lastClickedCard;

	private readonly Dictionary<int, uint> _draftPackIndexToDraftPickGrpId = new Dictionary<int, uint>();

	private readonly Dictionary<int, string> _cosmeticInjectionCards = new Dictionary<int, string>();

	private readonly DictionaryList<DraftPackCardView, DeckBuilderPile?> _suggestedCards = new DictionaryList<DraftPackCardView, DeckBuilderPile?>();

	private DraftDeckManager _draftDeckManager;

	private DeckFormat _draftFormat;

	private Coroutine _layoutChangeCoroutine;

	private bool _isGettingInitialStatus;

	private EventContext _eventContext;

	private LimitedPlayerEvent _limitedEvent;

	private ICardRolloverZoom _zoomView;

	private Transform _popupsParent;

	private AspectRatio _lastAspectRatio;

	private TableDraftPopupView _tableDraftPopupView;

	private DeckDetailsPopup _deckDetailsPopup;

	private readonly Stopwatch _clickStopwatch = new Stopwatch();

	private AssetLookupSystem _assetLookupSystem;

	private ConnectionManager _connectionManager;

	private IEventsServiceWrapper _eventsServiceWrapper;

	private bool _showCollectionInfo;

	private bool _autoSelectTagsActive;

	private bool _exitWarningShown;

	private bool _okToPickCard;

	private float _cumulativeAutoPickTimers;

	private SystemMessageManager.SystemMessageHandle _autoPickPopup;

	private GlobalCoroutineExecutor _globalCoroutineExecutor;

	private DraftScreenUIData _draftData;

	private Coroutine _settingCardsCoroutine;

	[HideInInspector]
	public bool AutoPickCards;

	private static readonly Color MULTIPLEPRINTINGS_COLOR_VALUE = new Color(1f, 0.5f, 0f, 0.5f);

	private static readonly Color PREFERREDPRINTING_COLOR_VALUE = new Color(0.25f, 1f, 0.25f, 0.5f);

	private static readonly Color COSMETICINJECTION_COLOR_VALUE = new Color(0.5f, 0f, 0.5f, 0.5f);

	private bool _showDraftDebugOverlay;

	public override NavContentType NavContentType => NavContentType.Draft;

	private DictionaryList<DraftPackCardView, DeckBuilderPile?> CardsToAutopick => _draftDeckManager.GetReservedCards().FluentMerge(_suggestedCards).Take(DraftPod?.PickNumCardsToTake ?? 0)
		.ToDictionaryList();

	private DeckBuilderLayoutState DeckBuilderLayoutState => Pantry.Get<DeckBuilderLayoutState>();

	public IDraftPod DraftPod => _limitedEvent?.DraftPod;

	private bool AutoSelectTagsActive
	{
		get
		{
			return _autoSelectTagsActive;
		}
		set
		{
			_autoSelectTagsActive = value;
			RefreshPickHangars();
		}
	}

	public int NumberOfCardsCurrentlySelected => _draftDeckManager.ReservedCardCount();

	public DraftModes DraftMode => DraftPod.DraftMode;

	public bool UseColumnView
	{
		get
		{
			if (DeckBuilderLayoutState.LayoutInUse == DeckBuilderLayout.Column)
			{
				return !_isForceVertical;
			}
			return false;
		}
	}

	public override bool IsReadyToShow => !_isGettingInitialStatus;

	private static DeckFormat GetDraftFormat(FormatManager formatManager, EventContext ctxt)
	{
		return formatManager.GetSafeFormat(((LimitedPlayerEvent)ctxt.PlayerEvent).EventUXInfo.DeckSelectFormat);
	}

	public bool IsForceVertical()
	{
		return _isForceVertical;
	}

	private void Awake()
	{
		DraftPackHolder draftPackHolder = _draftPackHolder;
		draftPackHolder.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(draftPackHolder.OnCardClicked, new Action<MetaCardView>(HandleOnCardClicked));
		DraftPackHolder draftPackHolder2 = _draftPackHolder;
		draftPackHolder2.OnCardDragged = (Action<MetaCardView>)Delegate.Combine(draftPackHolder2.OnCardDragged, new Action<MetaCardView>(HandleOnCardDragged));
		_deckListView = UnityEngine.Object.Instantiate(_deckListViewPrefab, _deckListViewParent);
		_deckColumnView = UnityEngine.Object.Instantiate(_deckColumnViewPrefab, _deckColumnViewParent);
		DraftDeckView deckListView = _deckListView;
		deckListView.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(deckListView.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(HandleOnCardDropped));
		DraftDeckView deckListView2 = _deckListView;
		deckListView2.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(deckListView2.OnCardClicked, new Action<MetaCardView>(HandleOnHolderCardClicked));
		DraftDeckView deckColumnView = _deckColumnView;
		deckColumnView.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(deckColumnView.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(HandleOnCardDropped));
		DraftDeckView deckColumnView2 = _deckColumnView;
		deckColumnView2.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(deckColumnView2.OnCardClicked, new Action<MetaCardView>(HandleOnHolderCardClicked));
		DraftDeckView deckListView3 = _deckListView;
		deckListView3.OnConfirmClicked = (Action)Delegate.Combine(deckListView3.OnConfirmClicked, new Action(HandleOnConfirmPickButtonClicked));
		DraftDeckView deckColumnView3 = _deckColumnView;
		deckColumnView3.OnConfirmClicked = (Action)Delegate.Combine(deckColumnView3.OnConfirmClicked, new Action(HandleOnConfirmPickButtonClicked));
		_activeDeckView = (_deckListView.gameObject.activeInHierarchy ? _deckListView : _deckColumnView);
		if (_deckListView.ShowSideboardToggle != null)
		{
			_deckListView.ShowSideboardToggle.onValueChanged.AddListener(ShowSideboardToggle_OnValueChanged);
		}
		_draftHeaderViews = new DraftHeaderView[2];
		_draftHeaderViews[0] = _deckListView.DraftHeaderView;
		_draftHeaderViews[1] = _deckColumnView.DraftHeaderView;
		DraftHeaderView[] draftHeaderViews = _draftHeaderViews;
		for (int i = 0; i < draftHeaderViews.Length; i++)
		{
			draftHeaderViews[i].SetHeaderOnClickCallback(HandleOnStateHeaderClicked);
		}
		_eventsServiceWrapper = Pantry.Get<IEventsServiceWrapper>();
		_connectionManager = Pantry.Get<ConnectionManager>();
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Combine(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
		_globalCoroutineExecutor = Pantry.Get<GlobalCoroutineExecutor>();
	}

	private void Update()
	{
		if (_active && !_draftPackHolder.IsAnimating && AutoPickCards && _draftPackHolder.CardViews.Count > 0)
		{
			DictionaryList<DraftPackCardView, DeckBuilderPile?> pickedCards = ResolveSuggestedPicks(_draftPackHolder.CardViews, DraftPod.SuggestedCards);
			DraftCards(pickedCards, autoPicked: true);
		}
	}

	private static DictionaryList<DraftPackCardView, DeckBuilderPile?> ResolveSuggestedPicks<TSuggestions>(List<DraftPackCardView> candidates, TSuggestions suggestions) where TSuggestions : IReadOnlyList<int>
	{
		return suggestions.Aggregate((candidates.ToList(), new DictionaryList<DraftPackCardView, DeckBuilderPile?>()), delegate((List<DraftPackCardView> Candidates, DictionaryList<DraftPackCardView, DeckBuilderPile?> Taken) soFar, int nextSuggestion)
		{
			DraftPackCardView draftPackCardView = soFar.Candidates.FirstOrDefault((DraftPackCardView c) => c.Card.GrpId == nextSuggestion);
			if (draftPackCardView == null)
			{
				draftPackCardView = soFar.Candidates.FirstOrDefault();
			}
			if (draftPackCardView != null)
			{
				soFar.Candidates.Remove(draftPackCardView);
				soFar.Taken.Add(draftPackCardView, DeckBuilderPile.MainDeck);
			}
			return soFar;
		}).Taken;
	}

	private void OnDeckbuilderLayoutChanged(DeckBuilderLayout layout)
	{
		if (_layoutChangeCoroutine == null)
		{
			_layoutChangeCoroutine = StartCoroutine(DelayedUpdateDeckView());
		}
		RefreshPickHangars();
	}

	private void OnApplicationFocus(bool focus)
	{
		if (_showCollectionInfo)
		{
			UpdateCardCollectionInfo(show: false);
		}
	}

	private void OnEnable()
	{
		Languages.LanguageChangedSignal.Listeners += ResetLanguage;
		DeckBuilderLayoutState.OnLayoutChanged += OnDeckbuilderLayoutChanged;
	}

	private void OnDisable()
	{
		Languages.LanguageChangedSignal.Listeners -= ResetLanguage;
		DeckBuilderLayoutState.OnLayoutChanged -= OnDeckbuilderLayoutChanged;
		Pantry.ResetScope(Pantry.Scope.Deckbuilder);
		if (_layoutChangeCoroutine != null)
		{
			StopCoroutine(_layoutChangeCoroutine);
			_layoutChangeCoroutine = null;
		}
	}

	public void Init(SceneLoader sceneLoader, Camera camera, ICardRolloverZoom zoomView, Transform popupsParent, CardDatabase cardDatabase, IAccountClient accountClient, FormatManager formatManager, InventoryManager inventoryManager, ITitleCountManager titleCountManager, AssetLookupSystem assetLookupSystem, IActionSystem actionSystem, CosmeticsProvider cosmeticsProvider, IPreferredPrintingDataProvider preferredPrintingDataProvider, CardViewBuilder cardViewBuilder, StoreManager storeManager, CardBackCatalog cardBackCatalog, PetCatalog petCatalog, AvatarCatalog avatarCatalog, DecksManager decksManager, IBILogger biLogger, IEmoteDataProvider emoteDataProvider, IClientLocProvider locManager, IUnityObjectPool unityObjectPool)
	{
		_sceneLoader = sceneLoader;
		_camera = camera;
		_zoomView = zoomView;
		_popupsParent = popupsParent;
		_cardViewBuilder = cardViewBuilder;
		_cardDatabase = cardDatabase;
		_accountClient = accountClient;
		_formatManager = formatManager;
		_inventoryManager = inventoryManager;
		_titleCountManager = titleCountManager;
		_actionSystem = actionSystem;
		_assetLookupSystem = assetLookupSystem;
		_cosmeticsProvider = cosmeticsProvider;
		_preferredPrintingDataProvider = preferredPrintingDataProvider;
		_draftPackHolder.Init(camera, zoomView, this, _cardDatabase, _cardViewBuilder);
		_draftDeckManager = new DraftDeckManager(_cardDatabase);
		_deckListView.Init(zoomView, assetLookupSystem, _cardDatabase, _cardViewBuilder, this);
		_deckColumnView.Init(zoomView, assetLookupSystem, _cardDatabase, _cardViewBuilder, this);
		_tableDraftPopupView = UnityEngine.Object.Instantiate(_tableDraftPopupViewPrefab, _popupsParent);
		_tableDraftPopupView.transform.SetAsLastSibling();
		_tableDraftPopupView.gameObject.UpdateActive(active: false);
		string prefabPath = _assetLookupSystem.GetPrefabPath<DeckDetailsPrefab, DeckDetailsPopup>();
		_deckDetailsPopup = AssetLoader.Instantiate<DeckDetailsPopup>(prefabPath, _popupsParent);
		_deckDetailsPopup.Init(locManager, _cosmeticsProvider, avatarCatalog, petCatalog, _assetLookupSystem, decksManager, _zoomView, biLogger, _cardDatabase, _cardViewBuilder, emoteDataProvider, unityObjectPool, storeManager, isReadOnly: true);
		_deckDetailsPopup.transform.SetAsLastSibling();
		_deckDetailsPopup.Activate(activate: false);
		_deckDetailsPopup.DisableSleeveSelection();
		_deckDetailsPopup.SetInteractable(isInteractable: false);
	}

	private void OnFdReconnected()
	{
		if (!(DraftPod is HumanDraftPod) || DraftPod.DraftState != DraftState.Picking)
		{
			return;
		}
		_eventsServiceWrapper.TryRejoinHumanDraft(_eventContext.PlayerEvent.EventInfo.InternalEventName).Then(delegate(Promise<ICourseInfoWrapper> p)
		{
			if (DraftPod is HumanDraftPod humanDraftPod)
			{
				LimitedPlayerEvent.OnRejoinHumanDraft(_limitedEvent, humanDraftPod, p);
			}
		});
	}

	private void OnDraftTableInfoResetCallback(TableInfo tableInfo)
	{
		ReconcileDeckContentsWithPool(tableInfo.PickedCards.Select((int _) => (uint)_));
	}

	private void ReconcileDeckContentsWithPool(IEnumerable<uint> currentPool)
	{
		Deck deck = new Deck(_cardDatabase)
		{
			Name = Languages.ActiveLocProvider.GetLocalizedText("Draft/Draft_Deck_Title"),
			Main = ToCardCollection(new List<string>(), _preferredPrintingDataProvider, _cardDatabase, _sceneLoader.BILogger),
			Sideboard = ToCardCollection(new List<string>(), _preferredPrintingDataProvider, _cardDatabase, _sceneLoader.BILogger)
		};
		Dictionary<uint, int> dictionary = (from _ in deck.Main.Concat(deck.Sideboard)
			group _ by _.Card.GrpId).ToDictionary((IGrouping<uint, ICardCollectionItem> _) => _.Key, (IGrouping<uint, ICardCollectionItem> _) => _.Select((ICardCollectionItem c) => c.Quantity).Sum());
		Dictionary<uint, int> dictionary2 = (from _ in currentPool
			group _ by _).ToDictionary((IGrouping<uint, uint> _) => _.Key, (IGrouping<uint, uint> _) => _.Count());
		Dictionary<uint, int> dictionary3 = new Dictionary<uint, int>();
		Dictionary<uint, int> dictionary4 = new Dictionary<uint, int>();
		uint key;
		int value;
		foreach (KeyValuePair<uint, int> item in dictionary2)
		{
			item.Deconstruct(out key, out value);
			uint key2 = key;
			int num = value;
			dictionary.TryGetValue(key2, out var value2);
			if (num > value2)
			{
				int value3 = num - value2;
				dictionary3[key2] = value3;
			}
		}
		foreach (KeyValuePair<uint, int> item2 in dictionary)
		{
			item2.Deconstruct(out key, out value);
			uint key3 = key;
			int num2 = value;
			dictionary2.TryGetValue(key3, out var value4);
			if (num2 > value4)
			{
				int value5 = num2 - value4;
				dictionary4[key3] = value5;
			}
		}
		foreach (KeyValuePair<uint, int> item3 in dictionary4)
		{
			item3.Deconstruct(out key, out value);
			uint num3 = key;
			int num4 = value;
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(num3);
			value = deck.Main[num3]?.Quantity ?? 0;
			int num5 = deck.Sideboard[num3]?.Quantity ?? 0;
			int num6 = value;
			int num7 = num5;
			if (num7 > 0)
			{
				int num8 = Math.Min(num7, num4);
				num4 -= num8;
				deck.Sideboard.Add(cardPrintingById.ConvertToCardModel(), -num8);
			}
			if (num6 > 0 && num4 > 0)
			{
				deck.Main.Add(cardPrintingById.ConvertToCardModel(), -num4);
			}
		}
		foreach (KeyValuePair<uint, int> item4 in dictionary3)
		{
			item4.Deconstruct(out key, out value);
			uint id = key;
			int quantityToAdd = value;
			CardPrintingData cardPrintingById2 = _cardDatabase.CardDataProvider.GetCardPrintingById(id);
			deck.Main.Add(cardPrintingById2.ConvertToCardModel(), quantityToAdd);
		}
		_draftDeckManager.UpdateDeckFromServer(deck.Main, deck.Sideboard);
		if (dictionary3.Any() || dictionary4.Any())
		{
			UpdateActiveDeckVisual();
		}
	}

	public override void Activate(bool active)
	{
		if (_sparksParticles != null)
		{
			_sparksParticles.UpdateActive(active);
		}
		if (active)
		{
			HideDraftTimer();
			_cumulativeAutoPickTimers = 0f;
			IDraftPod draftPod = DraftPod;
			draftPod.OnDraftPacksUpdated = (Action<PickInfo>)Delegate.Combine(draftPod.OnDraftPacksUpdated, new Action<PickInfo>(OnDraftPacksUpdatedCallback));
			IDraftPod draftPod2 = DraftPod;
			draftPod2.OnDraftHeadersUpdated = (Action<DynamicDraftStateVisualData>)Delegate.Combine(draftPod2.OnDraftHeadersUpdated, new Action<DynamicDraftStateVisualData>(OnDraftHeadersUpdatedCallback));
			IDraftPod draftPod3 = DraftPod;
			draftPod3.OnDraftFinalized = (Action)Delegate.Combine(draftPod3.OnDraftFinalized, new Action(OnDraftFinalizedCallback));
			IDraftPod draftPod4 = DraftPod;
			draftPod4.OnPickedCardsUpdated = (Action<List<int>, Dictionary<uint, string>>)Delegate.Combine(draftPod4.OnPickedCardsUpdated, new Action<List<int>, Dictionary<uint, string>>(OnPickedCardsUpdatedCallback));
			IDraftPod draftPod5 = DraftPod;
			draftPod5.OnDraftTableInfoReset = (Action<TableInfo>)Delegate.Combine(draftPod5.OnDraftTableInfoReset, new Action<TableInfo>(OnDraftTableInfoResetCallback));
			_draftPackHolder.gameObject.UpdateActive(active: true);
			_draftPackHolder.RolloverZoomView = _zoomView;
			UpdateDeckView();
			_loadingView.ShowContent();
			DraftPackHolder draftPackHolder = _draftPackHolder;
			draftPackHolder.OnAnimatingCardsInStarted = (Action)Delegate.Combine(draftPackHolder.OnAnimatingCardsInStarted, new Action(UpdateWaitingOnPacksText));
			DraftPackHolder draftPackHolder2 = _draftPackHolder;
			draftPackHolder2.OnAnimatingCardsOutFinished = (Action)Delegate.Combine(draftPackHolder2.OnAnimatingCardsOutFinished, new Action(UpdateWaitingOnPacksText));
			DraftDeckView deckListView = _deckListView;
			deckListView.OnDeckDetailsButtonClicked = (Action)Delegate.Combine(deckListView.OnDeckDetailsButtonClicked, new Action(DetailsButton_OnClick));
			_deckListView.UpdateConfirmButtonText();
			DraftDeckView deckColumnView = _deckColumnView;
			deckColumnView.OnDeckDetailsButtonClicked = (Action)Delegate.Combine(deckColumnView.OnDeckDetailsButtonClicked, new Action(DetailsButton_OnClick));
			_deckColumnView.UpdateConfirmButtonText();
			_deckDetailsPopup.SetDeckDetailsRequested(addListener: true);
			_actionSystem.PushFocus(this);
			StartCoroutine(Coroutine_GetInitialStatus());
		}
		else
		{
			if (_autoPickPopup != null)
			{
				SystemMessageManager.Instance.Close(_autoPickPopup);
				_autoPickPopup = null;
			}
			_actionSystem.PopFocus(this);
			DraftPod.Cleanup();
			_limitedEvent = null;
			_draftDeckManager.Clear();
			_zoomView.Close();
			_clickStopwatch.Reset();
			DraftDeckView deckListView2 = _deckListView;
			deckListView2.OnDeckDetailsButtonClicked = (Action)Delegate.Remove(deckListView2.OnDeckDetailsButtonClicked, new Action(DetailsButton_OnClick));
			if (_deckColumnView != null)
			{
				DraftDeckView deckColumnView2 = _deckColumnView;
				deckColumnView2.OnDeckDetailsButtonClicked = (Action)Delegate.Remove(deckColumnView2.OnDeckDetailsButtonClicked, new Action(DetailsButton_OnClick));
			}
			_deckDetailsPopup.SetDeckDetailsRequested(addListener: false);
			DraftPackHolder draftPackHolder3 = _draftPackHolder;
			draftPackHolder3.OnAnimatingCardsInStarted = (Action)Delegate.Remove(draftPackHolder3.OnAnimatingCardsInStarted, new Action(UpdateWaitingOnPacksText));
			DraftPackHolder draftPackHolder4 = _draftPackHolder;
			draftPackHolder4.OnAnimatingCardsOutFinished = (Action)Delegate.Remove(draftPackHolder4.OnAnimatingCardsOutFinished, new Action(UpdateWaitingOnPacksText));
		}
		_active = active;
	}

	public override void OnFinishClose()
	{
		_draftPackHolder.ClearCards();
	}

	public override void OnNavBarScreenChange(Action screenChangeAction)
	{
		if (_exitWarningShown || DraftPod.DraftMode == DraftModes.BotDraft)
		{
			screenChangeAction();
			return;
		}
		SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("Draft/LeavingTable_Title"), Languages.ActiveLocProvider.GetLocalizedText("Draft/LeavingTable_Description"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_Cancel"), null, Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ClientPrompt/ClientPrompt_Button_Yes"), delegate
		{
			_exitWarningShown = true;
			screenChangeAction();
		});
	}

	public void SetDraftData(EventContext evt)
	{
		_limitedEvent = (LimitedPlayerEvent)evt.PlayerEvent;
		_draftDeckManager.SetNumCardsToPick(_limitedEvent.DraftPod.PickNumCardsToTake);
		_draftFormat = GetDraftFormat(_formatManager, evt);
		_draftData = evt.PlayerEvent.EventUXInfo.EventComponentData?.DraftScreenUI;
		_eventContext = evt;
		UpdateDeckBuilderContext();
	}

	private void UpdateDeckView()
	{
		_activeDeckView = (UseColumnView ? _deckColumnView : _deckListView);
		_deckListView.gameObject.UpdateActive(!UseColumnView);
		_deckColumnView.gameObject.UpdateActive(UseColumnView);
		_draftPackHolder.SetCardLayout(UseColumnView, _camera.GetAspectRatio());
		UpdateActiveDeckVisual();
	}

	private IEnumerator DelayedUpdateDeckView()
	{
		yield return new WaitUntil(() => !_draftPackHolder.IsAnimating);
		UpdateDeckView();
		_layoutChangeCoroutine = null;
	}

	public void OnOpenAltView()
	{
		if (!_showCollectionInfo && !_draftPackHolder.IsAnimating)
		{
			UpdateCardCollectionInfo(show: true);
		}
	}

	public void OnCloseAltView()
	{
		if (_showCollectionInfo)
		{
			UpdateCardCollectionInfo(show: false);
		}
	}

	private void HandleOnCardClicked(MetaCardView cardView)
	{
		if (!_draftPackHolder.IsAnimating && cardView is DraftPackCardView draftPackCardView)
		{
			bool num = draftPackCardView == _lastClickedCard && _clickStopwatch.IsRunning && _clickStopwatch.Elapsed.TotalSeconds < 0.5;
			_lastClickedCard = draftPackCardView;
			if (num)
			{
				_lastClickedCard = null;
				ReserveCardAndLockIn(draftPackCardView, null);
			}
			else
			{
				_clickStopwatch.Restart();
				ToggleCardReservation(draftPackCardView);
			}
		}
	}

	private void HandleOnHolderCardClicked(MetaCardView cardView)
	{
		List<MetaCardHolder> mainDeckMetaCardHolderList = _activeDeckView.GetMainDeckMetaCardHolderList();
		List<MetaCardHolder> sideboardMetaCardHolderList = _activeDeckView.GetSideboardMetaCardHolderList();
		bool flag = false;
		if (mainDeckMetaCardHolderList.Contains(cardView.Holder))
		{
			_draftDeckManager.MoveCardInDeck(cardView.Card, DeckBuilderPile.Sideboard);
			flag = true;
			UpdateActiveDeckVisual();
		}
		else if (sideboardMetaCardHolderList.Contains(cardView.Holder))
		{
			_draftDeckManager.MoveCardInDeck(cardView.Card, DeckBuilderPile.MainDeck);
			UpdateActiveDeckVisual();
			flag = true;
		}
		if (flag)
		{
			UpdateSideboardPrefs();
		}
	}

	private void HandleOnCardDragged(MetaCardView cardView)
	{
		ToggleCardReservation(cardView as DraftPackCardView);
	}

	private void HandleOnCardDropped(MetaCardView cardView, MetaCardHolder destination)
	{
		if (cardView == null)
		{
			return;
		}
		cardView.OnEndDrag(new PointerEventData(EventSystem.current));
		if (cardView is DraftPackCardView draftPackCardView)
		{
			draftPackCardView.Holder.RolloverZoomView.Close();
			if (cardView.Holder == _draftPackHolder && destination != _draftPackHolder)
			{
				ReserveCardAndLockIn(draftPackCardView, destination);
				return;
			}
		}
		DeckBuilderPile destinationPile = ((!_activeDeckView.GetMainDeckMetaCardHolderList().Contains(destination)) ? DeckBuilderPile.Sideboard : DeckBuilderPile.MainDeck);
		_draftDeckManager.MoveCardInDeck(cardView.Card, destinationPile);
		UpdateActiveDeckVisual();
		UpdateSideboardPrefs();
	}

	private void HandleOnStateHeaderClicked()
	{
		StartCoroutine(DraftPod.GetTableVisualData(delegate(DynamicDraftStateVisualData headerData, BustVisualData[] busts, PlayerBoosterVisualData[] playerBoosters)
		{
			_tableDraftPopupView.gameObject.UpdateActive(active: true);
			_tableDraftPopupView.InitTable(DraftPod, busts, _assetLookupSystem);
			_tableDraftPopupView.UpdateTable(headerData.PassDirectionIsLeft, playerBoosters);
			DraftHeaderView[] draftHeaderViews = _draftHeaderViews;
			for (int i = 0; i < draftHeaderViews.Length; i++)
			{
				draftHeaderViews[i].UpdateDraftState(headerData);
			}
		}, delegate(string errMsg)
		{
			PromiseExtensions.Logger.Error("HandleOnStateHeaderClicked Error: " + errMsg);
		}));
	}

	private void ShowSideboardToggle_OnValueChanged(bool value)
	{
		UpdateActiveDeckVisual();
	}

	private void HandleOnConfirmPickButtonClicked()
	{
		if (!_draftPackHolder.IsAnimating && _draftDeckManager.AtMaxReservedCards())
		{
			DraftCards(_draftDeckManager.GetReservedCards());
			if (_draftDeckManager.AnyReservedForSideboard())
			{
				UpdateSideboardPrefs();
			}
		}
	}

	private void DetailsButton_OnClick()
	{
		OpenDetailsPopup();
	}

	private void OpenDetailsPopup()
	{
		UpdateDeckBuilderContext();
		_deckDetailsPopup.UpdateDetailsPopup(isReadOnly: true);
		_deckDetailsPopup.Activate(activate: true);
	}

	private void UpdateDeckBuilderContext()
	{
		Pantry.Get<DeckBuilderContextProvider>().Context = GenerateDraftDeckBuilderContext(_formatManager, _eventContext, _draftDeckManager.GetDeck());
		Pantry.Get<DeckBuilderModelProvider>().ResetModel();
	}

	public static DeckBuilderContext GenerateDraftDeckBuilderContext(FormatManager formatManager, EventContext eventContext, Deck deck)
	{
		return new DeckBuilderContext(new DeckInfo
		{
			id = Guid.NewGuid(),
			name = (deck?.Name ?? string.Empty),
			format = GetDraftFormat(formatManager, eventContext).FormatName,
			deckTileId = (deck?.DeckTileId ?? 0),
			deckArtId = (deck?.DeckArtId ?? 0),
			lastUpdated = DateTime.UtcNow,
			mainDeck = (deck?.Main.Select((ICardCollectionItem cci) => new CardInDeck(cci.Card.GrpId, (uint)cci.Quantity)).ToList() ?? new List<CardInDeck>()),
			sideboard = (deck?.Sideboard.Select((ICardCollectionItem cci) => new CardInDeck(cci.Card.GrpId, (uint)cci.Quantity)).ToList() ?? new List<CardInDeck>())
		}, eventContext, sideboarding: false, firstEdit: false, DeckBuilderMode.DeckBuilding, ambiguousFormat: false, default(Guid), null, null, null, cachingEnabled: false, isPlayblade: false, null, isInvalidForEventFormat: false, null, isDrafting: true);
	}

	private void UpdateCardCollectionInfo(bool show)
	{
		foreach (DraftPackCardView cardView in _draftPackHolder.GetAllCardViews())
		{
			if (show)
			{
				_inventoryManager.Cards.TryGetValue(cardView.Card.GrpId, out var value);
				_titleCountManager.OwnedTitleCounts.TryGetValue(cardView.Card.TitleId, out var value2);
				Deck deck = _draftDeckManager.GetDeck();
				int num = deck.MainDeckIds.Count((uint id) => id == cardView.Card.GrpId);
				num += deck.SideboardIds.Count((uint id) => id == cardView.Card.GrpId);
				value += num;
				value2 += num;
				int maxCollected = (int)cardView.Card.Printing.MaxCollected;
				value2 = Math.Min(value2, maxCollected);
				value = Math.Min(value, maxCollected);
				int collected = ((value > 0) ? value2 : 0);
				cardView.CardView.ShowCollectionInfo(active: true, collected, maxCollected);
			}
			else
			{
				cardView.CardView.ShowCollectionInfo(active: false);
			}
		}
		_showCollectionInfo = show;
	}

	private void UpdateWaitingOnPacksText()
	{
		_deckListView.ActivateWaitingOnPacksText(!_draftPackHolder.IsAnimating && _draftPackHolder.CardViews.Count == 0);
		_deckColumnView.ActivateWaitingOnPacksText(!_draftPackHolder.IsAnimating && _draftPackHolder.CardViews.Count == 0);
	}

	private void UpdateSideboardPrefs()
	{
		Deck deck = _draftDeckManager.GetDeck();
		if (deck.Sideboard.Count > 0)
		{
			MDNPlayerPrefs.SetDraftDeckSideboardIds(_limitedEvent.CourseData.Id.ToString(), string.Join(",", deck.SideboardIds.Select((uint s) => s.ToString())));
		}
		else
		{
			MDNPlayerPrefs.DeleteDraftDeckSideboardIds(_limitedEvent.CourseData.Id.ToString());
		}
	}

	private void ToggleCardReservation(DraftPackCardView draftCardView)
	{
		DeckBuilderPile deckBuilderPile = (_activeDeckView.IsShowingSideboard() ? DeckBuilderPile.Sideboard : DeckBuilderPile.MainDeck);
		if (_draftDeckManager.IsCardAlreadyReserved(draftCardView))
		{
			_draftDeckManager.TryRemoveReservedCard(draftCardView);
			UpdateReservedCardsWithService();
		}
		else if (_draftDeckManager.TryAddReservedCard(draftCardView, deckBuilderPile))
		{
			UpdateReservedCardsWithService();
		}
		if (deckBuilderPile == DeckBuilderPile.Sideboard)
		{
			UpdateSideboardPrefs();
		}
		draftCardView.Holder.RolloverZoomView.Close();
		_deckListView.UpdateConfirmButtonText();
		_deckColumnView.UpdateConfirmButtonText();
		bool confirmPickInteractable = _draftDeckManager.AtMaxReservedCards();
		_deckListView.SetConfirmPickInteractable(confirmPickInteractable);
		_deckColumnView.SetConfirmPickInteractable(confirmPickInteractable);
		RefreshPickHangars();
	}

	private void ReserveCardAndLockIn(DraftPackCardView draftCardView, MetaCardHolder destination)
	{
		List<MetaCardHolder> mainDeckMetaCardHolderList = _activeDeckView.GetMainDeckMetaCardHolderList();
		List<MetaCardHolder> sideboardMetaCardHolderList = _activeDeckView.GetSideboardMetaCardHolderList();
		DeckBuilderPile deckBuilderPile = (DeckBuilderPile)(((int?)(mainDeckMetaCardHolderList.Contains(destination) ? new DeckBuilderPile?(DeckBuilderPile.MainDeck) : (sideboardMetaCardHolderList.Contains(destination) ? new DeckBuilderPile?(DeckBuilderPile.Sideboard) : ((DeckBuilderPile?)null)))) ?? (_activeDeckView.IsShowingSideboard() ? 1 : 0));
		if (_draftDeckManager.IsCardAlreadyReserved(draftCardView))
		{
			_draftDeckManager.UpdateLockReservation(draftCardView, lockedIn: true);
			_draftDeckManager.UpdateReservedDestination(draftCardView, deckBuilderPile);
		}
		else
		{
			if (!_draftDeckManager.TryAddReservedCard(draftCardView, deckBuilderPile, lockedIn: true))
			{
				return;
			}
			UpdateReservedCardsWithService();
		}
		if (deckBuilderPile == DeckBuilderPile.Sideboard)
		{
			UpdateSideboardPrefs();
		}
		bool flag = _draftDeckManager.AtMaxReservedCards();
		if (flag && _draftDeckManager.AllReservedCardsLocked())
		{
			DraftCards(_draftDeckManager.GetReservedCards());
			return;
		}
		_deckListView.UpdateConfirmButtonText();
		_deckColumnView.UpdateConfirmButtonText();
		_deckListView.SetConfirmPickInteractable(flag);
		_deckColumnView.SetConfirmPickInteractable(flag);
		UpdateActiveDeckVisual(draftCardView, hideDraftedCardFromPool: true);
		RefreshPickHangars();
	}

	private void RefreshPickHangars()
	{
		DictionaryList<DraftPackCardView, DeckBuilderPile?> cardsToAutopick = CardsToAutopick;
		DictionaryList<DraftPackCardView, DeckBuilderPile?> reservedCards = _draftDeckManager.GetReservedCards();
		foreach (DraftPackCardView cardView in _draftPackHolder.CardViews)
		{
			bool flag = !cardView.IsDragDetected && cardsToAutopick.ContainsKey(cardView);
			bool flag2 = reservedCards.ContainsKey(cardView);
			cardView.ActivatePickTag(flag2 && !cardView.UseButtonOverlay);
			cardView.ActivateAutoSelectTag(!flag2 && !cardView.UseButtonOverlay && flag && AutoSelectTagsActive);
		}
	}

	private void ResetPickHangars()
	{
		foreach (DraftPackCardView cardView in _draftPackHolder.CardViews)
		{
			cardView.ActivatePickTag(activate: false);
			cardView.ActivateAutoSelectTag(activate: false);
		}
	}

	private void ReserveCardsWithService()
	{
		List<int> list = _draftDeckManager.GetReservedCards().Keys.Select((DraftPackCardView cv) => (int)cv.Card.GrpId).ToList();
		if (list.Count > 0)
		{
			StartCoroutine(DraftPod.ReserveCards(list, null));
		}
	}

	private void UpdateReservedCardsWithService()
	{
		StartCoroutine(DraftPod.ClearReservedCards(delegate(bool success)
		{
			if (success)
			{
				ReserveCardsWithService();
			}
		}));
	}

	private void ClearReservedCards()
	{
		RefreshPickHangars();
		_deckListView.SetConfirmPickInteractable(value: false);
		_deckColumnView.SetConfirmPickInteractable(value: false);
	}

	private void DraftCards(DictionaryList<DraftPackCardView, DeckBuilderPile?> pickedCards, bool autoPicked = false)
	{
		if (!_okToPickCard || pickedCards == null || pickedCards.Count == 0)
		{
			return;
		}
		_okToPickCard = false;
		DictionaryList<DraftPackCardView, DeckBuilderPile?> pickedCards2 = pickedCards.ToDictionaryList((KeyValuePair<DraftPackCardView, DeckBuilderPile?> pair) => pair.Key, (KeyValuePair<DraftPackCardView, DeckBuilderPile?> pair) => pair.Value);
		List<DraftPackCardView> list = pickedCards.Keys.ToList();
		list.FirstOrDefault()?.Holder.RolloverZoomView.Close();
		foreach (DraftPackCardView item in list)
		{
			item.StartZoom();
		}
		UpdateCardCollectionInfo(show: false);
		UpdateActiveDeckVisual();
		_activeDeckView.OnCardPicked();
		HideDraftTimer();
		ClearReservedCards();
		_suggestedCards.Clear();
		OnCardPicked?.Invoke(list);
		ResetPickHangars();
		StartCoroutine(Coroutine_MakePick(pickedCards2, autoPicked));
	}

	private void UpdateActiveDeckVisual([CanBeNull] DraftPackCardView draftedCard = null, bool hideDraftedCardFromPool = false)
	{
		if (hideDraftedCardFromPool && draftedCard != null)
		{
			draftedCard.UndoAction = (Action<DraftPackCardView>)Delegate.Combine(draftedCard.UndoAction, new Action<DraftPackCardView>(UndoReserveLockCardPick));
			draftedCard.ActivateUndoButton(activate: true);
		}
		Deck deckForVisuals = _draftDeckManager.GetDeckForVisuals();
		_activeDeckView.UpdateDeckVisual(deckForVisuals, _draftFormat);
	}

	private void UndoReserveLockCardPick(DraftPackCardView draftedCard)
	{
		if (_draftDeckManager.TryRemoveReservedCard(draftedCard))
		{
			draftedCard.ActivateUndoButton(activate: false);
			draftedCard.UndoAction = (Action<DraftPackCardView>)Delegate.Remove(draftedCard.UndoAction, new Action<DraftPackCardView>(UndoReserveLockCardPick));
			UpdateActiveDeckVisual();
			RefreshPickHangars();
			_deckListView.UpdateConfirmButtonText();
			_deckColumnView.UpdateConfirmButtonText();
			_deckListView.SetConfirmPickInteractable(value: false);
			_deckColumnView.SetConfirmPickInteractable(value: false);
		}
	}

	private void HideDraftTimer()
	{
		_deckListView.ShowDraftTimer = false;
		_deckColumnView.ShowDraftTimer = false;
		AutoSelectTagsActive = false;
	}

	private void ShowDraftTimer()
	{
		_deckListView.ShowDraftTimer = true;
		_deckColumnView.ShowDraftTimer = true;
		AutoSelectTagsActive = false;
	}

	private IEnumerator Coroutine_StartTimer()
	{
		IDraftPod draftPod = DraftPod;
		if (draftPod == null || draftPod.DraftMode != DraftModes.HumanDraft)
		{
			yield break;
		}
		ShowDraftTimer();
		DictionaryList<DraftPackCardView, DeckBuilderPile?> collection = ResolveSuggestedPicks(_draftPackHolder.CardViews, DraftPod.SuggestedCards);
		_suggestedCards.Clear();
		_suggestedCards.AddRange(collection);
		float secondsRemaining = DraftPod.PickSecondsRemaining;
		float secondsTotal = DraftPod.PickSecondsTotal;
		while (secondsRemaining > 0f && _okToPickCard)
		{
			_activeDeckView.UpdateDraftTimer(secondsRemaining, secondsTotal);
			if (secondsRemaining < 5f)
			{
				AutoSelectTagsActive = true;
			}
			secondsRemaining -= Time.unscaledDeltaTime;
			yield return null;
		}
		if (!_okToPickCard)
		{
			yield break;
		}
		bool flag = !_draftDeckManager.AtMaxReservedCards();
		if (flag)
		{
			_cumulativeAutoPickTimers += DraftPod.PickSecondsTotal;
			if (_cumulativeAutoPickTimers > (float?)_draftData?.TimeoutWarnSec)
			{
				ShowAutopickingDialog();
			}
		}
		DraftCards(CardsToAutopick, flag);
	}

	private void ShowAutopickingDialog()
	{
		if (_autoPickPopup == null)
		{
			_autoPickPopup = SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("Draft/Autopicking_Title"), Languages.ActiveLocProvider.GetLocalizedText("Draft/Autopicking_Description"), Languages.ActiveLocProvider.GetLocalizedText("Draft/Autopicking_Button"), delegate
			{
				_cumulativeAutoPickTimers = 0f;
				_autoPickPopup = null;
			});
		}
	}

	private void ResetLanguage()
	{
		foreach (DraftPackCardView allCardView in _draftPackHolder.GetAllCardViews())
		{
			allCardView.UpdateVisuals();
		}
		_deckListView.ResetLanguage();
		_deckColumnView.ResetLanguage();
	}

	private void OnDraftPacksUpdatedCallback(PickInfo pickInfo)
	{
		TaskbarFlash.Flash();
		_draftPackHolder.AnimateClockwise = pickInfo.PassDirection == EDirection.NEG;
		Dictionary<uint, string> dictionary = CosmeticsUtils.StylesStringToDictionary(pickInfo.PackStyles);
		_cosmeticInjectionCards.Clear();
		if (dictionary != null)
		{
			foreach (int packCard in pickInfo.PackCards)
			{
				CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById((uint)packCard);
				string value;
				if (cardPrintingById == null)
				{
					SimpleLog.LogError($"Bot Draft pack contained unknown grpID: {packCard}");
				}
				else if (dictionary.TryGetValue(cardPrintingById.ArtId, out value) && !string.IsNullOrEmpty(value))
				{
					_cosmeticInjectionCards[(int)cardPrintingById.GrpId] = value;
				}
			}
		}
		_packCollection = ToCardCollection(pickInfo.PackCards, _preferredPrintingDataProvider, _cardDatabase, _sceneLoader.BILogger, in _draftPackIndexToDraftPickGrpId, dictionary, preSortCollection: true);
		if (_settingCardsCoroutine != null)
		{
			StopCoroutine(_settingCardsCoroutine);
		}
		CardCollection packCollection = _packCollection;
		if (packCollection != null && packCollection.Count > 0)
		{
			_settingCardsCoroutine = _globalCoroutineExecutor.StartGlobalCoroutine(_draftPackHolder.Coroutine_SetCards(_packCollection, sortPackCollection: false, SettingCards_OnComplete));
		}
	}

	private void SettingCards_OnComplete()
	{
		_settingCardsCoroutine = null;
		UpdateDebugVisuals();
		_okToPickCard = true;
		_globalCoroutineExecutor.StartGlobalCoroutine(Coroutine_StartTimer());
	}

	private void OnDraftFinalizedCallback()
	{
		StartCoroutine(Coroutine_FinalizeDraft());
	}

	private void OnDraftHeadersUpdatedCallback(DynamicDraftStateVisualData dynamicData)
	{
		DraftHeaderView[] draftHeaderViews = _draftHeaderViews;
		for (int i = 0; i < draftHeaderViews.Length; i++)
		{
			draftHeaderViews[i].UpdateDraftState(dynamicData);
		}
	}

	private IEnumerator Coroutine_FinalizeDraft()
	{
		Deck deck = _draftDeckManager.GetDeck();
		List<CardInDeck> main = WrapperDeckUtilities.ToCardInDeckList(deck.Main);
		List<CardInDeck> sideboard = WrapperDeckUtilities.ToCardInDeckList(deck.Sideboard);
		List<CardSkin> skins = WrapperDeckUtilities.ExtractSkinsFromDeck(deck);
		Guid deckId = Guid.NewGuid();
		Client_Deck deck2 = new Client_Deck();
		deck2.UpdateWith(new Client_DeckSummary
		{
			DeckId = deckId,
			Name = deck.Name,
			Format = _draftFormat.FormatName,
			DeckTileId = deck.DeckTileId,
			DeckArtId = deck.DeckArtId
		});
		Client_DeckContents contents = new Client_DeckContents
		{
			Piles = 
			{
				[EDeckPile.Main] = DeckServiceWrapperHelpers.ToClientModel(main),
				[EDeckPile.Sideboard] = DeckServiceWrapperHelpers.ToClientModel(sideboard)
			},
			Skins = DeckServiceWrapperHelpers.ToClientModel(skins)
		};
		deck2.UpdateWith(contents);
		ClientInventoryUpdateReportItem inventoryUpdate = null;
		_inventoryManager.Subscribe(InventoryUpdateSource.ModifyPlayerInventory, GetInventoryUpdate);
		WrapperController.EnableLoadingIndicator(enabled: true);
		Promise<ICourseInfoWrapper> completeDraft = _limitedEvent.CompleteDraft();
		yield return completeDraft.AsCoroutine();
		if (!completeDraft.Successful)
		{
			if (completeDraft.ErrorSource != ErrorSource.Debounce)
			{
				_inventoryManager.UnSubscribe(InventoryUpdateSource.ModifyPlayerInventory, GetInventoryUpdate);
				WrapperController.EnableLoadingIndicator(enabled: false);
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Draft_Complete_Error_Text"));
			}
			yield break;
		}
		Stopwatch timeout = new Stopwatch();
		timeout.Start();
		yield return new WaitUntil(() => inventoryUpdate != null || timeout.Elapsed >= TimeSpan.FromSeconds(5.0));
		_inventoryManager.UnSubscribe(InventoryUpdateSource.ModifyPlayerInventory, GetInventoryUpdate);
		WrapperController.EnableLoadingIndicator(enabled: false);
		MDNPlayerPrefs.DeleteDraftDeckSideboardIds(_limitedEvent.CourseData.Id.ToString());
		if (!MDNPlayerPrefs.GetDraftHasSeenVaultPopup(_accountClient.AccountInformation?.PersonaID))
		{
			MDNPlayerPrefs.SetDraftHasSeenVaultPopup(_accountClient.AccountInformation?.PersonaID, value: true);
			DraftVaultProgress vaultProgressInstance = UnityEngine.Object.Instantiate(_vaultProgressPrefab, _popupsParent);
			yield return new WaitUntilAction(vaultProgressInstance.Button.OnClick);
			UnityEngine.Object.Destroy(vaultProgressInstance.gameObject);
		}
		if (inventoryUpdate != null && inventoryUpdate.delta.gemsDelta > 0)
		{
			ContentControllerRewards rewardsPanel = _sceneLoader.GetRewardsContentController();
			ClientInventoryUpdateReportItem t = new ClientInventoryUpdateReportItem
			{
				delta = 
				{
					gemsDelta = inventoryUpdate.delta.gemsDelta
				}
			};
			yield return rewardsPanel.AddAndDisplayRewardsCoroutine(t, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/GemCard/GemReward"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/GemCard/GemRewardDescription"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
			yield return new WaitUntil(() => !rewardsPanel.Visible);
		}
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
		List<CardInDeck> list = new List<CardInDeck>();
		list.AddRange(main);
		list.AddRange(sideboard);
		foreach (CardInDeck item in list)
		{
			if (dictionary.ContainsKey(item.Id))
			{
				dictionary[item.Id] += item.Quantity;
			}
			else
			{
				dictionary.Add(item.Id, item.Quantity);
			}
		}
		Dictionary<uint, string> dictionary2 = WrapperDeckUtilities.SkinsToOverrideLookup(skins);
		DeckInfo deck3 = DeckServiceWrapperHelpers.ToAzureModel(deck2);
		EventContext eventContext = _eventContext;
		Dictionary<uint, uint> cardPoolOverride = dictionary;
		Dictionary<uint, string> cardSkinOverride = dictionary2;
		DeckBuilderContext context = new DeckBuilderContext(deck3, eventContext, sideboarding: false, firstEdit: true, DeckBuilderMode.DeckBuilding, ambiguousFormat: false, default(Guid), cardPoolOverride, cardSkinOverride);
		_sceneLoader.GoToDeckBuilder(context);
		void GetInventoryUpdate(ClientInventoryUpdateReportItem update)
		{
			inventoryUpdate = update;
		}
	}

	private void OnPickedCardsUpdatedCallback(List<int> cards, Dictionary<uint, string> styles = null)
	{
		CardCollection cardCollection = ToCardCollection(cards, _preferredPrintingDataProvider, _cardDatabase, _sceneLoader.BILogger, (Dictionary<int, uint>)null, styles);
		string draftDeckSideboardIds = MDNPlayerPrefs.GetDraftDeckSideboardIds(_limitedEvent.CourseData.Id.ToString());
		CardCollection cardCollection2 = ToCardCollection(string.IsNullOrEmpty(draftDeckSideboardIds) ? new List<string>() : draftDeckSideboardIds.Split(',').ToList(), _preferredPrintingDataProvider, _cardDatabase, _sceneLoader.BILogger);
		cardCollection.Remove(cardCollection2);
		_draftDeckManager.UpdateDeckFromServer(cardCollection, cardCollection2);
		UpdateActiveDeckVisual();
	}

	private IEnumerator Coroutine_GetInitialStatus()
	{
		_isGettingInitialStatus = true;
		DraftHeaderView[] draftHeaderViews = _draftHeaderViews;
		for (int i = 0; i < draftHeaderViews.Length; i++)
		{
			draftHeaderViews[i].InitDraftState(DraftPod.InitialDraftStateVisualData, _assetLookupSystem);
		}
		WrapperController.EnableLoadingIndicator(enabled: true);
		yield return DraftPod.GetDraftStatus(delegate(bool success)
		{
			WrapperController.EnableLoadingIndicator(enabled: false);
			_isGettingInitialStatus = false;
			if (success)
			{
				if (DraftPod.DraftState == DraftState.Completing)
				{
					SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("Draft/DraftCompletedReconnect_Title"), Languages.ActiveLocProvider.GetLocalizedText("Draft/DraftCompletedReconnect_Description"), delegate
					{
						StartCoroutine(Coroutine_FinalizeDraft());
					});
				}
			}
			else
			{
				WrapperController.EnableLoadingIndicator(enabled: false);
				_isGettingInitialStatus = false;
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Draft_Status_Get_Error_Text"), delegate
				{
					_sceneLoader.GoToEventScreen(_eventContext);
				});
			}
		});
	}

	private IEnumerator Coroutine_MakePick(DictionaryList<DraftPackCardView, DeckBuilderPile?> pickedCards, bool autoPicked)
	{
		uint value;
		List<int> intendedPickGrpIds = pickedCards.Select((KeyValuePair<DraftPackCardView, DeckBuilderPile?> kvp) => (int)((!_draftPackIndexToDraftPickGrpId.TryGetValue(_draftPackHolder.CardViews.IndexOf(kvp.Key), out value)) ? kvp.Key.Card.GrpId : value)).ToList();
		yield return DraftPod.MakePick(intendedPickGrpIds, autoPicked, delegate(bool success)
		{
			if (success)
			{
				_draftDeckManager.CommitPicks(pickedCards);
				UpdateActiveDeckVisual();
				_deckListView.UpdateConfirmButtonText();
				_deckColumnView.UpdateConfirmButtonText();
			}
			else
			{
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Draft_Pick_Error_Text"), delegate
				{
					_sceneLoader.GoToEventScreen(_eventContext);
				});
			}
			if (DraftPod.DraftMode == DraftModes.HumanDraft && !success)
			{
				ClientPlayerDraftMismatchedPick mismatchedPickPayload = ((HumanDraftPod)DraftPod).GetMismatchedPickPayload();
				foreach (int item in intendedPickGrpIds)
				{
					mismatchedPickPayload.IntendedPickGrpId = (uint)item;
					_sceneLoader.BILogger.Send(ClientBusinessEventType.PlayerDraftMismatchedPick, mismatchedPickPayload);
				}
			}
		});
		if (autoPicked && _cumulativeAutoPickTimers > (float?)_draftData?.TimeoutDiscoSec)
		{
			_sceneLoader.GoToLanding(new HomePageContext
			{
				AFKDraft = _eventContext.PlayerEvent.EventInfo.InternalEventName
			});
		}
	}

	private static CardCollection ToCardCollection(List<int> grpIdList, IPreferredPrintingDataProvider preferredPrintingDataProvider, CardDatabase cardDatabase, IBILogger biLogger, in Dictionary<int, uint> preferredPrintingMap, Dictionary<uint, string> styles = null, bool preSortCollection = false)
	{
		preferredPrintingMap?.Clear();
		CardCollection cardCollection = new CardCollection(cardDatabase, grpIdList?.Count ?? 0);
		List<CardPrintingData> list = new List<CardPrintingData>();
		if (grpIdList != null)
		{
			foreach (int grpId in grpIdList)
			{
				CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById((uint)grpId);
				if (cardPrintingById == null)
				{
					DeckError payload = new DeckError
					{
						GrpId = grpId.ToString(),
						Error = "Cannot find GrpId"
					};
					biLogger.Send(ClientBusinessEventType.DeckError, payload);
				}
				else
				{
					list.Add(cardPrintingById);
				}
			}
			if (preSortCollection)
			{
				list = CardSorter.Sort(list, cardDatabase, cardsSortedFromDatabase: false, SortTypeFilters.DraftPack).ToList();
			}
			foreach (var item3 in list.Select((CardPrintingData x, int idx) => (x: x, idx: idx)))
			{
				CardPrintingData item = item3.x;
				int item2 = item3.idx;
				string skinCode = null;
				uint num = item.GrpId;
				uint? num2 = null;
				if (styles != null && styles.ContainsKey(item.ArtId))
				{
					skinCode = (string.IsNullOrEmpty(styles[item.ArtId]) ? null : styles[item.ArtId]);
				}
				else
				{
					PreferredPrintingWithStyle preferredPrintingForTitleId = preferredPrintingDataProvider.GetPreferredPrintingForTitleId((int)item.TitleId);
					if (preferredPrintingForTitleId != null)
					{
						skinCode = preferredPrintingForTitleId.styleCode;
						num2 = num;
						num = (uint)preferredPrintingForTitleId.printingGrpId;
					}
				}
				CardData card = CardDataExtensions.CreateSkinCard(num, cardDatabase, skinCode);
				if (num2.HasValue)
				{
					preferredPrintingMap?.TryAdd(item2, num2.Value);
				}
				cardCollection.Add(card, 1);
			}
		}
		return cardCollection;
	}

	private static CardCollection ToCardCollection(List<string> grpIdList, IPreferredPrintingDataProvider preferredPrintingDataProvider, CardDatabase cardDatabase, IBILogger biLogger)
	{
		List<int> list = new List<int>();
		foreach (string grpId in grpIdList)
		{
			uint result = 0u;
			if (!uint.TryParse(grpId, out result))
			{
				DeckError payload = new DeckError
				{
					GrpId = grpId,
					Error = "Cannot parse GrpId"
				};
				biLogger.Send(ClientBusinessEventType.DeckError, payload);
			}
			else
			{
				list.Add((int)result);
			}
		}
		return ToCardCollection(list, preferredPrintingDataProvider, cardDatabase, biLogger, (Dictionary<int, uint>)null);
	}

	private void OnDestroy()
	{
		Languages.LanguageChangedSignal.Listeners -= ResetLanguage;
		if (_connectionManager != null)
		{
			ConnectionManager connectionManager = _connectionManager;
			connectionManager.OnFdReconnected = (Action)Delegate.Remove(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
		}
		if (_tableDraftPopupView != null && (bool)_tableDraftPopupView.gameObject)
		{
			UnityEngine.Object.Destroy(_tableDraftPopupView.gameObject);
		}
		if (_deckDetailsPopup != null && (bool)_deckDetailsPopup.gameObject)
		{
			UnityEngine.Object.Destroy(_deckDetailsPopup.gameObject);
		}
		if (_layoutChangeCoroutine != null)
		{
			StopCoroutine(_layoutChangeCoroutine);
			_layoutChangeCoroutine = null;
		}
	}

	private void UpdateDebugVisuals()
	{
		DraftPackHolder draftPackHolder = _draftPackHolder;
		if ((object)draftPackHolder == null || draftPackHolder.CardViews.Count <= 0)
		{
			return;
		}
		foreach (var item3 in _draftPackHolder.CardViews.Select((DraftPackCardView x, int idx) => (x: x, idx: idx)))
		{
			DraftPackCardView item = item3.x;
			int item2 = item3.idx;
			CardPrintingData printing = item.Card.Printing;
			Color? dimmed = null;
			if (_showDraftDebugOverlay)
			{
				bool num = _cosmeticInjectionCards != null && _cosmeticInjectionCards.ContainsKey((int)printing.GrpId) && _cosmeticInjectionCards[(int)printing.GrpId] == item.Card.SkinCode;
				bool flag = _draftPackIndexToDraftPickGrpId.ContainsKey(item2);
				IReadOnlyList<CardPrintingData> printingsByTitleId = _cardDatabase.DatabaseUtilities.GetPrintingsByTitleId(printing.TitleId);
				bool flag2 = printingsByTitleId != null && printingsByTitleId.Count > 1;
				if (num)
				{
					dimmed = COSMETICINJECTION_COLOR_VALUE;
				}
				else if (flag)
				{
					dimmed = PREFERREDPRINTING_COLOR_VALUE;
				}
				else if (flag2)
				{
					dimmed = MULTIPLEPRINTINGS_COLOR_VALUE;
				}
			}
			item.CardView.SetDimmed(dimmed);
		}
	}

	public bool ToggleDraftDebugDisplay()
	{
		_showDraftDebugOverlay = !_showDraftDebugOverlay;
		UpdateDebugVisuals();
		return _showDraftDebugOverlay;
	}
}
