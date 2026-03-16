using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class CardStyleEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly CardStyle Style;

		public readonly IReadOnlyList<CardStyle> Options;

		public ViewModel(CardStyle style, IReadOnlyList<CardStyle> options)
		{
			Style = style;
			Options = options ?? Array.Empty<CardStyle>();
		}

		public ViewModel Modify(CardStyle style)
		{
			return new ViewModel(style, Options);
		}
	}

	[SerializeField]
	private Button _deleteButton;

	[SerializeField]
	private TMP_Dropdown _cardStyleDropdown;

	private ViewModel _viewModel;

	public event Action<CardStyleEditor, CardStyle, CardStyle> ViewModelChanged;

	public event Action<CardStyleEditor> Deleted;

	private void Awake()
	{
		_cardStyleDropdown.onValueChanged.AddListener(OnCardStyleChanged);
		_deleteButton.onClick.AddListener(OnDeleteClicked);
	}

	private void OnDestroy()
	{
		_cardStyleDropdown.onValueChanged.AddListener(OnCardStyleChanged);
		_deleteButton.onClick.AddListener(OnDeleteClicked);
	}

	public void SetModel(ViewModel viewModel)
	{
		_viewModel = viewModel;
		_cardStyleDropdown.ClearOptions();
		foreach (CardStyle option in viewModel.Options)
		{
			_cardStyleDropdown.options.Add(new TMP_Dropdown.OptionData(option.Description));
		}
		_cardStyleDropdown.SetValueWithoutNotify(0);
		_cardStyleDropdown.RefreshShownValue();
		_cardStyleDropdown.interactable = viewModel.Options.Count > 1;
	}

	private void OnDeleteClicked()
	{
		this.Deleted?.Invoke(this);
	}

	private void OnCardStyleChanged(int idx)
	{
		CardStyle cardStyle = _viewModel.Options[idx];
		if (!_viewModel.Style.Equals(cardStyle))
		{
			CardStyle style = _viewModel.Style;
			_viewModel = _viewModel.Modify(cardStyle);
			this.ViewModelChanged?.Invoke(this, style, cardStyle);
		}
	}
}
