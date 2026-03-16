using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wizards.Mtga.Assets;

public class BotBattleLauncher : MonoBehaviour
{
	[SerializeField]
	private Button _loadDebugButton;

	[Space(10f)]
	[SerializeField]
	private Dropdown _sessionTypeDropdown;

	[SerializeField]
	private Button _testStartButton;

	[SerializeField]
	private Button _exportJsonButton;

	[Space(10f)]
	[SerializeField]
	private RectTransform _configRoot;

	[Space(10f)]
	[SerializeField]
	private BotBattleConfig_DeckTest _deckTestConfigPrefab;

	[SerializeField]
	private BotBattleConfig_SetTest _setTestConfigPrefab;

	[SerializeField]
	private BotBattleConfig_CardTest _cardTestConfigPrefab;

	private BotBattleConfigView _currentConfigView;

	private const string _botBattleSceneName = "BotBattleLauncher";

	private static bool _loadingScene;

	private static Scene? _loadedScene;

	public event Action<BotBattleDSConfig> StartButtonClicked;

	public event Action<BotBattleDSConfig> ExportConfigClicked;

	private void Start()
	{
		List<string> list = new List<string>();
		foreach (BotBattleSessionType value in EnumHelper.GetValues(typeof(BotBattleSessionType)))
		{
			list.Add(value.ToString());
		}
		_sessionTypeDropdown.ClearOptions();
		_sessionTypeDropdown.AddOptions(list);
		_sessionTypeDropdown.onValueChanged.AddListener(delegate(int value)
		{
			if (_currentConfigView.PanelType != (BotBattleSessionType)value)
			{
				SetCurrentConfig((BotBattleSessionType)value);
			}
		});
		_exportJsonButton.onClick.AddListener(delegate
		{
			if (!(_currentConfigView == null))
			{
				this.ExportConfigClicked?.Invoke(_currentConfigView.GetConfig());
			}
		});
		_testStartButton.onClick.AddListener(delegate
		{
			if (!(_currentConfigView == null))
			{
				this.StartButtonClicked?.Invoke(_currentConfigView.GetConfig());
			}
		});
		_loadDebugButton.onClick.AddListener(delegate
		{
			Scenes.LoadScene("DuelSceneDebugLauncher");
		});
		SetCurrentConfig(BotBattleSessionType.DeckTest);
	}

	private BotBattleConfigView GetPrefabByType(BotBattleSessionType sessionType)
	{
		return sessionType switch
		{
			BotBattleSessionType.DeckTest => _deckTestConfigPrefab, 
			BotBattleSessionType.SetTest => _setTestConfigPrefab, 
			BotBattleSessionType.CardTest => _cardTestConfigPrefab, 
			_ => _deckTestConfigPrefab, 
		};
	}

	private void SetCurrentConfig(BotBattleSessionType sessionType)
	{
		if (_currentConfigView != null)
		{
			UnityEngine.Object.Destroy(_currentConfigView.gameObject);
		}
		_currentConfigView = UnityEngine.Object.Instantiate(GetPrefabByType(sessionType), _configRoot);
	}

	private void OnDestroy()
	{
		if (this.StartButtonClicked != null)
		{
			Delegate[] invocationList = this.StartButtonClicked.GetInvocationList();
			foreach (Delegate obj in invocationList)
			{
				StartButtonClicked -= (Action<BotBattleDSConfig>)obj;
			}
		}
		if (this.ExportConfigClicked != null)
		{
			Delegate[] invocationList = this.ExportConfigClicked.GetInvocationList();
			foreach (Delegate obj2 in invocationList)
			{
				ExportConfigClicked -= (Action<BotBattleDSConfig>)obj2;
			}
		}
		_sessionTypeDropdown.onValueChanged.RemoveAllListeners();
		_exportJsonButton.onClick.RemoveAllListeners();
		_testStartButton.onClick.RemoveAllListeners();
		_loadDebugButton.onClick.RemoveAllListeners();
	}

	public static void Load(Action<BotBattleLauncher> onLoaded)
	{
		if (_loadingScene)
		{
			Debug.LogError("BOTBATTLE LAUNCHER ALREADY LOADING");
			return;
		}
		_loadingScene = true;
		SceneManager.sceneLoaded += OnSceneLoaded;
		Scenes.LoadScene("BotBattleLauncher", LoadSceneMode.Additive);
		void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			BotBattleLauncher sceneComponent = scene.GetSceneComponent<BotBattleLauncher>();
			if (sceneComponent != null)
			{
				_loadedScene = scene;
				onLoaded?.Invoke(sceneComponent);
				_loadingScene = false;
				SceneManager.sceneLoaded -= OnSceneLoaded;
			}
		}
	}

	public static void Unload(Action onUnloaded)
	{
		Scene launcherScene;
		if (_loadedScene.HasValue)
		{
			launcherScene = _loadedScene.Value;
			SceneManager.sceneUnloaded += OnSceneUnloaded;
			SceneManager.UnloadSceneAsync(launcherScene);
		}
		void OnSceneUnloaded(Scene scene)
		{
			if (scene == launcherScene)
			{
				onUnloaded?.Invoke();
				_loadedScene = null;
				SceneManager.sceneUnloaded -= OnSceneUnloaded;
			}
		}
	}
}
