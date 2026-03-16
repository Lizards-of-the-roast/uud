using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class EmblemConfigEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly EmblemData Emblem;

		public readonly IReadOnlyList<EmblemData> EmblemOptions;

		public ViewModel(EmblemData emblem, IReadOnlyList<EmblemData> emblemOptions)
		{
			Emblem = emblem;
			EmblemOptions = emblemOptions;
		}

		public ViewModel Modify(EmblemData emblem)
		{
			return new ViewModel(emblem, EmblemOptions);
		}
	}

	[SerializeField]
	private Button _deleteButton;

	[SerializeField]
	private TMP_Dropdown _emblemDropdown;

	[SerializeField]
	private TMP_Text _emblemDescription;

	private ViewModel _viewModel;

	public event Action<EmblemConfigEditor, ViewModel, ViewModel> ViewModelChanged;

	public event Action<EmblemConfigEditor> Deleted;

	private void Awake()
	{
		_emblemDropdown.onValueChanged.AddListener(OnEmblemDropdownChanged);
		_deleteButton.onClick.AddListener(OnDeleteClicked);
	}

	private void OnDestroy()
	{
		_emblemDropdown.onValueChanged.AddListener(OnEmblemDropdownChanged);
		_deleteButton.onClick.AddListener(OnDeleteClicked);
	}

	public void SetModel(ViewModel viewModel)
	{
		if (_viewModel.Equals(viewModel))
		{
			return;
		}
		_viewModel = viewModel;
		_emblemDropdown.ClearOptions();
		_emblemDropdown.ClearOptions();
		foreach (EmblemData emblemOption in _viewModel.EmblemOptions)
		{
			_emblemDropdown.options.Add(new TMP_Dropdown.OptionData(emblemOption.Title));
		}
		_emblemDropdown.RefreshShownValue();
		_emblemDropdown.SetValueWithoutNotify(FindSelectedIndex(viewModel.Emblem.Id, viewModel.EmblemOptions));
		_emblemDescription.text = viewModel.Emblem.Description;
	}

	private void OnDeleteClicked()
	{
		this.Deleted?.Invoke(this);
	}

	private void OnEmblemDropdownChanged(int idx)
	{
		ViewModel viewModel = _viewModel;
		_viewModel = _viewModel.Modify(_viewModel.EmblemOptions[idx]);
		this.ViewModelChanged(this, viewModel, _viewModel);
		_emblemDescription.text = _viewModel.Emblem.Description;
	}

	private static int FindSelectedIndex(uint grpId, IReadOnlyList<EmblemData> options)
	{
		for (int i = 0; i < options.Count; i++)
		{
			if (options[i].Id == grpId)
			{
				return i;
			}
		}
		return 0;
	}
}
