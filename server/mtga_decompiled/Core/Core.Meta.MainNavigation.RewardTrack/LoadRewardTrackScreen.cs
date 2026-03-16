using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wizards.Mtga.Logging;
using Wotc.Mtga.Cards.Database;

namespace Core.Meta.MainNavigation.RewardTrack;

public class LoadRewardTrackScreen : MonoBehaviour
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
		string prefabPath = _assetLookupSystem.GetPrefabPath<RewardTrackPrefab, ProgressionTracksContentController>();
		stopwatch.Reset();
		AssetLoader.Instantiate<ProgressionTracksContentController>(prefabPath, _parentTransform).Init(Pantry.Get<IBILogger>(), Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>(), Pantry.Get<CardMaterialBuilder>());
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
