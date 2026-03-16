using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wizards.Mtga.Logging;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Meta.MainNavigation.BoosterChamber;

public class LoadBoosterChamberScreen : MonoBehaviour, ISceneSetupTask
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
		string prefabPath = _assetLookupSystem.GetPrefabPath<BoosterChamberV2Prefab, BoosterChamberController>();
		stopwatch.Reset();
		AssetLoader.Instantiate<BoosterChamberController>(prefabPath, _parentTransform).Instantiate(Pantry.Get<ICardRolloverZoom>(), Pantry.Get<CardViewBuilder>(), _assetLookupSystem, Pantry.Get<IFrontDoorConnectionServiceWrapper>(), Pantry.Get<IBILogger>(), Pantry.Get<CardDatabase>(), Pantry.Get<VoucherDataProvider>(), Pantry.Get<ISetMetadataProvider>(), Pantry.Get<InventoryManager>(), WrapperController.EnableLoadingIndicator, Pantry.Get<IUnityObjectPool>(), WrapperController.Instance.SceneLoader.GetNavBar().WildcardButton, WrapperController.Instance.SparkyTourState);
		_setupCompleted = true;
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
