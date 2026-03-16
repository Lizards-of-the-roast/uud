using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class RankConfigEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly RankingClassType RankClass;

		public readonly int Tier;

		public readonly float MythicPercent;

		public readonly int MythicPlacement;

		public ViewModel(RankingClassType rankClass, int tier, float mythicPercent, int mythicPlacement)
		{
			RankClass = rankClass;
			Tier = tier;
			MythicPercent = mythicPercent;
			MythicPlacement = mythicPlacement;
		}

		public ViewModel Modify(RankingClassType? rankClass = null, int? tier = null, float? mythicPercent = null, int? mythicPlacement = null)
		{
			return new ViewModel(rankClass ?? RankClass, tier ?? Tier, mythicPercent ?? MythicPercent, mythicPlacement ?? MythicPlacement);
		}
	}

	private static List<RankingClassType> _ranks;

	[SerializeField]
	private TMP_Dropdown _classDropdown;

	[SerializeField]
	private TMP_InputField _tierInputField;

	[SerializeField]
	private TMP_InputField _mythicPercentInputField;

	[SerializeField]
	private TMP_InputField _mythicPlacementInputField;

	private ViewModel _viewModel;

	public event Action<ViewModel, ViewModel> ViewModelUpdated;

	private void Awake()
	{
		if (_ranks == null)
		{
			_ranks = new List<RankingClassType>();
			foreach (RankingClassType value in Enum.GetValues(typeof(RankingClassType)))
			{
				_ranks.Add(value);
			}
			_ranks.Sort(delegate(RankingClassType x, RankingClassType y)
			{
				int num = (int)x;
				return num.CompareTo((int)y);
			});
		}
		_classDropdown.ClearOptions();
		foreach (RankingClassType rank in _ranks)
		{
			_classDropdown.options.Add(new TMP_Dropdown.OptionData(EnumExtensions.EnumCleanName(rank)));
		}
		_classDropdown.onValueChanged.AddListener(OnRankClassChanged);
		_tierInputField.onValueChanged.AddListener(OnTierChanged);
		_mythicPercentInputField.onValueChanged.AddListener(OnMythicPercentChanged);
		_mythicPlacementInputField.onValueChanged.AddListener(OnMythicPlacementChanged);
	}

	private void OnDestroy()
	{
		this.ViewModelUpdated = null;
		_classDropdown.onValueChanged.RemoveListener(OnRankClassChanged);
		_tierInputField.onValueChanged.RemoveListener(OnTierChanged);
		_mythicPercentInputField.onValueChanged.RemoveListener(OnMythicPercentChanged);
		_mythicPlacementInputField.onValueChanged.RemoveListener(OnMythicPlacementChanged);
	}

	public void SetModel(ViewModel viewModel)
	{
		_viewModel = viewModel;
		_classDropdown.SetValueWithoutNotify(_ranks.IndexOf(viewModel.RankClass));
		_tierInputField.SetTextWithoutNotify(viewModel.Tier.ToString());
		_mythicPercentInputField.SetTextWithoutNotify(viewModel.MythicPercent.ToString());
		_mythicPlacementInputField.SetTextWithoutNotify(viewModel.MythicPlacement.ToString());
	}

	private void OnRankClassChanged(int idx)
	{
		ViewModel viewModel = _viewModel;
		_viewModel = _viewModel.Modify(_ranks[idx]);
		this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
	}

	private void OnTierChanged(string value)
	{
		if (int.TryParse(value, out var result))
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			int? tier = result;
			_viewModel = viewModel2.Modify(null, tier);
			this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
		}
	}

	private void OnMythicPercentChanged(string value)
	{
		if (float.TryParse(value, out var result))
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			float? mythicPercent = result;
			_viewModel = viewModel2.Modify(null, null, mythicPercent);
			this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
		}
	}

	private void OnMythicPlacementChanged(string value)
	{
		if (int.TryParse(value, out var result))
		{
			ViewModel viewModel = _viewModel;
			ref ViewModel viewModel2 = ref _viewModel;
			int? mythicPlacement = result;
			_viewModel = viewModel2.Modify(null, null, null, mythicPlacement);
			this.ViewModelUpdated?.Invoke(viewModel, _viewModel);
		}
	}
}
