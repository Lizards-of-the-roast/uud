using System.Collections.Generic;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Browsers;

public class KeywordSelectionBrowser : BrowserBase
{
	private readonly KeywordSelectionBrowserProvider keywordSelectionBrowserProvider;

	private readonly List<string> _selections = new List<string>();

	private KeywordFilter _keywordFilter;

	public KeywordSelectionBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		keywordSelectionBrowserProvider = provider as KeywordSelectionBrowserProvider;
	}

	protected override void InitializeUIElements()
	{
		GetBrowserElement("Header").GetComponent<BrowserHeader>().SetHeaderText(keywordSelectionBrowserProvider.HeaderText);
		_keywordFilter = GetBrowserElement("KeywordFilter").GetComponent<KeywordFilter>();
		_keywordFilter.Init(keywordSelectionBrowserProvider.KeywordOptions, keywordSelectionBrowserProvider.HintingOptions, keywordSelectionBrowserProvider.MinSelections, keywordSelectionBrowserProvider.MaxSelections, keywordSelectionBrowserProvider.InitialFilterText);
		_keywordFilter.SelectedKeywordsUpdatedHandlers += OnSelectionsChanged;
		base.InitializeUIElements();
	}

	protected override void ReleaseUIElements()
	{
		if (_keywordFilter != null)
		{
			_keywordFilter.SelectedKeywordsUpdatedHandlers -= OnSelectionsChanged;
		}
		base.ReleaseUIElements();
	}

	private void OnSelectionsChanged(IEnumerable<string> selections)
	{
		_selections.Clear();
		_selections.AddRange(selections);
		UpdateButtons();
	}

	public void ShowAllOptions()
	{
		_keywordFilter.ShowAllKeywords = true;
		UpdateButtons();
	}

	public List<string> GetSelections()
	{
		return _selections;
	}
}
