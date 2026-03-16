using System;
using System.Collections.Generic;
using GreClient.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class RngSeedEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly bool UseSpecifiedSeed;

		public readonly IReadOnlyList<uint> RngSeed;

		public ViewModel(bool useSpecifiedSeed, IReadOnlyList<uint> rngSeed)
		{
			UseSpecifiedSeed = useSpecifiedSeed;
			RngSeed = rngSeed;
		}

		public ViewModel Modify(bool? useSpecifiedSeed = null, IReadOnlyList<uint> rngSeed = null)
		{
			return new ViewModel(useSpecifiedSeed ?? UseSpecifiedSeed, rngSeed ?? RngSeed);
		}
	}

	[SerializeField]
	private Toggle _useSeedToggle;

	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private Button _randomizeButton;

	private ViewModel _viewModel;

	public Action<ViewModel, ViewModel> ViewModelChanged;

	private void Awake()
	{
		_useSeedToggle.onValueChanged.AddListener(OnUseSeedChanged);
		_inputField.onValueChanged.AddListener(UpdateRngSeed);
		_randomizeButton.onClick.AddListener(CreateRandomRngSeed);
	}

	private void OnDestroy()
	{
		_useSeedToggle.onValueChanged.RemoveListener(OnUseSeedChanged);
		_inputField.onValueChanged.RemoveListener(UpdateRngSeed);
		_randomizeButton.onClick.RemoveListener(CreateRandomRngSeed);
	}

	public void SetModel(ViewModel viewModel)
	{
		_viewModel = viewModel;
		_useSeedToggle.SetIsOnWithoutNotify(viewModel.UseSpecifiedSeed);
		_inputField.SetTextWithoutNotify(ToParsedRNGSeedText(viewModel.RngSeed));
		_inputField.interactable = viewModel.UseSpecifiedSeed;
		_randomizeButton.interactable = viewModel.UseSpecifiedSeed;
	}

	private void OnUseSeedChanged(bool value)
	{
		if (_viewModel.UseSpecifiedSeed != value)
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(value);
			ViewModelChanged?.Invoke(viewModel, _viewModel);
		}
	}

	private void UpdateRngSeed(string value)
	{
		IReadOnlyList<uint> readOnlyList = ToParsedRNGSeedList(value, 8);
		if (!string.IsNullOrEmpty(value) && readOnlyList.Count != 8)
		{
			_inputField.SetTextWithoutNotify(ToParsedRNGSeedText(_viewModel.RngSeed));
			return;
		}
		_inputField.SetTextWithoutNotify(ToParsedRNGSeedText(readOnlyList));
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		IReadOnlyList<uint> rngSeed = readOnlyList;
		_viewModel = viewModel2.Modify(null, rngSeed);
		ViewModelChanged?.Invoke(viewModel, _viewModel);
	}

	private void CreateRandomRngSeed()
	{
		UpdateRngSeed(ToParsedRNGSeedText(MatchConfig.CreateNewRNGSeed()));
	}

	private static string ToParsedRNGSeedText(IReadOnlyList<uint> rngSeed)
	{
		if (rngSeed.Count != 0)
		{
			return string.Join("|", rngSeed);
		}
		return string.Empty;
	}

	private static IReadOnlyList<uint> ToParsedRNGSeedList(string value, int maxLength)
	{
		if (string.IsNullOrEmpty(value))
		{
			return Array.Empty<uint>();
		}
		List<string> list = new List<string>(value.Split("|"));
		while (list.Count != maxLength)
		{
			if (list.Count > maxLength)
			{
				list.RemoveAt(list.Count - 1);
			}
			else
			{
				list.Insert(list.Count, string.Empty);
			}
		}
		List<uint> list2 = new List<uint>();
		for (int i = 0; i < list.Count; i++)
		{
			if (uint.TryParse(list[i], out var result))
			{
				list2.Add(result);
			}
			else
			{
				list2.Add(0u);
			}
		}
		return list2;
	}
}
