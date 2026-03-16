using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.Browsers;

public class LondonBrowser : CardBrowserBase, IGroupedCardProvider, ICardDragRegionProvider, ICardBrowser, IBrowser
{
	private readonly LondonWorkflow _workflow;

	private readonly List<DuelScene_CDC> _handGroup = new List<DuelScene_CDC>();

	private readonly List<DuelScene_CDC> _libraryGroup = new List<DuelScene_CDC>();

	private CardLayout_MultiLayout _multiLayout;

	private Vector3 _handPos = new Vector3(4.5f, -0.65f, 0f);

	private Vector3 _libraryPos = new Vector3(-6f, -0.65f, 0f);

	private Scrollbar _scrollbar;

	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	public uint RequiredPutbackCount { get; set; }

	public string HeaderText
	{
		set
		{
			GetBrowserElement("Header").GetComponent<BrowserHeader>().SetHeaderText(value);
		}
	}

	public string SubheaderText
	{
		set
		{
			GetBrowserElement("Header").GetComponent<BrowserHeader>().SetSubheaderText(value);
		}
	}

	public Vector2 HandScreenSpace => ComputeScreenPoint(_handPos);

	public Vector2 LibraryScreenSpace => ComputeScreenPoint(_libraryPos);

	public bool LibraryNeedsMorePutBack => _libraryGroup.Count - 1 < RequiredPutbackCount;

	public LondonBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager, ICardBuilder<DuelScene_CDC> cardBuilder)
		: base(browserManager, provider, gameManager)
	{
		_workflow = (LondonWorkflow)_duelSceneBrowserProvider;
		_handGroup = new List<DuelScene_CDC>();
		_libraryGroup = new List<DuelScene_CDC>();
		_cardBuilder = cardBuilder ?? NullCardBuilder<DuelScene_CDC>.Default;
		base.AllowsHoverInteractions = true;
	}

	public List<List<DuelScene_CDC>> GetCardGroups()
	{
		if (base.IsVisible)
		{
			return new List<List<DuelScene_CDC>> { _libraryGroup, _handGroup };
		}
		List<DuelScene_CDC> item = new List<DuelScene_CDC>();
		return new List<List<DuelScene_CDC>> { item, item };
	}

	public List<DuelScene_CDC> GetHandCards()
	{
		return _handGroup;
	}

	public List<DuelScene_CDC> GetLibraryCards()
	{
		return _libraryGroup;
	}

	public override bool AllowsDragInteractions(DuelScene_CDC cardView)
	{
		return cardViews.Contains(cardView);
	}

	private Vector2 ComputeScreenPoint(Vector3 position)
	{
		return CurrentCamera.Value.WorldToScreenPoint(cardHolder.CardRoot.TransformPoint(position));
	}

	public bool IsInLibrary(DuelScene_CDC card)
	{
		return _libraryGroup.Contains(card);
	}

	public bool IsInHand(DuelScene_CDC card)
	{
		return _handGroup.Contains(card);
	}

	public override void HandleDrag(DuelScene_CDC draggedCard)
	{
		Vector2 handScreenSpace = HandScreenSpace;
		Vector2 libraryScreenSpace = LibraryScreenSpace;
		Vector2 vector = CurrentCamera.Value.WorldToScreenPoint(draggedCard.transform.position);
		float magnitude = (handScreenSpace - vector).magnitude;
		float magnitude2 = (libraryScreenSpace - vector).magnitude;
		if (magnitude < magnitude2)
		{
			if (IsInLibrary(draggedCard))
			{
				_libraryGroup.Remove(draggedCard);
				_handGroup.Insert(0, draggedCard);
			}
		}
		else if (IsInHand(draggedCard) && LibraryNeedsMorePutBack)
		{
			_handGroup.Remove(draggedCard);
			_libraryGroup.Insert(1, draggedCard);
		}
		cardHolder.LayoutNow();
	}

	public override void OnDragRelease(DuelScene_CDC draggedCard)
	{
		base.OnDragRelease(draggedCard);
		_workflow.OnGroupsUpdated();
	}

	protected override ICardLayout GetCardHolderLayout()
	{
		return _multiLayout;
	}

	protected override void InitCardHolder()
	{
		base.InitCardHolder();
		cardHolder.IgnoreDummyCards = true;
		cardHolder.CardGroupProvider = this;
	}

	protected override void SetupCards()
	{
		cardViews = _workflow.GetCardsToDisplay();
		MtgPlayer owner = null;
		if (cardViews.Count > 0)
		{
			owner = cardViews[0].Model.Owner;
		}
		DuelScene_CDC item = _cardBuilder.CreateCDC(CardBrowserBase.CreateLibraryPlaceHolderCardData(owner));
		_handGroup.AddRange(cardViews);
		_libraryGroup.Add(item);
		cardViews.Add(item);
		MoveCardViewsToBrowser(cardViews);
	}

	protected override void ReleaseCards()
	{
		base.ReleaseCards();
		_handGroup.Clear();
		_libraryGroup.Clear();
		cardHolder.IgnoreDummyCards = false;
		cardHolder.CardGroupProvider = null;
	}

	protected override void InitializeUIElements()
	{
		_handPos = GetBrowserElement("CardGroupBMarker").transform.localPosition;
		_libraryPos = GetBrowserElement("CardGroupAMarker").transform.localPosition;
		List<ICardLayout> list = new List<ICardLayout>();
		list.Add(new CardLayout_ScrollableBrowser(GetCardHolderLayoutData("Surveil_Library")));
		list.Add(new CardLayout_ScrollableBrowser(GetCardHolderLayoutData("LondonMulligan_Hand")));
		List<Vector3> layoutCenters = new List<Vector3> { _libraryPos, _handPos };
		List<Quaternion> layoutRotations = new List<Quaternion>
		{
			Quaternion.identity,
			Quaternion.identity
		};
		_multiLayout = new CardLayout_MultiLayout(list, layoutCenters, layoutRotations, this);
		GameObject browserElement = GetBrowserElement("Scrollbar");
		if (browserElement != null)
		{
			_scrollbar = browserElement.GetComponent<Scrollbar>();
			_scrollbar.onValueChanged.RemoveAllListeners();
			_scrollbar.onValueChanged.AddListener(OnScroll);
			_scrollbar.value = scrollableLayout.ScrollPosition;
			_scrollbar.gameObject.SetActive(cardViews.Count > scrollableLayout.FrontCount);
		}
		base.InitializeUIElements();
	}

	protected override void OnScroll(float scrollValue)
	{
		if (_multiLayout.GetLayout(1) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			cardLayout_ScrollableBrowser.ScrollPosition = scrollValue;
			cardHolder.LayoutNow();
		}
	}

	protected override void ReleaseUIElements()
	{
		if (_scrollbar != null)
		{
			_scrollbar.onValueChanged.RemoveAllListeners();
		}
		base.ReleaseUIElements();
	}

	public IEnumerable<Vector2> GetDragZones()
	{
		return new List<Vector2> { LibraryScreenSpace, HandScreenSpace };
	}

	public int DetermineDragZoneIndex(DuelScene_CDC card)
	{
		if (_libraryGroup.Contains(card))
		{
			return 0;
		}
		if (_handGroup.Contains(card))
		{
			return 1;
		}
		return -1;
	}

	public bool CanChangeZones(DuelScene_CDC card)
	{
		if (!_libraryGroup.Contains(card))
		{
			if (_handGroup.Contains(card))
			{
				return LibraryNeedsMorePutBack;
			}
			return false;
		}
		return true;
	}
}
