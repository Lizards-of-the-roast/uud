using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public sealed class InputFieldDropdown : MonoBehaviour
{
	public readonly struct ViewModel
	{
		public readonly string Selection;

		public readonly IReadOnlyList<string> Options;

		public ViewModel(string selection, IReadOnlyList<string> options)
		{
			Selection = selection;
			Options = options ?? Array.Empty<string>();
		}
	}

	private sealed class TrigramIndex
	{
		private readonly Dictionary<string, HashSet<int>> _index = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

		private readonly List<string> _words = new List<string>();

		private const int N = 3;

		private string Normalize(string s)
		{
			return s.ToLowerInvariant();
		}

		public void BuildFrom(IEnumerable<string> words)
		{
			foreach (string word in words)
			{
				Add(word);
			}
		}

		private void Add(string word)
		{
			if (string.IsNullOrEmpty(word))
			{
				return;
			}
			int count = _words.Count;
			_words.Add(word);
			string text = Normalize(word);
			for (int i = 0; i <= text.Length - 3; i++)
			{
				string key = text.Substring(i, 3);
				if (!_index.TryGetValue(key, out var value))
				{
					value = (_index[key] = new HashSet<int>());
				}
				value.Add(count);
			}
		}

		public IEnumerable<string> Suggest(string query, int limit = int.MaxValue)
		{
			if (string.IsNullOrEmpty(query))
			{
				return Enumerable.Empty<string>();
			}
			string key = Normalize(query);
			HashSet<int> hashSet = new HashSet<int>();
			if (key.Length < 3)
			{
				return _words.Where((string w) => Normalize(w).Contains(key)).Take(limit);
			}
			List<HashSet<int>> list = new List<HashSet<int>>();
			for (int num = 0; num <= key.Length - 3; num++)
			{
				string key2 = key.Substring(num, 3);
				if (_index.TryGetValue(key2, out var value))
				{
					list.Add(value);
				}
			}
			if (list.Count == 0)
			{
				return Enumerable.Empty<string>();
			}
			list.Sort((HashSet<int> a, HashSet<int> b) => a.Count.CompareTo(b.Count));
			hashSet = new HashSet<int>(list[0]);
			for (int num2 = 1; num2 < list.Count; num2++)
			{
				hashSet.IntersectWith(list[num2]);
			}
			List<string> list2 = new List<string>();
			foreach (int item in hashSet)
			{
				string text = _words[item];
				if (Normalize(text).Contains(key))
				{
					list2.Add(text);
				}
				if (list2.Count >= limit)
				{
					break;
				}
			}
			return list2;
		}

		public void Clear()
		{
			_index.Clear();
			_words.Clear();
		}
	}

	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private TMP_Dropdown _dropdown;

	[SerializeField]
	private Color _invalidEntryTextColor;

	[SerializeField]
	private Color _partialMatchTextColor;

	private Color _defaultTextColor;

	private ViewModel _viewModel;

	public Action<string> ValueChanged;

	private readonly TrigramIndex _trigramIndex = new TrigramIndex();

	private readonly List<string> _filteredOptions = new List<string>();

	private void Awake()
	{
		_defaultTextColor = _inputField.textComponent.color;
		_inputField.onValueChanged.AddListener(OnInputFieldChanged);
		_dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
	}

	private void OnDestroy()
	{
		_inputField.onValueChanged.RemoveListener(OnInputFieldChanged);
		_dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
	}

	public void SetModel(ViewModel viewModel)
	{
		bool num = RebuildDropdown(_viewModel.Options, viewModel.Options);
		_viewModel = viewModel;
		if (num)
		{
			_trigramIndex.Clear();
			_trigramIndex.BuildFrom(viewModel.Options);
			PopulateDropdown(viewModel.Selection, viewModel.Options);
		}
		_inputField.SetTextWithoutNotify(viewModel.Selection);
		_inputField.textComponent.color = GetColorForMatch(viewModel.Selection, viewModel.Options, HasPartialMatch());
	}

	private bool HasPartialMatch()
	{
		return _filteredOptions.Count switch
		{
			0 => false, 
			1 => !string.IsNullOrEmpty(_filteredOptions[0]), 
			_ => true, 
		};
	}

	private Color GetColorForMatch(string selection, IReadOnlyList<string> options, bool partialMatch)
	{
		if (!options.Contains(selection))
		{
			if (!partialMatch)
			{
				return _invalidEntryTextColor;
			}
			return _partialMatchTextColor;
		}
		return _defaultTextColor;
	}

	private bool RebuildDropdown(IReadOnlyList<string> x, IReadOnlyList<string> y)
	{
		if (x == null)
		{
			x = Array.Empty<string>();
		}
		if (y == null)
		{
			y = Array.Empty<string>();
		}
		if (x.Count != y.Count)
		{
			return true;
		}
		for (int i = 0; i < x.Count; i++)
		{
			if (x[i] != y[i])
			{
				return true;
			}
		}
		return false;
	}

	private void PopulateDropdown(string selection, IReadOnlyList<string> allOptions)
	{
		_dropdown.ClearOptions();
		_filteredOptions.Clear();
		_filteredOptions.Add(string.Empty);
		List<string> filteredOptions = _filteredOptions;
		object collection;
		if (!string.IsNullOrEmpty(selection))
		{
			collection = _trigramIndex.Suggest(selection);
		}
		else
		{
			collection = allOptions;
		}
		filteredOptions.AddRange((IEnumerable<string>)collection);
		int valueWithoutNotify = 0;
		for (int i = 0; i < _filteredOptions.Count; i++)
		{
			string text = _filteredOptions[i];
			if (text == selection)
			{
				valueWithoutNotify = i;
			}
			_dropdown.options.Add(new TMP_Dropdown.OptionData(text));
		}
		_dropdown.RefreshShownValue();
		_dropdown.SetValueWithoutNotify(valueWithoutNotify);
	}

	private void OnInputFieldChanged(string newValue)
	{
		ModifyValue(newValue);
	}

	private void OnDropdownValueChanged(int idx)
	{
		ModifyValue(_filteredOptions[idx]);
	}

	private void ModifyValue(string value)
	{
		if (!(value == _viewModel.Selection))
		{
			ValueChanged?.Invoke(value);
			PopulateDropdown(value, _viewModel.Options);
			_inputField.textComponent.color = GetColorForMatch(value, _viewModel.Options, HasPartialMatch());
		}
	}
}
