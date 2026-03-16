using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class TeamConfigListEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly IReadOnlyList<TeamConfigEditor.ViewModel> Configs;

		private readonly string _myPlayerId;

		private readonly IReadOnlyDictionary<string, IReadOnlyList<GreClient.Network.DeckConfig>> _deckOptions;

		private readonly IReadOnlyList<EmblemData> _emblemOptions;

		private readonly IReadOnlyList<string> _avatarOptions;

		private readonly IReadOnlyList<string> _sleeveOptions;

		private readonly IReadOnlyList<(string petId, string variantId)> _petOptions;

		private readonly IReadOnlyDictionary<GreClient.Network.DeckConfig, IReadOnlyList<CardStyle>> _cardStyleOptions;

		private readonly IReadOnlyList<string> _titleOptions;

		public TeamConfigEditor.ViewModel this[int idx] => Configs[idx];

		public ViewModel(IReadOnlyList<TeamConfigEditor.ViewModel> configs, string myPlayerId, IReadOnlyDictionary<string, IReadOnlyList<GreClient.Network.DeckConfig>> deckOptions, IReadOnlyDictionary<GreClient.Network.DeckConfig, IReadOnlyList<CardStyle>> cardStyleOptions, IReadOnlyList<EmblemData> emblemOptions, IReadOnlyList<string> avatarOptions, IReadOnlyList<string> sleeveOptions, IReadOnlyList<(string petId, string variantId)> petOptions, IReadOnlyList<string> titleOptions)
		{
			Configs = configs ?? Array.Empty<TeamConfigEditor.ViewModel>();
			_myPlayerId = myPlayerId;
			_deckOptions = deckOptions ?? DictionaryExtensions.Empty<string, IReadOnlyList<GreClient.Network.DeckConfig>>();
			_emblemOptions = emblemOptions ?? Array.Empty<EmblemData>();
			_avatarOptions = avatarOptions ?? Array.Empty<string>();
			_sleeveOptions = sleeveOptions ?? Array.Empty<string>();
			_petOptions = petOptions ?? Array.Empty<(string, string)>();
			_cardStyleOptions = cardStyleOptions ?? DictionaryExtensions.Empty<GreClient.Network.DeckConfig, IReadOnlyList<CardStyle>>();
			_titleOptions = titleOptions ?? Array.Empty<string>();
		}

		public ViewModel UpdateTeam(int idx, TeamConfigEditor.ViewModel team)
		{
			if (idx < 0 || idx >= Configs.Count)
			{
				return this;
			}
			List<TeamConfigEditor.ViewModel> list = new List<TeamConfigEditor.ViewModel>(Configs);
			list[idx] = team;
			if (Configs[idx].ContainsStartingPlayer() != team.ContainsStartingPlayer() && team.ContainsStartingPlayer())
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (i == idx || !list[i].ContainsStartingPlayer())
					{
						continue;
					}
					List<PlayerConfigEditor.ViewModel> list2 = new List<PlayerConfigEditor.ViewModel>();
					foreach (PlayerConfigEditor.ViewModel config in list[i].Players.Configs)
					{
						bool? startingPlayer = false;
						list2.Add(config.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, startingPlayer));
					}
					list[i] = list[i].Modify(null, new PlayerConfigListEditor.ViewModel(list2, _myPlayerId, _deckOptions, _cardStyleOptions, _emblemOptions, _avatarOptions, _sleeveOptions, _petOptions, _titleOptions));
				}
			}
			return new ViewModel(list, _myPlayerId, _deckOptions, _cardStyleOptions, _emblemOptions, _avatarOptions, _sleeveOptions, _petOptions, _titleOptions);
		}

		public ViewModel AddTeam()
		{
			IReadOnlyList<GreClient.Network.DeckConfig> readOnlyList2;
			if (!_deckOptions.TryGetValue(string.Empty, out var value))
			{
				IReadOnlyList<GreClient.Network.DeckConfig> readOnlyList = Array.Empty<GreClient.Network.DeckConfig>();
				readOnlyList2 = readOnlyList;
			}
			else
			{
				readOnlyList2 = value;
			}
			IReadOnlyList<GreClient.Network.DeckConfig> readOnlyList3 = readOnlyList2;
			GreClient.Network.DeckConfig selectedDeck = ((readOnlyList3.Count > 0) ? readOnlyList3[0] : GreClient.Network.DeckConfig.Default());
			List<TeamConfigEditor.ViewModel> list = new List<TeamConfigEditor.ViewModel>(Configs);
			list.Add(new TeamConfigEditor.ViewModel("New Team", new PlayerConfigListEditor.ViewModel(new PlayerConfigEditor.ViewModel[1]
			{
				new PlayerConfigEditor.ViewModel("Opponent", PlayerType.Bot, _myPlayerId, new DeckConfigEditor.ViewModel(string.Empty, selectedDeck, _deckOptions), new CardStyleListEditor.ViewModel(Array.Empty<CardStyleEditor.ViewModel>(), Array.Empty<CardStyle>()), FamiliarStrategyType.Generic, "Avatar_Basic_AjaniGoldmane", string.Empty, (petId: string.Empty, variantId: string.Empty), "NoTitle", new RankConfigEditor.ViewModel(RankingClassType.None, 0, 0f, 0), ShuffleRestriction.None, 20u, 7u, treeOfCongress: false, startingPlayer: false, new EmblemConfigListEditor.ViewModel(Array.Empty<EmblemConfigEditor.ViewModel>(), _emblemOptions), _myPlayerId, _avatarOptions, _sleeveOptions, _petOptions, _titleOptions)
			}, _myPlayerId, _deckOptions, _cardStyleOptions, _emblemOptions, _avatarOptions, _sleeveOptions, _petOptions, _titleOptions)));
			return new ViewModel(list, _myPlayerId, _deckOptions, _cardStyleOptions, _emblemOptions, _avatarOptions, _sleeveOptions, _petOptions, _titleOptions);
		}

		public ViewModel RemoveTeam(int idx)
		{
			if (idx < 0 || idx >= Configs.Count)
			{
				return this;
			}
			List<TeamConfigEditor.ViewModel> list = new List<TeamConfigEditor.ViewModel>(Configs);
			list.RemoveAt(idx);
			return new ViewModel(list, _myPlayerId, _deckOptions, _cardStyleOptions, _emblemOptions, _avatarOptions, _sleeveOptions, _petOptions, _titleOptions);
		}

		public override bool Equals(object obj)
		{
			if (obj is ViewModel other)
			{
				return Equals(other);
			}
			return false;
		}

		private bool Equals(ViewModel other)
		{
			if (Configs.Count != other.Configs.Count)
			{
				return false;
			}
			for (int i = 0; i < Configs.Count; i++)
			{
				if (!Configs[i].Equals(other.Configs[i]))
				{
					return false;
				}
			}
			return true;
		}
	}

	[SerializeField]
	private uint _minTeams = 2u;

	[SerializeField]
	private uint _maxTeams = 4u;

	[SerializeField]
	private TMP_Text _nameLabel;

	[SerializeField]
	private VisibilityToggle _visibilityToggle;

	[SerializeField]
	private TeamConfigEditor _prefab;

	[SerializeField]
	private Transform _prefabRoot;

	[SerializeField]
	private Button _addTeamButton;

	private readonly List<TeamConfigEditor> _teamEditors = new List<TeamConfigEditor>();

	private bool _isVisible = true;

	private ViewModel _viewModel;

	public IReadOnlyList<TeamConfigEditor> TeamEditors => _teamEditors;

	public event Action<ViewModel, ViewModel> ViewModelChanged;

	private void Awake()
	{
		_visibilityToggle.ToggleChanged += OnVisibilityToggled;
		_addTeamButton.onClick.AddListener(AddTeam);
	}

	private void OnDestroy()
	{
		_visibilityToggle.ToggleChanged -= OnVisibilityToggled;
		_addTeamButton.onClick.RemoveListener(AddTeam);
	}

	public void SetModel(ViewModel viewModel)
	{
		_viewModel = viewModel;
		uint num = 1u;
		for (int i = 0; i < viewModel.Configs.Count; i++)
		{
			TeamConfigEditor orCreateTeamEditor = GetOrCreateTeamEditor(i);
			TeamConfigEditor.ViewModel viewModel2 = viewModel.Configs[i];
			orCreateTeamEditor.SetModel(viewModel2, num);
			orCreateTeamEditor.SetCanDelete(viewModel.Configs.Count > _minTeams);
			num += (uint)viewModel2.Players.Configs.Count;
		}
		while (_teamEditors.Count > viewModel.Configs.Count)
		{
			int index = _teamEditors.Count - 1;
			TeamConfigEditor teamConfigEditor = _teamEditors[index];
			teamConfigEditor.ViewModelChanged -= UpdateTeam;
			teamConfigEditor.Deleted -= RemoveTeam;
			_teamEditors.RemoveAt(index);
			UnityEngine.Object.Destroy(teamConfigEditor.gameObject);
		}
		_addTeamButton.interactable = _viewModel.Configs.Count < _maxTeams;
		SetTeamNameText();
	}

	private void OnVisibilityToggled(bool visible)
	{
		_isVisible = visible;
		SetTeamNameText();
	}

	private void SetTeamNameText()
	{
		int count = _viewModel.Configs.Count;
		int num = _viewModel.Configs.Sum((TeamConfigEditor.ViewModel x) => x.Players.Configs.Count);
		_nameLabel.text = (_isVisible ? "Teams" : string.Format("Teams <size=70%>({0} {1}, {2} {3})</size>", count, (count == 1) ? "Team" : "Teams", num, (num == 1) ? "Player" : "Players"));
	}

	private TeamConfigEditor GetOrCreateTeamEditor(int index)
	{
		if (_teamEditors.Count > index)
		{
			return _teamEditors[index];
		}
		TeamConfigEditor teamConfigEditor = UnityEngine.Object.Instantiate(_prefab, _prefabRoot);
		teamConfigEditor.ViewModelChanged += UpdateTeam;
		teamConfigEditor.Deleted += RemoveTeam;
		_teamEditors.Add(teamConfigEditor);
		return teamConfigEditor;
	}

	private void UpdateTeam(TeamConfigEditor teamConfigEditor, TeamConfigEditor.ViewModel oldTeam, TeamConfigEditor.ViewModel updatedTeam)
	{
		int num = _teamEditors.IndexOf(teamConfigEditor);
		if (num != -1)
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.UpdateTeam(num, updatedTeam);
			this.ViewModelChanged?.Invoke(viewModel, _viewModel);
		}
	}

	public void AddTeam()
	{
		if (_teamEditors.Count < _maxTeams)
		{
			ViewModel viewModel = _viewModel;
			ViewModel viewModel2 = _viewModel.AddTeam();
			SetModel(viewModel2);
			this.ViewModelChanged?.Invoke(viewModel, viewModel2);
		}
	}

	public void RemoveTeam(TeamConfigEditor toRemove)
	{
		int num = _teamEditors.IndexOf(toRemove);
		if (num != -1 && _teamEditors.Count > _minTeams)
		{
			ViewModel viewModel = _viewModel;
			ViewModel viewModel2 = _viewModel.RemoveTeam(num);
			SetModel(viewModel2);
			this.ViewModelChanged?.Invoke(viewModel, viewModel2);
		}
	}
}
