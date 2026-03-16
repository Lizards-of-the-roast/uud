using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.DuelScene.Browsers;

public class SurveilBrowser : CardBrowserBase, IGroupedCardProvider
{
	private readonly IBasicBrowserProvider browserProvider;

	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	private CardLayout_MultiLayout multiLayout;

	private readonly List<DuelScene_CDC> graveyardGroup = new List<DuelScene_CDC>();

	private readonly List<DuelScene_CDC> libraryGroup = new List<DuelScene_CDC>();

	private Vector3 _graveyardCenterPoint = Vector3.zero;

	private Vector3 _libraryCenterPoint = Vector3.zero;

	private Scrollbar _graveyardScrollbar;

	private Scrollbar _libraryScrollbar;

	public Action DragStartEvent;

	public Action DragReleaseEvent;

	private bool _dragActive;

	public SurveilBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		browserProvider = _duelSceneBrowserProvider as IBasicBrowserProvider;
		_cardBuilder = gameManager.Context.Get<ICardBuilder<DuelScene_CDC>>() ?? NullCardBuilder<DuelScene_CDC>.Default;
		base.AllowsHoverInteractions = true;
	}

	protected override ICardLayout GetCardHolderLayout()
	{
		return multiLayout;
	}

	public List<List<DuelScene_CDC>> GetCardGroups()
	{
		if (base.IsVisible)
		{
			return new List<List<DuelScene_CDC>> { graveyardGroup, libraryGroup };
		}
		List<DuelScene_CDC> item = new List<DuelScene_CDC>();
		return new List<List<DuelScene_CDC>> { item, item };
	}

	public List<DuelScene_CDC> GetGraveyardCards()
	{
		return graveyardGroup;
	}

	public List<DuelScene_CDC> GetLibraryCards()
	{
		return libraryGroup;
	}

	protected override void InitCardHolder()
	{
		base.InitCardHolder();
		cardHolder.IgnoreDummyCards = true;
		cardHolder.CardGroupProvider = this;
	}

	protected override void SetupCards()
	{
		cardViews = browserProvider.GetCardsToDisplay();
		MtgPlayer owner = null;
		if (cardViews.Count > 0)
		{
			owner = cardViews[0].Model.Owner;
		}
		CardData cardData = CardBrowserBase.CreateLibraryPlaceHolderCardData(owner);
		cardViews.Add(_cardBuilder.CreateCDC(cardData));
		MoveCardViewsToBrowser(cardViews);
		libraryGroup.AddRange(cardViews);
	}

	protected override void ReleaseCards()
	{
		base.ReleaseCards();
		graveyardGroup.Clear();
		libraryGroup.Clear();
		cardHolder.IgnoreDummyCards = false;
		cardHolder.CardGroupProvider = null;
	}

	public override void Close()
	{
		_graveyardScrollbar.onValueChanged.RemoveAllListeners();
		_libraryScrollbar.onValueChanged.RemoveAllListeners();
		base.Close();
	}

	protected override void InitializeUIElements()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(browserProvider.GetHeaderText());
		component.SetSubheaderText(browserProvider.GetSubHeaderText());
		List<ICardLayout> list = new List<ICardLayout>();
		list.Add(new CardLayout_ScrollableBrowser(GetCardHolderLayoutData("Surveil_Graveyard")));
		list.Add(new CardLayout_ScrollableBrowser(GetCardHolderLayoutData("Surveil_Library")));
		_graveyardCenterPoint = GetBrowserElement("CardGroupAMarker").transform.localPosition;
		_libraryCenterPoint = GetBrowserElement("CardGroupBMarker").transform.localPosition;
		List<Vector3> layoutCenters = new List<Vector3> { _graveyardCenterPoint, _libraryCenterPoint };
		List<Quaternion> layoutRotations = new List<Quaternion>
		{
			Quaternion.identity,
			Quaternion.identity
		};
		multiLayout = new CardLayout_MultiLayout(list, layoutCenters, layoutRotations, this);
		_graveyardScrollbar = GetBrowserElement("GraveyardScrollbar").GetComponent<Scrollbar>();
		_graveyardScrollbar.onValueChanged.RemoveAllListeners();
		_graveyardScrollbar.onValueChanged.AddListener(OnGraveyardScroll);
		CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser = (CardLayout_ScrollableBrowser)multiLayout.GetLayout(0);
		_graveyardScrollbar.value = cardLayout_ScrollableBrowser.ScrollPosition;
		_graveyardScrollbar.gameObject.SetActive(graveyardGroup.Count > cardLayout_ScrollableBrowser.FrontCount);
		_libraryScrollbar = GetBrowserElement("LibraryScrollbar").GetComponent<Scrollbar>();
		_libraryScrollbar.onValueChanged.RemoveAllListeners();
		_libraryScrollbar.onValueChanged.AddListener(OnLibraryScroll);
		CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser2 = (CardLayout_ScrollableBrowser)multiLayout.GetLayout(1);
		_libraryScrollbar.value = cardLayout_ScrollableBrowser2.ScrollPosition;
		_libraryScrollbar.gameObject.SetActive(libraryGroup.Count > cardLayout_ScrollableBrowser2.FrontCount);
		base.InitializeUIElements();
	}

	private void OnGraveyardScroll(float scrollValue)
	{
		if (multiLayout.GetLayout(0) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			cardLayout_ScrollableBrowser.ScrollPosition = scrollValue;
			cardHolder.LayoutNow();
		}
	}

	private void OnLibraryScroll(float scrollValue)
	{
		if (multiLayout.GetLayout(1) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser)
		{
			cardLayout_ScrollableBrowser.ScrollPosition = scrollValue;
			cardHolder.LayoutNow();
		}
	}

	public override bool AllowsDragInteractions(DuelScene_CDC cardView)
	{
		return cardViews.Contains(cardView);
	}

	public override void HandleDrag(DuelScene_CDC draggedCard)
	{
		if (!_dragActive)
		{
			_dragActive = true;
			DragStartEvent?.Invoke();
		}
		Vector2 vector = CurrentCamera.Value.WorldToScreenPoint(draggedCard.Root.parent.TransformPoint(_graveyardCenterPoint));
		Vector2 vector2 = CurrentCamera.Value.WorldToScreenPoint(draggedCard.Root.parent.TransformPoint(_libraryCenterPoint));
		Vector2 vector3 = CurrentCamera.Value.WorldToScreenPoint(draggedCard.transform.position);
		float magnitude = (vector - vector3).magnitude;
		float magnitude2 = (vector2 - vector3).magnitude;
		if (magnitude < magnitude2)
		{
			if (libraryGroup.Contains(draggedCard))
			{
				libraryGroup.Remove(draggedCard);
				graveyardGroup.Insert(0, draggedCard);
			}
		}
		else if (graveyardGroup.Contains(draggedCard))
		{
			graveyardGroup.Remove(draggedCard);
			libraryGroup.Insert(0, draggedCard);
		}
		if (multiLayout.GetLayouts().Count() == 2 && multiLayout.GetLayout(0) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser && multiLayout.GetLayout(1) is CardLayout_ScrollableBrowser cardLayout_ScrollableBrowser2)
		{
			_graveyardScrollbar.gameObject.SetActive(graveyardGroup.Count > cardLayout_ScrollableBrowser.FrontCount);
			_libraryScrollbar.gameObject.SetActive(libraryGroup.Count > cardLayout_ScrollableBrowser2.FrontCount);
		}
		cardHolder.LayoutNow();
	}

	public override void OnDragRelease(DuelScene_CDC draggedCard)
	{
		base.OnDragRelease(draggedCard);
		_dragActive = false;
		DragReleaseEvent?.Invoke();
	}
}
