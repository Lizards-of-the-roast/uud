using System.Diagnostics;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.MainNavigation;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wizards.Mtga.Logging;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Wizards.MDN.DeckManager;

public class LoadDeckManager : MonoBehaviour, ISceneSetupTask
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
		string prefabPath = _assetLookupSystem.GetPrefabPath<DeckManagerPrefab, DeckManagerController>();
		stopwatch.Reset();
		AssetLoader.Instantiate<DeckManagerController>(prefabPath, _parentTransform).Init(_assetLookupSystem, Pantry.Get<IBILogger>(), WrapperController.Instance.DecksManager, Pantry.Get<CardDatabase>(), Pantry.Get<CardViewBuilder>(), Languages.ActiveLocProvider, Pantry.Get<CosmeticsProvider>(), WrapperController.Instance.Store.AvatarCatalog, WrapperController.Instance.Store.PetCatalog, Pantry.Get<ICardRolloverZoom>(), WrapperController.Instance.EmoteDataProvider, WrapperController.Instance.UnityObjectPool, WrapperController.Instance.Store, Pantry.Get<FormatManager>(), WrapperController.Instance.EventManager, Pantry.Get<ISetMetadataProvider>());
		_setupCompleted = true;
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		LoggerManager.Unregister(_logger);
	}
}
