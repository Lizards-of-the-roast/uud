using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class EmblemConfigListEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly IReadOnlyList<EmblemConfigEditor.ViewModel> Emblems;

		public readonly IReadOnlyList<EmblemData> Options;

		public ViewModel(IReadOnlyList<EmblemConfigEditor.ViewModel> emblems, IReadOnlyList<EmblemData> options)
		{
			Emblems = emblems;
			Options = options;
		}

		public ViewModel AddEmblem(EmblemData emblemData)
		{
			return new ViewModel(new List<EmblemConfigEditor.ViewModel>(Emblems)
			{
				new EmblemConfigEditor.ViewModel(emblemData, Options)
			}, Options);
		}

		public ViewModel UpdateEmblem(int idx, EmblemConfigEditor.ViewModel emblem)
		{
			if (idx < 0 || idx >= Emblems.Count)
			{
				return this;
			}
			return new ViewModel(new List<EmblemConfigEditor.ViewModel>(Emblems) { [idx] = emblem }, Options);
		}

		public ViewModel RemoveEmblem(int idx)
		{
			if (idx < 0 || idx >= Emblems.Count)
			{
				return this;
			}
			List<EmblemConfigEditor.ViewModel> list = new List<EmblemConfigEditor.ViewModel>(Emblems);
			list.RemoveAt(idx);
			return new ViewModel(list, Options);
		}
	}

	[SerializeField]
	private TMP_Dropdown _addEmblemDropdown;

	[SerializeField]
	private EmblemConfigEditor _prefab;

	[SerializeField]
	private Transform _prefabRoot;

	private readonly List<EmblemConfigEditor> _emblemEditors = new List<EmblemConfigEditor>();

	private ViewModel _viewModel;

	public event Action<ViewModel, ViewModel> ViewModelChanged;

	private void Awake()
	{
		_addEmblemDropdown.onValueChanged.AddListener(OnAddEmblem);
	}

	private void OnDestroy()
	{
		_addEmblemDropdown.onValueChanged.RemoveListener(OnAddEmblem);
	}

	public void SetModel(ViewModel viewModel)
	{
		if (_viewModel.Equals(viewModel))
		{
			return;
		}
		if (_viewModel.Options != viewModel.Options)
		{
			_addEmblemDropdown.ClearOptions();
			_addEmblemDropdown.options.Add(new TMP_Dropdown.OptionData("Select Emblem To Add"));
			foreach (EmblemData option in viewModel.Options)
			{
				_addEmblemDropdown.options.Add(new TMP_Dropdown.OptionData(option.Title));
			}
		}
		_addEmblemDropdown.SetValueWithoutNotify(0);
		_addEmblemDropdown.RefreshShownValue();
		for (int i = 0; i < viewModel.Emblems.Count; i++)
		{
			GetOrCreateEmblemEditor(i).SetModel(viewModel.Emblems[i]);
		}
		while (_emblemEditors.Count > viewModel.Emblems.Count)
		{
			int index = _emblemEditors.Count - 1;
			EmblemConfigEditor emblemConfigEditor = _emblemEditors[index];
			emblemConfigEditor.ViewModelChanged -= UpdateEmblem;
			emblemConfigEditor.Deleted -= RemoveEmblem;
			_emblemEditors.RemoveAt(index);
			UnityEngine.Object.Destroy(emblemConfigEditor.gameObject);
		}
		_viewModel = viewModel;
	}

	private EmblemConfigEditor GetOrCreateEmblemEditor(int index)
	{
		if (_emblemEditors.Count > index)
		{
			return _emblemEditors[index];
		}
		EmblemConfigEditor emblemConfigEditor = UnityEngine.Object.Instantiate(_prefab, _prefabRoot);
		emblemConfigEditor.ViewModelChanged += UpdateEmblem;
		emblemConfigEditor.Deleted += RemoveEmblem;
		_emblemEditors.Add(emblemConfigEditor);
		return emblemConfigEditor;
	}

	private void OnAddEmblem(int idx)
	{
		if (idx != 0 && _viewModel.Options.Count > idx - 1)
		{
			EmblemData emblemData = _viewModel.Options[idx - 1];
			ViewModel viewModel = _viewModel;
			ViewModel viewModel2 = _viewModel.AddEmblem(emblemData);
			SetModel(viewModel2);
			this.ViewModelChanged?.Invoke(viewModel, viewModel2);
		}
	}

	private void UpdateEmblem(EmblemConfigEditor editor, EmblemConfigEditor.ViewModel oldEmblem, EmblemConfigEditor.ViewModel updatedEmblem)
	{
		int num = _emblemEditors.IndexOf(editor);
		if (num != -1)
		{
			ViewModel viewModel = _viewModel;
			ViewModel model = _viewModel.UpdateEmblem(num, updatedEmblem);
			SetModel(model);
			this.ViewModelChanged?.Invoke(viewModel, _viewModel);
		}
	}

	private void RemoveEmblem(EmblemConfigEditor toRemove)
	{
		int num = _emblemEditors.IndexOf(toRemove);
		if (num != -1)
		{
			ViewModel viewModel = _viewModel;
			ViewModel viewModel2 = _viewModel.RemoveEmblem(num);
			SetModel(viewModel2);
			this.ViewModelChanged?.Invoke(viewModel, viewModel2);
		}
	}
}
