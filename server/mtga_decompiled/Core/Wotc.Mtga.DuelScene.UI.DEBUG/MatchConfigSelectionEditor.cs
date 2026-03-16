using System;
using System.Collections.Generic;
using GreClient.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class MatchConfigSelectionEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly string DirectoryPath;

		public readonly string SelectedDirectory;

		public readonly MatchConfig SelectedConfig;

		public readonly IReadOnlyList<string> Directories;

		public readonly IReadOnlyList<MatchConfig> Options;

		public readonly bool RefreshDirectory;

		public readonly bool CreateNewConfig;

		public ViewModel(string directoryPath, string selectedDirectory, MatchConfig selected, IReadOnlyList<string> directories, IReadOnlyList<MatchConfig> options, bool refreshDirectory = false, bool createNewConfig = false)
		{
			DirectoryPath = directoryPath;
			SelectedDirectory = selectedDirectory;
			SelectedConfig = selected;
			Directories = directories;
			Options = options;
			RefreshDirectory = refreshDirectory;
			CreateNewConfig = createNewConfig;
		}

		public int SelectedConfigIdx()
		{
			if (Options.Count == 0)
			{
				return -1;
			}
			for (int i = 0; i < Options.Count; i++)
			{
				if (Options[i].Name == SelectedConfig.Name)
				{
					return i;
				}
			}
			return 0;
		}

		public ViewModel Modify(string selectedDirectory = null, MatchConfig? selectedConfig = null, bool? refreshDirectory = false, bool? createNewConfig = false)
		{
			return new ViewModel(DirectoryPath, selectedDirectory ?? SelectedDirectory, selectedConfig ?? SelectedConfig, Directories, Options, refreshDirectory ?? RefreshDirectory, createNewConfig ?? CreateNewConfig);
		}
	}

	[SerializeField]
	private Button _copyDirectoryPathButton;

	[SerializeField]
	private Button _refreshDirectoryButton;

	[SerializeField]
	private TMP_Dropdown _directorySelectionDropdown;

	[SerializeField]
	private TMP_Dropdown _configSelectionDropdown;

	private ViewModel _viewModel;

	public event Action<ViewModel, ViewModel> ViewModelChanged;

	private void Awake()
	{
		_copyDirectoryPathButton.onClick.AddListener(CopyDirectoryPath);
		_refreshDirectoryButton.onClick.AddListener(RefreshDirectory);
		_directorySelectionDropdown.onValueChanged.AddListener(OnConfigDirectoryChanged);
		_configSelectionDropdown.onValueChanged.AddListener(OnConfigSelectionChanged);
	}

	private void OnDestroy()
	{
		_copyDirectoryPathButton.onClick.RemoveListener(CopyDirectoryPath);
		_refreshDirectoryButton.onClick.RemoveListener(RefreshDirectory);
		_directorySelectionDropdown.onValueChanged.RemoveListener(OnConfigDirectoryChanged);
		_configSelectionDropdown.onValueChanged.RemoveListener(OnConfigSelectionChanged);
	}

	public void SetModel(ViewModel viewModel)
	{
		_viewModel = viewModel;
		_directorySelectionDropdown.gameObject.UpdateActive(viewModel.Directories.Count > 1);
		_directorySelectionDropdown.ClearOptions();
		foreach (string directory in viewModel.Directories)
		{
			_directorySelectionDropdown.options.Add(new TMP_Dropdown.OptionData(directory));
		}
		_directorySelectionDropdown.SetValueWithoutNotify(viewModel.Directories.IndexOf(viewModel.SelectedDirectory));
		_directorySelectionDropdown.RefreshShownValue();
		_configSelectionDropdown.ClearOptions();
		foreach (MatchConfig option in viewModel.Options)
		{
			_configSelectionDropdown.options.Add(new TMP_Dropdown.OptionData(option.Name));
		}
		_configSelectionDropdown.options.Add(new TMP_Dropdown.OptionData("<CREATE NEW>"));
		_configSelectionDropdown.SetValueWithoutNotify(viewModel.SelectedConfigIdx());
		_configSelectionDropdown.RefreshShownValue();
	}

	private void CopyDirectoryPath()
	{
		GUIUtility.systemCopyBuffer = _viewModel.DirectoryPath;
	}

	public void RefreshDirectory()
	{
		ViewModel viewModel = _viewModel;
		ref ViewModel viewModel2 = ref _viewModel;
		bool? refreshDirectory = true;
		_viewModel = viewModel2.Modify(null, null, refreshDirectory, false);
		this.ViewModelChanged?.Invoke(viewModel, _viewModel);
	}

	private void OnConfigDirectoryChanged(int idx)
	{
		string text = _viewModel.Directories[idx];
		if (!(_viewModel.SelectedDirectory == text))
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(text, null, false, false);
			this.ViewModelChanged?.Invoke(viewModel, _viewModel);
		}
	}

	private void OnConfigSelectionChanged(int idx)
	{
		if (idx == _viewModel.Options.Count)
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(null, createNewConfig: true, selectedConfig: null, refreshDirectory: false);
			this.ViewModelChanged?.Invoke(viewModel, _viewModel);
			return;
		}
		MatchConfig matchConfig = _viewModel.Options[idx];
		if (!_viewModel.SelectedConfig.Equals(matchConfig))
		{
			ViewModel viewModel2 = _viewModel;
			_viewModel = _viewModel.Modify(null, matchConfig, false, false);
			this.ViewModelChanged?.Invoke(viewModel2, _viewModel);
		}
	}
}
