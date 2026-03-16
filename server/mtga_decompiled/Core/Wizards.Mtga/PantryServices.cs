using System;
using System.Collections.Generic;
using Assets.Core.Code.AssetBundles;
using Core.Achievements;
using Core.Code.AssetBundles.Manifest;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.ClientFeatureToggle;
using Core.Code.Decks;
using Core.Code.Input;
using Core.Code.PlayerInbox;
using Core.Code.PrizeWall;
using Core.MainNavigation.RewardTrack;
using Core.Meta.Cards.Views;
using Core.Meta.MainNavigation;
using Core.Meta.MainNavigation.Achievements;
using Core.Meta.MainNavigation.Challenge;
using Core.Meta.MainNavigation.Notifications;
using Core.Meta.MainNavigation.PopUps;
using Core.Meta.MainNavigation.SocialV2;
using Core.Meta.MainNavigation.SystemMessage;
using Core.Meta.MainNavigation.Tournaments;
using Core.Meta.NewPlayerExperience.Graph;
using Core.Meta.Social;
using Core.Meta.UI;
using Core.NPEStitcher;
using Core.Shared.Code;
using Core.Shared.Code.CardFilters;
using Core.Shared.Code.Connection;
using Core.Shared.Code.DebugTools;
using Core.Shared.Code.Network;
using Core.Shared.Code.Providers;
using Core.Shared.Code.ServiceFactories;
using Core.Shared.Code.WrapperFactories;
using MTGA.KeyboardManager;
using MTGA.Loc;
using MTGA.Social;
using MovementSystem;
using Platforms;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Network;
using SharedClientCore.SharedClientCore.Code.PVPChallenge;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.MDN;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Credits;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Diagnostics;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PlayBlade;
using Wizards.Mtga.PreferredPrinting;
using Wizards.Mtga.PrivateGame;
using Wizards.Mtga.Store;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.LearnMore;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Wizards.Mtga;

public static class PantryServices
{
	public static Dictionary<Type, Func<object>> Static = new Dictionary<Type, Func<object>>
	{
		{
			typeof(IBILogger),
			BILoggerWrapperFactory.Create
		},
		{
			typeof(EnvironmentManager),
			EnvironmentManager.Create
		},
		{
			typeof(ISqlHelper),
			SqlHelperFactory.Create
		}
	};

	public static Dictionary<Type, Func<object>> Application = new Dictionary<Type, Func<object>>
	{
		{
			typeof(SettingsMenuHost),
			SettingsMenuHost.Create
		},
		{
			typeof(FrontDoorConnectionManager),
			FrontDoorConnectionManager.Create
		},
		{
			typeof(ConnectionManager),
			ConnectionManager.Create
		},
		{
			typeof(Matchmaking),
			Matchmaking.Create
		},
		{
			typeof(ConnectionStatusResponder),
			ConnectionStatusResponder.Create
		},
		{
			typeof(ConnectionIndicator),
			ConnectionIndicator.Create
		},
		{
			typeof(IAccountClient),
			WizardsAccountsClient.Create
		},
		{
			typeof(ResourceErrorMessageManager),
			ResourceErrorMessageManager.Create
		},
		{
			typeof(ISplineMovementSystem),
			SplineMovementSystem.Create
		},
		{
			typeof(IUnityObjectPool),
			() => PlatformContext.CreateUnityPool("Shared", keepAlive: true, null, Pantry.Get<ISplineMovementSystem>())
		},
		{
			typeof(IObjectPool),
			PlatformContext.CreateObjectPool
		},
		{
			typeof(IClientLocProvider),
			LocalizationManagerFactory.Create
		},
		{
			typeof(IFontProvider),
			FontManagerFactory.Create
		},
		{
			typeof(AssetLookupManager),
			AssetLookupManagerFactory.Create
		},
		{
			typeof(TooltipSystem),
			TooltipSystemFactory.Create
		},
		{
			typeof(DeckViewBuilder),
			DeckViewBuilder.Create
		},
		{
			typeof(ICreditsDataProvider),
			CreditsDataProvider.Create
		},
		{
			typeof(ICreditsTextProvider),
			CreditsTextProvider.PantryCreate
		},
		{
			typeof(ILearnToPlayContentBuilder),
			LearnToPlayContentBuilder.PantryCreate
		},
		{
			typeof(ITableOfContentsSectionBuilder),
			TableOfContentsBuilder.PantryCreate
		},
		{
			typeof(ITutorialLauncher),
			TutorialLauncher.PantryCreate
		},
		{
			typeof(KeyboardManager),
			() => new KeyboardManager()
		},
		{
			typeof(IActionSystem),
			ActionSystemFactory.Create
		},
		{
			typeof(PopupManager),
			() => new PopupManager(Pantry.Get<KeyboardManager>(), Pantry.Get<IActionSystem>())
		},
		{
			typeof(CardMaterialBuilder),
			CardMaterialBuilderFactory.Create
		},
		{
			typeof(IEmoteDataProvider),
			EmoteDataProviderFactory.Create
		},
		{
			typeof(GlobalCoroutineExecutor),
			GlobalCoroutineExecutorFactory.Create
		},
		{
			typeof(MatchManager),
			MatchManagerFactory.Create
		},
		{
			typeof(CardColorCaches),
			CardColorCaches.Create
		},
		{
			typeof(ISetMetadataProvider),
			SetMetadataProvider.Create
		},
		{
			typeof(IThermalStatusProvider),
			ThermalStatusProviderFactory.Create
		},
		{
			typeof(ICPUUsageProvider),
			CPUUsageProviderFactory.Create
		},
		{
			typeof(CardDatabaseLoader),
			LoadCardDatabaseUniTask.CreateLoader
		},
		{
			typeof(CardDatabase),
			() => Pantry.Get<CardDatabaseLoader>().CardDatabase
		},
		{
			typeof(ICardDatabaseAdapter),
			Pantry.Get<CardDatabase>
		},
		{
			typeof(ICardDataProvider),
			() => Pantry.Get<CardDatabase>().CardDataProvider
		},
		{
			typeof(IAbilityDataProvider),
			() => Pantry.Get<CardDatabase>().AbilityDataProvider
		},
		{
			typeof(IGreLocProvider),
			() => Pantry.Get<CardDatabase>().GreLocProvider
		},
		{
			typeof(ISystemMessageManager),
			SystemMessageFactory.Create
		},
		{
			typeof(StandaloneStoreConfig),
			() => new StandaloneStoreConfig()
		}
	};

	public static Dictionary<Type, Func<object>> Environment = new Dictionary<Type, Func<object>>
	{
		{
			typeof(IFrontDoorConnectionServiceWrapper),
			FrontDoorConnectionServiceWrapperFactory.Create
		},
		{
			typeof(IPreconDeckServiceWrapper),
			PreconDeckServiceWrapperFactory.Create
		},
		{
			typeof(IListingServiceWrapper),
			ListingServiceWrapperFactory.Create
		},
		{
			typeof(ICosmeticsServiceWrapper),
			CosmeticsServiceWrapperFactory.Create
		},
		{
			typeof(ListingsProvider),
			() => new ListingsProvider(Pantry.Get<IListingServiceWrapper>())
		},
		{
			typeof(CosmeticsProvider),
			CosmeticsProviderFactory.Create
		},
		{
			typeof(WrapperSceneManagement),
			WrapperSceneManagement.Create
		},
		{
			typeof(IEventsServiceWrapper),
			EventsServiceWrapperFactory.Create
		},
		{
			typeof(IInventoryServiceWrapper),
			InventoryServiceWrapperFactory.Create
		},
		{
			typeof(IDeckServiceWrapper),
			DeckServiceWrapperFactory.Create
		},
		{
			typeof(IPreferredPrintingServiceWrapper),
			PreferredPrintingServiceWrapperFactory.Create
		},
		{
			typeof(IPreferredPrintingDataProvider),
			PreferredPrintingDataProvider.Create
		},
		{
			typeof(IDesignerMetadataServiceWrapper),
			AwsDesignerMetadataServiceWrapper.Create
		},
		{
			typeof(IMercantileServiceWrapper),
			MercantileServiceWrapperFactory.Create
		},
		{
			typeof(ISurveyConfigProvider),
			SurveyConfigProvider.Create
		},
		{
			typeof(IFormatsServiceWrapper),
			FormatsServiceWrapperFactory.Create
		},
		{
			typeof(IQuestServiceWrapper),
			QuestServiceWrapperFactory.Create
		},
		{
			typeof(IRenewalServiceWrapper),
			RenewalServiceWrapperFactory.Create
		},
		{
			typeof(IPlayerRankServiceWrapper),
			PlayerRankServiceWrapperFactory.Create
		},
		{
			typeof(IStartHookServiceWrapper),
			StartHookServiceWrapperFactory.Create
		},
		{
			typeof(INodeGraphServiceWrapper),
			NodeGraphServiceWrapperFactory.Create
		},
		{
			typeof(IMatchdoorServiceWrapper),
			MatchDoorServiceWrapperFactory.Create
		},
		{
			typeof(ISystemMessageServiceWrapper),
			SystemMessageServiceWrapperFactory.Create
		},
		{
			typeof(IBotMatchServiceWrapper),
			BotMatchServiceWrapperFactory.Create
		},
		{
			typeof(IActiveMatchesServiceWrapper),
			ActiveMatchesServiceWrapperFactory.Create
		},
		{
			typeof(IPlayerPrefsServiceWrapper),
			PlayerPrefsServiceWrapperFactory.Create
		},
		{
			typeof(IPlayBladeConfigServiceWrapper),
			AwsPlayBladeConfigServiceWrapper.Create
		},
		{
			typeof(INetDeckFolderServiceWrapper),
			NetDeckFolderServiceWrapper.Create
		},
		{
			typeof(IPrizeWallServiceWrapper),
			PrizeWallServiceWrapperFactory.Create
		},
		{
			typeof(IPlayBladeSelectionProvider),
			PlayBladeSelectionProvider.Create
		},
		{
			typeof(RecentlyPlayedDataProvider),
			RecentlyPlayedDataProvider.Create
		},
		{
			typeof(PlayerPrefsDataProvider),
			PlayerPrefsDataProvider.Create
		},
		{
			typeof(ViewedEventsDataProvider),
			ViewedEventsDataProvider.Create
		},
		{
			typeof(PlayBladeConfigDataProvider),
			PlayBladeConfigDataProvider.Create
		},
		{
			typeof(QuestDataProvider),
			QuestDataProvider.Create
		},
		{
			typeof(DeckDataProvider),
			() => new DeckDataProvider(Pantry.Get<IDeckServiceWrapper>())
		},
		{
			typeof(ICustomTokenProvider),
			() => new CustomTokenProvider(Pantry.Get<IInventoryServiceWrapper>())
		},
		{
			typeof(SeasonAndRankDataProvider),
			SeasonAndRankDataProvider.Create
		},
		{
			typeof(NetDeckFolderDataProvider),
			NetDeckFolderDataProvider.Create
		},
		{
			typeof(TutorialSkippedFromInGameSignal),
			TutorialSkippedFromInGameSignal.Create
		},
		{
			typeof(VoucherDataProvider),
			VoucherDataProvider.Create
		},
		{
			typeof(DesignerMetadataProvider),
			DesignerMetadataProvider.Create
		},
		{
			typeof(ISetMetadataServiceWrapper),
			SetMetadataServiceWrapperFactory.Create
		},
		{
			typeof(IPlayerInboxServiceWrapper),
			PlayerInboxServiceWrapperFactory.Create
		},
		{
			typeof(PlayerInboxDataProvider),
			PlayerInboxDataProvider.Create
		},
		{
			typeof(ClientFeatureToggleDataProvider),
			ClientFeatureToggleDataProvider.Create
		},
		{
			typeof(StaticContentController),
			StaticContentControllerFactory.Create
		},
		{
			typeof(IConflictingAccountsServiceWrapper),
			ConflictingAccountsServiceWrapperFactory.Create
		},
		{
			typeof(ICardNicknamesProvider),
			CardNicknameProvider.Create
		},
		{
			typeof(IEmergencyCardBansProvider),
			EmergencyCardBansProvider.Create
		},
		{
			typeof(PrizeWallDataProvider),
			PrizeWallDataProvider.Create
		},
		{
			typeof(IChallengeCommunicationWrapper),
			ChallengeCommunicationWrapperFactory.Create
		},
		{
			typeof(ChallengeDataProvider),
			ChallengeDataProvider.Create
		},
		{
			typeof(PVPChallengeController),
			PVPChallengeControllerFactory.Create
		},
		{
			typeof(IGatheringServiceWrapper),
			GatheringServiceWrapperFactory.Create
		},
		{
			typeof(GatheringManager),
			GatheringManagerWrapperFactory.Create
		},
		{
			typeof(IChallengeServiceWrapper),
			ChallengeServiceWrapperFactory.Create
		},
		{
			typeof(IChallengeDeckValidation),
			ChallengeDeckValidationFactory.Create
		},
		{
			typeof(DeckFolderStatesDataProvider),
			DeckFolderStatesDataProvider.Create
		},
		{
			typeof(CampaignGraphManager),
			CampaignGraphManager.Create
		},
		{
			typeof(INpeStrategy),
			NpeStrategyFactory.Create
		},
		{
			typeof(InventoryManager),
			InventoryManagerFactory.Create
		},
		{
			typeof(ISetMasteryStrategy),
			SetMasteryStrategyFactory.Create
		},
		{
			typeof(SetMasteryDataProvider),
			SetMasteryDataProvider.Create
		},
		{
			typeof(ICarouselServiceWrapper),
			CarouselServiceWrapperFactory.Create
		},
		{
			typeof(CarouselDataProvider),
			CarouselDataProvider.Create
		},
		{
			typeof(SystemMessageDataProvider),
			SystemMessageDataProvider.Create
		},
		{
			typeof(FormatManager),
			FormatManager.Create
		},
		{
			typeof(IColorChallengeStrategy),
			ColorChallengeStrategyFactory.Create
		},
		{
			typeof(NewPlayerExperienceStrategy),
			NewPlayerExperienceStrategy.Create
		},
		{
			typeof(BotTool),
			BotTool.Create
		},
		{
			typeof(PopupNotificationManager),
			PopupNotificationManager.Create
		},
		{
			typeof(IErrorReporter),
			() => new BacktraceErrorReporter()
		},
		{
			typeof(ICreditsDataProvider),
			CreditsDataProvider.Create
		},
		{
			typeof(ICreditsTextProvider),
			CreditsTextProvider.PantryCreate
		},
		{
			typeof(ILearnToPlayContentBuilder),
			LearnToPlayContentBuilder.PantryCreate
		},
		{
			typeof(ITableOfContentsSectionBuilder),
			TableOfContentsBuilder.PantryCreate
		},
		{
			typeof(ITutorialLauncher),
			TutorialLauncher.PantryCreate
		},
		{
			typeof(CardViewBuilder),
			CardViewBuilderFactory.Create
		},
		{
			typeof(EventManager),
			EventManagerFactory.Create
		},
		{
			typeof(ISocialManager),
			SocialManagerFactory.Create
		},
		{
			typeof(NPEState),
			NPEState.Create
		},
		{
			typeof(DecksManager),
			DecksManager.Create
		},
		{
			typeof(ILobbyServiceWrapper),
			LobbyWrapperFactory.CreateServiceWrapper
		},
		{
			typeof(ILobbyDataProvider),
			LobbyWrapperFactory.CreateDataProvider
		},
		{
			typeof(ILobbyController),
			LobbyWrapperFactory.CreateController
		},
		{
			typeof(ITournamentServiceWrapper),
			TournamentWrapperFactory.CreateServiceWrapper
		},
		{
			typeof(ITournamentDataProvider),
			TournamentWrapperFactory.CreateDataProvider
		},
		{
			typeof(ITournamentController),
			TournamentWrapperFactory.CreateController
		},
		{
			typeof(StoreManager),
			StoreFactory.PantryCreate
		},
		{
			typeof(IAchievementServiceWrapper),
			AchievementServiceWrapper.Create
		},
		{
			typeof(IAchievementDataProvider),
			AchievementDataProvider.Create
		},
		{
			typeof(IAchievementManager),
			AchievementManager.Create
		},
		{
			typeof(IAchievementsToastProvider),
			AchievementToastsProvider.Create
		},
		{
			typeof(IQueueTipProvider),
			QueueTipProvider.Create
		},
		{
			typeof(AssetBundleSourcesModel),
			AssetBundleSourcesModel.Create
		},
		{
			typeof(AssetBundleProvisioner),
			AssetBundleProvisioner.Create
		},
		{
			typeof(ITitleCountManager),
			TitleCountManager.Create
		},
		{
			typeof(ManifestMetadataProvider),
			() => new ManifestMetadataProvider()
		},
		{
			typeof(ManifestProvider),
			() => new ManifestProvider()
		}
	};

	public static Dictionary<Type, Func<object>> Wrapper = new Dictionary<Type, Func<object>>
	{
		{
			typeof(SceneUITransforms),
			SceneUITransforms.Create
		},
		{
			typeof(ICardRolloverZoom),
			CardZoomDelegatorFactory.Create
		},
		{
			typeof(CardRolloverZoomBase),
			CardZoomFactory.Create
		},
		{
			typeof(DeckBuilderLayoutState),
			DeckBuilderLayoutState.Create
		},
		{
			typeof(DeckBuilderActionsHandler),
			DeckBuilderActionsHandler.Create
		},
		{
			typeof(DeckBuilderVisualsUpdater),
			DeckBuilderVisualsUpdater.Create
		},
		{
			typeof(WrapperCompass),
			WrapperCompass.Create
		}
	};

	public static Dictionary<Type, Func<object>> DeckBuilder = new Dictionary<Type, Func<object>>
	{
		{
			typeof(DeckBuilderContextProvider),
			DeckBuilderContextProvider.Create
		},
		{
			typeof(DeckBuilderModelProvider),
			DeckBuilderModelProvider.Create
		},
		{
			typeof(DeckBuilderCardFilterProvider),
			DeckBuilderCardFilterProvider.Create
		},
		{
			typeof(DeckBuilderPreferredPrintingState),
			DeckBuilderPreferredPrintingState.Create
		},
		{
			typeof(CompanionUtil),
			CompanionUtil.Create
		},
		{
			typeof(MetaCardViewDragState),
			MetaCardViewDragState.Create
		}
	};
}
