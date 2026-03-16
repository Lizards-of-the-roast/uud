using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Loc;

public class KeywordSelectionBrowserProvider : IDuelSceneBrowserProvider
{
	private Action<IReadOnlyList<string>> onComplete;

	private bool showAllButtonAvailable = true;

	private bool canCancel;

	private readonly Dictionary<string, ButtonStateData> buttonStateData = new Dictionary<string, ButtonStateData>();

	private KeywordSelectionBrowser openedBrowser;

	public string InitialFilterText { get; }

	public string HeaderText { get; }

	public IReadOnlyCollection<string> KeywordOptions { get; }

	public IReadOnlyCollection<string> HintingOptions { get; }

	public uint MinSelections { get; }

	public uint MaxSelections { get; }

	public int MinOptionsToEnableHinting => 15;

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.KeywordSelection;
	}

	public KeywordSelectionBrowserProvider(IReadOnlyCollection<string> keywordOptions, IReadOnlyCollection<string> hintingOptions, uint minSelections, uint maxSelections, Action<IReadOnlyList<string>> onComplete, bool canCancel, string filterDescription = null, string prompt = null)
	{
		KeywordOptions = keywordOptions;
		HintingOptions = hintingOptions;
		MinSelections = minSelections;
		MaxSelections = maxSelections;
		this.onComplete = onComplete;
		this.canCancel = canCancel;
		InitialFilterText = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/KeywordSelection/KeywordSelection_Filter", ("description", filterDescription));
		HeaderText = prompt;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		UpdateButtonStateData();
		return buttonStateData;
	}

	private void Browser_OnButtonPressed(string buttonKey)
	{
		if (buttonKey == "ShowAllButton")
		{
			showAllButtonAvailable = false;
			openedBrowser.ShowAllOptions();
			openedBrowser.UpdateButtons();
		}
		if (buttonKey == "DoneButton")
		{
			OnDone();
		}
		if (buttonKey == "CancelButton")
		{
			OnCancel();
		}
	}

	public void SetOpenedBrowser(IBrowser browser)
	{
		openedBrowser = (KeywordSelectionBrowser)browser;
		openedBrowser.ButtonPressedHandlers += Browser_OnButtonPressed;
		openedBrowser.ClosedHandlers += Browser_OnClosed;
	}

	private void Browser_OnClosed()
	{
		openedBrowser.ButtonPressedHandlers -= Browser_OnButtonPressed;
		openedBrowser.ClosedHandlers -= Browser_OnClosed;
	}

	private void OnDone()
	{
		onComplete(openedBrowser.GetSelections());
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_done, AudioManager.Default);
		openedBrowser.Close();
	}

	private void OnCancel()
	{
		onComplete(null);
		openedBrowser.Close();
	}

	private void UpdateButtonStateData()
	{
		this.buttonStateData.Clear();
		ButtonStateData buttonStateData = new ButtonStateData();
		buttonStateData.BrowserElementKey = "DoneButton";
		buttonStateData.LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_Done";
		buttonStateData.StyleType = ButtonStyle.StyleType.Secondary;
		buttonStateData.Enabled = openedBrowser != null && openedBrowser.GetSelections().Count >= MinSelections && openedBrowser.GetSelections().Count <= MaxSelections;
		this.buttonStateData.Add(buttonStateData.BrowserElementKey, buttonStateData);
		ButtonStateData buttonStateData2 = new ButtonStateData();
		buttonStateData2.IsActive = HintingOptions.Count > 0 && KeywordOptions.Count > MinOptionsToEnableHinting && showAllButtonAvailable;
		buttonStateData2.BrowserElementKey = "ShowAllButton";
		buttonStateData2.LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_ShowAll";
		buttonStateData2.StyleType = ButtonStyle.StyleType.OutlinedSmall;
		this.buttonStateData.Add(buttonStateData2.BrowserElementKey, buttonStateData2);
		ButtonStateData buttonStateData3 = new ButtonStateData();
		buttonStateData3.IsActive = canCancel;
		buttonStateData3.BrowserElementKey = "CancelButton";
		buttonStateData3.LocalizedString = "DuelScene/KeywordSelection/KeywordSelection_Cancel";
		buttonStateData3.StyleType = ButtonStyle.StyleType.Secondary;
		this.buttonStateData.Add(buttonStateData3.BrowserElementKey, buttonStateData3);
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}
}
