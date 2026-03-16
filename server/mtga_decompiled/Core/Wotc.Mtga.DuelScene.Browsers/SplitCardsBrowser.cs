using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene.Interactions.Grouping;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Browsers;

public class SplitCardsBrowser : CardBrowserBase, IGroupedCardProvider
{
	private readonly GroupWorkflow _groupWorkflow;

	private readonly List<DuelScene_CDC> _topSplit = new List<DuelScene_CDC>();

	private readonly List<DuelScene_CDC> _bottomSplit = new List<DuelScene_CDC>();

	private Vector3 _topSplitPosition;

	private Vector3 _bottomSplitPosition;

	private CardLayout_MultiLayout _multiLayout;

	private Scrollbar _scrollbarA;

	private Scrollbar _scrollbarB;

	public SplitCardsBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		_groupWorkflow = _duelSceneBrowserProvider as GroupWorkflow;
		base.AllowsHoverInteractions = true;
		_topSplitPosition = new Vector3(0f, 1.95f, 0f);
		_bottomSplitPosition = new Vector3(0f, -3.25f, 0f);
		_topSplit = new List<DuelScene_CDC>();
		_bottomSplit = new List<DuelScene_CDC>();
	}

	protected override ICardLayout GetCardHolderLayout()
	{
		return _multiLayout;
	}

	protected override void InitCardHolder()
	{
		base.InitCardHolder();
		cardHolder.CardGroupProvider = this;
	}

	protected override void SetupCards()
	{
		List<List<DuelScene_CDC>> cardsToDisplay = _groupWorkflow.GetCardsToDisplay();
		cardViews = new List<DuelScene_CDC>();
		for (int i = 0; i < cardsToDisplay.Count; i++)
		{
			cardViews.AddRange(cardsToDisplay[i]);
		}
		List<DuelScene_CDC> list = _topSplit;
		if (_groupWorkflow.IsGroupFaceDown(0) && !_groupWorkflow.IsGroupFaceDown(1))
		{
			list = _bottomSplit;
		}
		list.AddRange(cardViews);
		MoveCardViewsToBrowser(cardViews);
	}

	public List<List<DuelScene_CDC>> GetCardGroups()
	{
		if (base.IsVisible)
		{
			return new List<List<DuelScene_CDC>> { _topSplit, _bottomSplit };
		}
		List<DuelScene_CDC> item = new List<DuelScene_CDC>();
		return new List<List<DuelScene_CDC>> { item, item };
	}

	public override bool AllowsDragInteractions(DuelScene_CDC cardView)
	{
		return cardViews.Contains(cardView);
	}

	public override void HandleDrag(DuelScene_CDC draggedCard)
	{
		Vector2 vector = CurrentCamera.Value.WorldToScreenPoint(cardHolder.transform.TransformPoint(_topSplitPosition));
		Vector2 vector2 = CurrentCamera.Value.WorldToScreenPoint(cardHolder.transform.TransformPoint(_bottomSplitPosition));
		Vector2 vector3 = CurrentCamera.Value.WorldToScreenPoint(draggedCard.transform.position);
		float magnitude = (vector - vector3).magnitude;
		float magnitude2 = (vector2 - vector3).magnitude;
		if (magnitude < magnitude2)
		{
			if (_bottomSplit.Contains(draggedCard))
			{
				_bottomSplit.Remove(draggedCard);
				_topSplit.Add(draggedCard);
			}
		}
		else if (_topSplit.Contains(draggedCard))
		{
			_topSplit.Remove(draggedCard);
			_bottomSplit.Add(draggedCard);
		}
		_scrollbarA.gameObject.SetActive(_topSplit.Count > scrollableLayout.FrontCount);
		_scrollbarB.gameObject.SetActive(_bottomSplit.Count > scrollableLayout.FrontCount);
		cardHolder.LayoutNow();
	}

	protected override void InitializeUIElements()
	{
		bool flag = false;
		for (int i = 0; i < _groupWorkflow.GroupCount; i++)
		{
			if (_groupWorkflow.IsGroupFaceDown(i))
			{
				flag = true;
				break;
			}
		}
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ClientPrompt/Divide_Pile_Title"));
		component.SetSubheaderText(Languages.ActiveLocProvider.GetLocalizedText(flag ? "DuelScene/ClientPrompt/Divide_Pile_Facedown_Text" : "DuelScene/ClientPrompt/Divide_Pile_Text"));
		_topSplitPosition = GetBrowserElement("CardGroupAMarker").transform.localPosition;
		_bottomSplitPosition = GetBrowserElement("CardGroupBMarker").transform.localPosition;
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
		_scrollbarA = GetBrowserElement("CardGroupAScrollbar").GetComponent<Scrollbar>();
		_scrollbarA.onValueChanged.RemoveAllListeners();
		_scrollbarA.onValueChanged.AddListener(OnScrollA);
		_scrollbarA.value = 1f;
		_scrollbarA.gameObject.SetActive(_topSplit.Count > scrollableLayout.FrontCount);
		_scrollbarB = GetBrowserElement("CardGroupBScrollbar").GetComponent<Scrollbar>();
		_scrollbarB.onValueChanged.RemoveAllListeners();
		_scrollbarB.onValueChanged.AddListener(OnScrollB);
		_scrollbarB.value = 1f;
		_scrollbarB.gameObject.SetActive(_bottomSplit.Count > scrollableLayout.FrontCount);
		base.InitializeUIElements();
	}

	private void OnScrollA(float scrollValue)
	{
		if (_multiLayout.GetLayout(0) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			cardLayout_ScrollableBrowser.ScrollPosition = scrollValue;
			cardHolder.LayoutNow();
		}
	}

	private void OnScrollB(float scrollValue)
	{
		if (_multiLayout.GetLayout(1) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			cardLayout_ScrollableBrowser.ScrollPosition = scrollValue;
			cardHolder.LayoutNow();
		}
	}

	private void CreateMultiLayout()
	{
		CardLayout_ScrollableBrowser cardHolderLayoutData = GetCardHolderLayoutData(_groupWorkflow.GetCardHolderLayoutKey());
		List<ICardLayout> list = new List<ICardLayout>();
		for (int i = 0; i < 2; i++)
		{
			CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser = new CardLayout_ScrollableBrowser(cardHolderLayoutData);
			cardLayout_ScrollableBrowser.ScrollPosition = 1f;
			list.Add(cardLayout_ScrollableBrowser);
		}
		List<Vector3> layoutCenters = new List<Vector3> { _topSplitPosition, _bottomSplitPosition };
		List<Quaternion> layoutRotations = new List<Quaternion>
		{
			_groupWorkflow.IsGroupFaceDown(0) ? Quaternion.AngleAxis(180f, Vector3.up) : Quaternion.identity,
			_groupWorkflow.IsGroupFaceDown(1) ? Quaternion.AngleAxis(180f, Vector3.up) : Quaternion.identity
		};
		_multiLayout = new CardLayout_MultiLayout(list, layoutCenters, layoutRotations, this);
	}

	protected override void ReleaseCards()
	{
		base.ReleaseCards();
		_topSplit.Clear();
		_bottomSplit.Clear();
		cardHolder.CardGroupProvider = null;
	}
}
