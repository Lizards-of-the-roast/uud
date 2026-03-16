using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.MainNavigation.RewardTrack;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wizards.Mtga.Logging;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Profile;

public class LoadProfileScreen : MonoBehaviour, ISceneSetupTask
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
		string prefabPath = _assetLookupSystem.GetPrefabPath<ProfilePrefab, ProfileContentController>();
		stopwatch.Reset();
		AssetLoader.Instantiate<ProfileContentController>(prefabPath, _parentTransform).Initialize(Pantry.Get<SeasonAndRankDataProvider>(), Languages.ActiveLocProvider, Pantry.Get<IEmoteDataProvider>(), _assetLookupSystem, Pantry.Get<IAccountClient>(), Pantry.Get<CosmeticsProvider>(), Pantry.Get<SetMasteryDataProvider>(), WrapperController.Instance.Store, Pantry.Get<IBILogger>(), Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>(), Pantry.Get<CardMaterialBuilder>(), Pantry.Get<IUnityObjectPool>(), Pantry.Get<ICardRolloverZoom>(), WrapperController.Instance.DecksManager, WrapperController.Instance.Store, Pantry.Get<ISetMetadataProvider>(), Pantry.Get<IFrontDoorConnectionServiceWrapper>(), Pantry.Get<ITitleCountManager>());
		_setupCompleted = true;
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
