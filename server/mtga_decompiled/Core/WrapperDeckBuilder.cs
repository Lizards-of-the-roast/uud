using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Code.Decks;
using Core.Code.Input;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using Core.Meta.MainNavigation.Cosmetics;
using Core.Meta.MainNavigation.DeckBuilder.CardViewer;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using Newtonsoft.Json;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Decks.DeckValidation;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Arena.DeckValidation.Client;
using Wizards.Arena.Enums.Deck;
using Wizards.Arena.Enums.Format;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.PlayBlade;
using Wizards.Mtga.UI;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class WrapperDeckBuilder : NavContentController
{
	public const string CACHED_DECK_KEY = "WrapperDeckBuilder_CachedDeck";

	public const string CACHED_DECK_ISFIRSTEDIT_KEY = "WrapperDeckBuilder_CachedDeck_IsFirstEdit";

	private bool _isActive;

	private const int WAIT_TO_SHOW = 4;

	private int _frameCount;

	private bool _failedToLoad;

	private ICardRolloverZoom _zoomHandler;

	[SerializeField]
	private DeckBuilderWidget _deckbuilder;

	private SystemMessageManager.SystemMessageHandle _sideboardWarning;

	private bool _isSavingDeck;

	private bool _isDeckSaveSuccess;

	private bool _isDeckSubmitSuccess;

	private DecksManager _decksManager;

	private IClientLocProvider _localizationManager;

	private CosmeticsProvider _cosmeticsProvider;

	private DesignerMetadataProvider _designerMetadataProvider;

	private IAccountClient _accountClient;

	private CardDatabase _cardDatabase;

	private InventoryManager _inventoryManager;

	private ITitleCountManager _titleCountManager;

	private FormatManager _formatManager;

	private IEmergencyCardBansProvider _emergencyCardBansProvider;

	private ISetMetadataProvider _setMetadataProvider;

	private DeckBuilderActionsHandler _deckBuilderActionsHandler;

	private PVPChallengeController _pvpChallengeController;

	public override NavContentType NavContentType => NavContentType.DeckBuilder;

	private DeckBuilderContext Context => Pantry.Get<DeckBuilderContextProvider>().Context;

	public bool IsEditingDeck => Context.IsEditingDeck;

	public bool IsSideboarding => Context.IsSideboarding;

	public bool CanCraft
	{
		get
		{
			if (Context.CanCraft)
			{
				return !Context.IsReadOnly;
			}
			return false;
		}
	}

	public bool IsReadOnly => Context.StartingMode == DeckBuilderMode.ReadOnly;

	public override bool IsReadyToShow
	{
		get
		{
			if (_frameCount < 4)
			{
				return false;
			}
			if (Context == null)
			{
				return false;
			}
			if (Context.Deck != null && !Context.Deck.isLoaded)
			{
				return false;
			}
			return true;
		}
	}

	public override bool SkipScreen => _failedToLoad;

	private void Awake()
	{
		_deckbuilder.DoneClicked += OnDeckbuilderDoneButtonClicked;
		_deckbuilder.NewDeckButtonClicked += OnNewDeck;
		Pantry.Get<DeckBuilderActionsHandler>().CardViewerRequested += OpenCardViewer;
		ScreenKeepAlive.KeepScreenAwake();
	}

	private void OnEnable()
	{
		SceneLoader.GetSceneLoader().GetDeckManager()?.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		if ((bool)_deckbuilder)
		{
			_deckbuilder.DoneClicked -= OnDeckbuilderDoneButtonClicked;
			_deckbuilder.NewDeckButtonClicked -= OnNewDeck;
			Pantry.Get<DeckBuilderActionsHandler>().CardViewerRequested -= OpenCardViewer;
		}
		Pantry.Get<DeckBuilderModelProvider>().ResetModel();
		ScreenKeepAlive.AllowScreenTimeout();
	}

	public void Initialize(ICardRolloverZoom cardZoomView, IBILogger logger, CosmeticsProvider cosmetics, DesignerMetadataProvider designerMetadataProvider, IClientLocProvider localizationManager, ISetMetadataProvider setMetadataProvider)
	{
		WrapperController instance = WrapperController.Instance;
		IUnityObjectPool unityObjectPool = Pantry.Get<IUnityObjectPool>();
		_cosmeticsProvider = cosmetics;
		_designerMetadataProvider = designerMetadataProvider;
		_localizationManager = localizationManager;
		_zoomHandler = cardZoomView;
		_inventoryManager = instance.InventoryManager;
		_titleCountManager = Pantry.Get<ITitleCountManager>();
		_cardDatabase = instance.CardDatabase;
		_formatManager = instance.FormatManager;
		IActionSystem actionSystem = Pantry.Get<IActionSystem>();
		_deckbuilder.Initialize(_zoomHandler, _inventoryManager, _cardDatabase, instance.CardViewBuilder, instance.Store, instance.EventManager, _formatManager, instance.AssetLookupSystem, logger, _cosmeticsProvider, instance.Store.CardbackCatalog, instance.Store.PetCatalog, instance.Store.AvatarCatalog, instance.DecksManager, actionSystem, instance.EmoteDataProvider, _localizationManager, unityObjectPool, setMetadataProvider);
		_decksManager = instance.DecksManager;
		_deckbuilder.SetOnStoreCallback(onStoreClicked);
		_accountClient = Pantry.Get<IAccountClient>();
		_emergencyCardBansProvider = Pantry.Get<IEmergencyCardBansProvider>();
		_setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
		_deckBuilderActionsHandler = Pantry.Get<DeckBuilderActionsHandler>();
		_pvpChallengeController = Pantry.Get<PVPChallengeController>();
	}

	public override void OnBeginClose()
	{
		if (_sideboardWarning != null)
		{
			SystemMessageManager.Instance.Close(_sideboardWarning);
			_sideboardWarning = null;
		}
		if (_inventoryManager?.CardsToTagNew != null)
		{
			_inventoryManager.AcknowledgeAllCards();
		}
		base.OnBeginClose();
		Pantry.ResetScope(Pantry.Scope.Deckbuilder);
	}

	public DeckFormat GetContextFormat()
	{
		return Context?.Format;
	}

	public override void Activate(bool active)
	{
		_failedToLoad = false;
		_frameCount = 0;
		_isActive = active;
		if (active)
		{
			_isSavingDeck = false;
			if (Context.Deck == null || Context.Deck.isLoaded)
			{
				OnReadyToLoad();
				return;
			}
			_decksManager.GetFullDeck(Context.Deck.id).ThenOnMainThread(delegate(Promise<Client_Deck> promise)
			{
				if (promise.Successful)
				{
					Context.Deck.UpdateWith(promise.Result.Contents);
					OnReadyToLoad();
				}
				else
				{
					SimpleLog.LogWarningForRelease($"GetFullDeck failed for {Context.Deck.id}");
					_failedToLoad = true;
					NavigateAwayBasedOnContext();
					SystemMessageManager.Instance.ShowOk(_localizationManager.GetLocalizedText("SystemMessage/System_Network_Error_Title"), _localizationManager.GetLocalizedText("SystemMessage/System_Decks_Get_Failure_Text"));
				}
			});
		}
		else
		{
			_deckbuilder.gameObject.UpdateActive(active: false);
			_deckbuilder.ShowOrHide(active: false);
			_zoomHandler.Close();
		}
	}

	private void OnReadyToLoad()
	{
		_deckbuilder.gameObject.UpdateActive(active: true);
		_deckbuilder.ShowOrHide(active: true);
	}

	private void onStoreClicked(Action storeRedirect)
	{
		HandleDone(storeRedirect);
	}

	public void OnBackButton()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, base.gameObject);
	}

	public void OnNewDeck()
	{
		if (!_decksManager.ShowDeckLimitError())
		{
			DeckBuilderContext context = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(_formatManager.GetDefaultFormat().NewDeck(_decksManager)), null, sideboarding: false, firstEdit: true, DeckBuilderMode.DeckBuilding, ambiguousFormat: true);
			SceneLoader.GetSceneLoader().GoToDeckBuilder(context, reloadIfAlreadyLoaded: true);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		}
	}

	private void OnDeckbuilderDoneButtonClicked()
	{
		HandleDone(null);
	}

	public override void OnNavBarScreenChange(Action screenChangeAction)
	{
		HandleDone(screenChangeAction);
	}

	public override void OnHandheldBackButton()
	{
		if (!_deckbuilder.OnHandheldBackButton())
		{
			HandleDone(null);
		}
	}

	public override void OnNavBarExit(Action exitAction)
	{
		HandleDone(exitAction);
	}

	public void ForceSaveDeck(string errorHeader, string errorDesc)
	{
		StartCoroutine(Coroutine_SaveDeck(_deckbuilder.GetDeck(), errorHeader, errorDesc));
	}

	private void HandleDone(Action navAction)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_done, base.gameObject);
		bool flag = false;
		if (!Context.IsEditingDeck || Context.IsReadOnly)
		{
			flag = true;
		}
		else if (!_deckbuilder.HasChangesInCurrentDeck() && Context.IsConstructed && !Context.IsSideboarding)
		{
			flag = true;
		}
		if (flag)
		{
			NavigateAwayAfterDone("Leaving Deck Builder w/o changes", navAction);
			return;
		}
		Action button2Action = delegate
		{
			ClearCachedDeck();
			NavigateAwayAfterDone("Discard the unsaved changes on the deck", navAction);
		};
		if (navAction != null)
		{
			if (Context.IsLimited)
			{
				SystemMessageManager.Instance.ShowMessage(_localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_DiscardChanges"), _localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageDescription_DiscardChanges"), _localizationManager.GetLocalizedText("MainNav/DeckBuilder/Cancel_Button"), null, _localizationManager.GetLocalizedText("MainNav/DeckBuilder/DiscardChanges_Button"), button2Action);
				return;
			}
			SystemMessageManager.Instance.ShowMessage(_localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_ConfirmSavingDeck"), _localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageContent_ConfirmSavingDeck"), _localizationManager.GetLocalizedText("MainNav/DeckBuilder/Cancel_Button"), null, _localizationManager.GetLocalizedText("MainNav/DeckBuilder/DiscardChanges_Button"), button2Action, _localizationManager.GetLocalizedText("MainNav/DeckBuilder/SaveAndExit_Button"), delegate
			{
				SaveHelper(navAction);
			});
		}
		else
		{
			SaveHelper(null);
		}
	}

	private void OnConstructedSaveSuccess(Client_Deck deckResult)
	{
		if (Context.IsPlayQueueEvent && (Context.Event.PlayerEvent.EventInfo.SkipDeckValidation || DeckValidationHelper.CalculateIsDeckLegalAndOwned(Context.Format, deckResult, _inventoryManager.Cards, _titleCountManager.OwnedTitleCounts, _cardDatabase, _emergencyCardBansProvider, _setMetadataProvider, _cosmeticsProvider, _designerMetadataProvider, Context.DeckValidationEventData()).IsValid))
		{
			string userAccountID = _accountClient.AccountInformation?.PersonaID;
			string internalEventName = Context.Event.PlayerEvent.EventInfo.InternalEventName;
			MDNPlayerPrefs.SetSelectedDeckId(userAccountID, internalEventName, deckResult.Id.ToString());
		}
	}

	private void NavigateAwayAfterDone(string context, Action navAction = null)
	{
		PVPChallengeData pVPChallengeData = _pvpChallengeController?.GetActiveCurrentChallengeData();
		if (pVPChallengeData != null && navAction != null && navAction.Method != typeof(NavBarController).GetMethod("ChangeToHomePage", BindingFlags.Instance | BindingFlags.NonPublic))
		{
			_pvpChallengeController.LeaveChallenge(pVPChallengeData.ChallengeId, confirm: true, navAction);
			return;
		}
		if (navAction == null)
		{
			navAction = NavigateAwayBasedOnContext;
		}
		navAction();
	}

	private void NavigateAwayBasedOnContext()
	{
		if (Context.IsEvent)
		{
			if (Context.IsColorChallengeEvent)
			{
				SceneLoader.GetSceneLoader().GoToEventScreen(Context.Event);
			}
			else if (Context.IsPlayQueueEvent)
			{
				PlayBladeController.PlayBladeVisualStates initialBladeState;
				if (PlayBladeController.PreviousPlayBladeVisualState == PlayBladeController.PlayBladeVisualStates.Challenge)
				{
					initialBladeState = PlayBladeController.PlayBladeVisualStates.Challenge;
				}
				else
				{
					initialBladeState = PlayBladeController.PlayBladeVisualStates.Events;
					if (Context.Deck != null)
					{
						IPlayBladeSelectionProvider playBladeSelectionProvider = Pantry.Get<IPlayBladeSelectionProvider>();
						BladeSelectionData selection = playBladeSelectionProvider.GetSelection();
						if (selection.findMatch.DeckId != Context.Deck.id)
						{
							selection.findMatch.DeckId = Context.Deck.id;
							playBladeSelectionProvider.SetSelection(selection);
						}
					}
				}
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext
				{
					InitialBladeState = initialBladeState,
					ChallengeId = Context.ChallengeId
				});
			}
			else if (Context.DeckSelectContext != null)
			{
				SceneLoader.GetSceneLoader().GoToConstructedDeckSelect(Context.DeckSelectContext);
			}
			else
			{
				SceneLoader.GetSceneLoader().GoToEventScreen(Context.Event);
			}
		}
		else if (Context.IsPlayblade)
		{
			HomePageContext homePageContext = new HomePageContext();
			homePageContext.InitialBladeState = PlayBladeController.PlayBladeVisualStates.Events;
			SceneLoader.GetSceneLoader().GoToLanding(homePageContext);
		}
		else if (Context.PreconContext != null)
		{
			SceneLoader.GetSceneLoader().GoToStoreItem(Context.PreconContext ?? string.Empty, StoreTabType.Decks, string.Empty);
		}
		else
		{
			SceneLoader.GetSceneLoader().GoToDeckManager();
		}
	}

	private IEnumerator Coroutine_SaveDeck(DeckInfo deckToSave, string errorHeader = "", string errorDesc = "")
	{
		if (_isSavingDeck)
		{
			yield break;
		}
		_isSavingDeck = true;
		_isDeckSaveSuccess = false;
		if (EditingDeckInLimitedEvent(Context))
		{
			yield return StartCoroutine(Coroutine_SubmitDeckForEvent(deckToSave));
			_isDeckSaveSuccess = _isDeckSubmitSuccess;
			yield break;
		}
		if (Context.IsFirstEdit)
		{
			yield return StartCoroutine(Coroutine_CreateConstructedDeck(deckToSave));
		}
		else
		{
			yield return StartCoroutine(Coroutine_UpdateConstructedDeck(deckToSave));
		}
		if (_isDeckSaveSuccess)
		{
			if (Context.Event != null && !Context.IsPlayQueueEvent && Context.IsFirstEdit)
			{
				yield return StartCoroutine(Coroutine_SubmitDeckForEvent(deckToSave));
			}
			ClearCachedDeck();
		}
		if (!_isDeckSaveSuccess && !string.IsNullOrEmpty(errorHeader) && !string.IsNullOrEmpty(errorDesc))
		{
			SystemMessageManager.Instance.ShowOk(errorHeader, errorDesc);
		}
	}

	private static bool EditingDeckInLimitedEvent(DeckBuilderContext context)
	{
		if (!context.IsConstructed)
		{
			return context.Event != null;
		}
		return false;
	}

	private IEnumerator Coroutine_SaveDeckAndExit(DeckInfo deckToSave, Action navAction)
	{
		yield return Coroutine_SaveDeck(deckToSave);
		if (_isDeckSaveSuccess)
		{
			NavigateAwayAfterDone("Successfully saved deck", navAction);
		}
		else
		{
			SystemMessageManager.Instance.ShowOk(_localizationManager.GetLocalizedText("SystemMessage/System_Network_Error_Title"), _localizationManager.GetLocalizedText("SystemMessage/System_Deck_Creation_Failure_Text"));
		}
	}

	private IEnumerator Coroutine_SubmitDeckForEvent(DeckInfo deckToSave)
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		Client_Deck original = DeckServiceWrapperHelpers.ToClientModel(deckToSave);
		Promise<Client_Deck> submitDeck = Context.Event.PlayerEvent.SubmitEventDeck(WrapperDeckUtilities.GetSubmitDeck(original, _decksManager));
		yield return submitDeck.AsCoroutine();
		_isDeckSubmitSuccess = submitDeck.Successful;
		if (_isDeckSubmitSuccess)
		{
			ClearCachedDeck();
		}
		else
		{
			Debug.LogError(submitDeck.Error.Message);
			Utils.GetDeckSubmissionErrorMessages(submitDeck.Error, out var errTitle, out var errText);
			SystemMessageManager.Instance.ShowOk(errTitle, errText);
			NavigateAwayBasedOnContext();
		}
		WrapperController.EnableLoadingIndicator(enabled: false);
		_isSavingDeck = false;
	}

	private IEnumerator Coroutine_CreateConstructedDeck(DeckInfo deckToSave)
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		Client_Deck deck = DeckServiceWrapperHelpers.ToClientModel(deckToSave);
		Promise<Client_DeckSummary> createRequest = _decksManager.CreateDeck(deck, DeckActionType.CreatedNew.ToString());
		yield return createRequest.AsCoroutine();
		WrapperController.EnableLoadingIndicator(enabled: false);
		if (createRequest.Successful)
		{
			OnConstructedSaveSuccess(deck);
			ClearCachedDeck();
			_isDeckSaveSuccess = true;
		}
		else
		{
			_isDeckSaveSuccess = false;
		}
		_isSavingDeck = false;
	}

	private IEnumerator Coroutine_UpdateConstructedDeck(DeckInfo deckToSave)
	{
		Client_Deck client_Deck = DeckServiceWrapperHelpers.ToClientModel(deckToSave);
		Client_Deck deck = _decksManager.GetDeck(client_Deck.Id);
		deck.UpdateWith(client_Deck.Summary);
		deck.UpdateWith(client_Deck.Contents);
		if (Context.IsEvent)
		{
			DeckFormat safeFormat = _formatManager.GetSafeFormat(deck.Summary.Format);
			DeckFormat format = Context.Format;
			if (safeFormat != format)
			{
				if (Context.Event.PlayerEvent.EventInfo.SkipDeckValidation)
				{
					deck.Summary.Format = format.FormatName;
				}
				else
				{
					bool isValid = DeckValidationHelper.CalculateIsDeckLegalAndOwned(safeFormat, deck, _inventoryManager.Cards, _titleCountManager.OwnedTitleCounts, _cardDatabase, _emergencyCardBansProvider, _setMetadataProvider, _cosmeticsProvider, _designerMetadataProvider, Context.DeckValidationEventData()).IsValid;
					if (DeckValidationHelper.CalculateIsDeckLegalAndOwned(format, deck, _inventoryManager.Cards, _titleCountManager.OwnedTitleCounts, _cardDatabase, _emergencyCardBansProvider, _setMetadataProvider, _cosmeticsProvider, _designerMetadataProvider, Context.DeckValidationEventData()).IsValid && !isValid)
					{
						deck.Summary.Format = format.FormatName;
					}
				}
			}
		}
		WrapperController.EnableLoadingIndicator(enabled: true);
		Promise<Client_DeckSummary> updateRequest = _decksManager.UpdateDeck(deck, DeckActionType.Updated);
		yield return updateRequest.AsCoroutine();
		WrapperController.EnableLoadingIndicator(enabled: false);
		if (updateRequest.Successful)
		{
			deck.UpdateWith(updateRequest.Result);
			OnConstructedSaveSuccess(deck);
			ClearCachedDeck();
			_isDeckSaveSuccess = true;
		}
		else
		{
			SystemMessageManager.Instance.ShowOk(_localizationManager.GetLocalizedText("SystemMessage/System_Network_Error_Title"), _localizationManager.GetLocalizedText("SystemMessage/System_Deck_Update_Failure_Text"));
			_isDeckSaveSuccess = false;
		}
		_isSavingDeck = false;
	}

	private void SaveHelper(Action navAction)
	{
		DeckInfo deck = _deckbuilder.GetDeck();
		SaveStep_DeckName(deck, navAction);
	}

	private void SaveStep_DeckName(DeckInfo deckToSave, Action navAction)
	{
		if (Context.IsSideboarding || DeckValidationUtils.ValidateDeckNameWithSystemMessages(deckToSave.id, deckToSave.name, delegate
		{
			_deckbuilder.FocusNameInputField();
		}, delegate(string newName)
		{
			_deckbuilder.FocusNameInputField();
			deckToSave.name = newName;
			Pantry.Get<DeckBuilderModelProvider>().SetDeckName(newName);
		}, Context.IsConstructed))
		{
			SaveStep_DeckValid(deckToSave, navAction);
		}
	}

	private void SaveStep_DeckValid(DeckInfo deckToSave, Action navAction)
	{
		if (Context.Format.SideboardBehavior == FormatSideboardBehavior.CompanionOnly)
		{
			deckToSave.sideboard = DeckSideboardUtilities.SideboardForCompanionsOnly(deckToSave.sideboard, deckToSave.companion);
		}
		ClientSideDeckValidationResult clientSideDeckValidationResult = ((Context.Event?.PlayerEvent?.EventInfo?.AllowUncollectedCards != true && Context.Format.FormatType == MDNEFormatType.Constructed) ? DeckValidationHelper.CalculateIsDeckLegalAndOwned(Context.Format, deckToSave, _inventoryManager.Cards, _titleCountManager.OwnedTitleCounts, _cardDatabase, _emergencyCardBansProvider, _setMetadataProvider, _cosmeticsProvider, _designerMetadataProvider, Context.DeckValidationEventData()) : DeckValidationHelper.CalculateIsDeckLegal(Context.Format, deckToSave, _cardDatabase, _emergencyCardBansProvider, _setMetadataProvider, _cosmeticsProvider, _designerMetadataProvider));
		int num;
		object obj;
		if (!clientSideDeckValidationResult.IsValid)
		{
			if (clientSideDeckValidationResult.CanSave)
			{
				num = (Context.CanSaveInvalidDecks ? 1 : 0);
				if (num != 0)
				{
					obj = "MainNav/DeckBuilder/DeckBuilder_MessageContent_InvalidDeck";
					goto IL_018e;
				}
			}
			else
			{
				num = 0;
			}
			obj = "MainNav/DeckBuilder/DeckBuilder_MessageContent_InvalidDeck_CantSave";
			goto IL_018e;
		}
		CardInDeck companion = deckToSave.companion;
		if (companion != null && companion.Quantity != 0 && !deckToSave.isCompanionValid)
		{
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(deckToSave.companion.Id);
			string localizedText = _cardDatabase.GreLocProvider.GetLocalizedText(cardPrintingById.TitleId);
			SystemMessageManager.ShowSystemMessage((MTGALocalizedString)"MainNav/DeckBuilder/CompanionInvalid_Title", new MTGALocalizedString
			{
				Key = "MainNav/DeckBuilder/CompanionInvalid_Body",
				Parameters = new Dictionary<string, string> { { "cardName", localizedText } }
			}, showCancel: true, delegate
			{
				SaveStep_FormatNeedsSideboard(deckToSave, navAction);
			});
		}
		else
		{
			SaveStep_RestrictSideboardTo15CardMaximum(deckToSave, navAction);
		}
		return;
		IL_018e:
		string key = (string)obj;
		string localizedText2 = _localizationManager.GetLocalizedText(key);
		string localizedText3 = _localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_InvalidDeck");
		string invalidReasons = clientSideDeckValidationResult.GetInvalidReasons(_localizationManager);
		localizedText2 = string.Format(localizedText2, invalidReasons);
		if (num != 0)
		{
			SystemMessageManager.ShowSystemMessage(localizedText3, localizedText2, showCancel: true, delegate
			{
				SaveStep_FormatNeedsSideboard(deckToSave, navAction);
			});
		}
		else
		{
			SystemMessageManager.ShowSystemMessage(localizedText3, localizedText2, showCancel: false, delegate
			{
			});
		}
	}

	private void SaveStep_RestrictSideboardTo15CardMaximum(DeckInfo deckToSave, Action navAction)
	{
		if (_formatManager.GetSafeFormat(deckToSave.format).MaxSideboardCards <= 15 && deckToSave.sideboard.Quantity() > 15)
		{
			SystemMessageManager.ShowSystemMessage(_localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_InvalidDeck"), _localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_MoreThanXInSideboard", ("MaxSideboardCards", "15")));
		}
		else
		{
			SaveStep_FormatNeedsSideboard(deckToSave, navAction);
		}
	}

	private void SaveStep_FormatNeedsSideboard(DeckInfo deckToSave, Action navAction)
	{
		if (deckToSave.sideboard.Count == 0 && (Context.Format.FormatName == "TraditionalStandard" || Context.Format.FormatName == "TraditionalAlchemy" || Context.Format.FormatName == "TraditionalHistoric" || Context.Format.FormatName == "TraditionalExplorer"))
		{
			_sideboardWarning = SystemMessageManager.Instance.ShowOkCancel(_localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_ConfirmSavingDeck"), _localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_RequiresSideboard"), delegate
			{
				_sideboardWarning = null;
				SaveStep_CardsNeedSideboard(deckToSave, navAction);
			}, delegate
			{
				_sideboardWarning = null;
			});
		}
		else
		{
			SaveStep_CardsNeedSideboard(deckToSave, navAction);
		}
	}

	private void SaveStep_CardsNeedSideboard(DeckInfo deckToSave, Action navAction)
	{
		if (deckToSave.sideboard.Count == 0 && Context.Format.MaxSideboardCards > 0)
		{
			HashSet<uint> hashSet = new HashSet<uint> { 66771u, 69452u, 69984u, 70191u, 75274u, 73207u };
			HashSet<string> hashSet2 = new HashSet<string>();
			foreach (CardInDeck item2 in deckToSave.mainDeck)
			{
				CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(item2.Id);
				string localizedText = _cardDatabase.GreLocProvider.GetLocalizedText(cardPrintingById.TitleId);
				if (cardPrintingById.UsesSideboard || hashSet.Contains(item2.Id))
				{
					hashSet2.Add(localizedText);
					continue;
				}
				foreach (CardPrintingData linkedFacePrinting in cardPrintingById.LinkedFacePrintings)
				{
					if (linkedFacePrinting.UsesSideboard)
					{
						hashSet2.Add(localizedText);
					}
				}
			}
			if (hashSet2.Count > 0)
			{
				string item = string.Join(Environment.NewLine, hashSet2.ToArray());
				string localizedText2 = _localizationManager.GetLocalizedText("MainNav/DeckBuilder/Warning_NoSideboard", ("cardList", item));
				_sideboardWarning = SystemMessageManager.Instance.ShowOkCancel(_localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_ConfirmSavingDeck"), localizedText2, delegate
				{
					_sideboardWarning = null;
					SaveStep_CardsNeedCommander(deckToSave, navAction);
				}, delegate
				{
					_sideboardWarning = null;
				});
				return;
			}
		}
		SaveStep_CardsNeedCommander(deckToSave, navAction);
	}

	private void SaveStep_CardsNeedCommander(DeckInfo deckToSave, Action navAction)
	{
		if (!Context.Format.FormatIncludesCommandZone)
		{
			HashSet<uint> hashSet = new HashSet<uint> { 70464u, 70465u, 70466u };
			HashSet<string> hashSet2 = new HashSet<string>();
			foreach (CardInDeck item2 in deckToSave.mainDeck)
			{
				if (hashSet.Contains(item2.Id))
				{
					CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(item2.Id);
					string localizedText = _cardDatabase.GreLocProvider.GetLocalizedText(cardPrintingById.TitleId);
					hashSet2.Add(localizedText);
				}
			}
			if (hashSet2.Count > 0)
			{
				string item = string.Join(Environment.NewLine, hashSet2.ToArray());
				string localizedText2 = _localizationManager.GetLocalizedText("MainNav/DeckBuilder/Warning_NoCommander", ("cardList", item));
				_sideboardWarning = SystemMessageManager.Instance.ShowOkCancel(_localizationManager.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_ConfirmSavingDeck"), localizedText2, delegate
				{
					_sideboardWarning = null;
					StartCoroutine(Coroutine_SaveDeckAndExit(deckToSave, navAction));
				}, delegate
				{
					_sideboardWarning = null;
				});
				return;
			}
		}
		StartCoroutine(Coroutine_SaveDeckAndExit(deckToSave, navAction));
	}

	private void Update()
	{
		if (_isActive && _frameCount < 4)
		{
			_frameCount++;
		}
	}

	private void OpenCardViewer(CardPrintingData cardPrintingData, string skin, int quantityToCraft, Action<string> onSkinSelected)
	{
		CardViewerUtilities.OpenCardViewer(_cardDatabase, cardPrintingData, skin, quantityToCraft, onSkinSelected, HandleDone);
	}

	private void OpenSleevePopup(CardBackSelectorPopup popup, string currentSleeve, List<CardBackSelectorDisplayData> allSleeves)
	{
		_deckBuilderActionsHandler.OpenDeckDetailsCosmeticsSelector(_cardDatabase, DisplayCosmeticsTypes.Sleeve);
	}

	public static void CacheDeck(DeckBuilderModel model, DeckBuilderContext context)
	{
		if (model != null && context != null && context.IsCachingEnabled && !context.IsReadOnly)
		{
			DeckInfoV3 value = DeckInfoV3.FromDeckInfo(model.GetServerModel());
			PlayerPrefsExt.SetString("WrapperDeckBuilder_CachedDeck", JsonConvert.SerializeObject(value));
			PlayerPrefsExt.SetInt("WrapperDeckBuilder_CachedDeck_IsFirstEdit", context.IsFirstEdit ? 1 : 0);
			PlayerPrefsExt.Save();
		}
	}

	public static bool HasCachedDeck()
	{
		string text = PlayerPrefsExt.GetString("WrapperDeckBuilder_CachedDeck");
		if (text != null)
		{
			return text.Length > 0;
		}
		return false;
	}

	public static bool TryRestoreDeckFromCache(out DeckInfo result, out bool isFirstEdit)
	{
		string text = PlayerPrefsExt.GetString("WrapperDeckBuilder_CachedDeck");
		if (text != null && text.Length > 0)
		{
			result = DeckInfoV3.DeserializeToDeckInfo(text);
			result.isLoaded = true;
			isFirstEdit = PlayerPrefsExt.GetInt("WrapperDeckBuilder_CachedDeck_IsFirstEdit") == 1;
			return true;
		}
		result = null;
		isFirstEdit = false;
		return false;
	}

	public static void ClearCachedDeck()
	{
		PlayerPrefsExt.DeleteKey("WrapperDeckBuilder_CachedDeck");
		PlayerPrefsExt.DeleteKey("WrapperDeckBuilder_CachedDeck_IsFirstEdit");
		PlayerPrefsExt.Save();
	}
}
