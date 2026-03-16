using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class CardStyleListEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly IReadOnlyList<CardStyleEditor.ViewModel> Styles;

		public readonly IReadOnlyList<CardStyle> Options;

		public readonly IReadOnlyList<string> AvailableCodes;

		public ViewModel(IReadOnlyList<CardStyleEditor.ViewModel> styles, IReadOnlyList<CardStyle> options)
		{
			Styles = styles;
			Options = options;
			List<string> list = new List<string>();
			foreach (CardStyle option in options)
			{
				if (!list.Contains(option.Style))
				{
					list.Add(option.Style);
				}
			}
			AvailableCodes = list;
		}

		public ViewModel Modify(IReadOnlyList<CardStyleEditor.ViewModel> styles)
		{
			return new ViewModel(styles, Options);
		}

		public ViewModel AddCardStyle(CardStyle cardStyle)
		{
			List<CardStyleEditor.ViewModel> list = new List<CardStyleEditor.ViewModel>(Styles);
			list.Add(new CardStyleEditor.ViewModel(cardStyle, Options));
			return Modify(list);
		}

		public ViewModel UpdateCardStyle(int idx, CardStyle cardStyle)
		{
			if (idx < 0 || idx >= Styles.Count)
			{
				return this;
			}
			List<CardStyleEditor.ViewModel> list = new List<CardStyleEditor.ViewModel>(Styles);
			list[idx] = new CardStyleEditor.ViewModel(cardStyle, Options);
			return Modify(list);
		}

		public ViewModel RemoveCardStyle(int idx)
		{
			if (idx < 0 || idx >= Styles.Count)
			{
				return this;
			}
			List<CardStyleEditor.ViewModel> list = new List<CardStyleEditor.ViewModel>(Styles);
			list.RemoveAt(idx);
			return Modify(list);
		}
	}

	[SerializeField]
	private TMP_Dropdown _addCardStyleDropdown;

	[SerializeField]
	private CardStyleEditor _prefab;

	[SerializeField]
	private Transform _prefabRoot;

	private readonly List<CardStyleEditor> _editors = new List<CardStyleEditor>();

	private ViewModel _viewModel;

	public event Action<ViewModel, ViewModel> ViewModelChanged;

	private void Awake()
	{
		_addCardStyleDropdown.onValueChanged.AddListener(OnAddCardStyle);
	}

	private void OnDestroy()
	{
		_addCardStyleDropdown.onValueChanged.RemoveListener(OnAddCardStyle);
	}

	public void SetModelAndNotify(ViewModel newViewModel)
	{
		ViewModel viewModel = _viewModel;
		SetModel(newViewModel);
		this.ViewModelChanged?.Invoke(viewModel, newViewModel);
	}

	public void SetModel(ViewModel viewModel)
	{
		if (_viewModel.Equals(viewModel))
		{
			return;
		}
		_addCardStyleDropdown.ClearOptions();
		_addCardStyleDropdown.interactable = viewModel.Options.Count > 0;
		if (_addCardStyleDropdown.interactable)
		{
			_addCardStyleDropdown.options.Add(new TMP_Dropdown.OptionData("Select Card Style To Add"));
			foreach (string availableCode in viewModel.AvailableCodes)
			{
				_addCardStyleDropdown.options.Add(new TMP_Dropdown.OptionData("APPLY TO ALL: " + availableCode));
			}
			foreach (CardStyle option in viewModel.Options)
			{
				_addCardStyleDropdown.options.Add(new TMP_Dropdown.OptionData(option.Description));
			}
			_addCardStyleDropdown.SetValueWithoutNotify(0);
			_addCardStyleDropdown.RefreshShownValue();
		}
		for (int i = 0; i < viewModel.Styles.Count; i++)
		{
			GetOrCreateEditor(i).SetModel(viewModel.Styles[i]);
		}
		while (_editors.Count > viewModel.Styles.Count)
		{
			int index = _editors.Count - 1;
			CardStyleEditor cardStyleEditor = _editors[index];
			cardStyleEditor.ViewModelChanged -= UpdateCardStyle;
			cardStyleEditor.Deleted -= RemoveCardStyle;
			_editors.RemoveAt(index);
			UnityEngine.Object.Destroy(cardStyleEditor.gameObject);
		}
		_viewModel = viewModel;
	}

	private CardStyleEditor GetOrCreateEditor(int index)
	{
		if (_editors.Count > index)
		{
			return _editors[index];
		}
		CardStyleEditor cardStyleEditor = UnityEngine.Object.Instantiate(_prefab, _prefabRoot);
		cardStyleEditor.ViewModelChanged += UpdateCardStyle;
		cardStyleEditor.Deleted += RemoveCardStyle;
		_editors.Add(cardStyleEditor);
		return cardStyleEditor;
	}

	private void OnAddCardStyle(int idx)
	{
		int num = idx - 1;
		int count = _viewModel.AvailableCodes.Count;
		ViewModel modelAndNotify = ((num < count) ? AddStyleCodeToAllApplicable(_viewModel.AvailableCodes[num]) : _viewModel.AddCardStyle(_viewModel.Options[num - count]));
		SetModelAndNotify(modelAndNotify);
	}

	private ViewModel AddStyleCodeToAllApplicable(string styleCode)
	{
		ViewModel result = _viewModel;
		foreach (CardStyle option in _viewModel.Options)
		{
			if (!(option.Style != styleCode))
			{
				result = result.AddCardStyle(option);
			}
		}
		return result;
	}

	private void UpdateCardStyle(CardStyleEditor editor, CardStyle oldCardStyle, CardStyle updatedCardStyle)
	{
		int num = _editors.IndexOf(editor);
		if (num != -1)
		{
			ViewModel viewModel = _viewModel;
			ViewModel model = _viewModel.UpdateCardStyle(num, updatedCardStyle);
			SetModel(model);
			this.ViewModelChanged?.Invoke(viewModel, _viewModel);
		}
	}

	private void RemoveCardStyle(CardStyleEditor toRemove)
	{
		int num = _editors.IndexOf(toRemove);
		if (num != -1)
		{
			ViewModel viewModel = _viewModel;
			ViewModel viewModel2 = _viewModel.RemoveCardStyle(num);
			SetModel(viewModel2);
			this.ViewModelChanged?.Invoke(viewModel, viewModel2);
		}
	}
}
