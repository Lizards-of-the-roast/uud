using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.MainNavigation;
using Core.Meta.UI;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wizards.Mtga.Logging;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

public class LoadStoreScreen : MonoBehaviour, ISceneSetupTask
{
	[SerializeField]
	private Transform _parentTransform;

	private UnityLogger _logger;

	private AssetLookupSystem _assetLookupSystem;

	private bool _setupCompleted;

	public bool SetupCompleted => _setupCompleted;

	private void Awake()
	{
		_logger = new UnityLogger("SceneLoader", LoggerLevel.Error);
		LoggerManager.Register(_logger);
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		string prefabPath = _assetLookupSystem.GetPrefabPath<StorePrefab, ContentController_StoreCarousel>();
		stopwatch.Reset();
		AssetLoader.Instantiate<ContentController_StoreCarousel>(prefabPath, _parentTransform).Init(_assetLookupSystem, Pantry.Get<ICardRolloverZoom>(), Pantry.Get<IBILogger>(), Pantry.Get<IGreLocProvider>(), Pantry.Get<IClientLocProvider>(), Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>(), Pantry.Get<SettingsMenuHost>(), Pantry.Get<SceneUITransforms>().ContentParent, Pantry.Get<ISetMetadataProvider>());
		_setupCompleted = true;
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
