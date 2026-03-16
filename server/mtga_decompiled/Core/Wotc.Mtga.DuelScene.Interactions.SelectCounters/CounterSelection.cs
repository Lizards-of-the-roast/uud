using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Pooling;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public class CounterSelection : IDuelSceneBrowserProvider, IButtonSelectionBrowserProvider, IBrowserHeaderProvider
{
	public const string BUTTON_SEPARATOR = "_";

	public const string COUNTER_BUTTON_PREFIX = "Counter_";

	public const string SELECTION_BUTTON_PREFIX = "Selection_";

	public const string COUNTER_BUTTON_FORMAT = "Counter_{0}";

	public const string SELECTION_BUTTON_FORMAT = "Selection_{0}";

	private readonly IBrowserController _browserController;

	private readonly IButtonStateBuilder _buttonStateBuilder;

	private readonly IBrowserTextProvider _browserTextProvider;

	private readonly IObjectPool _pool;

	private readonly Dictionary<string, ButtonStateData> _buttonStateData = new Dictionary<string, ButtonStateData>();

	private readonly Dictionary<string, ButtonStateData> _scrollListButtonStateData = new Dictionary<string, ButtonStateData>();

	private readonly Dictionary<string, ButtonStateData> _selectedScrollListButtonStateData = new Dictionary<string, ButtonStateData>();

	private readonly SortedDictionary<uint, uint> _counterTypeToCount = new SortedDictionary<uint, uint>();

	private readonly List<uint> _selections = new List<uint>();

	private IBrowser _browser;

	private uint _toRemoveCount;

	private string _header = string.Empty;

	private string _subHeader = string.Empty;

	public event Action<IEnumerable<uint>> Submitted;

	public string GetHeaderText()
	{
		return _header;
	}

	public string GetSubHeaderText()
	{
		return _subHeader;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return _buttonStateData;
	}

	public Dictionary<string, ButtonStateData> GetScrollListButtonStateData()
	{
		return _scrollListButtonStateData;
	}

	public Dictionary<string, ButtonStateData> GetSelectedScrollListButtonStateData()
	{
		return _selectedScrollListButtonStateData;
	}

	public bool SortButtonsByKey()
	{
		return false;
	}

	public CounterSelection(IBrowserController browserController, IButtonStateBuilder buttonStateBuilder, IBrowserTextProvider browserTextProvider, IObjectPool pool)
	{
		_browserController = browserController ?? NullBrowserController.Default;
		_buttonStateBuilder = buttonStateBuilder ?? NullButtonStateBuilder.Default;
		_browserTextProvider = browserTextProvider ?? NullBrowserTextProvider.Default;
		_pool = pool ?? NullObjectPool.Default;
	}

	public void Apply(uint sourceId, IReadOnlyCollection<CounterPair> pairs, uint toRemoveCount)
	{
		if (_browser != null)
		{
			_browser.ButtonPressedHandlers -= OnBrowserButtonPressed;
			_browser.Close();
			_browser = null;
		}
		_toRemoveCount = toRemoveCount;
		BrowserText browserText = _browserTextProvider.GetBrowserText(sourceId, toRemoveCount);
		_header = browserText.Header;
		_subHeader = browserText.SubHeader;
		_selections.Clear();
		_counterTypeToCount.Clear();
		foreach (CounterPair pair in pairs)
		{
			_counterTypeToCount[(uint)pair.CounterType] = pair.Count;
		}
		UpdateBrowserButtons();
		_browser = _browserController.OpenBrowser(this);
		_browser.ButtonPressedHandlers += OnBrowserButtonPressed;
	}

	private void OnBrowserButtonPressed(string buttonKey)
	{
		ButtonStateData value;
		ButtonStateData value2;
		if (buttonKey == "SubmitButton")
		{
			SubmitSelections();
		}
		else if (_scrollListButtonStateData.TryGetValue(buttonKey, out value) && value.Enabled && buttonKey.StartsWith("Counter_"))
		{
			uint counterType = uint.Parse(buttonKey.Split('_')[1]);
			MakeSelection(counterType);
		}
		else if (_selectedScrollListButtonStateData.TryGetValue(buttonKey, out value2) && value2.Enabled && buttonKey.StartsWith("Selection_"))
		{
			uint counterType2 = uint.Parse(buttonKey.Split('_')[1]);
			RemoveSelection(counterType2);
		}
	}

	private void MakeSelection(uint counterType)
	{
		_selections.Add(counterType);
		RefreshBrowser();
	}

	private void RemoveSelection(uint counterType)
	{
		_selections.Remove(counterType);
		RefreshBrowser();
	}

	private void SubmitSelections()
	{
		this.Submitted?.Invoke(_selections);
	}

	private void RefreshBrowser()
	{
		UpdateBrowserButtons();
		if (_browser is ButtonSelectionBrowser buttonSelectionBrowser)
		{
			buttonSelectionBrowser.Refresh();
		}
	}

	public void CleanUp()
	{
		if (_browser != null)
		{
			_browser.ButtonPressedHandlers -= OnBrowserButtonPressed;
			_browser.Close();
			_browser = null;
		}
		_buttonStateData.Clear();
		_scrollListButtonStateData.Clear();
		_selectedScrollListButtonStateData.Clear();
		_counterTypeToCount.Clear();
		_selections.Clear();
		_header = string.Empty;
		_subHeader = string.Empty;
		this.Submitted = null;
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.ButtonSelection;
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	private void UpdateBrowserButtons()
	{
		UpdateDefaultBrowserButtons();
		UpdateScrollListButtons();
		UpdateSelectedScrollListButtons();
	}

	private void UpdateDefaultBrowserButtons()
	{
		_buttonStateData.Clear();
		_buttonStateData["SubmitButton"] = new ButtonStateData
		{
			IsActive = true,
			LocalizedString = new MTGALocalizedString
			{
				Key = "DuelScene/ClientPrompt/Submit_N",
				Parameters = new Dictionary<string, string> { 
				{
					"submitCount",
					_selections.Count.ToString()
				} }
			},
			BrowserElementKey = "SingleButton",
			Enabled = (_selections.Count == _toRemoveCount),
			StyleType = ButtonStyle.StyleType.Main
		};
	}

	private void UpdateScrollListButtons()
	{
		_scrollListButtonStateData.Clear();
		foreach (uint counterType in _counterTypeToCount.Keys)
		{
			string key = $"Counter_{counterType}";
			List<uint> list = _selections.FindAll((uint x) => x == counterType);
			uint num = _counterTypeToCount[counterType] - (uint)list.Count;
			bool enabled = num != 0 && _selections.Count < _toRemoveCount;
			ButtonStateData value = _buttonStateBuilder.CreateButtonStateData(counterType, num, ButtonStyle.StyleType.Secondary, enabled, GetBrowserType());
			_scrollListButtonStateData[key] = value;
		}
	}

	private void UpdateSelectedScrollListButtons()
	{
		_selectedScrollListButtonStateData.Clear();
		Dictionary<uint, uint> dictionary = _pool.PopObject<Dictionary<uint, uint>>();
		foreach (uint selection in _selections)
		{
			if (dictionary.ContainsKey(selection))
			{
				dictionary[selection]++;
			}
			else
			{
				dictionary[selection] = 1u;
			}
		}
		foreach (KeyValuePair<uint, uint> item in dictionary)
		{
			uint key = item.Key;
			string key2 = $"Selection_{key}";
			ButtonStateData value = _buttonStateBuilder.CreateButtonStateData(key, item.Value, ButtonStyle.StyleType.Secondary, enabled: true, GetBrowserType());
			_selectedScrollListButtonStateData[key2] = value;
		}
		dictionary.Clear();
		_pool.PushObject(dictionary);
	}
}
