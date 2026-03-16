using System;
using System.Collections.Generic;
using System.IO;
using GreClient.Network;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class DeckConfigEditor : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly string SelectedDirectory;

		public readonly DeckConfig SelectedDeck;

		public readonly IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> DeckOptions;

		public readonly IReadOnlyList<string> Directories;

		public readonly IReadOnlyList<DeckConfig> Decks;

		public ViewModel(string selectedDirectory, DeckConfig selectedDeck, IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> deckOptions)
		{
			SelectedDirectory = selectedDirectory;
			SelectedDeck = selectedDeck;
			DeckOptions = deckOptions ?? DictionaryExtensions.Empty<string, IReadOnlyList<DeckConfig>>();
			Directories = new List<string>(DeckOptions.Keys);
			IReadOnlyList<DeckConfig> decks;
			if (!DeckOptions.TryGetValue(selectedDirectory, out var value))
			{
				IReadOnlyList<DeckConfig> readOnlyList = Array.Empty<DeckConfig>();
				decks = readOnlyList;
			}
			else
			{
				decks = value;
			}
			Decks = decks;
			if (!Decks.Contains(SelectedDeck))
			{
				SelectedDeck = ((Decks.Count > 0) ? Decks[0] : DeckConfig.Default());
			}
		}

		public bool TryModifyDeckOptions(in DeckConfig modifiedDeck, out Dictionary<string, IReadOnlyList<DeckConfig>> modifiedDeckOptions)
		{
			modifiedDeckOptions = new Dictionary<string, IReadOnlyList<DeckConfig>>(DeckOptions);
			if (!DeckOptions.TryGetValue(SelectedDirectory, out var value))
			{
				return false;
			}
			int num = value.FindIndex(modifiedDeck.Name, (DeckConfig possibleMatch, string name) => possibleMatch.Name == name);
			if (num == -1)
			{
				return false;
			}
			List<DeckConfig> list = new List<DeckConfig>(value);
			list[num] = modifiedDeck;
			modifiedDeckOptions[SelectedDirectory] = list;
			return true;
		}

		public ViewModel(DeckConfig selectedDeck)
		{
			SelectedDirectory = string.Empty;
			SelectedDeck = selectedDeck;
			DeckOptions = DictionaryExtensions.Empty<string, IReadOnlyList<DeckConfig>>();
			Directories = new List<string>();
			Decks = new List<DeckConfig> { selectedDeck };
		}

		public ViewModel Modify(string selectedDirectory = null, DeckConfig? selectedDeck = null, IReadOnlyDictionary<string, IReadOnlyList<DeckConfig>> deckOptions = null)
		{
			return new ViewModel(selectedDirectory ?? SelectedDirectory, selectedDeck ?? SelectedDeck, deckOptions ?? DeckOptions);
		}
	}

	[SerializeField]
	private TMP_Dropdown _directoryDropdown;

	[SerializeField]
	private TMP_Dropdown _deckDropdown;

	private ViewModel _viewModel;

	public ViewModel CurrentView => _viewModel;

	public string RelativeDeckPath => Path.Combine(_viewModel.SelectedDirectory, $"{_viewModel.SelectedDeck}.txt");

	public event Action<ViewModel, ViewModel> ViewModelChanged;

	private void Awake()
	{
		_directoryDropdown.onValueChanged.AddListener(OnDirectoryChanged);
		_deckDropdown.onValueChanged.AddListener(OnDeckChanged);
	}

	private void OnDestroy()
	{
		_directoryDropdown.onValueChanged.RemoveListener(OnDirectoryChanged);
		_deckDropdown.onValueChanged.RemoveListener(OnDeckChanged);
	}

	public void SetModelAndNotify(ViewModel viewModel)
	{
		ViewModel viewModel2 = _viewModel;
		SetModel(viewModel);
		if (!viewModel2.Equals(_viewModel))
		{
			this.ViewModelChanged?.Invoke(viewModel2, _viewModel);
		}
	}

	public void SetModel(ViewModel viewModel)
	{
		if (!_viewModel.Equals(viewModel))
		{
			_viewModel = viewModel;
			RefreshDropdowns();
		}
	}

	private void RefreshDropdowns()
	{
		_directoryDropdown.gameObject.UpdateActive(_viewModel.Directories.Count > 1);
		_directoryDropdown.ClearOptions();
		foreach (string directory in _viewModel.Directories)
		{
			_directoryDropdown.options.Add(new TMP_Dropdown.OptionData(directory));
		}
		_directoryDropdown.RefreshShownValue();
		_directoryDropdown.SetValueWithoutNotify(_viewModel.Directories.IndexOf(_viewModel.SelectedDirectory));
		_deckDropdown.ClearOptions();
		foreach (DeckConfig deck in _viewModel.Decks)
		{
			_deckDropdown.options.Add(new TMP_Dropdown.OptionData(deck.Name));
		}
		_deckDropdown.SetValueWithoutNotify(_viewModel.Decks.IndexOf(_viewModel.SelectedDeck));
		_deckDropdown.RefreshShownValue();
		_deckDropdown.interactable = _viewModel.Decks.Count >= 1;
	}

	private void OnDirectoryChanged(int index)
	{
		string text = _viewModel.Directories[index];
		if (!(text == _viewModel.SelectedDirectory))
		{
			IReadOnlyList<DeckConfig> value2;
			DeckConfig value = ((_viewModel.DeckOptions.TryGetValue(text, out value2) && value2.Count > 2) ? value2[2] : _viewModel.SelectedDeck);
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(text, value);
			RefreshDropdowns();
			this.ViewModelChanged?.Invoke(viewModel, _viewModel);
		}
	}

	private void OnDeckChanged(int deckIdx)
	{
		DeckConfig value = _viewModel.Decks[deckIdx];
		if (!value.Equals(_viewModel.SelectedDeck))
		{
			ViewModel viewModel = _viewModel;
			_viewModel = _viewModel.Modify(null, value);
			this.ViewModelChanged?.Invoke(viewModel, _viewModel);
		}
	}
}
