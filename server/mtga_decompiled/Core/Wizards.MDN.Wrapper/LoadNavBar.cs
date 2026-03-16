using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.MainNavigation;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wizards.Mtga.Logging;

namespace Wizards.MDN.Wrapper;

public class LoadNavBar : MonoBehaviour, ISceneSetupTask
{
	[SerializeField]
	private Transform _parentTransform;

	private UnityLogger _logger = new UnityLogger("SceneLoader", LoggerLevel.Error);

	public bool _setupCompeleted;

	public bool SetupCompleted => _setupCompeleted;

	private void Awake()
	{
		Stopwatch stopwatch = new Stopwatch();
		LoggerManager.Register(_logger);
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		stopwatch.Start();
		string prefabPath = assetLookupSystem.GetPrefabPath<NavBarPrefab, NavBarController>();
		stopwatch.Reset();
		NavBarController navBarController = AssetLoader.Instantiate<NavBarController>(prefabPath, _parentTransform);
		navBarController.gameObject.SetActive(value: true);
		navBarController.RefreshCurrencyDisplay();
		_setupCompeleted = true;
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
