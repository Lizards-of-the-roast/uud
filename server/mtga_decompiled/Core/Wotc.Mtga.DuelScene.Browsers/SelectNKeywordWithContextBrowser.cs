using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.Interactions;

namespace Wotc.Mtga.DuelScene.Browsers;

public class SelectNKeywordWithContextBrowser : CardBrowserBase, IGroupedCardProvider
{
	private readonly Vector3 _handPos = new Vector3(-6f, 0f, 0f);

	private Dictionary<string, ButtonStateData> _buttonData;

	private CardLayout_MultiLayout _cardLayout;

	private CardLayout_ScrollableBrowser _cardLayoutScroll;

	private Scrollbar _scrollbar;

	private KeywordFilter _keywordFilter;

	public bool ShowAllKeywords
	{
		set
		{
			_keywordFilter.ShowAllKeywords = value;
		}
	}

	public Dictionary<string, ButtonStateData> ButtonData
	{
		set
		{
			_buttonData = value;
			if (_buttonData == null)
			{
				return;
			}
			foreach (string buttonKey in _buttonData.Keys)
			{
				StyledButton component = uiElementData[_buttonData[buttonKey].BrowserElementKey].GameObject.GetComponent<StyledButton>();
				component.gameObject.SetActive(_buttonData[buttonKey].IsActive);
				if (_buttonData[buttonKey].IsActive)
				{
					if (_buttonData[buttonKey].StyleType == ButtonStyle.StyleType.Main)
					{
						mainButton = component;
					}
					component.SetModel(new PromptButtonData
					{
						ButtonText = _buttonData[buttonKey].LocalizedString,
						ButtonIcon = _buttonData[buttonKey].Sprite,
						Style = _buttonData[buttonKey].StyleType,
						Enabled = _buttonData[buttonKey].Enabled,
						ButtonCallback = delegate
						{
							OnButtonCallback(buttonKey);
						}
					});
				}
			}
		}
	}

	public event Action<IEnumerable<string>> SelectionsChangedHandlers;

	public SelectNKeywordWithContextBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		base.AllowsHoverInteractions = true;
		_cardLayoutScroll = new CardLayout_ScrollableBrowser(GetCardHolderLayoutData("SelectNKeywordWithContext"));
		_cardLayout = new CardLayout_MultiLayout(new List<ICardLayout> { _cardLayoutScroll }, new List<Vector3> { _handPos }, new List<Quaternion> { Quaternion.identity }, this);
	}

	public void Init(string headerText, string subheaderText, ICollection<uint> cardIds, IReadOnlyList<string> allKeywords, IReadOnlyList<string> hintedKeywords, uint minSelectionsAllowed, uint maxSelectionsAllowed, string initialFilterText)
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(headerText);
		component.SetSubheaderText(subheaderText);
		cardViews.Clear();
		foreach (uint cardId in cardIds)
		{
			if (entityViewManager.TryGetCardView(cardId, out var cardView))
			{
				cardViews.Add(cardView);
			}
		}
		MoveCardViewsToBrowser(cardViews);
		_scrollbar.gameObject.SetActive(cardViews.Count > _cardLayoutScroll.FrontCount);
		ScrollWheelInputForScrollbar component2 = GetBrowserElement("Scrollbar").GetComponent<ScrollWheelInputForScrollbar>();
		component2.ElementCount = cardViews.Count;
		component2.ElementsFeatured = _cardLayoutScroll.FrontCount;
		_keywordFilter.Init(allKeywords, hintedKeywords, minSelectionsAllowed, maxSelectionsAllowed, initialFilterText);
	}

	public List<List<DuelScene_CDC>> GetCardGroups()
	{
		if (base.IsVisible)
		{
			return new List<List<DuelScene_CDC>> { cardViews };
		}
		return new List<List<DuelScene_CDC>>
		{
			new List<DuelScene_CDC>()
		};
	}

	protected override ICardLayout GetCardHolderLayout()
	{
		return _cardLayout;
	}

	private void OnCardsScrolled(float scrollValue)
	{
		_cardLayoutScroll.ScrollPosition = scrollValue;
		cardHolder.LayoutNow();
	}

	private void OnSelectionsChanged(IEnumerable<string> selections)
	{
		this.SelectionsChangedHandlers?.Invoke(selections);
	}

	protected override void SetupCards()
	{
	}

	protected override void InitializeUIElements()
	{
		_scrollbar = GetBrowserElement("Scrollbar").GetComponent<Scrollbar>();
		_scrollbar.onValueChanged.AddListener(OnCardsScrolled);
		_scrollbar.value = _cardLayoutScroll.ScrollPosition;
		_scrollbar.gameObject.SetActive(cardViews.Count > _cardLayoutScroll.FrontCount);
		_keywordFilter = GetBrowserElement("KeywordFilter").GetComponent<KeywordFilter>();
		_keywordFilter.SelectedKeywordsUpdatedHandlers += OnSelectionsChanged;
		base.InitializeUIElements();
	}

	protected override void ReleaseUIElements()
	{
		if (_keywordFilter != null)
		{
			_keywordFilter.SelectedKeywordsUpdatedHandlers -= OnSelectionsChanged;
		}
		if (_scrollbar != null)
		{
			_scrollbar.onValueChanged.RemoveListener(OnCardsScrolled);
			_scrollbar = null;
		}
		base.ReleaseUIElements();
	}
}
