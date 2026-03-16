using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.Browsers;

public class SimpleSelectCardsBrowserProvider : ISelectCardsBrowserProvider, IBasicBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider
{
	public readonly string Header;

	public readonly string SubHeader;

	private readonly Dictionary<string, ButtonStateData> _buttons;

	public readonly IReadOnlyDictionary<string, Action> ButtonKeyToActionMap;

	public readonly Dictionary<DuelScene_CDC, HighlightType> CardHighlights;

	public List<DuelScene_CDC> _cardsToDisplay;

	public List<DuelScene_CDC> _selectable;

	public List<DuelScene_CDC> _notSelectable;

	public Action<DuelScene_CDC> OnCardSelected { get; private set; }

	public SelectCardsBrowser OpenedBrowser { get; private set; }

	public bool AllowKeyboardSelection => false;

	public bool ApplyTargetOffset { get; }

	public bool ApplySourceOffset { get; }

	public bool ApplyControllerOffset { get; }

	public SimpleSelectCardsBrowserProvider(List<DuelScene_CDC> cardViews, HashSet<uint> selectableIds, string header = null, string subHeader = null, Dictionary<string, ButtonStateData> buttons = null, IReadOnlyDictionary<string, Action> buttonKeyToActionMap = null, Action<DuelScene_CDC> onCardSelected = null)
	{
		_cardsToDisplay = cardViews;
		Header = header ?? string.Empty;
		SubHeader = subHeader ?? string.Empty;
		_buttons = buttons ?? new Dictionary<string, ButtonStateData>();
		ButtonKeyToActionMap = buttonKeyToActionMap ?? new Dictionary<string, Action>();
		CardHighlights = new Dictionary<DuelScene_CDC, HighlightType>();
		_notSelectable = new List<DuelScene_CDC>();
		_selectable = new List<DuelScene_CDC>();
		OnCardSelected = onCardSelected;
		foreach (DuelScene_CDC cardView in cardViews)
		{
			if (selectableIds.Contains(cardView.Model.Instance.InstanceId))
			{
				_selectable.Add(cardView);
			}
			else
			{
				_notSelectable.Add(cardView);
			}
		}
		ApplyTargetOffset = true;
		ApplySourceOffset = false;
		ApplyControllerOffset = false;
	}

	~SimpleSelectCardsBrowserProvider()
	{
		OnCardSelected = null;
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.SelectCards;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return _buttons;
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
	}

	public static Dictionary<string, ButtonStateData> CreateButtonMap(MTGALocalizedString doneLocKey, string cancelLocKey, bool doneEnabled = true)
	{
		return new Dictionary<string, ButtonStateData>
		{
			["DoneButton"] = new ButtonStateData
			{
				LocalizedString = doneLocKey,
				BrowserElementKey = "2Button_Left",
				StyleType = ButtonStyle.StyleType.Main,
				Enabled = doneEnabled
			},
			["CancelButton"] = new ButtonStateData
			{
				LocalizedString = cancelLocKey,
				BrowserElementKey = "2Button_Right",
				StyleType = ButtonStyle.StyleType.Secondary
			}
		};
	}

	public static IReadOnlyDictionary<string, Action> CreateActionMap(Action doneAction = null, Action cancelAction = null)
	{
		return new Dictionary<string, Action>
		{
			["DoneButton"] = doneAction,
			["CancelButton"] = cancelAction
		};
	}

	public void SetOpenedBrowser(ICardBrowser openedBrowser)
	{
		OpenedBrowser = openedBrowser as SelectCardsBrowser;
		openedBrowser.ButtonPressedHandlers += OnButtonPressed;
		openedBrowser.CardViewSelectedHandlers += OnCardSelected;
	}

	internal void UpdateSubmitButton(bool canSubmit, MTGALocalizedString submitLocString)
	{
		_buttons["DoneButton"].Enabled = canSubmit;
		_buttons["DoneButton"].LocalizedString = submitLocString;
		OpenedBrowser.UpdateButtons();
	}

	private void OnButtonPressed(string buttonKey)
	{
		if (ButtonKeyToActionMap.TryGetValue(buttonKey, out var value))
		{
			value?.Invoke();
		}
		else if (buttonKey == "CancelButton")
		{
			OpenedBrowser.Close();
		}
	}

	public IEnumerable<DuelScene_CDC> GetSelectableCdcs()
	{
		return _selectable;
	}

	public IEnumerable<DuelScene_CDC> GetNonSelectableCdcs()
	{
		return _notSelectable;
	}

	public Dictionary<DuelScene_CDC, HighlightType> GetBrowserHighlights()
	{
		return CardHighlights;
	}

	public List<DuelScene_CDC> GetCardsToDisplay()
	{
		return _cardsToDisplay;
	}

	public string GetCardHolderLayoutKey()
	{
		return "Default";
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	public string GetHeaderText()
	{
		return Header;
	}

	public string GetSubHeaderText()
	{
		return SubHeader;
	}
}
