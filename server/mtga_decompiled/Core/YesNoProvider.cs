using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;

public class YesNoProvider : IDuelSceneBrowserProvider
{
	public readonly string Header;

	public readonly string SubHeader;

	private readonly Dictionary<string, ButtonStateData> _buttons;

	public readonly IReadOnlyDictionary<string, Action> ButtonKeyToActionMap;

	public YesNoProvider(string header = null, string subHeader = null, Dictionary<string, ButtonStateData> buttons = null, IReadOnlyDictionary<string, Action> buttonKeyToActionMap = null)
	{
		Header = header ?? string.Empty;
		SubHeader = subHeader ?? string.Empty;
		_buttons = buttons ?? new Dictionary<string, ButtonStateData>();
		ButtonKeyToActionMap = buttonKeyToActionMap ?? new Dictionary<string, Action>();
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.YesNo;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return _buttons;
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public static Dictionary<string, ButtonStateData> CreateButtonMap(string yesLocKey, string noLocKey)
	{
		return new Dictionary<string, ButtonStateData>
		{
			["YesButton"] = new ButtonStateData
			{
				LocalizedString = yesLocKey,
				BrowserElementKey = "2Button_Left",
				StyleType = ButtonStyle.StyleType.Secondary
			},
			["NoButton"] = new ButtonStateData
			{
				LocalizedString = noLocKey,
				BrowserElementKey = "2Button_Right",
				StyleType = ButtonStyle.StyleType.Secondary
			}
		};
	}

	public static IReadOnlyDictionary<string, Action> CreateActionMap(Action yesAction = null, Action noAction = null)
	{
		return new Dictionary<string, Action>
		{
			["YesButton"] = yesAction,
			["NoButton"] = noAction
		};
	}
}
