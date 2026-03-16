using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GreClient.CardData;
using Mopsicus.InfiniteScroll;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;

namespace Wotc.Mtga.DuelScene.Interactions;

public class KeywordFilter : MonoBehaviour
{
	public readonly struct Keyword
	{
		public readonly string RawText;

		public readonly string DisplayText;

		public readonly string SearchText;

		public Keyword(string rawText, string displayText, string searchText)
		{
			RawText = rawText ?? string.Empty;
			DisplayText = displayText ?? string.Empty;
			SearchText = searchText ?? string.Empty;
		}

		public static IEnumerable<Keyword> ConvertToKeywords(IEnumerable<string> text)
		{
			foreach (string item in text)
			{
				if (!string.IsNullOrEmpty(item))
				{
					string text2 = stripLineBreakRegex.Replace(item, string.Empty);
					string searchText = stripItalicsRegex.Replace(text2, string.Empty);
					yield return new Keyword(item, text2, searchText);
				}
			}
		}

		public static IEnumerable<string> GetRawText(IEnumerable<Keyword> keywords)
		{
			foreach (Keyword keyword in keywords)
			{
				yield return keyword.RawText;
			}
		}
	}

	private static Regex stripLineBreakRegex = new Regex("(</?nobr>)", RegexOptions.Compiled);

	private static Regex stripItalicsRegex = new Regex("(</?i>)", RegexOptions.Compiled);

	[SerializeField]
	private TMP_InputField FilterInput;

	[SerializeField]
	private TMP_Text AutoCompleteLabel;

	[SerializeField]
	private Transform OptionsParent;

	[SerializeField]
	private Toggle OptionsTogglePrefab;

	[SerializeField]
	private Color AutoCompleteColor = Color.grey;

	[SerializeField]
	private Scrollbar Scrollbar;

	[SerializeField]
	private InfiniteScroll _infiniteScroll;

	private List<Keyword> _allKeywords;

	private List<Keyword> _hintedKeywords;

	private uint _maxSelectionsAllowed;

	private string _autoCompleteHexColor = string.Empty;

	private List<Keyword> _filteredKeywords;

	private List<Keyword> _selectedKeywords;

	private bool _showAllKeywords;

	private Dictionary<Keyword, Toggle> _keywordToToggle;

	private Dictionary<Toggle, Keyword> _toggleToKeyword;

	private int _itemHeight;

	private string _prvFilter = string.Empty;

	private static StringBuilder sb = new StringBuilder();

	public bool ShowAllKeywords
	{
		set
		{
			_showAllKeywords = value;
			_prvFilter = string.Empty;
			OnFilterChanged(FilterInput.text);
		}
	}

	public event Action<IEnumerable<string>> SelectedKeywordsUpdatedHandlers;

	public void Init(IEnumerable<string> allKeywords, IEnumerable<string> hintedKeywords, uint minSelectionsAllowed, uint maxSelectionsAllowed, string initialFilterText)
	{
		_allKeywords.AddRange(Keyword.ConvertToKeywords(allKeywords));
		_hintedKeywords.AddRange(Keyword.ConvertToKeywords(hintedKeywords));
		_allKeywords.Sort(CompareKeywords);
		_hintedKeywords.Sort(CompareKeywords);
		_showAllKeywords |= _hintedKeywords.Count == 0;
		IEnumerable<Keyword> collection = (_showAllKeywords ? _allKeywords : _hintedKeywords);
		_filteredKeywords.AddRange(collection);
		_maxSelectionsAllowed = Math.Max(minSelectionsAllowed, maxSelectionsAllowed);
		((TMP_Text)FilterInput.placeholder).SetText(initialFilterText);
		InfiniteScroll infiniteScroll = _infiniteScroll;
		infiniteScroll.OnFill = (Action<int, GameObject>)Delegate.Combine(infiniteScroll.OnFill, new Action<int, GameObject>(OnFillItem));
		_infiniteScroll.OnHeight += OnHeightItem;
		_infiniteScroll.InitData(_filteredKeywords.Count);
	}

	private void Awake()
	{
		_autoCompleteHexColor = $"#{(byte)(AutoCompleteColor.r * 255f):X2}{(byte)(AutoCompleteColor.g * 255f):X2}{(byte)(AutoCompleteColor.b * 255f):X2}";
		IObjectPool objectPool = Pantry.Get<IObjectPool>();
		_allKeywords = objectPool.PopObject<List<Keyword>>();
		_allKeywords.Clear();
		_hintedKeywords = objectPool.PopObject<List<Keyword>>();
		_hintedKeywords.Clear();
		_selectedKeywords = objectPool.PopObject<List<Keyword>>();
		_selectedKeywords.Clear();
		_keywordToToggle = objectPool.PopObject<Dictionary<Keyword, Toggle>>();
		_keywordToToggle.Clear();
		_toggleToKeyword = objectPool.PopObject<Dictionary<Toggle, Keyword>>();
		_toggleToKeyword.Clear();
		_filteredKeywords = objectPool.PopObject<List<Keyword>>();
		_filteredKeywords.Clear();
		AutoCompleteLabel.text = string.Empty;
		FilterInput.text = string.Empty;
		FilterInput.onValueChanged.AddListener(OnFilterChanged);
		FilterInput.onSubmit.AddListener(OnFilterSubmitted);
		FilterInput.Select();
		Scrollbar.value = 0f;
		_itemHeight = (int)OptionsTogglePrefab.gameObject.GetComponent<RectTransform>().rect.height;
	}

	private void OnDestroy()
	{
		if (_infiniteScroll != null)
		{
			_infiniteScroll.OnHeight -= OnHeightItem;
			InfiniteScroll infiniteScroll = _infiniteScroll;
			infiniteScroll.OnFill = (Action<int, GameObject>)Delegate.Remove(infiniteScroll.OnFill, new Action<int, GameObject>(OnFillItem));
			_infiniteScroll = null;
		}
		if (FilterInput != null)
		{
			FilterInput.onValueChanged.RemoveListener(OnFilterChanged);
			FilterInput.onSubmit.RemoveListener(OnFilterSubmitted);
			FilterInput = null;
		}
		IObjectPool objectPool = Pantry.Get<IObjectPool>();
		objectPool?.PushObject(_filteredKeywords);
		_filteredKeywords = null;
		objectPool?.PushObject(_toggleToKeyword);
		_toggleToKeyword = null;
		objectPool?.PushObject(_keywordToToggle);
		_keywordToToggle = null;
		objectPool?.PushObject(_selectedKeywords);
		_selectedKeywords = null;
		objectPool?.PushObject(_hintedKeywords);
		_hintedKeywords = null;
		objectPool?.PushObject(_allKeywords);
		_allKeywords = null;
	}

	private void OnFillItem(int index, GameObject go)
	{
		Toggle toggleView = go.GetComponent<Toggle>();
		TMP_Text toggleViewText = toggleView.gameObject.GetComponentInChildren<TMP_Text>();
		Keyword keyword = _filteredKeywords[index];
		_toggleToKeyword[toggleView] = keyword;
		_keywordToToggle[keyword] = toggleView;
		toggleView.transform.SetParent(OptionsParent.transform);
		toggleView.gameObject.GetComponent<RectTransform>().localRotation = Quaternion.identity;
		toggleView.onValueChanged.RemoveAllListeners();
		toggleView.isOn = _selectedKeywords.Contains(keyword);
		toggleViewText.SetText(CardUtilities.FormatComplexTitle(keyword.DisplayText));
		toggleViewText.color = (toggleView.isOn ? Color.black : Color.white);
		toggleView.onValueChanged.AddListener(delegate(bool isOn)
		{
			toggleViewText.color = (isOn ? Color.black : Color.white);
			OnKeywordToggled(toggleView);
		});
	}

	private int OnHeightItem(int index)
	{
		return _itemHeight;
	}

	private void OnFilterChanged(string newFilter)
	{
		PoplulateFilteredKeywords(newFilter);
		foreach (Keyword selectedKeyword in _selectedKeywords)
		{
			if (!_filteredKeywords.Contains(selectedKeyword))
			{
				_filteredKeywords.Add(selectedKeyword);
			}
		}
		_infiniteScroll.InitData(_filteredKeywords.Count);
		Keyword result;
		string text = (TryGetBestMatchForSearchText(newFilter, out result) ? GetAutoCompleteText(newFilter, result, _autoCompleteHexColor) : string.Empty);
		AutoCompleteLabel.text = text;
		_prvFilter = newFilter;
	}

	private void PoplulateFilteredKeywords(string searchText)
	{
		if (shouldClearCache(_prvFilter, searchText))
		{
			_filteredKeywords.Clear();
			IEnumerable<Keyword> collection = ((!_showAllKeywords && searchText.Length == 0) ? _hintedKeywords : _allKeywords);
			_filteredKeywords.AddRange(collection);
		}
		string[] splitString = searchText.Split(' ');
		bool[] results = new bool[_filteredKeywords.Count];
		Parallel.For(0, _filteredKeywords.Count, delegate(int index)
		{
			results[index] = true;
			for (int i = 0; i < splitString.Length; i++)
			{
				if (!_filteredKeywords[index].SearchText.Contains(splitString[i], StringComparison.OrdinalIgnoreCase))
				{
					results[index] = false;
				}
			}
		});
		for (int num = _filteredKeywords.Count - 1; num >= 0; num--)
		{
			if (!results[num])
			{
				_filteredKeywords.RemoveAt(num);
			}
		}
		static bool shouldClearCache(string prvFilter, string newFilter)
		{
			if (!string.IsNullOrEmpty(prvFilter) && !string.IsNullOrEmpty(newFilter) && prvFilter.Length <= newFilter.Length)
			{
				return !prvFilter.Contains(newFilter, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
	}

	public static string GetAutoCompleteText(string input, Keyword matchingKeyword, string colorFormat)
	{
		if (matchingKeyword.Equals(default(Keyword)))
		{
			return string.Empty;
		}
		string searchText = matchingKeyword.SearchText;
		if (searchText.Length > input.Length)
		{
			sb.Clear();
			sb.Append(input);
			sb.Append($"<color={colorFormat}>");
			sb.Append(searchText.Remove(0, input.Length));
			sb.Append("</color>");
			string result = sb.ToString();
			sb.Clear();
			return result;
		}
		return string.Empty;
	}

	private void OnFilterSubmitted(string newFilter)
	{
		if (TryGetBestMatchForSearchTextWithContains(newFilter, out var result) && _keywordToToggle.ContainsKey(result))
		{
			_keywordToToggle[result].isOn = !_keywordToToggle[result].isOn;
		}
	}

	private void OnKeywordToggled(Toggle toggleView)
	{
		if (toggleView.isOn)
		{
			_selectedKeywords.Add(_toggleToKeyword[toggleView]);
			while (_selectedKeywords.Count > _maxSelectionsAllowed)
			{
				Keyword key = _selectedKeywords[0];
				_selectedKeywords.RemoveAt(0);
				_keywordToToggle[key].isOn = false;
			}
		}
		else
		{
			_selectedKeywords.Remove(_toggleToKeyword[toggleView]);
		}
		this.SelectedKeywordsUpdatedHandlers?.Invoke(Keyword.GetRawText(_selectedKeywords));
	}

	private bool TryGetBestMatchForSearchText(string text, out Keyword result)
	{
		if (!string.IsNullOrEmpty(text))
		{
			Keyword keywordResult = default(Keyword);
			Parallel.For(0, _allKeywords.Count, delegate(int index, ParallelLoopState loopState)
			{
				if (_allKeywords[index].SearchText.StartsWith(text, StringComparison.CurrentCultureIgnoreCase))
				{
					keywordResult = _allKeywords[index];
					loopState.Stop();
				}
			});
			if (!string.IsNullOrEmpty(keywordResult.SearchText))
			{
				result = keywordResult;
				return true;
			}
		}
		result = default(Keyword);
		return false;
	}

	private bool TryGetBestMatchForSearchTextWithContains(string text, out Keyword result)
	{
		if (!string.IsNullOrEmpty(text))
		{
			foreach (Keyword filteredKeyword in _filteredKeywords)
			{
				if (filteredKeyword.SearchText.Contains(text, StringComparison.CurrentCultureIgnoreCase))
				{
					result = filteredKeyword;
					return true;
				}
			}
		}
		result = default(Keyword);
		return false;
	}

	private int CompareKeywords(Keyword lhs, Keyword rhs)
	{
		return lhs.DisplayText.CompareTo(rhs.DisplayText);
	}
}
