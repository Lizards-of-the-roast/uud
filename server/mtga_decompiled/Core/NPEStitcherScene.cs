using System.Collections;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Input;
using Core.NPEStitcher;
using Core.Shared.Code.Connection;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Assets;
using Wizards.Mtga.Deeplink;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class NPEStitcherScene : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	[SerializeField]
	private VideoClip _cinematicVideoClip;

	private NPEObjectivesController _NPEObjectivesController;

	private AccountInformation _accountInfo;

	private NPEState _npeState;

	private IBILogger _biLogger;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private AssetLookupSystem _assetLookupSystem;

	private SettingsMenuHost _settingsMenuHost;

	private INpeStrategy npeStrategy;

	private GameObject _loadingIndicator;

	[SerializeField]
	private Transform _loadingIndicatorParent;

	private FrontDoorConnectionManager _frontDoorConnectionManager;

	private static bool _unloading;

	public PriorityLevelEnum Priority => PriorityLevelEnum.NPE;

	public static IEnumerator Coroutine_Load(AccountInformation accountInfo, NPEState npeState, IBILogger biLogger, KeyboardManager keyboardManager, IActionSystem actionSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, AssetLookupSystem assetLookupSystem, INpeStrategy npeStrategy, FrontDoorConnectionManager frontDoorConnectionManager, SettingsMenuHost settingsMenuHost)
	{
		yield return Scenes.LoadSceneAsync("NPEStitcher");
		NPEStitcherScene sceneComponent = SceneManager.GetSceneByName("NPEStitcher").GetSceneComponent<NPEStitcherScene>();
		sceneComponent._accountInfo = accountInfo;
		sceneComponent._npeState = npeState;
		sceneComponent._biLogger = biLogger;
		sceneComponent._keyboardManager = keyboardManager;
		sceneComponent._actionSystem = actionSystem;
		sceneComponent._cardDatabase = cardDatabase;
		sceneComponent._cardViewBuilder = cardViewBuilder;
		sceneComponent._assetLookupSystem = assetLookupSystem;
		sceneComponent._frontDoorConnectionManager = frontDoorConnectionManager;
		sceneComponent._settingsMenuHost = settingsMenuHost;
		sceneComponent.SetupLoadingIndicatorHandheldOnly();
		sceneComponent.EnableLoadingIndicator(enable: true);
		sceneComponent.npeStrategy = npeStrategy ?? Pantry.Get<INpeStrategy>();
		yield return sceneComponent.StartCoroutine(sceneComponent.Init());
	}

	private IEnumerator Init()
	{
		yield return new WaitUntil(() => npeStrategy.Initialized);
		yield return npeStrategy.Refresh();
		bool enable = false;
		_keyboardManager.Subscribe(this);
		_actionSystem.PushFocus(this);
		Application.deepLinkActivated += OnDeepLink;
		if (npeStrategy.State == NpeModuleState.Uninitialized)
		{
			_frontDoorConnectionManager.ShowConnectionFailedMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_DownForMaintenance"), "", allowRetry: false, exitInsteadOfLogout: true);
			Debug.LogError("Client tried checking to see if eventqueue was up, for NPE logic, but its promise return unsuccessful.");
			yield break;
		}
		if (!npeStrategy.Available && !_accountInfo.HasRole_WotCAccess())
		{
			_frontDoorConnectionManager.ShowConnectionFailedMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_DownForMaintenance"), "", allowRetry: false, exitInsteadOfLogout: true);
			yield break;
		}
		if ((npeStrategy.State & NpeModuleState.CanJoin) > NpeModuleState.Uninitialized)
		{
			if (_npeState.TutorialState == NPEState.TutorialStates.EngageWithEvent)
			{
				enable = true;
				npeStrategy.Join(onJoined);
			}
			else if (_npeState.TutorialState == NPEState.TutorialStates.Underway)
			{
				tutorialCompleted();
			}
			else
			{
				Debug.LogError("NPE Event course is 'Join', but we are in Progression State: " + _npeState.TutorialState);
			}
		}
		else if ((npeStrategy.State & NpeModuleState.CanPlay) > NpeModuleState.Uninitialized)
		{
			int nextGameNumber = npeStrategy.NextGameNumber;
			_npeState.NPEUnderway(nextGameNumber);
			_npeState.BI_NPEProgressUpdate(new NPEState.NPEProgressContext(NPEState.NPEProgressMarker.NPE_Home_Hit_Onto_Game, nextGameNumber, 0u, Phase.None, Step.None, won: false, _npeState.LastJoinedNPEGameNumber < 0));
			if (_npeState.SkipTutorialWasQueuedUpFromInGame)
			{
				SkipTutorialRequested();
			}
			else if (nextGameNumber == 1 && _npeState.LastJoinedNPEGameNumber == 0 && MDNPlayerPrefs.CheckIfInExperimentalGroup_Experiment003(_accountInfo.PersonaID))
			{
				JoinNextMatch();
			}
			else
			{
				GetNPEObjectives().ReturnToScene(nextGameNumber, _npeState.LastJoinedNPEGameNumber);
			}
		}
		else if ((npeStrategy.State & NpeModuleState.HaveRewards) > NpeModuleState.Uninitialized)
		{
			tutorialCompleted();
		}
		else if (npeStrategy.State == NpeModuleState.CanSkip)
		{
			SkipTutorialRequested();
		}
		else if (npeStrategy.State == NpeModuleState.Error)
		{
			NavigateFromFinishedTutorial();
		}
		else if (npeStrategy.State == NpeModuleState.Complete)
		{
			tutorialCompleted();
		}
		else
		{
			Debug.LogError("[NPE] unhandled state: " + npeStrategy.State);
		}
		EnableLoadingIndicator(enable);
	}

	private void OnDestroy()
	{
		_keyboardManager.Unsubscribe(this);
		_actionSystem.PopFocus(this);
		Application.deepLinkActivated -= OnDeepLink;
	}

	public void OnDeepLink(string url)
	{
		DeepLinking.LogDeepLinkNotUsed(url, "In NPE, DeepLink ignored", _biLogger);
	}

	private void tutorialCompleted()
	{
		_npeState.ConsiderTutorialCompleted();
		GetNPEObjectives().ShowSceneAfterFinishingAllGames();
		npeStrategy.ClaimRewards(delegate
		{
		});
	}

	public void SkipTutorialRequested()
	{
		GetNPEObjectives();
		_npeState.SkipTutorial(npeStrategy);
	}

	private void onJoined(bool success, Error error)
	{
		if ((npeStrategy.State & NpeModuleState.CanPlay) > NpeModuleState.Uninitialized)
		{
			_npeState.NPEUnderway(0);
			JoinNextMatch();
		}
	}

	private NPEObjectivesController GetNPEObjectives()
	{
		if (_NPEObjectivesController == null)
		{
			SpawnNPEObjectivesController();
			_NPEObjectivesController.Init(_npeState, _cardDatabase, _keyboardManager, _actionSystem, _assetLookupSystem, _cardViewBuilder, _settingsMenuHost, null);
			_NPEObjectivesController.FinishSystemAction = NavigateFromFinishedTutorial;
			_NPEObjectivesController.PlayButtonSystemAction = JoinNextMatch;
		}
		return _NPEObjectivesController;
	}

	private void SpawnNPEObjectivesController()
	{
		_assetLookupSystem.Blackboard.Clear();
		NPEObjectivesControllerPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<NPEObjectivesControllerPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
		_NPEObjectivesController = AssetLoader.Instantiate(payload.Prefab, base.gameObject.transform);
	}

	private void JoinNextMatch()
	{
		_npeState.RememberPlayingThisGame();
		npeStrategy.PlayMatch();
		_npeState.BI_NPEProgressUpdate(new NPEState.NPEProgressContext(NPEState.NPEProgressMarker.Onto_Game, _npeState.ActiveNPEGameNumber));
		_npeState.LockDownSkipButtonWhileQueuing();
	}

	private void SetupLoadingIndicatorHandheldOnly()
	{
		if (PlatformUtils.IsHandheld())
		{
			_loadingIndicator = AssetLoader.Instantiate(_assetLookupSystem.GetPrefabPath<LoadingPanelPrefab, GameObject>(), _loadingIndicatorParent);
		}
	}

	private void EnableLoadingIndicator(bool enable)
	{
		if (!(_loadingIndicator == null))
		{
			_loadingIndicator.UpdateActive(enable);
		}
	}

	private void NavigateFromFinishedTutorial()
	{
		if (_NPEObjectivesController != null)
		{
			Object.Destroy(_NPEObjectivesController.gameObject);
		}
		_NPEObjectivesController = null;
		Object.FindObjectOfType<Bootstrap>().NpeCompleteStartFullGame();
	}

	public static void Unload()
	{
		if (_unloading)
		{
			Debug.LogError("NPE STITCHER SCENE ALREADY UNLOADING");
			return;
		}
		Scene stitcherScene = SceneManager.GetSceneByName("NPEStitcher");
		if (stitcherScene.IsValid() && stitcherScene.isLoaded)
		{
			Debug.Log("scene name is " + stitcherScene.name);
			stitcherScene.GetSceneComponent<NPEStitcherScene>().EnableLoadingIndicator(enable: false);
			_unloading = true;
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			SceneManager.UnloadSceneAsync(stitcherScene);
		}
		void OnSceneUnloaded(Scene unloadedScene)
		{
			Debug.Log("unloaded scene name is " + unloadedScene.name);
			if (unloadedScene == stitcherScene)
			{
				SceneManager.sceneUnloaded -= OnSceneUnloaded;
				_unloading = false;
			}
		}
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			if (PlatformUtils.IsHandheld())
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
				SceneLoader.ApplicationQuit();
			}
			else
			{
				_settingsMenuHost.Open();
			}
			return true;
		}
		return false;
	}

	public void OnBack(ActionContext context)
	{
		if (PlatformUtils.IsHandheld())
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
			if (!Application.isEditor)
			{
				SceneLoader.ApplicationQuit();
			}
		}
		else
		{
			_settingsMenuHost.Open();
		}
	}
}
