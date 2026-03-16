using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Promises;
using Wizards.GeneralUtilities;
using Wizards.Mtga;
using Wizards.Mtga.Logging;

namespace Core.Meta.MainNavigation.LearntoPlayv2;

public class LoadLearnToPlayScreen : MonoBehaviour, ISceneSetupTask
{
	[SerializeField]
	[AdditionalInformation("This is where the learn to play prefab shall be created under.")]
	private Transform _parentTransform;

	private Stopwatch _stopwatch = new Stopwatch();

	private AssetLookupSystem _assetLookupSystem;

	private UnityLogger _logger;

	private Promise<Unit> _finalizeSetupPromise;

	public bool SetupCompleted { get; private set; }

	private void Awake()
	{
		_logger = new UnityLogger("SceneLoader", LoggerLevel.Error);
		LoggerManager.Register(_logger);
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_stopwatch.Start();
		string prefabPath = _assetLookupSystem.GetPrefabPath<LearnToPlayPrefab, LearnToPlayControllerV2>();
		_stopwatch.Restart();
		LearnToPlayControllerV2 learnToPlayController = AssetLoader.Instantiate<LearnToPlayControllerV2>(prefabPath, _parentTransform);
		learnToPlayController.Init();
		_finalizeSetupPromise = new Until(() => learnToPlayController.IsReadyToShow || learnToPlayController.SkipScreen).Then(delegate
		{
			FinalizeSetup();
		});
	}

	public void FinalizeSetup()
	{
		_finalizeSetupPromise = null;
		SetupCompleted = true;
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
