using System;
using System.Collections.Generic;
using GreClient.Network;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class PlayerConfigListEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly IReadOnlyList<PlayerConfigEditor.ViewModel> Configs;

		private readonly string _myPlayerId;

		private readonly IReadOnlyDictionary<string, IReadOnlyList<GreClient.Network.DeckConfig>> _deckOptions;

		private readonly IReadOnlyDictionary<GreClient.Network.DeckConfig, IReadOnlyList<CardStyle>> _cardStyleOptions;

		private readonly IReadOnlyList<EmblemData> _emblemOptions;

		private readonly IReadOnlyList<string> _avatarOptions;

		private readonly IReadOnlyList<string> _sleeveOptions;

		private readonly IReadOnlyList<(string petId, string variantId)> _petOptions;

		private readonly IReadOnlyList<string> _titleOptions;

		public PlayerConfigEditor.ViewModel this[int idx] => Configs[idx];

		public ViewModel(IReadOnlyList<PlayerConfigEditor.ViewModel> players, string myPlayerId, IReadOnlyDictionary<string, IReadOnlyList<GreClient.Network.DeckConfig>> deckOptions, IReadOnlyDictionary<GreClient.Network.DeckConfig, IReadOnlyList<CardStyle>> cardStyleOptions, IReadOnlyList<EmblemData> emblemOptions, IReadOnlyList<string> avatarOptions, IReadOnlyList<string> sleeveOptions, IReadOnlyList<(string petId, string variantId)> petOptions, IReadOnlyList<string> titleOptions)
		{
			Configs = players ?? Array.Empty<PlayerConfigEditor.ViewModel>();
			_myPlayerId = myPlayerId;
			_deckOptions = deckOptions ?? DictionaryExtensions.Empty<string, IReadOnlyList<GreClient.Network.DeckConfig>>();
			_cardStyleOptions = cardStyleOptions ?? DictionaryExtensions.Empty<GreClient.Network.DeckConfig, IReadOnlyList<CardStyle>>();
			_emblemOptions = emblemOptions ?? Array.Empty<EmblemData>();
			_avatarOptions = avatarOptions ?? Array.Empty<string>();
			_sleeveOptions = sleeveOptions ?? Array.Empty<string>();
			_petOptions = petOptions ?? Array.Empty<(string, string)>();
			_titleOptions = titleOptions ?? Array.Empty<string>();
		}

		public ViewModel UpdatePlayer(int idx, PlayerConfigEditor.ViewModel player)
		{
			if (idx < 0 || idx >= Configs.Count)
			{
				return this;
			}
			List<PlayerConfigEditor.ViewModel> list = new List<PlayerConfigEditor.ViewModel>(Configs);
			if (list[idx].StartingPlayer != player.StartingPlayer && player.StartingPlayer)
			{
				for (int i = 0; i < Configs.Count; i++)
				{
					if (i != idx && list[i].StartingPlayer)
					{
						int index = i;
						PlayerConfigEditor.ViewModel viewModel = list[i];
						bool? startingPlayer = false;
						list[index] = viewModel.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, startingPlayer);
					}
				}
			}
			list[idx] = player;
			return new ViewModel(list, _myPlayerId, _deckOptions, _cardStyleOptions, _emblemOptions, _avatarOptions, _sleeveOptions, _petOptions, _titleOptions);
		}

		public ViewModel AddPlayer()
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
			return new ViewModel(new List<PlayerConfigEditor.ViewModel>(Configs)
			{
				new PlayerConfigEditor.ViewModel("Opponent", PlayerType.Bot, _myPlayerId, new DeckConfigEditor.ViewModel(string.Empty, selectedDeck, _deckOptions), new CardStyleListEditor.ViewModel(Array.Empty<CardStyleEditor.ViewModel>(), Array.Empty<CardStyle>()), FamiliarStrategyType.None, "Avatar_Basic_AjaniGoldmane", string.Empty, (petId: string.Empty, variantId: string.Empty), "NoTitle", new RankConfigEditor.ViewModel(RankingClassType.None, 0, 0f, 0), ShuffleRestriction.None, 20u, 7u, treeOfCongress: false, startingPlayer: false, new EmblemConfigListEditor.ViewModel(Array.Empty<EmblemConfigEditor.ViewModel>(), _emblemOptions), _myPlayerId, _avatarOptions, _sleeveOptions, _petOptions, _titleOptions)
			}, _myPlayerId, _deckOptions, _cardStyleOptions, _emblemOptions, _avatarOptions, _sleeveOptions, _petOptions, _titleOptions);
		}

		public ViewModel RemovePlayer(int idx)
		{
			if (idx < 0 || idx >= Configs.Count)
			{
				return this;
			}
			List<PlayerConfigEditor.ViewModel> list = new List<PlayerConfigEditor.ViewModel>(Configs);
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
			if (Configs == other.Configs && _myPlayerId == other._myPlayerId)
			{
				return _deckOptions == other._deckOptions;
			}
			return false;
		}
	}

	[SerializeField]
	private uint _minPlayers = 1u;

	[SerializeField]
	private uint _maxPlayers = 4u;

	[SerializeField]
	private PlayerConfigEditor _playerConfigEditorPrefab;

	[SerializeField]
	private Transform _playerEditorRoot;

	[SerializeField]
	private Button _addPlayerButton;

	private readonly List<PlayerConfigEditor> _playerEditors = new List<PlayerConfigEditor>();

	private ViewModel _viewModel;

	public IReadOnlyList<PlayerConfigEditor> PlayerEditors => _playerEditors;

	public event Action<ViewModel, ViewModel> ViewModelChanged;

	private void Awake()
	{
		_addPlayerButton.onClick.AddListener(AddPlayer);
	}

	private void OnDestroy()
	{
		_addPlayerButton.onClick.RemoveListener(AddPlayer);
	}

	public void SetModel(ViewModel viewModel)
	{
		_viewModel = viewModel;
		for (int i = 0; i < viewModel.Configs.Count; i++)
		{
			PlayerConfigEditor orCreatePlayerEditor = GetOrCreatePlayerEditor(i);
			orCreatePlayerEditor.SetModel(viewModel.Configs[i]);
			orCreatePlayerEditor.SetCanDelete(viewModel.Configs.Count > _minPlayers);
		}
		while (_playerEditors.Count > viewModel.Configs.Count)
		{
			int index = _playerEditors.Count - 1;
			PlayerConfigEditor playerConfigEditor = _playerEditors[index];
			playerConfigEditor.ViewModelChanged -= UpdatePlayer;
			playerConfigEditor.Deleted -= RemovePlayer;
			_playerEditors.RemoveAt(index);
			UnityEngine.Object.Destroy(playerConfigEditor.gameObject);
		}
		_addPlayerButton.interactable = _viewModel.Configs.Count < _maxPlayers;
	}

	private PlayerConfigEditor GetOrCreatePlayerEditor(int index)
	{
		if (_playerEditors.Count > index)
		{
			return _playerEditors[index];
		}
		PlayerConfigEditor playerConfigEditor = UnityEngine.Object.Instantiate(_playerConfigEditorPrefab, _playerEditorRoot);
		playerConfigEditor.ViewModelChanged += UpdatePlayer;
		playerConfigEditor.Deleted += RemovePlayer;
		_playerEditors.Add(playerConfigEditor);
		return playerConfigEditor;
	}

	private void UpdatePlayer(PlayerConfigEditor playerConfigEditor, PlayerConfigEditor.ViewModel oldPlayer, PlayerConfigEditor.ViewModel updatedPlayer)
	{
		int num = _playerEditors.IndexOf(playerConfigEditor);
		if (num != -1)
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.UpdatePlayer(num, updatedPlayer);
			this.ViewModelChanged?.Invoke(viewModel, _viewModel);
		}
	}

	public void SetPlayerLabels(uint startingSeatId)
	{
		foreach (PlayerConfigEditor playerEditor in _playerEditors)
		{
			playerEditor.SetPlayerLabel($"Player {startingSeatId++}");
		}
	}

	public void AddPlayer()
	{
		if (_playerEditors.Count < _maxPlayers)
		{
			ViewModel viewModel = _viewModel;
			ViewModel viewModel2 = _viewModel.AddPlayer();
			SetModel(viewModel2);
			this.ViewModelChanged?.Invoke(viewModel, viewModel2);
		}
	}

	public void RemovePlayer(PlayerConfigEditor toRemove)
	{
		int num = _playerEditors.IndexOf(toRemove);
		if (num != -1 && _playerEditors.Count > _minPlayers)
		{
			ViewModel viewModel = _viewModel;
			ViewModel viewModel2 = _viewModel.RemovePlayer(num);
			SetModel(viewModel2);
			this.ViewModelChanged?.Invoke(viewModel, viewModel2);
		}
	}
}
