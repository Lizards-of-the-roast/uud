using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtgo.Gre.External.Messaging;

public class SettingsButton : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup _panel;

	[SerializeField]
	private Button _button;

	private MatchManager _matchManager;

	private SettingsMenuHost _settingsMenuHost;

	public static SettingsButton Instantiate(AssetLookupSystem assetLookupSystem, SettingsMenuHost settingsMenuHost, MatchManager matchManager, Transform root)
	{
		SettingsButton settingsButton = AssetLoader.Instantiate<SettingsButton>(assetLookupSystem.GetPrefabPath<SettingsButtonPrefab, SettingsButton>(), root);
		settingsButton.Init(settingsMenuHost, matchManager);
		return settingsButton;
	}

	public void Init(SettingsMenuHost settingsMenuHost, MatchManager matchManager)
	{
		_button.onClick.AddListener(OnClicked);
		_matchManager = matchManager;
		if (_matchManager != null)
		{
			_matchManager.SideboardSubmitted += OnSideboardSubmitted;
			_matchManager.MatchStateChanged += OnMatchStateChanged;
			_matchManager.MatchCompleted += OnMatchCompleted;
		}
		_settingsMenuHost = settingsMenuHost;
	}

	private void OnDestroy()
	{
		_settingsMenuHost = null;
		if (_matchManager != null)
		{
			_matchManager.SideboardSubmitted -= OnSideboardSubmitted;
			_matchManager.MatchStateChanged -= OnMatchStateChanged;
			_matchManager.MatchCompleted -= OnMatchCompleted;
			_matchManager = null;
		}
		if ((bool)_button && _button.onClick != null)
		{
			_button.onClick.RemoveListener(OnClicked);
		}
	}

	private void OnClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		Object.FindObjectOfType<GameManager>()?.InteractionSystem?.CancelAnyDrag();
		_settingsMenuHost.Open();
	}

	private void OnMatchStateChanged(MatchState matchState)
	{
		Hide();
	}

	private void OnSideboardSubmitted()
	{
		Hide();
	}

	private void OnMatchCompleted()
	{
		Hide();
	}

	private void Hide()
	{
		_panel.interactable = false;
		_panel.blocksRaycasts = false;
		_panel.alpha = 0f;
	}
}
