using AssetLookupTree;
using Core.Code.Input;
using Core.Code.PrizeWall;
using Core.Meta.MainNavigation.Challenge;
using MTGA.KeyboardManager;
using MTGA.Social;
using Wizards.Arena.Client.Logging;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace EventPage;

public readonly struct SharedEventPageClasses
{
	public readonly SceneLoader SceneLoader;

	public readonly CardDatabase CardDatabase;

	public readonly CardViewBuilder CardViewBuilder;

	public readonly CardMaterialBuilder CardMaterialBuilder;

	public readonly EventManager EventManager;

	public readonly InventoryManager InventoryManager;

	public readonly CardSkinCatalog CardSkinCatalogWrapper;

	public readonly IPreconDeckServiceWrapper PreconDeckManager;

	public readonly DecksManager DecksManager;

	public readonly FormatManager FormatManager;

	public readonly AccountInformation AccountInformation;

	public readonly StoreManager StoreManager;

	public readonly ISocialManager SocialManager;

	public readonly Matchmaking Matchmaking;

	public readonly ILogger Logger;

	public readonly AssetLookupSystem AssetLookupSystem;

	public readonly IBILogger BILogger;

	public readonly CosmeticsProvider CosmeticsProvider;

	public readonly IPreferredPrintingDataProvider PreferredPrintingDataProvider;

	public readonly IClientLocProvider ClientLocManager;

	public readonly ICustomTokenProvider CustomTokenProvider;

	public readonly KeyboardManager KeyboardManager;

	public readonly IActionSystem ActionSystem;

	public readonly PrizeWallDataProvider PrizeWallDataProvider;

	public readonly PVPChallengeController ChallengeController;

	public SharedEventPageClasses(SceneLoader sceneLoader, AccountInformation accountInformation, Matchmaking matchmaking, EventManager eventManager, InventoryManager inventoryManager, DecksManager decksManager, FormatManager formatManager, StoreManager storeManager, ISocialManager socialManager, IPreconDeckServiceWrapper preconDeckManager, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, CardSkinCatalog cardSkinCatalogWrapper, ILogger logger, AssetLookupSystem assetLookupSystem, IBILogger biLogger, CosmeticsProvider cosmeticsProvider, IPreferredPrintingDataProvider preferredPrintingDataProvider, IClientLocProvider clientLocManager, ICustomTokenProvider customTokenProvider, KeyboardManager keyboardManager, IActionSystem actionSystem, PrizeWallDataProvider prizeWallDataProvider, PVPChallengeController challengeController)
	{
		SceneLoader = sceneLoader;
		CardDatabase = cardDatabase;
		CardViewBuilder = cardViewBuilder;
		CardMaterialBuilder = cardMaterialBuilder;
		EventManager = eventManager;
		InventoryManager = inventoryManager;
		CardSkinCatalogWrapper = cardSkinCatalogWrapper;
		PreconDeckManager = preconDeckManager;
		DecksManager = decksManager;
		FormatManager = formatManager;
		AccountInformation = accountInformation;
		StoreManager = storeManager;
		SocialManager = socialManager;
		Matchmaking = matchmaking;
		Logger = logger;
		AssetLookupSystem = assetLookupSystem;
		BILogger = biLogger;
		CosmeticsProvider = cosmeticsProvider;
		PreferredPrintingDataProvider = preferredPrintingDataProvider;
		ClientLocManager = clientLocManager;
		CustomTokenProvider = customTokenProvider;
		KeyboardManager = keyboardManager;
		ActionSystem = actionSystem;
		PrizeWallDataProvider = prizeWallDataProvider;
		ChallengeController = challengeController;
	}
}
