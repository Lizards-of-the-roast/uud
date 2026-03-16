using System.Diagnostics;
using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.GeneralUtilities;
using Wizards.Mtga;
using Wizards.Mtga.Logging;

namespace Core.Meta.MainNavigation.Achievements;

public class LoadAchievementsScreen : MonoBehaviour, ISceneSetupTask
{
	[SerializeField]
	[AdditionalInformation("This is where the achievements prefab shall be created under.")]
	private Transform _parentTransform;

	private Stopwatch _stopwatch = new Stopwatch();

	private AssetLookupSystem _assetLookupSystem;

	private UnityLogger _logger;

	private bool _setupCompleted;

	public bool SetupCompleted => _setupCompleted;

	private void Awake()
	{
		_logger = new UnityLogger("SceneLoader", LoggerLevel.Error);
		LoggerManager.Register(_logger);
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_stopwatch.Start();
		string prefabPath = AchievementsScreenHelperFunctions.GetPrefabPath(_assetLookupSystem, "AchievementScreen");
		_stopwatch.Reset();
		AssetLoader.Instantiate(prefabPath, _parentTransform);
		_stopwatch.Stop();
		_setupCompleted = true;
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
