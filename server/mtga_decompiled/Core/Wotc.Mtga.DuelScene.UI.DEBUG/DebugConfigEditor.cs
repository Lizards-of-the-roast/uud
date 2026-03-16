using System;
using System.Collections.Generic;
using GreClient.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class DebugConfigEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly MatchConfigSelectionEditor.ViewModel ConfigSelector;

		public readonly MatchConfigEditor.ViewModel MatchConfig;

		public string SubDirectory => ConfigSelector.SelectedDirectory;

		public MatchConfig SelectedConfig => ConfigSelector.SelectedConfig;

		public string SelectedBattlefield => MatchConfig.BattlefieldSelection.SelectedBattlefield;

		public ViewModel(MatchConfigSelectionEditor.ViewModel configSelector, MatchConfigEditor.ViewModel matchConfig)
		{
			ConfigSelector = configSelector;
			MatchConfig = matchConfig;
		}

		public ViewModel Modify(MatchConfigSelectionEditor.ViewModel? configSelector = null, MatchConfigEditor.ViewModel? matchConfig = null)
		{
			return new ViewModel(configSelector ?? ConfigSelector, matchConfig ?? MatchConfig);
		}
	}

	[SerializeField]
	private Button _closeButton;

	[SerializeField]
	private Button _decklistEditorButton;

	[SerializeField]
	private MatchConfigSelectionEditor _matchConfigSelectionEditor;

	[SerializeField]
	private MatchConfigEditor _matchConfigEditor;

	[SerializeField]
	private DebugDecklistEditorPanel _decklistEditorPanel;

	private ViewModel _viewModel;

	public DebugDecklistEditorPanel DecklistEditor => _decklistEditorPanel;

	public event Action<ViewModel, ViewModel> ViewModelChanged;

	public event Action Closed;

	private void Awake()
	{
		_closeButton.onClick.AddListener(CloseButtonClicked);
		_matchConfigSelectionEditor.ViewModelChanged += OnMatchConfigSelectionChanged;
		_matchConfigEditor.ViewModelUpdated += OnMatchConfigChanged;
		_decklistEditorButton.onClick.AddListener(OnDecklistEditorButtonClicked);
		_decklistEditorPanel.ConfigEditor.ViewModelChanged += OnDecklistEditorChanges;
		_decklistEditorPanel.HasUnsavedChanges.ValueUpdated += RefreshButtonInteractability;
		_decklistEditorPanel.Closed += OnDecklistEditorClosed;
	}

	private void OnDestroy()
	{
		_closeButton.onClick.RemoveListener(CloseButtonClicked);
		_matchConfigSelectionEditor.ViewModelChanged -= OnMatchConfigSelectionChanged;
		_matchConfigEditor.ViewModelUpdated -= OnMatchConfigChanged;
		_decklistEditorButton.onClick.RemoveListener(OnDecklistEditorButtonClicked);
		if (_decklistEditorPanel != null)
		{
			_decklistEditorPanel.ConfigEditor.ViewModelChanged -= OnDecklistEditorChanges;
			_decklistEditorPanel.HasUnsavedChanges.ValueUpdated -= RefreshButtonInteractability;
			_decklistEditorPanel.Closed -= OnDecklistEditorClosed;
		}
	}

	private void RefreshButtonInteractability(bool _ = false)
	{
		_decklistEditorButton.interactable = !_decklistEditorPanel.IsOpen;
	}

	private void OnDecklistEditorButtonClicked()
	{
		if (_decklistEditorPanel.IsOpen)
		{
			_decklistEditorPanel.Close();
		}
		else
		{
			if (!TryFindThisPlayerView(_viewModel.MatchConfig.Teams, out var _, out var _, out var myPlayer))
			{
				SimpleLog.LogError("Can't open decklist editor because we don't have our human player assigned in the config. Assign yourself a player slot first to edit your deck.");
				return;
			}
			_decklistEditorPanel.Open(myPlayer.Deck);
		}
		RefreshButtonInteractability();
	}

	private void OnDecklistEditorClosed()
	{
		RefreshButtonInteractability();
		_matchConfigSelectionEditor.RefreshDirectory();
	}

	private void OnDecklistEditorChanges(DeckConfigEditor.ViewModel _, DeckConfigEditor.ViewModel newModel)
	{
		ApplyDecklistSelectionToConfig(newModel);
	}

	private void ApplyDecklistSelectionToConfig(DeckConfigEditor.ViewModel newModel)
	{
		TeamConfigListEditor.ViewModel teams = _viewModel.MatchConfig.Teams;
		if (!TryFindThisPlayerView(teams, out var myTeamIndex, out var myPlayerIndex, out var _))
		{
			SimpleLog.LogError("Can't apply decklist changes because can't find YOU in config. How did you get into the decklist editor in the first place??");
		}
		else
		{
			_matchConfigEditor.GetPlayerEditor(myTeamIndex, myPlayerIndex).DeckEditor.SetModelAndNotify(newModel);
		}
	}

	public void SetModel(ViewModel viewModel)
	{
		_viewModel = viewModel;
		_matchConfigSelectionEditor.SetModel(viewModel.ConfigSelector);
		_matchConfigEditor.SetModel(viewModel.MatchConfig);
	}

	private bool TryFindThisPlayerView(TeamConfigListEditor.ViewModel allTeams, out int myTeamIndex, out int myPlayerIndex, out PlayerConfigEditor.ViewModel myPlayer)
	{
		bool flag = false;
		myPlayer = default(PlayerConfigEditor.ViewModel);
		myTeamIndex = -1;
		myPlayerIndex = -1;
		IReadOnlyList<TeamConfigEditor.ViewModel> configs = allTeams.Configs;
		for (int i = 0; i < configs.Count; i++)
		{
			IReadOnlyList<PlayerConfigEditor.ViewModel> configs2 = configs[i].Players.Configs;
			for (int j = 0; j < configs2.Count; j++)
			{
				if (configs2[j].PlayerType == PlayerType.You)
				{
					myPlayer = configs2[j];
					flag = true;
					myTeamIndex = i;
					myPlayerIndex = j;
					break;
				}
			}
		}
		if (flag)
		{
			return true;
		}
		Debug.LogWarning("Failed to find a Player, can't update config", this);
		return false;
	}

	private void OnMatchConfigSelectionChanged(MatchConfigSelectionEditor.ViewModel oldSelector, MatchConfigSelectionEditor.ViewModel updatedSelector)
	{
		ViewModel viewModel = _viewModel;
		_viewModel = _viewModel.Modify(updatedSelector);
		this.ViewModelChanged?.Invoke(viewModel, _viewModel);
	}

	private void OnMatchConfigChanged(MatchConfigEditor.ViewModel oldMatchConfig, MatchConfigEditor.ViewModel updatedMatchConfig)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		MatchConfigEditor.ViewModel? matchConfig = updatedMatchConfig;
		_viewModel = viewModel2.Modify(null, matchConfig);
		this.ViewModelChanged?.Invoke(viewModel, _viewModel);
	}

	private void CloseButtonClicked()
	{
		this.Closed?.Invoke();
	}
}
