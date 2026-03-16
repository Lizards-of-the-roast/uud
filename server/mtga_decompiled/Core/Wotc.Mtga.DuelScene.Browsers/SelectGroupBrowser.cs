using System.Collections.Generic;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Browsers;

public class SelectGroupBrowser : CardBrowserBase, IGroupedCardProvider
{
	private readonly GroupedBrowserWorkflow<SelectNGroupRequest> selectNGroupWorkflow;

	private List<DuelScene_CDC> topGroup = new List<DuelScene_CDC>();

	private List<DuelScene_CDC> bottomGroup = new List<DuelScene_CDC>();

	private Vector3 topGroupPosition;

	private Vector3 bottomGroupPosition;

	private CardLayout_MultiLayout multiLayout;

	private Vector3 _originalScrollbarPosition;

	private Vector3 _leftPileScrollbarPosition;

	private Vector3 _rightPileScrollBarPosition;

	private Scrollbar _leftScrollbar;

	private Scrollbar _rightScrollbar;

	public SelectGroupBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		base.AllowsHoverInteractions = true;
		selectNGroupWorkflow = _duelSceneBrowserProvider as GroupedBrowserWorkflow<SelectNGroupRequest>;
		topGroupPosition = new Vector3(0f, 1.95f, 0f);
		bottomGroupPosition = new Vector3(0f, -3.05f, 0f);
	}

	protected override ICardLayout GetCardHolderLayout()
	{
		return multiLayout;
	}

	protected override void SetupCards()
	{
		List<List<DuelScene_CDC>> cardsToDisplay = selectNGroupWorkflow.GetCardsToDisplay();
		cardViews = new List<DuelScene_CDC>();
		topGroup = cardsToDisplay[0];
		cardViews.AddRange(topGroup);
		bottomGroup = cardsToDisplay[1];
		cardViews.AddRange(bottomGroup);
		MoveCardViewsToBrowser(cardViews);
		cardHolder.Layout = GetCardHolderLayout();
		cardHolder.LayoutNow();
	}

	protected override void InitializeUIElements()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		if (selectNGroupWorkflow is SelectNPileGroupWorkflow)
		{
			component.SetHeaderText(Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ClientPrompt/Select_Pile_Sacrifice"));
		}
		else
		{
			component.SetHeaderText(Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ClientPrompt/Select_Pile_Title"));
		}
		component.SetSubheaderText(_gameManager.PromptEngine.GetPromptText((int)selectNGroupWorkflow.BaseRequest.Prompt.PromptId));
		topGroupPosition = GetBrowserElement("CardGroupAMarker").transform.localPosition;
		bottomGroupPosition = GetBrowserElement("CardGroupBMarker").transform.localPosition;
		GameObject browserElement = GetBrowserElement("Pile1");
		GameObject browserElement2 = GetBrowserElement("Pile2");
		if (browserElement != null)
		{
			browserElement?.GetComponent<TextMeshProUGUI>().SetText("Pile 1");
		}
		if (browserElement2 != null)
		{
			browserElement2?.GetComponent<TextMeshProUGUI>().SetText("Pile 2");
		}
		CreateMultiLayout();
		_leftScrollbar = GetBrowserElement("LeftScrollbar").GetComponent<Scrollbar>();
		_leftScrollbar.onValueChanged.RemoveAllListeners();
		_leftScrollbar.onValueChanged.AddListener(OnLeftScroll);
		CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser = (CardLayout_ScrollableBrowser)multiLayout.GetLayout(0);
		_leftScrollbar.value = cardLayout_ScrollableBrowser.ScrollPosition;
		_leftScrollbar.gameObject.SetActive(topGroup.Count > cardLayout_ScrollableBrowser.FrontCount);
		_rightScrollbar = GetBrowserElement("RightScrollbar").GetComponent<Scrollbar>();
		_rightScrollbar.onValueChanged.RemoveAllListeners();
		_rightScrollbar.onValueChanged.AddListener(OnRightScroll);
		CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser2 = (CardLayout_ScrollableBrowser)multiLayout.GetLayout(1);
		_rightScrollbar.value = cardLayout_ScrollableBrowser2.ScrollPosition;
		_rightScrollbar.gameObject.SetActive(bottomGroup.Count > cardLayout_ScrollableBrowser2.FrontCount);
		base.InitializeUIElements();
	}

	private void OnLeftScroll(float scrollValue)
	{
		if (multiLayout.GetLayout(0) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			cardLayout_ScrollableBrowser.ScrollPosition = scrollValue;
			cardHolder.LayoutNow();
		}
	}

	private void OnRightScroll(float scrollValue)
	{
		if (multiLayout.GetLayout(1) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			cardLayout_ScrollableBrowser.ScrollPosition = scrollValue;
			cardHolder.LayoutNow();
		}
	}

	private void CreateMultiLayout()
	{
		CardLayout_ScrollableBrowser cardHolderLayoutData = GetCardHolderLayoutData(selectNGroupWorkflow.GetCardHolderLayoutKey());
		List<ICardLayout> list = new List<ICardLayout>();
		for (int i = 0; i < 2; i++)
		{
			CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser = new CardLayout_ScrollableBrowser(cardHolderLayoutData);
			cardLayout_ScrollableBrowser.ScrollPosition = 1f;
			list.Add(cardLayout_ScrollableBrowser);
		}
		List<Vector3> layoutCenters = new List<Vector3> { topGroupPosition, bottomGroupPosition };
		List<Quaternion> layoutRotations = new List<Quaternion>
		{
			Quaternion.identity,
			Quaternion.identity
		};
		multiLayout = new CardLayout_MultiLayout(list, layoutCenters, layoutRotations, this);
	}

	public List<List<DuelScene_CDC>> GetCardGroups()
	{
		if (base.IsVisible)
		{
			return new List<List<DuelScene_CDC>> { topGroup, bottomGroup };
		}
		List<DuelScene_CDC> item = new List<DuelScene_CDC>();
		return new List<List<DuelScene_CDC>> { item, item };
	}

	protected override void ReleaseCards()
	{
		base.ReleaseCards();
		topGroup.Clear();
		bottomGroup.Clear();
	}
}
