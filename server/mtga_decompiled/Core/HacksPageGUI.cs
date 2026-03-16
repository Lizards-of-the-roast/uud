using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Assets.Core.Meta.Utilities;
using Assets.Core.Shared.Code;
using Core.Meta.MainNavigation.Store;
using Core.Shared.Code.Connection;
using GreClient.Network;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.Arena.TcpConnection;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Deeplink;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.Store;
using Wizards.Unification.Models.Event;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Wrapper.Draft;
using Wotc.Mtgo.Gre.External.Messaging;

public class HacksPageGUI : IDebugGUIPage
{
	private readonly PAPA _papa;

	private string _overrideTimeString = string.Empty;

	private string _autoPickEnableText = "Enable";

	private string _draftOverlayDebugText = "Enable";

	private DoorbellMockGUI _doorbellMockGUI = new DoorbellMockGUI();

	private BackgroundDownloadingGUI _backgroundDownloadingGUI = new BackgroundDownloadingGUI();

	private FDMessagesDebugGUI _fDMessagesDebugGUI = new FDMessagesDebugGUI();

	private Vector2 _shortcutsTabScrollPosition;

	private DebugInfoIMGUIOnGui _GUI;

	private static GUIStyle _textInputStyleCache;

	public static string BOOSTER_OPEN_OVERRIDE_JSON = "";

	private string _rewardJSON = "";

	private string _draftInternalEventName = "";

	private string _prizeWallId = "TestStandalonePrizeWall";

	private string _videoUrl = "";

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Hacks;

	public string TabName => "Hacks";

	public bool HiddenInTab => false;

	private static GUIStyle _textInputStyle
	{
		get
		{
			if (_textInputStyleCache == null)
			{
				_textInputStyleCache = new GUIStyle(GUI.skin.GetStyle("TextField"));
			}
			return _textInputStyleCache;
		}
	}

	public HacksPageGUI(PAPA papa)
	{
		_papa = papa;
	}

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		return true;
	}

	public void OnGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical(GUILayout.MaxWidth(520f));
		DrawLeftColumn();
		GUILayout.EndVertical();
		GUILayout.BeginVertical(GUILayout.MaxWidth(520f));
		DrawRightColumn();
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	private void DisconnectFrontDoor(TcpConnectionCloseType status, string reason)
	{
		Pantry.Get<IFrontDoorConnectionServiceWrapper>()?.Close(status, "HACK_" + reason);
	}

	private void DisconnectMatchDoor(IGREConnection greConnection)
	{
		greConnection.Close(TcpConnectionCloseType.NormalClosure, "HACK");
	}

	private void ShowDisconnectButton(TcpConnectionCloseType status, string title)
	{
		if (_GUI.ShowDebugButton(title, 500f))
		{
			FrontDoorConnectionManager frontDoorConnectionManager = Pantry.Get<FrontDoorConnectionManager>();
			if (frontDoorConnectionManager != null && frontDoorConnectionManager.IdleTimerActive)
			{
				DisconnectFrontDoor(status, title);
			}
			else
			{
				Debug.Log("Can't force Client Idle disconnect when idle timer is inactive (during matches for instance).");
			}
		}
	}

	private void DrawLeftColumn()
	{
		_shortcutsTabScrollPosition = _GUI.BeginScrollView(_shortcutsTabScrollPosition);
		if (_GUI.ShowDebugButton("Force Restart Game", 500f))
		{
			UnityEngine.Object.FindObjectOfType<Bootstrap>()?.RestartGame("Debug Hack");
		}
		if (Pantry.CurrentEnvironment != EnvironmentDescription.NullEnvironment && Pantry.Get<IFrontDoorConnectionServiceWrapper>() != null)
		{
			ShowDisconnectButton(TcpConnectionCloseType.NormalClosure, $"Force disconnect - {TcpConnectionCloseType.NormalClosure}");
			ShowDisconnectButton(TcpConnectionCloseType.ClientSideIdle, $"Force disconnect - {TcpConnectionCloseType.ClientSideIdle}");
			ShowDisconnectButton(TcpConnectionCloseType.ClosedByServer, $"Force disconnect - {TcpConnectionCloseType.ClosedByServer}");
		}
		if ((bool)MatchSceneManager.Instance)
		{
			string text = (MatchSceneManager.Instance.AutoConnectToMatchServer ? "Disable" : "Enable");
			if (_GUI.ShowDebugButton(text + " Match Autoconnect", 500f))
			{
				MatchSceneManager.Instance.AutoConnectToMatchServer = !MatchSceneManager.Instance.AutoConnectToMatchServer;
			}
			if (_GUI.ShowDebugButton("Join Match Now", 500f))
			{
				MatchSceneManager.Instance.JoinMatchNow();
			}
		}
		MatchManager matchManager = _papa.MatchManager;
		if (matchManager != null)
		{
			IGREConnection greConnection = matchManager.GreConnection;
			if (greConnection != null)
			{
				if (_GUI.ShowDebugButton("Force disconnect MatchDoor", 500f))
				{
					DisconnectMatchDoor(greConnection);
				}
				if (_GUI.ShowDebugButton("Force disconnect Match and FrontDoor", 500f))
				{
					DisconnectFrontDoor(TcpConnectionCloseType.NormalClosure, "Force disconnect Match and FrontDoor");
					DisconnectMatchDoor(greConnection);
				}
				if (_GUI.ShowDebugButton("Load Wrapper Scene", 500f))
				{
					PAPA.SceneLoading.LoadWrapperScene();
				}
			}
		}
		if (_GUI.ShowDebugButton("FORCE BUG: Nonfatal Exception", 500f))
		{
			ForceNonFatalException();
		}
		if (_GUI.ShowDebugButton("FORCE CRASH: Fatal, produce Unity minidump", 500f))
		{
			Application.ForceCrash(0);
		}
		if (_GUI.ShowDebugButton("FORCE LONG ERROR: Long error message for Backtrace", 500f))
		{
			ForceBacktraceErrorLogOverflow();
		}
		if (_GUI.ShowDebugButton("FORCE OOM: Force an Out-Of-Memory crash", 500f))
		{
			ForceOom();
		}
		WrapperController instance = WrapperController.Instance;
		if (instance != null)
		{
			if (_GUI.ShowDebugButton("Go to Landing Scene", 500f))
			{
				TestLandingScene();
			}
			if (_GUI.ShowDebugButton("Go to Landing Scene (post game, won 2 of 3)", 500f))
			{
				TestLandingScenePostGame();
			}
			GUILayout.BeginHorizontal();
			if (_GUI.ShowDebugButton("Show Standalone Prize Wall: ", 210f))
			{
				SceneLoader.GetSceneLoader().GoToPrizeWall(_prizeWallId, new PrizeWallContext(NavContentType.Home));
			}
			_prizeWallId = GUILayout.TextField(_prizeWallId, _textInputStyle);
			GUILayout.EndHorizontal();
			if (_GUI.ShowDebugButton("Test Sideboard Constructed", 500f))
			{
				TestSideboardingConstructed();
			}
			if (_GUI.ShowDebugButton(_autoPickEnableText + " Draft Autopick", 500f))
			{
				_autoPickEnableText = (ToggleDraftAutopick() ? "Disable" : "Enable");
			}
			if (_GUI.ShowDebugButton(_draftOverlayDebugText + " Debug Overlay in Draft", 500f))
			{
				DraftContentController draftContentController = SceneLoader.GetSceneLoader().GetDraftContentController();
				if ((bool)draftContentController)
				{
					_draftOverlayDebugText = (draftContentController.ToggleDraftDebugDisplay() ? "Disable" : "Enable");
				}
			}
			GUILayout.BeginHorizontal();
			if (_GUI.ShowDebugButton("Test Rewards Display with JSON:", 210f))
			{
				TestRewardsFakeData(_rewardJSON);
			}
			_rewardJSON = GUILayout.TextField(_rewardJSON, _textInputStyle);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Booster Open Override:");
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (_GUI.ShowDebugButton("Clear", 50f))
			{
				BOOSTER_OPEN_OVERRIDE_JSON = "";
			}
			BOOSTER_OPEN_OVERRIDE_JSON = GUILayout.TextField(BOOSTER_OPEN_OVERRIDE_JSON, _textInputStyle);
			GUILayout.EndHorizontal();
			if (_GUI.ShowDebugButton("Test Set Deferred Deeplink", 500f))
			{
				DeepLinking.SetDeferredURL("https://play.mtgarena.com/store/gems");
			}
			if (_GUI.ShowDebugButton("Test Deck Upgrade", 500f))
			{
				TestDeckUpgrade();
			}
			if (_GUI.ShowDebugButton("Debug Pack Select", 500f))
			{
				TestPacketSelect();
			}
			if (_GUI.ShowDebugButton("Drop All Events", 500f))
			{
				DropAllEvents();
			}
			if (_GUI.ShowDebugButton("Drop All Drafts", 500f))
			{
				DropAllDrafts();
			}
			GUILayout.BeginHorizontal();
			if (_GUI.ShowDebugButton("Drop Draft InternalEventName: ", 210f))
			{
				DropSpecificDraft(_draftInternalEventName);
			}
			_draftInternalEventName = GUILayout.TextField(_draftInternalEventName, _textInputStyle);
			GUILayout.EndHorizontal();
		}
		AccountInformation accountInformation = _papa.AccountClient?.AccountInformation;
		if (accountInformation != null)
		{
			if (_GUI.ShowDebugButton("Test Renewal Education", 500f))
			{
				TestRenewalPreview();
			}
			if (_GUI.ShowDebugButton("Test Renewal Rewards", 500f))
			{
				PAPA.StartGlobalCoroutine(TestRenewalRewards());
			}
			if (_GUI.ShowDebugButton("Reset First Time Language", 500f))
			{
				MDNPlayerPrefs.PLAYERPREFS_HasSelectedInitialLanguage = false;
			}
			if (_GUI.ShowDebugButton("Reset Tutorial Skipped", 500f))
			{
				MDNPlayerPrefs.SetOnboardingSkipped(accountInformation.PersonaID, value: false);
			}
			if (_GUI.ShowDebugButton("Reset Handheld MOZ Tutorial", 500f))
			{
				MDNPlayerPrefs.SetHasSeenHandheldMOZTutorial(accountInformation.PersonaID, value: false);
			}
			if (_GUI.ShowDebugButton("Reset Notify Card Sleeves", 500f))
			{
				MDNPlayerPrefs.FirstTimeSleeveNotify = true;
			}
			if (_GUI.ShowDebugButton("Reset Notify Cosmetics", 500f))
			{
				MDNPlayerPrefs.FirstTimeCosmeticNotify = true;
			}
			if (instance != null && _GUI.ShowDebugButton("Reset Renewal Flag", 500f))
			{
				MDNPlayerPrefs.ClearRotationEducationViewed(accountInformation.PersonaID, instance.RenewalManager.GetCurrentRenewalId());
			}
		}
		bool dEBUG_AlwaysSurvey = MDNPlayerPrefs.DEBUG_AlwaysSurvey;
		if (_GUI.ShowDebugButton($"Always Post-Match Survey: {dEBUG_AlwaysSurvey}", 500f))
		{
			MDNPlayerPrefs.DEBUG_AlwaysSurvey = !dEBUG_AlwaysSurvey;
		}
		if (_GUI.ShowDebugButton("Store - ForceCrashAfterPurchase currently:" + (StoreManager.ForceCrashAfterPurchase ? "true" : "false"), 500f))
		{
			StoreManager.ForceCrashAfterPurchase = !StoreManager.ForceCrashAfterPurchase;
		}
		if (_GUI.ShowDebugButton("Store - ForceCrashBeforeEntitlements currently:" + (StoreManager.ForceCrashBeforeEntitlements ? "true" : "false"), 500f))
		{
			StoreManager.ForceCrashBeforeEntitlements = !StoreManager.ForceCrashBeforeEntitlements;
		}
		if (_GUI.ShowDebugButton("Store - DebugAddFakeListingToCatalogRequest currently:" + (StoreManager.DebugAddFakeListingToCatalogRequest ? "true" : "false"), 500f))
		{
			StoreManager.DebugAddFakeListingToCatalogRequest = !StoreManager.DebugAddFakeListingToCatalogRequest;
		}
		if (_GUI.ShowDebugButton("Store - Multi purchasing :" + (StoreUtils.MultiPurchasing ? "true" : "false"), 500f))
		{
			StoreUtils.MultiPurchasing = !StoreUtils.MultiPurchasing;
		}
		if (_GUI.ShowDebugButton($"Toggle HashAllFilesOnStartup currently:{MDNPlayerPrefs.HashAllFilesOnStartup}", 500f))
		{
			MDNPlayerPrefs.HashAllFilesOnStartup = !MDNPlayerPrefs.HashAllFilesOnStartup;
		}
		if (_GUI.ShowDebugButton($"Toggle HashFilesSkippedOnce currently:{MDNPlayerPrefs.HashFilesSkippedOnce}", 500f))
		{
			MDNPlayerPrefs.HashFilesSkippedOnce = !MDNPlayerPrefs.HashFilesSkippedOnce;
		}
		if (_GUI.ShowDebugButton($"Toggle KeepUnusedBundles currently: {MDNPlayerPrefs.KeepUnusedBundles}", 500f))
		{
			MDNPlayerPrefs.KeepUnusedBundles = !MDNPlayerPrefs.KeepUnusedBundles;
			if (!MDNPlayerPrefs.KeepUnusedBundles)
			{
				Pantry.Get<FrontDoorConnectionManager>().RestartGame("Clear Unused Bundles Requested");
			}
		}
		if (_GUI.ShowDebugButton($"Toggle ResourceErrorLoggerShouldThrowOnAssetBundleError currently:{MDNPlayerPrefs.DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError}", 500f))
		{
			MDNPlayerPrefs.DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError = !MDNPlayerPrefs.DEBUG_ResourceErrorLoggerShouldThrowOnAssetBundleError;
		}
		if (_GUI.ShowDebugButton("Show Review", 500f))
		{
			PAPA.StartGlobalCoroutine(PlatformContext.GetReviewContext().Coroutine_RequestPlatformStoreReview());
		}
		if (_GUI.ShowDebugButton("Clear Review Flags", 500f))
		{
			MDNPlayerPrefs.ShowReviewRequest = true;
		}
		if (_GUI.ShowDebugButton($"Force update: {MDNPlayerPrefs.ForceUpdate}", 500f))
		{
			MDNPlayerPrefs.ForceUpdate = !MDNPlayerPrefs.ForceUpdate;
		}
		if (_GUI.ShowDebugButton("Force show previous cycling tip (stops autocycling)", 500f))
		{
			CyclingTipsView[] array = UnityEngine.Object.FindObjectsOfType<CyclingTipsView>();
			foreach (CyclingTipsView obj in array)
			{
				obj.StopTips();
				obj.SetPreviousTipNoAnimation();
			}
		}
		if (_GUI.ShowDebugButton("Force show next cycling tip (stops autocycling)", 500f))
		{
			CyclingTipsView[] array = UnityEngine.Object.FindObjectsOfType<CyclingTipsView>();
			foreach (CyclingTipsView obj2 in array)
			{
				obj2.StopTips();
				obj2.SetNextTipNoAnimation();
			}
		}
		if (_GUI.ShowDebugButton("Load EOE Cinematic Scene", 500f))
		{
			UrlOpener.OpenURL("unitydl://" + DeepLinking.DEEPLINK_HOST + "/cinematic/EOE_Cinematic");
		}
		if (_GUI.ShowDebugButton("Load ECL Cinematic Scene", 500f))
		{
			UrlOpener.OpenURL("unitydl://" + DeepLinking.DEEPLINK_HOST + "/cinematic/ECL_Cinematic");
		}
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Video To Load (URL, ALT asset name, or local file path)", 100f);
		_videoUrl = GUILayout.TextField(_videoUrl, _textInputStyle);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (_GUI.ShowDebugButton("Play Video using ALT (direct audio)", 500f))
		{
			UrlOpener.OpenURL("unitydl://" + DeepLinking.DEEPLINK_HOST + "/cinematic/VideoScene?videoUrl=" + _videoUrl);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (_GUI.ShowDebugButton("Play Video from URL (direct audio)", 500f))
		{
			UrlOpener.OpenURL("unitydl://" + DeepLinking.DEEPLINK_HOST + "/cinematic/VideoScene?videoUrl=" + _videoUrl + "&videoPlayLookupMode=url");
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (_GUI.ShowDebugButton("Play Video using ALT (wwise audio)", 500f))
		{
			UrlOpener.OpenURL("unitydl://" + DeepLinking.DEEPLINK_HOST + "/cinematic/VideoScene?videoPlayMode=wwise&videoUrl=" + _videoUrl);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (_GUI.ShowDebugButton("Play Video from URL (wwise audio)", 500f))
		{
			UrlOpener.OpenURL("unitydl://" + DeepLinking.DEEPLINK_HOST + "/cinematic/VideoScene?videoPlayMode=wwise&videoUrl=" + _videoUrl + "&videoPlayLookupMode=url");
		}
		GUILayout.EndHorizontal();
		string text2 = (MDNPlayerPrefs.DEBUG_IncludeAllCardTestDeckFormats ? "ON" : "OFF");
		if (_GUI.ShowDebugButton("Include 'All Card' Test Deck Formats: " + text2, 500f))
		{
			MDNPlayerPrefs.DEBUG_IncludeAllCardTestDeckFormats = !MDNPlayerPrefs.DEBUG_IncludeAllCardTestDeckFormats;
			Pantry.Get<FormatManager>().RefreshFormats();
		}
		ShowCarouselTime();
		_doorbellMockGUI.DoOnGUI();
		_fDMessagesDebugGUI.DoOnGUI();
		_backgroundDownloadingGUI.DoOnGUI();
		GUILayout.EndScrollView();
	}

	private void DrawRightColumn()
	{
		if (!(WrapperController.Instance == null))
		{
			WrapperController.DebugFlags debugFlag = WrapperController.Instance.DebugFlag;
			_GUI.ShowLabel("When entering HomeScreen:");
			debugFlag.VouchersPopup = _GUI.ShowToggle(debugFlag.VouchersPopup, "Test Vouchers Popup");
			debugFlag.SeasonPayoutPopup = _GUI.ShowToggle(debugFlag.SeasonPayoutPopup, "Test SeasonPayout Popup");
			debugFlag.EventPayoutPopup = _GUI.ShowToggle(debugFlag.EventPayoutPopup, "Test EventPayout Popup");
			debugFlag.MythicQualifyPopup = _GUI.ShowToggle(debugFlag.MythicQualifyPopup, "Test MythicQualify Popup");
			debugFlag.BannedPopup = _GUI.ShowToggle(debugFlag.BannedPopup, "Test Banned Popup");
			debugFlag.LoginGrantPopup = _GUI.ShowToggle(debugFlag.LoginGrantPopup, "Test Login Grant Popup");
			debugFlag.SetAnnouncePopup = _GUI.ShowToggle(debugFlag.SetAnnouncePopup, "Test SetAnnounce Popup");
			debugFlag.MOZTutorialPopup = _GUI.ShowToggle(debugFlag.MOZTutorialPopup, "Test MOZ Tutorial Popup");
			WrapperController.Instance.DebugFlag = debugFlag;
		}
	}

	private static void ForceNonFatalException()
	{
		((object)null).GetHashCode();
	}

	private static void ForceBacktraceErrorLogOverflow()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Forced error message that is very long.  ");
		char[] array = new char[254];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = 'A';
		}
		stringBuilder.Append(array);
		Debug.LogError(stringBuilder.ToString());
	}

	private static void ForceOom()
	{
		List<Guid> list = new List<Guid>(1000000);
		while (true)
		{
			list.Add(Guid.NewGuid());
		}
	}

	private void TestLandingScene()
	{
		SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
	}

	private void TestLandingScenePostGame()
	{
		SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext
		{
			PostMatchContext = new PostMatchContext
			{
				WonGame = true,
				GamesWon = 2
			}
		});
	}

	private void TestSideboardingConstructed()
	{
		DeckMessage deckMessage = new DeckMessage();
		DeckInfo selectedDeckInfo = SceneLoader.GetSceneLoader().GetSelectedDeckInfo();
		if (selectedDeckInfo != null)
		{
			foreach (CardInDeck item in selectedDeckInfo.mainDeck)
			{
				for (int i = 0; i < item.Quantity; i++)
				{
					deckMessage.DeckCards.Add(item.Id);
				}
			}
			foreach (CardInDeck item2 in selectedDeckInfo.sideboard)
			{
				for (int j = 0; j < item2.Quantity; j++)
				{
					deckMessage.SideboardCards.Add(item2.Id);
				}
			}
		}
		DeckInfo deck = new DeckInfo
		{
			format = "Standard",
			name = "SB-C Test",
			mainDeck = (from id in deckMessage.DeckCards
				group id by id into g
				select new CardInDeck(g.Key, (uint)g.Count())).ToList(),
			sideboard = (from id in deckMessage.SideboardCards
				group id by id into g
				select new CardInDeck(g.Key, (uint)g.Count())).ToList()
		};
		EventContext evt = null;
		DeckBuilderContext context = new DeckBuilderContext(deck, evt, sideboarding: true);
		SceneLoader.GetSceneLoader().GoToDeckBuilder(context);
	}

	private bool ToggleDraftAutopick()
	{
		DraftContentController draftContentController = SceneLoader.GetSceneLoader().GetDraftContentController();
		if ((bool)draftContentController)
		{
			draftContentController.AutoPickCards = !draftContentController.AutoPickCards;
			return draftContentController.AutoPickCards;
		}
		return false;
	}

	private void TestRewardsFakeData(string rewardJson)
	{
		ContentControllerRewardsTestUtils.TEST_ShowRewards(SceneLoader.GetSceneLoader().GetRewardsContentController(), rewardJson);
	}

	private void TestDeckUpgrade()
	{
		SceneLoader.GetSceneLoader().TestRewardTreeDeckUpgrade();
	}

	private void TestPacketSelect()
	{
		SceneLoader.GetSceneLoader().GoToPacketSelect(null);
	}

	private void DropAllEvents()
	{
		FrontDoorConnectionAWS fDCAWS = Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS;
		foreach (EventContext eventContext in WrapperController.Instance.EventManager.EventContexts)
		{
			fDCAWS.DropFromEvent(eventContext.PlayerEvent.EventInfo.InternalEventName);
		}
	}

	private void DropSpecificDraft(string internalEventName)
	{
		if (WrapperController.Instance.EventManager.EventContexts.Exists((EventContext x) => x.PlayerEvent.EventInfo.InternalEventName == internalEventName))
		{
			IEventsServiceWrapper events = Pantry.Get<IEventsServiceWrapper>();
			events.DropFromAllPodQueues().Then((Promise<ModuleResponse> p) => events.DropFromEvent(internalEventName));
			SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			SceneLoader.GetSceneLoader().TryGetEventPageContentController()?.ClearCachedEventPages();
		}
	}

	private void DropAllDrafts()
	{
		IEventsServiceWrapper events = Pantry.Get<IEventsServiceWrapper>();
		events.DropFromAllPodQueues().Then(delegate
		{
			foreach (EventContext eventContext in WrapperController.Instance.EventManager.EventContexts)
			{
				if (eventContext.PlayerEvent.EventInfo.FormatType == MDNEFormatType.Draft)
				{
					events.DropFromEvent(eventContext.PlayerEvent.EventInfo.InternalEventName);
				}
			}
		});
		SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
		SceneLoader.GetSceneLoader().TryGetEventPageContentController()?.ClearCachedEventPages();
	}

	private void TestRenewalPreview()
	{
		RotationPreviewPopup rotationPreviewPopup = SceneLoader.GetSceneLoader().GetRotationPreviewPopup();
		RenewalPreviewPopup renewalPreview = SceneLoader.GetSceneLoader().GetRenewalPreviewPopup();
		SetSparkyEffects(active: false);
		rotationPreviewPopup.Init(delegate
		{
			renewalPreview.Init(delegate
			{
				SetSparkyEffects(active: true);
			});
		});
	}

	private IEnumerator TestRenewalRewards()
	{
		RotationPreviewPopup rotationPreviewPopup = SceneLoader.GetSceneLoader().GetRotationPreviewPopup();
		RenewalPopup renewalPopup = SceneLoader.GetSceneLoader().GetRenewalPopup();
		ClientInventoryUpdateReportItem inventoryUpdate = RenewalPopup.TEST_CreateTestInventoryUpdate();
		SetSparkyEffects(active: false);
		rotationPreviewPopup.Init(delegate
		{
			renewalPopup.Init(_papa.AssetLookupSystem, _papa.CardDatabase, _papa.CardViewBuilder, inventoryUpdate, _papa.KeyBoardManager);
		});
		yield return new WaitUntil(() => renewalPopup.IsShowing);
		yield return new WaitUntil(() => !renewalPopup.IsShowing);
		SetSparkyEffects(active: true);
	}

	private void SetSparkyEffects(bool active)
	{
		if (SceneObjectBeacon.Beacons.TryGetValue("Sparky", out var value))
		{
			value.SetActive(active);
		}
		if (SceneObjectBeacon.Beacons.TryGetValue("SparkyHightlight_ColorMastery", out value))
		{
			value.SetActive(active);
		}
	}

	private void ShowCarouselTime()
	{
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("UTC Timestamp", 100f);
		_GUI.ShowTextField(ServerGameTime.GameTime.ToString(CultureInfo.CurrentCulture));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("UTC Override", 100f);
		_overrideTimeString = _GUI.ShowCarouselTextField(_overrideTimeString);
		GUILayout.EndHorizontal();
		if (DateTime.TryParse(_overrideTimeString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			ServerGameTime.SetClientOverride(result);
			_GUI.SetCarouselTextStyleColour(isGreen: true);
		}
		else
		{
			ServerGameTime.SetClientOverride(null);
			_GUI.SetCarouselTextStyleColour(isGreen: false);
		}
	}
}
