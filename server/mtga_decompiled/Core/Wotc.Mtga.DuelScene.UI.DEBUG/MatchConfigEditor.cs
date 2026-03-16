using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class MatchConfigEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly string Name;

		public readonly BattlefieldSelectionEditor.ViewModel BattlefieldSelection;

		public readonly GameType GameType;

		public readonly GameVariant GameVariant;

		public readonly MatchWinCondition WinCondition;

		public readonly MulliganType MulliganType;

		public readonly bool UseSpecifiedSeed;

		public readonly IReadOnlyList<uint> RngSeed;

		public readonly uint FreeMulligans;

		public readonly uint MaxHandSize;

		public readonly ShuffleRestriction ShuffleRestriction;

		public readonly TimerPackage Timers;

		public readonly uint LandsPerTurn;

		public readonly TeamConfigListEditor.ViewModel Teams;

		public ViewModel(string name, BattlefieldSelectionEditor.ViewModel battlefieldSelection, GameType gameType, GameVariant gameVariant, MatchWinCondition winCondition, MulliganType mulliganType, bool useSpecifiedSeed, IReadOnlyList<uint> rngSeed, uint freeMulligans, uint maxHandSize, ShuffleRestriction shuffleRestriction, TimerPackage timers, uint landsPerTurn, TeamConfigListEditor.ViewModel teams)
		{
			Name = name;
			BattlefieldSelection = battlefieldSelection;
			GameType = gameType;
			GameVariant = gameVariant;
			WinCondition = winCondition;
			MulliganType = mulliganType;
			UseSpecifiedSeed = useSpecifiedSeed;
			RngSeed = rngSeed;
			FreeMulligans = freeMulligans;
			MaxHandSize = maxHandSize;
			ShuffleRestriction = shuffleRestriction;
			Timers = timers;
			LandsPerTurn = landsPerTurn;
			Teams = teams;
		}

		public ViewModel Modify(string name = null, BattlefieldSelectionEditor.ViewModel? battlefieldSelection = null, GameType? gameType = null, GameVariant? gameVariant = null, MatchWinCondition? winCondition = null, MulliganType? mulliganType = null, ShuffleRestriction? shuffleRestriction = null, bool? useSpecifiedSeed = null, IReadOnlyList<uint> rngSeed = null, uint? freeMulligans = null, uint? maxHandSize = null, TimerPackage? timers = null, uint? landsPerTurn = null, TeamConfigListEditor.ViewModel? teams = null)
		{
			return new ViewModel(name ?? Name, battlefieldSelection ?? BattlefieldSelection, gameType ?? GameType, gameVariant ?? GameVariant, winCondition ?? WinCondition, mulliganType ?? MulliganType, useSpecifiedSeed ?? UseSpecifiedSeed, rngSeed ?? RngSeed, freeMulligans ?? FreeMulligans, maxHandSize ?? MaxHandSize, shuffleRestriction ?? ShuffleRestriction, timers ?? Timers, landsPerTurn ?? LandsPerTurn, teams ?? Teams);
		}
	}

	[SerializeField]
	private MatchConfigNameEditor _configNameEditor;

	[SerializeField]
	private BattlefieldSelectionEditor _battlefieldSelectionEditor;

	[SerializeField]
	private TMP_Dropdown _gameTypeDropdown;

	[SerializeField]
	private TMP_Dropdown _gameVariantDropdown;

	[SerializeField]
	private TMP_Dropdown _winConditionDropdown;

	[SerializeField]
	private TMP_Dropdown _mulliganDropdown;

	[SerializeField]
	private TMP_Dropdown _shuffleRestrictionDropdown;

	[SerializeField]
	private TMP_Dropdown _timersDropdown;

	[SerializeField]
	private RngSeedEditor _rngSeedEditor;

	[SerializeField]
	private TMP_InputField _freeMulligansInputField;

	[SerializeField]
	private TMP_InputField _maxHandSizeInputField;

	[SerializeField]
	private TMP_InputField _landsPerTurnInputField;

	[SerializeField]
	private TeamConfigListEditor _teamConfigListEditor;

	private ViewModel _viewModel;

	public event Action<ViewModel, ViewModel> ViewModelUpdated;

	public PlayerConfigEditor GetPlayerEditor(int teamIndex, int playerIndex)
	{
		return _teamConfigListEditor.TeamEditors[teamIndex].PlayerEditors[playerIndex];
	}

	private void Awake()
	{
		_gameTypeDropdown.ClearOptions();
		foreach (GameType value6 in Enum.GetValues(typeof(GameType)))
		{
			_gameTypeDropdown.options.Add(new TMP_Dropdown.OptionData(EnumExtensions.EnumCleanName(value6)));
		}
		_gameVariantDropdown.ClearOptions();
		foreach (GameVariant value7 in Enum.GetValues(typeof(GameVariant)))
		{
			string text = EnumExtensions.EnumCleanName(value7);
			if (!text.Contains("Placeholder"))
			{
				_gameVariantDropdown.options.Add(new TMP_Dropdown.OptionData(text));
			}
		}
		_winConditionDropdown.ClearOptions();
		foreach (MatchWinCondition value8 in Enum.GetValues(typeof(MatchWinCondition)))
		{
			_winConditionDropdown.options.Add(new TMP_Dropdown.OptionData(EnumExtensions.EnumCleanName(value8)));
		}
		_mulliganDropdown.ClearOptions();
		foreach (MulliganType value9 in Enum.GetValues(typeof(MulliganType)))
		{
			_mulliganDropdown.options.Add(new TMP_Dropdown.OptionData(EnumExtensions.EnumCleanName(value9)));
		}
		_shuffleRestrictionDropdown.ClearOptions();
		foreach (ShuffleRestriction value10 in Enum.GetValues(typeof(ShuffleRestriction)))
		{
			_shuffleRestrictionDropdown.options.Add(new TMP_Dropdown.OptionData(EnumExtensions.EnumCleanName(value10)));
		}
		_timersDropdown.ClearOptions();
		foreach (TimerPackage value11 in Enum.GetValues(typeof(TimerPackage)))
		{
			_timersDropdown.options.Add(new TMP_Dropdown.OptionData(EnumExtensions.EnumCleanName(value11)));
		}
		_configNameEditor.ViewModelUpdated += OnConfigNameUpdated;
		_battlefieldSelectionEditor.ViewModelChanged += UpdateBattlefieldSelection;
		RngSeedEditor rngSeedEditor = _rngSeedEditor;
		rngSeedEditor.ViewModelChanged = (Action<RngSeedEditor.ViewModel, RngSeedEditor.ViewModel>)Delegate.Combine(rngSeedEditor.ViewModelChanged, new Action<RngSeedEditor.ViewModel, RngSeedEditor.ViewModel>(UpdateRngSeed));
		_teamConfigListEditor.ViewModelChanged += UpdateTeams;
		_gameTypeDropdown.onValueChanged.AddListener(OnGameTypeChanged);
		_gameVariantDropdown.onValueChanged.AddListener(OnGameVariantChanged);
		_winConditionDropdown.onValueChanged.AddListener(OnWinConditionChanged);
		_mulliganDropdown.onValueChanged.AddListener(OnMulliganTypeChanged);
		_timersDropdown.onValueChanged.AddListener(OnTimersChanged);
		_freeMulligansInputField.onValueChanged.AddListener(OnFreeMulligansChanged);
		_maxHandSizeInputField.onValueChanged.AddListener(OnMaxHandSizeChanged);
		_landsPerTurnInputField.onValueChanged.AddListener(OnLandsPerTurnChanged);
		_shuffleRestrictionDropdown.onValueChanged.AddListener(OnShuffleRestrictionChanged);
	}

	private void OnDestroy()
	{
		_configNameEditor.ViewModelUpdated -= OnConfigNameUpdated;
		_battlefieldSelectionEditor.ViewModelChanged -= UpdateBattlefieldSelection;
		RngSeedEditor rngSeedEditor = _rngSeedEditor;
		rngSeedEditor.ViewModelChanged = (Action<RngSeedEditor.ViewModel, RngSeedEditor.ViewModel>)Delegate.Remove(rngSeedEditor.ViewModelChanged, new Action<RngSeedEditor.ViewModel, RngSeedEditor.ViewModel>(UpdateRngSeed));
		_teamConfigListEditor.ViewModelChanged -= UpdateTeams;
		_gameTypeDropdown.onValueChanged.RemoveListener(OnGameTypeChanged);
		_gameVariantDropdown.onValueChanged.RemoveListener(OnGameVariantChanged);
		_winConditionDropdown.onValueChanged.RemoveListener(OnWinConditionChanged);
		_mulliganDropdown.onValueChanged.RemoveListener(OnMulliganTypeChanged);
		_timersDropdown.onValueChanged.RemoveListener(OnTimersChanged);
		_freeMulligansInputField.onValueChanged.RemoveListener(OnFreeMulligansChanged);
		_maxHandSizeInputField.onValueChanged.RemoveListener(OnMaxHandSizeChanged);
		_landsPerTurnInputField.onValueChanged.RemoveListener(OnLandsPerTurnChanged);
		_shuffleRestrictionDropdown.onValueChanged.RemoveListener(OnShuffleRestrictionChanged);
	}

	public void SetModel(ViewModel viewModel)
	{
		_viewModel = viewModel;
		_configNameEditor.SetViewModel(viewModel.Name);
		_battlefieldSelectionEditor.SetModel(viewModel.BattlefieldSelection);
		_gameTypeDropdown.SetValueWithoutNotify((int)viewModel.GameType);
		_gameVariantDropdown.SetValueWithoutNotify((int)viewModel.GameVariant);
		_winConditionDropdown.SetValueWithoutNotify((int)viewModel.WinCondition);
		_mulliganDropdown.SetValueWithoutNotify((int)viewModel.MulliganType);
		_timersDropdown.SetValueWithoutNotify((int)viewModel.Timers);
		_shuffleRestrictionDropdown.SetValueWithoutNotify((int)viewModel.ShuffleRestriction);
		_freeMulligansInputField.SetTextWithoutNotify(_viewModel.FreeMulligans.ToString());
		_maxHandSizeInputField.SetTextWithoutNotify(_viewModel.MaxHandSize.ToString());
		_landsPerTurnInputField.SetTextWithoutNotify(viewModel.LandsPerTurn.ToString());
		_rngSeedEditor.SetModel(new RngSeedEditor.ViewModel(viewModel.UseSpecifiedSeed, viewModel.RngSeed));
		_teamConfigListEditor.SetModel(viewModel.Teams);
	}

	private void UpdateTeams(TeamConfigListEditor.ViewModel oldTeams, TeamConfigListEditor.ViewModel updatedTeams)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		TeamConfigListEditor.ViewModel? teams = updatedTeams;
		_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, teams);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void OnConfigNameUpdated(string oldName, string newName)
	{
		ViewModel viewModel = _viewModel;
		_viewModel = _viewModel.Modify(newName);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void UpdateBattlefieldSelection(BattlefieldSelectionEditor.ViewModel oldBattlefieldSelection, BattlefieldSelectionEditor.ViewModel updatedBattlefieldSelection)
	{
		ViewModel viewModel = _viewModel;
		_viewModel = _viewModel.Modify(null, updatedBattlefieldSelection);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void UpdateRngSeed(RngSeedEditor.ViewModel oldSeed, RngSeedEditor.ViewModel updatedSeed)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		bool? useSpecifiedSeed = updatedSeed.UseSpecifiedSeed;
		IReadOnlyList<uint> rngSeed = updatedSeed.RngSeed;
		_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, useSpecifiedSeed, rngSeed);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void OnGameTypeChanged(int idx)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		GameType? gameType = (GameType)idx;
		_viewModel = viewModel2.Modify(null, null, gameType);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void OnGameVariantChanged(int idx)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		GameVariant? gameVariant = (GameVariant)idx;
		_viewModel = viewModel2.Modify(null, null, null, gameVariant);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void OnWinConditionChanged(int idx)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		MatchWinCondition? winCondition = (MatchWinCondition)idx;
		_viewModel = viewModel2.Modify(null, null, null, null, winCondition);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void OnMulliganTypeChanged(int idx)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		MulliganType? mulliganType = (MulliganType)idx;
		_viewModel = viewModel2.Modify(null, null, null, null, null, mulliganType);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void OnFreeMulligansChanged(string value)
	{
		if (uint.TryParse(value, out var result))
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			uint? freeMulligans = result;
			_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, freeMulligans);
			this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
		}
	}

	private void OnMaxHandSizeChanged(string value)
	{
		if (uint.TryParse(value, out var result))
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			uint? maxHandSize = result;
			_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, maxHandSize);
			this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
		}
	}

	private void OnShuffleRestrictionChanged(int value)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		ShuffleRestriction? shuffleRestriction = (ShuffleRestriction)value;
		_viewModel = viewModel2.Modify(null, null, null, null, null, null, shuffleRestriction);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void OnTimersChanged(int idx)
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		TimerPackage? timers = (TimerPackage)idx;
		_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, null, timers);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void OnLandsPerTurnChanged(string value)
	{
		if (uint.TryParse(value, out var result))
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			uint? landsPerTurn = result;
			_viewModel = viewModel2.Modify(null, null, null, null, null, null, null, null, null, null, null, null, landsPerTurn);
			this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
		}
	}
}
