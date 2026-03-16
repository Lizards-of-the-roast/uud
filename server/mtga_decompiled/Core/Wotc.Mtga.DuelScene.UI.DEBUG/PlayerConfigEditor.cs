using System;
using System.Collections.Generic;
using GreClient.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class PlayerConfigEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly string Name;

		public readonly string PlayerId;

		public readonly PlayerType PlayerType;

		public readonly DeckConfigEditor.ViewModel Deck;

		public readonly CardStyleListEditor.ViewModel Styles;

		public readonly FamiliarStrategyType FamiliarStrategy;

		public readonly string SelectedAvatar;

		public readonly string SelectedSleeve;

		public readonly string SelectedTitle;

		public readonly (string petId, string variantId) SelectedPet;

		public readonly RankConfigEditor.ViewModel Rank;

		public readonly ShuffleRestriction ShuffleRestriction;

		public readonly uint StartingLife;

		public readonly uint StartingHandSize;

		public readonly bool TreeOfCongress;

		public readonly bool StartingPlayer;

		public readonly EmblemConfigListEditor.ViewModel Emblems;

		public readonly string MyPlayerId;

		public readonly IReadOnlyList<string> AvatarOptions;

		public readonly IReadOnlyList<string> SleeveOptions;

		public readonly IReadOnlyList<(string petId, string variantId)> PetOptions;

		public readonly IReadOnlyList<string> TitleOptions;

		public string SelectedDeckDirectory => Deck.SelectedDirectory;

		public GreClient.Network.DeckConfig SelectedDeck => Deck.SelectedDeck;

		public string SelectedDeckName => SelectedDeck.Name;

		public ViewModel(string name, PlayerType playerType, string playerId, DeckConfigEditor.ViewModel deck, CardStyleListEditor.ViewModel styles, FamiliarStrategyType familiarStrategy, string selectedAvatar, string selectedSleeve, (string petId, string variantId) selectedPet, string selectedTitle, RankConfigEditor.ViewModel rank, ShuffleRestriction shuffleRestriction, uint startingLife, uint startingHandSize, bool treeOfCongress, bool startingPlayer, EmblemConfigListEditor.ViewModel emblems, string myPlayerId, IReadOnlyList<string> avatarOptions, IReadOnlyList<string> sleeveOptions, IReadOnlyList<(string petId, string variantId)> petOptions, IReadOnlyList<string> titleOptions)
		{
			Name = name;
			PlayerType = playerType;
			PlayerId = playerId;
			Deck = deck;
			Styles = styles;
			FamiliarStrategy = familiarStrategy;
			ShuffleRestriction = shuffleRestriction;
			SelectedAvatar = selectedAvatar;
			SelectedSleeve = selectedSleeve;
			SelectedPet = selectedPet;
			SelectedTitle = selectedTitle;
			Rank = rank;
			StartingLife = startingLife;
			StartingHandSize = startingHandSize;
			TreeOfCongress = treeOfCongress;
			StartingPlayer = startingPlayer;
			Emblems = emblems;
			MyPlayerId = myPlayerId;
			AvatarOptions = avatarOptions ?? Array.Empty<string>();
			SleeveOptions = sleeveOptions ?? Array.Empty<string>();
			PetOptions = petOptions ?? Array.Empty<(string, string)>();
			TitleOptions = titleOptions ?? Array.Empty<string>();
		}

		public ViewModel Modify(string screenName = null, PlayerType? playerType = null, string playerId = null, DeckConfigEditor.ViewModel? deck = null, CardStyleListEditor.ViewModel? styles = null, FamiliarStrategyType? familiarStrategy = null, string selectedAvatar = null, string selectedSleeve = null, (string petId, string variantId)? selectedPet = null, string selectedTitle = null, RankConfigEditor.ViewModel? rank = null, ShuffleRestriction? shuffleRestriction = null, uint? startingLife = null, uint? startingHandSize = null, bool? treeOfCongress = null, bool? startingPlayer = null, EmblemConfigListEditor.ViewModel? emblems = null)
		{
			return new ViewModel(screenName ?? Name, playerType ?? PlayerType, playerId ?? PlayerId, deck ?? Deck, styles ?? Styles, familiarStrategy ?? FamiliarStrategy, selectedAvatar ?? SelectedAvatar, selectedSleeve ?? SelectedSleeve, selectedPet ?? SelectedPet, selectedTitle ?? SelectedTitle, rank ?? Rank, shuffleRestriction ?? ShuffleRestriction, startingLife ?? StartingLife, startingHandSize ?? StartingHandSize, treeOfCongress ?? TreeOfCongress, startingPlayer ?? StartingPlayer, emblems ?? Emblems, MyPlayerId, AvatarOptions, SleeveOptions, PetOptions, TitleOptions);
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
			if (Name == other.Name && PlayerId == other.PlayerId && PlayerType == other.PlayerType && Deck.Equals(other.Deck) && Styles.Equals(other.Styles) && FamiliarStrategy == other.FamiliarStrategy && StartingLife == other.StartingLife && StartingHandSize == other.StartingHandSize && TreeOfCongress == other.TreeOfCongress && StartingPlayer == other.StartingPlayer && ShuffleRestriction == other.ShuffleRestriction && MyPlayerId == other.MyPlayerId)
			{
				return Rank.Equals(other.Rank);
			}
			return false;
		}
	}

	[SerializeField]
	private TMP_Text _playerLabel;

	[SerializeField]
	private Button _deleteButton;

	[SerializeField]
	private VisibilityToggle _visibilityToggle;

	[SerializeField]
	private TMP_InputField _screenNameInputField;

	[SerializeField]
	private TMP_Dropdown _playerTypeDropdown;

	[SerializeField]
	private TMP_Dropdown _familiarStrategyDropdown;

	[SerializeField]
	private TMP_InputField _playerIdInputField;

	[SerializeField]
	private Button _copyPlayerIdButton;

	[SerializeField]
	private DeckConfigEditor _deckConfigEditor;

	[SerializeField]
	private CardStyleListEditor _cardStyleListEditor;

	[SerializeField]
	private InputFieldDropdown _avatarDropdown;

	[SerializeField]
	private InputFieldDropdown _sleeveSelector;

	[SerializeField]
	private InputFieldDropdown _petDropdown;

	[SerializeField]
	private InputFieldDropdown _titleSelector;

	[SerializeField]
	private Toggle _startingPlayerToggle;

	[SerializeField]
	private TMP_InputField _startingLifeInputField;

	[SerializeField]
	private TMP_InputField _startingHandSizeInputField;

	[SerializeField]
	private Toggle _treeOfCongressToggle;

	[SerializeField]
	private TMP_Dropdown _shuffleRestrictionDropdown;

	[SerializeField]
	private EmblemConfigListEditor _emblemEditor;

	[SerializeField]
	private RankConfigEditor _rankEditor;

	private bool _isVisible = true;

	private string _playerLabelText;

	private ViewModel _viewModel;

	private Dictionary<string, (string, string)> _flattenedPetOptions = new Dictionary<string, (string, string)>();

	private List<string> _petOptions = new List<string>();

	public DeckConfigEditor DeckEditor => _deckConfigEditor;

	public event Action<PlayerConfigEditor, ViewModel, ViewModel> ViewModelChanged;

	public event Action<PlayerConfigEditor> Deleted;

	private void Awake()
	{
		_playerTypeDropdown.ClearOptions();
		foreach (PlayerType value2 in Enum.GetValues(typeof(PlayerType)))
		{
			_playerTypeDropdown.options.Add(new TMP_Dropdown.OptionData(value2.ToString()));
		}
		_familiarStrategyDropdown.ClearOptions();
		foreach (FamiliarStrategyType value3 in Enum.GetValues(typeof(FamiliarStrategyType)))
		{
			if (value3 != FamiliarStrategyType.None)
			{
				_familiarStrategyDropdown.options.Add(new TMP_Dropdown.OptionData(value3.ToString()));
			}
		}
		_shuffleRestrictionDropdown.ClearOptions();
		foreach (ShuffleRestriction value4 in Enum.GetValues(typeof(ShuffleRestriction)))
		{
			_shuffleRestrictionDropdown.options.Add(new TMP_Dropdown.OptionData(EnumExtensions.EnumCleanName(value4)));
		}
		_visibilityToggle.ToggleChanged += OnVisibilityToggled;
		_emblemEditor.ViewModelChanged += OnEmblemsModified;
		_deckConfigEditor.ViewModelChanged += OnDeckChanged;
		_cardStyleListEditor.ViewModelChanged += OnCardStylesChanged;
		_rankEditor.ViewModelUpdated += OnRankChanged;
		_deleteButton.onClick.AddListener(OnDeleteClicked);
		_playerTypeDropdown.onValueChanged.AddListener(OnPlayerTypeChanged);
		_familiarStrategyDropdown.onValueChanged.AddListener(OnFamiliarStrategyChanged);
		_playerIdInputField.onValueChanged.AddListener(OnPlayerIdUpdated);
		_copyPlayerIdButton.onClick.AddListener(CopyPlayerId);
		InputFieldDropdown avatarDropdown = _avatarDropdown;
		avatarDropdown.ValueChanged = (Action<string>)Delegate.Combine(avatarDropdown.ValueChanged, new Action<string>(OnAvatarChanged));
		InputFieldDropdown sleeveSelector = _sleeveSelector;
		sleeveSelector.ValueChanged = (Action<string>)Delegate.Combine(sleeveSelector.ValueChanged, new Action<string>(OnSleeveChanged));
		InputFieldDropdown petDropdown = _petDropdown;
		petDropdown.ValueChanged = (Action<string>)Delegate.Combine(petDropdown.ValueChanged, new Action<string>(OnPetChanged));
		InputFieldDropdown titleSelector = _titleSelector;
		titleSelector.ValueChanged = (Action<string>)Delegate.Combine(titleSelector.ValueChanged, new Action<string>(OnTitleChanged));
		_startingLifeInputField.onValueChanged.AddListener(OnStartingLifeChanged);
		_startingHandSizeInputField.onValueChanged.AddListener(OnStartingHandSizeChanged);
		_treeOfCongressToggle.onValueChanged.AddListener(OnTreeOfCongressChanged);
		_startingPlayerToggle.onValueChanged.AddListener(OnStartingPlayerToggleChanged);
		_shuffleRestrictionDropdown.onValueChanged.AddListener(OnShuffleRestrictionUpdated);
		_screenNameInputField.onValueChanged.AddListener(OnScreenNameChanged);
	}

	private void OnDestroy()
	{
		_visibilityToggle.ToggleChanged -= OnVisibilityToggled;
		_emblemEditor.ViewModelChanged -= OnEmblemsModified;
		_deckConfigEditor.ViewModelChanged -= OnDeckChanged;
		_cardStyleListEditor.ViewModelChanged -= OnCardStylesChanged;
		_rankEditor.ViewModelUpdated -= OnRankChanged;
		_playerTypeDropdown.onValueChanged.RemoveListener(OnPlayerTypeChanged);
		_familiarStrategyDropdown.onValueChanged.RemoveListener(OnFamiliarStrategyChanged);
		_playerIdInputField.onValueChanged.RemoveListener(OnPlayerIdUpdated);
		_copyPlayerIdButton.onClick.RemoveListener(CopyPlayerId);
		InputFieldDropdown avatarDropdown = _avatarDropdown;
		avatarDropdown.ValueChanged = (Action<string>)Delegate.Remove(avatarDropdown.ValueChanged, new Action<string>(OnAvatarChanged));
		InputFieldDropdown sleeveSelector = _sleeveSelector;
		sleeveSelector.ValueChanged = (Action<string>)Delegate.Remove(sleeveSelector.ValueChanged, new Action<string>(OnSleeveChanged));
		InputFieldDropdown petDropdown = _petDropdown;
		petDropdown.ValueChanged = (Action<string>)Delegate.Remove(petDropdown.ValueChanged, new Action<string>(OnPetChanged));
		InputFieldDropdown titleSelector = _titleSelector;
		titleSelector.ValueChanged = (Action<string>)Delegate.Remove(titleSelector.ValueChanged, new Action<string>(OnTitleChanged));
		_deleteButton.onClick.RemoveListener(OnDeleteClicked);
		_startingLifeInputField.onValueChanged.RemoveListener(OnStartingLifeChanged);
		_startingHandSizeInputField.onValueChanged.RemoveListener(OnStartingHandSizeChanged);
		_treeOfCongressToggle.onValueChanged.RemoveListener(OnTreeOfCongressChanged);
		_startingPlayerToggle.onValueChanged.RemoveListener(OnStartingPlayerToggleChanged);
		_shuffleRestrictionDropdown.onValueChanged.RemoveListener(OnShuffleRestrictionUpdated);
		_screenNameInputField.onValueChanged.RemoveListener(OnScreenNameChanged);
	}

	private void OnVisibilityToggled(bool visible)
	{
		_isVisible = visible;
		_familiarStrategyDropdown.gameObject.SetActive(_isVisible && _viewModel.PlayerType == PlayerType.Bot);
		_playerLabel.text = FormattedPlayerLabelText(_playerLabelText);
	}

	public void SetModel(ViewModel viewModel)
	{
		if (_viewModel.Equals(viewModel))
		{
			return;
		}
		_viewModel = viewModel;
		_playerTypeDropdown.SetValueWithoutNotify((int)viewModel.PlayerType);
		_playerTypeDropdown.RefreshShownValue();
		_playerIdInputField.SetTextWithoutNotify(viewModel.PlayerId);
		_playerIdInputField.interactable = viewModel.PlayerType == PlayerType.Human;
		_avatarDropdown.SetModel(new InputFieldDropdown.ViewModel(viewModel.SelectedAvatar, viewModel.AvatarOptions));
		_sleeveSelector.SetModel(new InputFieldDropdown.ViewModel(viewModel.SelectedSleeve, viewModel.SleeveOptions));
		_flattenedPetOptions.Clear();
		_petOptions.Clear();
		foreach (var petOption in viewModel.PetOptions)
		{
			string text = normalizedPetOption(petOption);
			_flattenedPetOptions[text] = petOption;
			_petOptions.Add(text);
		}
		_petDropdown.SetModel(new InputFieldDropdown.ViewModel(normalizedPetOption(viewModel.SelectedPet), _petOptions));
		_titleSelector.SetModel(new InputFieldDropdown.ViewModel(viewModel.SelectedTitle, viewModel.TitleOptions));
		_familiarStrategyDropdown.gameObject.SetActive(_viewModel.PlayerType == PlayerType.Bot);
		_familiarStrategyDropdown.SetValueWithoutNotify(ToDropdownIndex(viewModel.FamiliarStrategy));
		_startingLifeInputField.SetTextWithoutNotify(_viewModel.StartingLife.ToString());
		_startingHandSizeInputField.SetTextWithoutNotify(_viewModel.StartingHandSize.ToString());
		_treeOfCongressToggle.SetIsOnWithoutNotify(_viewModel.TreeOfCongress);
		_startingPlayerToggle.SetIsOnWithoutNotify(_viewModel.StartingPlayer);
		_emblemEditor.SetModel(_viewModel.Emblems);
		_deckConfigEditor.SetModel(_viewModel.Deck);
		_cardStyleListEditor.SetModel(_viewModel.Styles);
		_shuffleRestrictionDropdown.SetValueWithoutNotify((int)viewModel.ShuffleRestriction);
		_screenNameInputField.SetTextWithoutNotify(viewModel.Name);
		_rankEditor.SetModel(viewModel.Rank);
		static string normalizedPetOption((string id, string variant) petOption)
		{
			if (!string.IsNullOrEmpty(petOption.variant))
			{
				return petOption.id + " (" + petOption.variant + ")";
			}
			return petOption.id;
		}
	}

	public void SetPlayerLabel(string playerLabelText)
	{
		_playerLabelText = playerLabelText;
		_playerLabel.text = FormattedPlayerLabelText(playerLabelText);
	}

	private string FormattedPlayerLabelText(string unformatted)
	{
		if (!_isVisible)
		{
			return $"{unformatted} <size=70%>({_viewModel.PlayerType}, {_viewModel.SelectedDeckName})</size>";
		}
		return unformatted;
	}

	public void SetCanDelete(bool canDelete)
	{
		_deleteButton.interactable = canDelete;
	}

	private void OnScreenNameChanged(string playerName)
	{
		if (!(playerName == _viewModel.Name))
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(playerName);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnAvatarChanged(string avatarId)
	{
		if (!(_viewModel.SelectedAvatar == avatarId))
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(null, null, null, null, null, null, avatarId);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnSleeveChanged(string sleeve)
	{
		if (!(sleeve == _viewModel.SelectedSleeve))
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(null, null, null, null, null, null, null, sleeve);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnPetChanged(string flattenedPetOption)
	{
		if (_flattenedPetOptions.TryGetValue(flattenedPetOption, out var value))
		{
			int index = _viewModel.PetOptions.IndexOf(value);
			(string, string) value2 = _viewModel.PetOptions[index];
			if (!(value2.Item1 == _viewModel.SelectedPet.petId) || !(value2.Item2 == _viewModel.SelectedPet.variantId))
			{
				ViewModel viewModel = _viewModel;
				ref ViewModel viewModel2 = ref _viewModel;
				(string, string)? selectedPet = value2;
				_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, selectedPet);
				this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
			}
		}
		else
		{
			_petDropdown.SetModel(new InputFieldDropdown.ViewModel(flattenedPetOption, _petOptions));
		}
	}

	private void OnTitleChanged(string title)
	{
		if (!(title == _viewModel.SelectedTitle))
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(null, null, null, null, null, null, null, null, null, title);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnPlayerTypeChanged(int idx)
	{
		if (idx != (int)_viewModel.PlayerType)
		{
			FamiliarStrategyType familiarStrategyType = ((idx == 1) ? FamiliarStrategyType.Generic : FamiliarStrategyType.None);
			_familiarStrategyDropdown.gameObject.SetActive(idx == 1);
			_familiarStrategyDropdown.SetValueWithoutNotify(ToDropdownIndex(familiarStrategyType));
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			PlayerType? playerType = (PlayerType)idx;
			string playerId = ((idx == 2) ? string.Empty : _viewModel.MyPlayerId);
			FamiliarStrategyType? familiarStrategy = familiarStrategyType;
			_viewModel = viewModel2.Modify(null, playerType, playerId, null, null, familiarStrategy);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnFamiliarStrategyChanged(int idx)
	{
		FamiliarStrategyType familiarStrategyType = FromDropdownIndex(idx);
		if (familiarStrategyType != _viewModel.FamiliarStrategy)
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			FamiliarStrategyType? familiarStrategy = familiarStrategyType;
			_viewModel = viewModel2.Modify(null, null, null, null, null, familiarStrategy);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnStartingLifeChanged(string value)
	{
		if (uint.TryParse(value, out var result))
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			uint? startingLife = result;
			_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, null, null, startingLife);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnStartingHandSizeChanged(string value)
	{
		if (uint.TryParse(value, out var result))
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			uint? startingHandSize = result;
			_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, startingHandSize);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnTreeOfCongressChanged(bool value)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		bool? treeOfCongress = value;
		_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, treeOfCongress);
		this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
	}

	private void OnStartingPlayerToggleChanged(bool value)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		bool? startingPlayer = value;
		_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, startingPlayer);
		this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
	}

	private void OnPlayerIdUpdated(string playerId)
	{
		if (!(playerId == _viewModel.PlayerId))
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(null, null, playerId);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnShuffleRestrictionUpdated(int idx)
	{
		if (idx != (int)_viewModel.ShuffleRestriction)
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			ShuffleRestriction? shuffleRestriction = (ShuffleRestriction)idx;
			_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, null, shuffleRestriction);
			this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
		}
	}

	private void OnEmblemsModified(EmblemConfigListEditor.ViewModel oldEmblems, EmblemConfigListEditor.ViewModel updatedEmblems)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		EmblemConfigListEditor.ViewModel? emblems = updatedEmblems;
		_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, emblems);
		this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
	}

	private void OnDeckChanged(DeckConfigEditor.ViewModel oldDeck, DeckConfigEditor.ViewModel updatedDeck)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		DeckConfigEditor.ViewModel? deck = updatedDeck;
		CardStyleListEditor.ViewModel? styles = _viewModel.Styles.Modify(Array.Empty<CardStyleEditor.ViewModel>());
		_viewModel = viewModel2.Modify(null, null, null, deck, styles);
		this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
	}

	private void OnCardStylesChanged(CardStyleListEditor.ViewModel oldStyles, CardStyleListEditor.ViewModel updatedStyles)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		CardStyleListEditor.ViewModel? styles = updatedStyles;
		_viewModel = viewModel2.Modify(null, null, null, null, styles);
		this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
	}

	private void OnRankChanged(RankConfigEditor.ViewModel oldRank, RankConfigEditor.ViewModel updatedRank)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		RankConfigEditor.ViewModel? rank = updatedRank;
		_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, rank);
		this.ViewModelChanged?.Invoke(this, viewModel, _viewModel);
	}

	private void OnDeleteClicked()
	{
		this.Deleted?.Invoke(this);
	}

	private void CopyPlayerId()
	{
		GUIUtility.systemCopyBuffer = _viewModel.PlayerId;
	}

	private static int ToDropdownIndex(FamiliarStrategyType strategyType)
	{
		return strategyType switch
		{
			FamiliarStrategyType.Generic => 0, 
			FamiliarStrategyType.None => 1, 
			FamiliarStrategyType.Goldfish => 1, 
			FamiliarStrategyType.Random => 2, 
			FamiliarStrategyType.Sparky => 3, 
			FamiliarStrategyType.NPE_Game1 => 4, 
			FamiliarStrategyType.NPE_Game2 => 5, 
			_ => 1, 
		};
	}

	private static FamiliarStrategyType FromDropdownIndex(int idx)
	{
		return idx switch
		{
			0 => FamiliarStrategyType.Generic, 
			1 => FamiliarStrategyType.Goldfish, 
			2 => FamiliarStrategyType.Random, 
			3 => FamiliarStrategyType.Sparky, 
			4 => FamiliarStrategyType.NPE_Game1, 
			5 => FamiliarStrategyType.NPE_Game2, 
			_ => FamiliarStrategyType.None, 
		};
	}
}
