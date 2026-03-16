using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.Input;
using Core.Meta.MainNavigation.PopUps;
using MTGA.KeyboardManager;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Logging;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation;

public class LoadHomePage : MonoBehaviour
{
	[SerializeField]
	private Transform _parentTransform;

	private UnityLogger _logger;

	private AssetLookupSystem _assetLookupSystem;

	private CosmeticsProvider _cosmeticsProvider;

	private HomePageCompassGuide _wrapperCompassGuide;

	private readonly Stopwatch _stopwatch = new Stopwatch();

	private void Awake()
	{
		_logger = new UnityLogger("SceneLoader", LoggerLevel.Error);
		LoggerManager.Register(_logger);
		_cosmeticsProvider = Pantry.Get<CosmeticsProvider>();
		_wrapperCompassGuide = Pantry.Get<WrapperCompass>().GetGuide<HomePageCompassGuide>();
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_stopwatch.Reset();
		string prefabPath = _assetLookupSystem.GetPrefabPath<ObjectivesPrefab, ContentControllerObjectives>();
		_stopwatch.Reset();
		AssetLoader.Instantiate<ContentControllerObjectives>(prefabPath, _parentTransform).Initialize(Pantry.Get<KeyboardManager>(), Pantry.Get<IActionSystem>(), Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>(), Pantry.Get<CardMaterialBuilder>(), Pantry.Get<IAccountClient>());
		_stopwatch.Start();
		string prefabPath2 = _assetLookupSystem.GetPrefabPath<HomePrefab, HomePageContentController>();
		_stopwatch.Reset();
		HomePageContentController homePageContentController = AssetLoader.Instantiate<HomePageContentController>(prefabPath2, _parentTransform);
		homePageContentController.Init(SceneLoader.GetSceneLoader().GetObjectivesController(), SceneLoader.GetSceneLoader().GetRewardsContentController(), Pantry.Get<ISocialManager>(), _assetLookupSystem, Pantry.Get<IAccountClient>(), Pantry.Get<InventoryManager>(), Pantry.Get<KeyboardManager>(), Pantry.Get<IActionSystem>(), _cosmeticsProvider, null, Pantry.Get<IFrontDoorConnectionServiceWrapper>(), Pantry.Get<IBILogger>(), Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>(), Pantry.Get<CardMaterialBuilder>(), Pantry.Get<EventManager>(), SceneLoader.GetSceneLoader().OnPlayBladeQueueSelected, SceneLoader.GetSceneLoader().OnPlayBladeFilterSelected, Pantry.Get<ICustomTokenProvider>(), Pantry.Get<PopupManager>(), Pantry.Get<ISetMetadataProvider>());
		SceneLoader.GetSceneLoader().GetPlayerInboxContentController().Hide();
		homePageContentController.SetContext(_wrapperCompassGuide);
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
