using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wizards.Mtga.Logging;
using Wotc.Mtga;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class LoadDeckBuilderScreen : MonoBehaviour
{
	[SerializeField]
	private Transform _parentTransform;

	private UnityLogger _logger;

	private AssetLookupSystem _assetLookupSystem;

	private void Awake()
	{
		_logger = new UnityLogger("SceneLoader", LoggerLevel.Error);
		LoggerManager.Register(_logger);
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		string prefabPath = _assetLookupSystem.GetPrefabPath<WrapperDeckBuilderPrefab, WrapperDeckBuilder>();
		stopwatch.Reset();
		AssetLoader.Instantiate<WrapperDeckBuilder>(prefabPath, _parentTransform).Initialize(Pantry.Get<ICardRolloverZoom>(), Pantry.Get<IBILogger>(), Pantry.Get<CosmeticsProvider>(), Pantry.Get<DesignerMetadataProvider>(), Pantry.Get<IClientLocProvider>(), Pantry.Get<ISetMetadataProvider>());
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
