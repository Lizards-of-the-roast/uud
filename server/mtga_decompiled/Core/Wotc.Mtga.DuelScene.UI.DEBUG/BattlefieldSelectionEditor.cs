using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class BattlefieldSelectionEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly string SelectedBattlefield;

		public readonly IReadOnlyList<BattlefieldData> BattlefieldsOptions;

		public ViewModel(string selectedBattlefield, IReadOnlyList<BattlefieldData> battlefieldsOptions)
		{
			SelectedBattlefield = selectedBattlefield;
			BattlefieldsOptions = battlefieldsOptions;
		}

		public ViewModel Modify(string selectedBattlefield)
		{
			return new ViewModel(selectedBattlefield ?? SelectedBattlefield, BattlefieldsOptions);
		}
	}

	private readonly System.Random _random = new System.Random();

	[SerializeField]
	private TMP_Dropdown _battlefieldDropdown;

	[SerializeField]
	private Button _randomizeButton;

	private ViewModel _viewModel;

	public event Action<ViewModel, ViewModel> ViewModelChanged;

	private void Awake()
	{
		_battlefieldDropdown.onValueChanged.AddListener(OnBattlefieldSelected);
		_randomizeButton.onClick.AddListener(RandomizeBattlefield);
	}

	private void OnDestroy()
	{
		_battlefieldDropdown.onValueChanged.RemoveListener(OnBattlefieldSelected);
		_randomizeButton.onClick.RemoveListener(RandomizeBattlefield);
	}

	public void SetModel(ViewModel viewModel)
	{
		if (_viewModel.Equals(viewModel))
		{
			return;
		}
		_viewModel = viewModel;
		_battlefieldDropdown.ClearOptions();
		foreach (BattlefieldData battlefieldsOption in _viewModel.BattlefieldsOptions)
		{
			_battlefieldDropdown.options.Add(new TMP_Dropdown.OptionData(battlefieldsOption.Name));
		}
		_battlefieldDropdown.RefreshShownValue();
		_battlefieldDropdown.SetValueWithoutNotify(_viewModel.BattlefieldsOptions.FindIndex(viewModel.SelectedBattlefield, (BattlefieldData data, string s) => data.Name == s));
	}

	private void OnBattlefieldSelected(int idx)
	{
		ViewModel viewModel = _viewModel;
		_viewModel = _viewModel.Modify(_viewModel.BattlefieldsOptions[idx].Name);
		this.ViewModelChanged?.Invoke(viewModel, _viewModel);
	}

	private void RandomizeBattlefield()
	{
		List<string> list = new List<string>();
		foreach (BattlefieldData battlefieldsOption in _viewModel.BattlefieldsOptions)
		{
			if (battlefieldsOption.InRandomPool)
			{
				list.Add(battlefieldsOption.Name);
			}
		}
		int index = _random.Next(0, list.Count);
		string text = list[index];
		int valueWithoutNotify = _viewModel.BattlefieldsOptions.FindIndex(text, (BattlefieldData data, string s) => data.Name == s);
		ViewModel viewModel = _viewModel;
		_viewModel = _viewModel.Modify(text);
		_battlefieldDropdown.SetValueWithoutNotify(valueWithoutNotify);
		this.ViewModelChanged?.Invoke(viewModel, _viewModel);
	}
}
