using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class TeamConfigEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly string TeamName;

		public readonly PlayerConfigListEditor.ViewModel Players;

		public ViewModel(string teamName, PlayerConfigListEditor.ViewModel players)
		{
			TeamName = teamName;
			Players = players;
		}

		public ViewModel Modify(string teamName = null, PlayerConfigListEditor.ViewModel? players = null)
		{
			return new ViewModel(TeamName, players ?? Players);
		}

		public bool ContainsStartingPlayer()
		{
			foreach (PlayerConfigEditor.ViewModel config in Players.Configs)
			{
				if (config.StartingPlayer)
				{
					return true;
				}
			}
			return false;
		}
	}

	[SerializeField]
	private TMP_Text _teamNameLabel;

	[SerializeField]
	private VisibilityToggle _visibilityToggle;

	[SerializeField]
	private Button _deleteButton;

	[SerializeField]
	private PlayerConfigListEditor _playerConfigListEditor;

	private bool _isVisible = true;

	private ViewModel _viewModel;

	public IReadOnlyList<PlayerConfigEditor> PlayerEditors => _playerConfigListEditor.PlayerEditors;

	public event Action<TeamConfigEditor, ViewModel, ViewModel> ViewModelChanged;

	public event Action<TeamConfigEditor> Deleted;

	private void Awake()
	{
		_visibilityToggle.ToggleChanged += OnVisibilityToggled;
		_playerConfigListEditor.ViewModelChanged += OnPlayersModified;
		_deleteButton.onClick.AddListener(OnDeleteClicked);
	}

	private void OnDestroy()
	{
		_visibilityToggle.ToggleChanged -= OnVisibilityToggled;
		_playerConfigListEditor.ViewModelChanged -= OnPlayersModified;
		_deleteButton.onClick.RemoveListener(OnDeleteClicked);
	}

	private void OnDeleteClicked()
	{
		this.Deleted?.Invoke(this);
	}

	private void OnPlayersModified(PlayerConfigListEditor.ViewModel oldPlayeres, PlayerConfigListEditor.ViewModel updatedPlayers)
	{
		ViewModel viewModel = _viewModel;
		_viewModel = _viewModel.Modify(null, updatedPlayers);
		this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
	}

	private void OnVisibilityToggled(bool visible)
	{
		_isVisible = visible;
		SetTeamNameText();
	}

	public void SetModel(ViewModel viewModel, uint startingSeatId)
	{
		_viewModel = viewModel;
		_playerConfigListEditor.SetModel(_viewModel.Players);
		_playerConfigListEditor.SetPlayerLabels(startingSeatId);
		SetTeamNameText();
	}

	private void SetTeamNameText()
	{
		int count = _viewModel.Players.Configs.Count;
		_teamNameLabel.text = (_isVisible ? _viewModel.TeamName : string.Format("{0} <size=70%>({1} {2})</size>", _viewModel.TeamName, count, (count == 1) ? "Player" : "Players"));
	}

	public void SetCanDelete(bool canDelete)
	{
		_deleteButton.interactable = canDelete;
	}
}
