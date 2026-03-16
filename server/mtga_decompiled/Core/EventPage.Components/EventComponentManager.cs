using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Assets.Core.Shared.Code;
using Core.Code.Familiar;
using Core.Code.Input;
using Core.Code.Promises;
using Core.Meta.Shared;
using EventPage.Components.NetworkModels;
using MTGA.KeyboardManager;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using UnityEngine;
using Wizards.Arena.Enums.Deck;
using Wizards.Arena.Enums.Event;
using Wizards.Arena.Enums.UILayout;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.BI;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Format;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PlayBlade;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Wrapper.Draft;

namespace EventPage.Components;

public class EventComponentManager : IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	public EventContext EventContext;

	private List<IComponentController> _componentControllers = new List<IComponentController>();

	private EventPageRewardsController _rewardController;

	private EventPageQuestsController _questController;

	private ClientInventoryUpdateReportItem _cardGrant;

	private SharedEventPageClasses _sharedClasses;

	public bool SkipPage;

	private bool _waitingForServer;

	private bool _autoClickMainButton;

	private RecentlyPlayedDataProvider _recentlyPlayedDataProvider;

	private IPlayBladeSelectionProvider _playBladeSelectionDataProvider;

	private IFormatManager _formatManager;

	public Action<bool> OnJoinPayEvent;

	public Action<bool> OnPrizeClaim;

	public EventComponentData ComponentData => EventContext.PlayerEvent.EventUXInfo.EventComponentData;

	public bool ReadyToShow => !_waitingForServer;

	public string JoinEventChoice { get; set; } = "";

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper_PopUps;

	public EventComponentManager(SharedEventPageClasses sharedClasses, EventContext eventContext)
	{
		EventContext = eventContext;
		_sharedClasses = sharedClasses;
		_recentlyPlayedDataProvider = Pantry.Get<RecentlyPlayedDataProvider>();
		_playBladeSelectionDataProvider = Pantry.Get<IPlayBladeSelectionProvider>();
		_formatManager = WrapperController.Instance.FormatManager;
		_rewardController = new EventPageRewardsController(_sharedClasses.InventoryManager, RewardsController_RewardsPanelClosed);
		_questController = new EventPageQuestsController(QuestController_RewardsPanelClosed, this, _sharedClasses.CardDatabase, _sharedClasses.CardMaterialBuilder, _sharedClasses.AssetLookupSystem);
	}

	public void AddComponent(IComponentController component)
	{
		_componentControllers.Add(component);
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
		_autoClickMainButton = false;
		EventContext = eventContext;
		_sharedClasses.InventoryManager.Subscribe(InventoryUpdateSource.ModifyPlayerInventory, OnCardsGranted);
		_sharedClasses.KeyboardManager.Subscribe(this);
		_sharedClasses.ActionSystem.PushFocus(this);
		foreach (IComponentController componentController in _componentControllers)
		{
			componentController.OnEventPageOpen(EventContext);
		}
		_rewardController.OnEventPageOpen();
		_questController.OnEventPageOpen();
		UpdateComponents(onOpen: true);
		if (EventContext.PlayerEvent.HasUnclaimedRewards)
		{
			PAPA.StartGlobalCoroutine(Coroutine_ClaimCumulativePrize());
		}
	}

	private IEnumerator Coroutine_ClaimCumulativePrize()
	{
		_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: true);
		Promise<ICourseInfoWrapper> claimPrize = EventContext.PlayerEvent.ClaimNoGatePrize();
		yield return claimPrize.AsCoroutine();
		if (!claimPrize.Successful)
		{
			if (claimPrize.ErrorSource != ErrorSource.Debounce)
			{
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Prize_Claim_Error_Text"));
			}
			yield break;
		}
		_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: false);
		yield return new WaitUntil(() => _rewardController.HasPendingRewards);
		_rewardController.ShowRewardsPanel();
		OnPrizeClaim?.Invoke(claimPrize.Successful);
	}

	public void OnEventPageClosed()
	{
		_sharedClasses.InventoryManager.UnSubscribe(InventoryUpdateSource.ModifyPlayerInventory, OnCardsGranted);
		_sharedClasses.KeyboardManager.Unsubscribe(this);
		_sharedClasses.ActionSystem.PopFocus(this);
		_rewardController.OnEventPageClosed();
		_questController.OnEventPageClosed();
	}

	private void OnCardsGranted(ClientInventoryUpdateReportItem item)
	{
		if (item.parentcontext.Contains("Event.GrantCardPool"))
		{
			_cardGrant = item;
		}
	}

	public bool HandleKeyDown(KeyCode currentKey, Modifiers mods)
	{
		if (currentKey == KeyCode.Escape && PlatformUtils.IsHandheld())
		{
			OnBackButtonClicked(EventContext.PlayerEvent.EventUXInfo.OpenedFromPlayBlade);
			return true;
		}
		return false;
	}

	public void OnBack(ActionContext context)
	{
		if (PlatformUtils.IsHandheld())
		{
			OnBackButtonClicked(EventContext.PlayerEvent.EventUXInfo.OpenedFromPlayBlade);
		}
		else
		{
			context.Used = false;
		}
	}

	public void UpdateComponents(bool onOpen = false)
	{
		foreach (IComponentController componentController in _componentControllers)
		{
			componentController.Update(EventContext.PlayerEvent);
		}
		if (_rewardController.HasPendingRewards && EventContext.PostMatchContext == null)
		{
			SetProgressBarState(EventPageStates.ClaimEventRewards);
			return;
		}
		switch (EventContext.PlayerEvent.CourseData.CurrentModule)
		{
		case PlayerEventModule.ClaimPrize:
			if (EventContext.PostMatchContext == null)
			{
				break;
			}
			goto IL_00eb;
		case PlayerEventModule.WinLossGate:
			if (EventContext.PostMatchContext == null)
			{
				break;
			}
			goto IL_00eb;
		case PlayerEventModule.WinNoGate:
			if (EventContext.PostMatchContext == null)
			{
				break;
			}
			goto IL_00eb;
		case PlayerEventModule.Complete:
			SetProgressBarState(EventPageStates.ClaimEventRewards);
			return;
		case PlayerEventModule.Draft:
			_recentlyPlayedDataProvider.AddRecentlyPlayedGame(EventContext.PlayerEvent.EventInfo.InternalEventName, Guid.Empty);
			_playBladeSelectionDataProvider.SetSelectedTab(BladeType.LastPlayed);
			LoadBotDraft();
			return;
		case PlayerEventModule.HumanDraft:
			_recentlyPlayedDataProvider.AddRecentlyPlayedGame(EventContext.PlayerEvent.EventInfo.InternalEventName, Guid.Empty);
			_playBladeSelectionDataProvider.SetSelectedTab(BladeType.LastPlayed);
			LoadHumanDraft();
			return;
		case PlayerEventModule.DeckSelect:
			if (_autoClickMainButton && EventContext.PlayerEvent.EventInfo.FormatType == MDNEFormatType.Sealed)
			{
				BuildDeckButtonClicked();
				return;
			}
			break;
		case PlayerEventModule.Join:
		case PlayerEventModule.Pay:
			{
				_autoClickMainButton = true;
				SetProgressBarState(EventPageStates.DisplayEvent);
				return;
			}
			IL_00eb:
			SetProgressBarState(EventPageStates.DisplayQuest);
			return;
		}
		bool num = EventContext.PostMatchContext == null;
		bool flag = EventContext.PlayerEvent.EventInfo.UpdateQuests || EventContext.PlayerEvent.EventInfo.UpdateDailyWeeklyRewards;
		if (!num && flag)
		{
			SetProgressBarState(EventPageStates.DisplayQuest);
		}
		else
		{
			SetProgressBarState(EventPageStates.DisplayEvent);
		}
	}

	public void SetProgressBarState(EventPageStates state)
	{
		foreach (IComponentController componentController in _componentControllers)
		{
			componentController.OnEventPageStateChanged(EventContext.PlayerEvent, state);
		}
		switch (state)
		{
		case EventPageStates.DisplayQuest:
			PAPA.StartGlobalCoroutine(_questController.Coroutine_ShowQuestBar(EventContext));
			break;
		case EventPageStates.DisplayEvent:
		{
			PostMatchContext postMatchContext = EventContext.PostMatchContext;
			if (postMatchContext != null && postMatchContext.WonGame && _rewardController.HasPendingRewards)
			{
				SetProgressBarState(EventPageStates.ClaimEventRewards);
			}
			else
			{
				_questController.HideQuestBar();
			}
			EventContext.PostMatchContext = null;
			break;
		}
		case EventPageStates.ClaimEventRewards:
			_rewardController.ShowRewardsPanel();
			break;
		case EventPageStates.ClaimQuestRewards:
			break;
		}
	}

	private void ShowDraftErrorMessage()
	{
		SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Draft_Commence_Error_Text"), delegate
		{
			_sharedClasses.SceneLoader.GoToLanding(new HomePageContext());
		});
	}

	private void LoadBotDraft()
	{
		IPlayerEvent playerEvent = EventContext.PlayerEvent;
		LimitedPlayerEvent limitedEvent = playerEvent as LimitedPlayerEvent;
		if (limitedEvent == null)
		{
			return;
		}
		if (limitedEvent.DraftPod == null)
		{
			WrapperController.EnableLoadingIndicator(enabled: true);
			_waitingForServer = true;
			limitedEvent.CreateBotDraft(_sharedClasses.AccountInformation.DisplayName, _sharedClasses.CosmeticsProvider.PlayerAvatarSelection).ThenOnMainThread(delegate(Promise<ICourseInfoWrapper> p)
			{
				if (p.Successful)
				{
					onDraftPodCreated();
				}
				else
				{
					ShowDraftErrorMessage();
				}
				WrapperController.EnableLoadingIndicator(enabled: false);
				_waitingForServer = false;
			});
		}
		else
		{
			onDraftPodCreated();
		}
		void onDraftPodCreated()
		{
			if (limitedEvent.DraftPod != null)
			{
				if (_autoClickMainButton)
				{
					_sharedClasses.SceneLoader.GoToDraftScene(EventContext);
				}
				else
				{
					SetProgressBarState(EventPageStates.DisplayEvent);
				}
			}
		}
	}

	private void LoadHumanDraft()
	{
		IPlayerEvent playerEvent = EventContext.PlayerEvent;
		LimitedPlayerEvent limitedEvent = playerEvent as LimitedPlayerEvent;
		if (limitedEvent == null)
		{
			return;
		}
		bool rejoinEvent = !string.IsNullOrEmpty(limitedEvent.DraftPod?.DraftId);
		WrapperController.EnableLoadingIndicator(enabled: true);
		_waitingForServer = true;
		limitedEvent.CreateHumanDraft(rejoinEvent, _sharedClasses.Logger, _sharedClasses.BILogger).ThenOnMainThread(delegate(Promise<Unit> p)
		{
			if (!p.Successful && p.ErrorSource != ErrorSource.Debounce)
			{
				ShowDraftErrorMessage();
			}
			WrapperController.EnableLoadingIndicator(enabled: false);
			_waitingForServer = false;
			ProcessDraftPod();
		});
		void ProcessDraftPod()
		{
			switch (limitedEvent.DraftPod?.DraftState)
			{
			case DraftState.Podmaking:
				if (_autoClickMainButton)
				{
					_sharedClasses.SceneLoader.GoToTableDraftQueueScene(EventContext);
					_autoClickMainButton = false;
				}
				else
				{
					SetProgressBarState(EventPageStates.DisplayEvent);
				}
				break;
			case DraftState.Picking:
			case DraftState.Completing:
				_sharedClasses.SceneLoader.GoToDraftScene(EventContext);
				SkipPage = true;
				break;
			default:
				Debug.LogError($"Unexpected draft state {limitedEvent.DraftPod?.DraftState} when loading human draft.");
				break;
			}
		}
	}

	public void Skipped()
	{
		SkipPage = false;
	}

	private void QuestController_RewardsPanelClosed()
	{
		SetProgressBarState(EventPageStates.DisplayEvent);
	}

	private void RewardsController_RewardsPanelClosed()
	{
		if (EventContext.PlayerEvent.EventInfo.LockedTime.CompareTo(ServerGameTime.GameTime) < 0)
		{
			_sharedClasses.SceneLoader.GoToLanding(new HomePageContext());
		}
		else
		{
			UpdateComponents();
		}
	}

	public void CardsComponent_OnClicked()
	{
		_sharedClasses.SceneLoader.GoToDeckBuilder(new DeckBuilderContext(null, EventContext, sideboarding: false, firstEdit: false, DeckBuilderMode.ReadOnlyCollection));
	}

	public void CashTournamentComponent_OnClicked(string eventName)
	{
		EventContext eventContext = _sharedClasses.EventManager.EventContexts.Find((EventContext c) => c.PlayerEvent.EventInfo.InternalEventName == eventName);
		if (eventContext != null)
		{
			_sharedClasses.SceneLoader.GoToEventScreen(eventContext, reloadIfAlreadyLoaded: true);
		}
	}

	public void ChestWidgetComponent_OnClicked(DeckBuilderContext deckBuilderContext)
	{
		deckBuilderContext.Event = EventContext;
		_sharedClasses.SceneLoader.GoToDeckBuilder(deckBuilderContext);
	}

	public void InspectPreconDecks_OnClicked(List<Guid> deckIds)
	{
		EventContext.DeckSelectContext = EventContext.DeckSelectSceneContext.InspectDeck;
		_sharedClasses.SceneLoader.GoToConstructedDeckSelect(new DeckSelectContext
		{
			EventContext = EventContext,
			PreconDeckIds = deckIds
		});
	}

	public void InspectSingleDeck_OnClicked(Client_Deck deck)
	{
		DeckBuilderContext deckBuilderContext = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(deck), EventContext, sideboarding: false, firstEdit: false, DeckBuilderMode.ReadOnly);
		deckBuilderContext.Format = _sharedClasses.FormatManager.GetSafeFormat(EventContext.PlayerEvent.EventUXInfo.DeckSelectFormat);
		_sharedClasses.SceneLoader.GoToDeckBuilder(deckBuilderContext);
	}

	public void Timer_OnEnded()
	{
		UpdateComponents();
	}

	public void CardPool_OnClicked(List<uint> cardPool)
	{
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>(cardPool.Count);
		foreach (uint item in cardPool)
		{
			dictionary.Add(item, 4u);
		}
		_sharedClasses.SceneLoader.GoToDeckBuilder(new DeckBuilderContext(null, EventContext, sideboarding: false, firstEdit: false, DeckBuilderMode.ReadOnlyCollection, ambiguousFormat: false, Guid.Empty, dictionary));
	}

	public void ResignComponent_OnClicked()
	{
		SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/EventPage/Resign_Confirmation_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/EventPage/Resign_Confirmation_Text"), showCancel: true, delegate
		{
			Resign();
		});
	}

	public void SelectedDeck_CopyToDecksClicked()
	{
		if (EventContext.PlayerEvent.CourseData.CourseDeck != null)
		{
			Client_Deck client_Deck = new Client_Deck(EventContext.PlayerEvent.CourseData.CourseDeck);
			DeckFormat deckFormat = EventContext.PlayerEvent.Format;
			if (deckFormat == null)
			{
				deckFormat = _formatManager.GetSafeFormat(client_Deck.Summary.Format, null);
			}
			DeckUtilities.UpdateDeckBasedOnEventFormat(deckFormat, client_Deck);
			CreateDeck(client_Deck);
		}
	}

	public void SelectedDeck_OnDeckBoxClicked(LayoutDeckButtonBehavior deckButtonBehavior)
	{
		switch (deckButtonBehavior)
		{
		case LayoutDeckButtonBehavior.Selectable:
			SelectDeckButtonClicked();
			break;
		case LayoutDeckButtonBehavior.Fixed:
			InspectSingleDeck_OnClicked(EventContext.PlayerEvent.CourseData.CourseDeck);
			break;
		case LayoutDeckButtonBehavior.Editable:
			BuildDeckButtonClicked();
			break;
		}
	}

	private void BuildDeckButtonClicked()
	{
		DeckInfo deckInfo = null;
		if (EventContext.PlayerEvent.CourseData.CourseDeck != null)
		{
			deckInfo = DeckServiceWrapperHelpers.ToAzureModel(EventContext.PlayerEvent.CourseData.CourseDeck);
		}
		string deckSelectFormat = EventContext.PlayerEvent.EventUXInfo.DeckSelectFormat;
		if (deckInfo != null && deckInfo.sideboard.Count + deckInfo.mainDeck.Count > 0)
		{
			if (string.IsNullOrEmpty(deckInfo.format))
			{
				deckInfo.format = deckSelectFormat;
			}
			DeckBuilderContext context = new DeckBuilderContext(deckInfo, EventContext);
			List<uint> cardPool = EventContext.PlayerEvent.CourseData.CardPool;
			bool num = EventContext.PlayerEvent.EventInfo.FormatType == MDNEFormatType.Draft;
			bool flag = deckInfo.mainDeck.Exists((CardInDeck x) => cardPool.Count((uint y) => y == x.Id) != x.Quantity) || deckInfo.sideboard.Exists((CardInDeck x) => cardPool.Count((uint y) => y == x.Id) != x.Quantity);
			if (num && flag)
			{
				List<CardInDeck> list = new List<CardInDeck>();
				Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
				Dictionary<uint, string> dictionary2 = CosmeticsUtils.StylesStringToDictionary(EventContext.PlayerEvent.CourseData.CardStyles);
				list.AddRange(deckInfo.mainDeck);
				list.AddRange(deckInfo.sideboard);
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
				DeckInfo deck = deckInfo;
				EventContext eventContext = EventContext;
				Dictionary<uint, uint> cardPoolOverride = dictionary;
				Dictionary<uint, string> cardSkinOverride = dictionary2;
				context = new DeckBuilderContext(deck, eventContext, sideboarding: false, firstEdit: false, DeckBuilderMode.DeckBuilding, ambiguousFormat: false, default(Guid), cardPoolOverride, cardSkinOverride);
			}
			_sharedClasses.SceneLoader.GoToDeckBuilder(context);
			return;
		}
		DeckInfo deckInfo2 = deckInfo ?? new DeckInfo
		{
			id = Guid.NewGuid()
		};
		deckInfo2.format = deckSelectFormat;
		deckInfo2.deckTileId = EventContext.PlayerEvent.CourseData.CardPool.First();
		deckInfo2.isLoaded = true;
		DeckBuilderContext context2 = new DeckBuilderContext(deckInfo2, EventContext, sideboarding: false, firstEdit: true);
		List<CardInDeck> list2 = (from x in EventContext.PlayerEvent.CourseData.CardPool
			group x by x into x
			select new CardInDeck(x.Key, (uint)x.Count()) into x
			where x.Quantity != 0
			select x).ToList();
		switch (EventContext.PlayerEvent.EventInfo.FormatType)
		{
		case MDNEFormatType.Draft:
		{
			Dictionary<uint, string> dictionary3 = CosmeticsUtils.StylesStringToDictionary(EventContext.PlayerEvent.CourseData.CardStyles);
			EventContext eventContext2 = EventContext;
			Dictionary<uint, uint> cardPoolOverride = list2.ToDictionary((CardInDeck x) => x.Id, (CardInDeck y) => y.Quantity);
			Dictionary<uint, string> cardSkinOverride = dictionary3;
			context2 = new DeckBuilderContext(deckInfo2, eventContext2, sideboarding: false, firstEdit: true, DeckBuilderMode.DeckBuilding, ambiguousFormat: false, default(Guid), cardPoolOverride, cardSkinOverride);
			deckInfo2.name = Languages.ActiveLocProvider.GetLocalizedText("MainNav/EventPage/DefaultDraftDeckName");
			deckInfo2.mainDeck = list2;
			deckInfo2.sideboard = new List<CardInDeck>();
			_sharedClasses.SceneLoader.GoToDeckBuilder(context2);
			break;
		}
		case MDNEFormatType.Sealed:
			deckInfo2.name = Languages.ActiveLocProvider.GetLocalizedText("MainNav/EventPage/DefaultSealedDeckName");
			deckInfo2.mainDeck = new List<CardInDeck>();
			deckInfo2.sideboard = list2;
			if (!MDNPlayerPrefs.GetHasOpenedSealedPool(_sharedClasses.AccountInformation.PersonaID, EventContext.PlayerEvent.CourseData.Id))
			{
				_sharedClasses.SceneLoader.GoToSealedBoosterOpen(context2, _cardGrant?.delta.gemsDelta ?? 0);
			}
			else
			{
				_sharedClasses.SceneLoader.GoToDeckBuilder(context2);
			}
			break;
		default:
			Debug.LogWarningFormat("Unknown deck select format: {0}", deckSelectFormat);
			deckInfo2.name = "Deck";
			deckInfo2.mainDeck = list2;
			deckInfo2.sideboard = new List<CardInDeck>();
			_sharedClasses.SceneLoader.GoToDeckBuilder(context2);
			break;
		}
	}

	public void SelectDeckButtonClicked()
	{
		InspectPreconDecksWidgetData inspectPreconDecksWidget = EventContext.PlayerEvent.EventUXInfo.EventComponentData.InspectPreconDecksWidget;
		_sharedClasses.SceneLoader.GoToConstructedDeckSelect(new DeckSelectContext
		{
			EventContext = EventContext,
			PreconDeckIds = inspectPreconDecksWidget?.DeckIds
		});
	}

	public void SelectedDeck_SubmitDeck(Guid deckId, Action<Client_Deck> onSuccess)
	{
		Client_Deck deck = _sharedClasses.DecksManager.GetDeck(deckId);
		if (deck != null)
		{
			SubmitDeck(deck, onSuccess);
			return;
		}
		EventContext.PlayerEvent.CourseData.CurrentModule = PlayerEventModule.DeckSelect;
		UpdateComponents();
	}

	public void MainButton_OnPayJoinButtonClicked(EventEntryFeeInfo entryFee)
	{
		if (entryFee.CurrencyType == EventEntryCurrencyType.Gem)
		{
			if (_sharedClasses.InventoryManager.Inventory.gems >= entryFee.Quantity)
			{
				SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Confirm_Purchase_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Confirm_Purchase_Text"), showCancel: true, delegate
				{
					JoinAndPayEvent(entryFee.CurrencyType, JoinEventChoice);
				});
			}
			else if (!_sharedClasses.StoreManager.StoreStatus.DisabledTags.Contains(EProductTag.Gems))
			{
				SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Buy_Gems_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Insufficient_Gems_Text"), showCancel: true, delegate
				{
					_sharedClasses.SceneLoader.GoToStore(StoreTabType.Gems, "Go to gem store from event");
				});
			}
			else
			{
				SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Buy_Gems_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Insufficient_Gems_Store_Offline_Text"));
			}
		}
		else
		{
			JoinAndPayEvent(entryFee.CurrencyType, JoinEventChoice);
		}
	}

	public void OnBackButtonClicked(bool returnToPlayBlade)
	{
		HomePageContext homePageContext = new HomePageContext();
		if (returnToPlayBlade)
		{
			homePageContext.InitialBladeState = PlayBladeController.PlayBladeVisualStates.Events;
		}
		_sharedClasses.SceneLoader.GoToLanding(homePageContext);
	}

	public void MainButton_OnPlayButtonClicked()
	{
		switch (EventContext.PlayerEvent.CourseData.CurrentModule)
		{
		case PlayerEventModule.ClaimPrize:
			PAPA.StartGlobalCoroutine(ClaimPrize());
			break;
		case PlayerEventModule.Draft:
		case PlayerEventModule.HumanDraft:
		{
			if (!(EventContext.PlayerEvent is LimitedPlayerEvent limitedPlayerEvent))
			{
				break;
			}
			DraftState? draftState = limitedPlayerEvent.DraftPod?.DraftState;
			if (draftState.HasValue)
			{
				switch (draftState.GetValueOrDefault())
				{
				case DraftState.Podmaking:
					_sharedClasses.SceneLoader.GoToTableDraftQueueScene(EventContext);
					break;
				case DraftState.Picking:
				case DraftState.Completing:
					_sharedClasses.SceneLoader.GoToDraftScene(EventContext);
					break;
				}
			}
			break;
		}
		case PlayerEventModule.Choice:
		case PlayerEventModule.DeckSelect:
			if (FormatUtilities.IsLimited(EventContext.PlayerEvent.EventInfo.FormatType))
			{
				BuildDeckButtonClicked();
			}
			else
			{
				SelectDeckButtonClicked();
			}
			break;
		case PlayerEventModule.TransitionToMatches:
		case PlayerEventModule.WinLossGate:
		case PlayerEventModule.WinNoGate:
			if (EventHelper.PreconWithInvalidDeck(EventContext.PlayerEvent))
			{
				SelectDeckButtonClicked();
			}
			else if (_sharedClasses.ChallengeController.GetAllChallenges().Exists((KeyValuePair<Guid, PVPChallengeData> pair) => pair.Value.ChallengePlayers.ContainsKey(pair.Value.LocalPlayerId)))
			{
				_sharedClasses.SocialManager.ShowEnteringQueueWithOutgoingChallengeMessage(delegate
				{
					PlayMatch();
				});
			}
			else
			{
				PlayMatch();
			}
			break;
		case PlayerEventModule.Jumpstart:
			_sharedClasses.SceneLoader.GoToPacketSelect(EventContext);
			break;
		case PlayerEventModule.Complete:
		case PlayerEventModule.NPEUpdate:
			break;
		}
	}

	private IEnumerator ClaimPrize()
	{
		_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: true);
		if (EventContext.PlayerEvent.EventInfo.EventTags.Contains(EventTag.PhysicalPrize) && EventContext.PlayerEvent.CurrentWins == EventContext.PlayerEvent.EventUXInfo.EventComponentData.ByCourseObjectiveTrack.ChestDescriptions.Count - 1)
		{
			string csEmail = "https://mtgarena-support.wizards.com/hc/en-us/requests/new?ticket_form_id=30039837353748";
			string text = "UNDEFINED";
			IAccountClient accountClient = Pantry.Get<IAccountClient>();
			if (!string.IsNullOrEmpty(accountClient.AccountInformation.CountryCode))
			{
				text = new RegionInfo(accountClient.AccountInformation.CountryCode).DisplayName;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(Languages.ActiveLocProvider.GetLocalizedText("Events/Rewards/Physical_PopUp_Directions"));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Email: " + (WrapperController.Instance.AccountClient?.AccountInformation?.Email ?? "UNDEFINED"));
			stringBuilder.AppendLine("Country: " + text + " (" + (WrapperController.Instance.AccountClient?.AccountInformation?.CountryCode ?? "UNDEFINED") + ")");
			SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("Events/Rewards/Physical_PopUp_Title"), stringBuilder.ToString(), Languages.ActiveLocProvider.GetLocalizedText("Events/Rewards/Physical_PopUp_ContactCS"), delegate
			{
				Application.OpenURL(csEmail);
			}, Languages.ActiveLocProvider.GetLocalizedText("Events/Rewards/Physical_PopUp_ConfirmInfo"), null);
		}
		Promise<ICourseInfoWrapper> claimPrize = EventContext.PlayerEvent.ClaimPrize();
		yield return claimPrize.AsCoroutine();
		if (EventContext.PlayerEvent.HasPrize(EventContext.PlayerEvent.CurrentWins))
		{
			if (!claimPrize.Successful)
			{
				if (claimPrize.ErrorSource != ErrorSource.Debounce)
				{
					SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Prize_Claim_Error_Text"), delegate
					{
						_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: false);
						_sharedClasses.SceneLoader.GoToLanding(new HomePageContext());
					});
				}
				UpdateComponents();
				yield break;
			}
			float timeout = 2f;
			while (timeout > 0f && !_rewardController.HasPendingRewards)
			{
				timeout -= Time.deltaTime;
				yield return null;
			}
			UpdateComponents();
		}
		else
		{
			_sharedClasses.SceneLoader.GoToLanding(new HomePageContext());
		}
		_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: false);
		OnPrizeClaim?.Invoke(claimPrize.Successful);
	}

	private Promise<NewMatchCreatedConfig> GetActiveMatch()
	{
		return Pantry.Get<IActiveMatchesServiceWrapper>().GetActiveMatches().Convert((List<NewMatchCreatedConfig> p) => p?.FirstOrDefault());
	}

	private void ReconnectToMatch(NewMatchCreatedConfig activeMatch)
	{
		WrapperController.EnableLoadingIndicator(enabled: true);
		string eventId = activeMatch.eventId;
		Matchmaking matchmaking = Pantry.Get<Matchmaking>();
		EventContext eventContext = Pantry.Get<EventManager>().EventContexts.FirstOrDefault((EventContext e) => e.PlayerEvent.MatchMakingName == eventId);
		matchmaking.LaunchFromReconnect();
		WrapperController.EnableLoadingIndicator(enabled: false);
		matchmaking.JoinMatchFromReconnect(activeMatch, eventContext);
	}

	private void PlayMatch()
	{
		if (EventContext.PlayerEvent.EventInfo.IsAiOpponent)
		{
			BotControlManager.SetUpBotTool(Pantry.Get<BotTool>(), _sharedClasses.AssetLookupSystem);
		}
		GetActiveMatch().ThenOnMainThread(delegate(Promise<NewMatchCreatedConfig> promise)
		{
			if (promise.Successful && promise.Result != null)
			{
				ReconnectToMatch(promise.Result);
			}
			else
			{
				_sharedClasses.Matchmaking.SetExpectedEvent(EventContext);
				WrapperController.EnableLoadingIndicator(enabled: true);
				EventContext.PlayerEvent.JoinNewMatchQueue().ThenOnMainThread(delegate(Promise<string> p)
				{
					WrapperController.EnableLoadingIndicator(enabled: false);
					if (p.Successful)
					{
						_sharedClasses.Matchmaking.SetupEventMatch(EventContext);
					}
					else
					{
						Debug.LogError($"Error joining event queue: {p.Error}");
						SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Join_Error_Text"));
					}
				});
			}
		});
	}

	private void JoinAndPayEvent(EventEntryCurrencyType currencyType, string eventChoice)
	{
		_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: true);
		EventContext.PlayerEvent.JoinAndPay(currencyType, eventChoice).ThenOnMainThread(delegate(Promise<ICourseInfoWrapper> p)
		{
			if (!p.Successful && p.ErrorSource != ErrorSource.Debounce)
			{
				string text = ((p.Error.Code == 5019) ? Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Pay_Error_Text") : Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Join_Error_Text"));
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), text);
			}
			_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: false);
			UpdateComponents();
			if (p.Successful && (currencyType == EventEntryCurrencyType.Gem || currencyType == EventEntryCurrencyType.DraftToken || currencyType == EventEntryCurrencyType.EventToken || currencyType == EventEntryCurrencyType.SealedToken))
			{
				BIEventTracker.TrackEvent(EBiEvent.PaidEventEntry);
			}
			if (p.Successful)
			{
				CourseData courseData = EventContext.PlayerEvent.CourseData;
				if (courseData != null && courseData.CurrentModule == PlayerEventModule.Jumpstart)
				{
					_sharedClasses.SceneLoader.GoToPacketSelect(EventContext);
				}
			}
			OnJoinPayEvent?.Invoke(p.Successful);
		});
	}

	private void CreateDeck(Client_Deck deck)
	{
		if (_sharedClasses.DecksManager.ShowDeckLimitError())
		{
			return;
		}
		_sharedClasses.DecksManager.CreateDeck(deck, DeckActionType.CopyLimited.ToString(), forceCopy: true).ThenOnMainThread(delegate(Promise<Client_DeckSummary> promise)
		{
			if (promise.Successful)
			{
				WrapperDeckBuilder.ClearCachedDeck();
				SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Add_Deck_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Add_Deck_Text"));
			}
			else
			{
				SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Add_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Creation_Failure_Text"));
			}
		});
	}

	private void SubmitDeck(Client_Deck eventDeck, Action<Client_Deck> onSuccess)
	{
		WrapperDeckUtilities.setLastPlayed(eventDeck);
		EventContext.PlayerEvent.SubmitEventDeck(WrapperDeckUtilities.GetSubmitDeck(eventDeck, _sharedClasses.DecksManager)).ThenOnMainThread(delegate(Promise<Client_Deck> p)
		{
			if (p.Successful)
			{
				onSuccess?.Invoke(eventDeck);
			}
			else
			{
				Error error = p.Error;
				Debug.LogError(error.Message);
				Utils.GetDeckSubmissionErrorMessages(error, out var errTitle, out var errText);
				SystemMessageManager.Instance.ShowOk(errTitle, errText, delegate
				{
					EventContext.PlayerEvent.CourseData.CurrentModule = PlayerEventModule.DeckSelect;
					UpdateComponents();
				});
			}
		});
	}

	private void Resign()
	{
		_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: true);
		EventContext.PlayerEvent.ResignFromEvent().ThenOnMainThread(delegate(Promise<ICourseInfoWrapper> p)
		{
			_sharedClasses.SceneLoader.EnableLoadingIndicator(shouldEnable: false);
			if (!p.Successful)
			{
				if (p.ErrorSource != ErrorSource.Debounce)
				{
					SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Drop_Error_Text"));
				}
			}
			else
			{
				UpdateComponents();
			}
		});
	}
}
