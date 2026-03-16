using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class PagesMetaCardHolder : MetaCardHolder, IScrollHandler, IEventSystemHandler
{
	[SerializeField]
	protected PagesMetaCardView _cardPrefab;

	[SerializeField]
	private Scrollbar _scrollbar;

	[SerializeField]
	private CustomButton _previousButton;

	[SerializeField]
	private CustomButton _nextButton;

	[SerializeField]
	private PageScrollDirection _scrollDir = PageScrollDirection.Vertical;

	[SerializeField]
	private float _pageWidth = 14.1f;

	[SerializeField]
	private float _pageHeight = 12.1f;

	[SerializeField]
	private float _scrollTime = 0.2f;

	private PagesMetaCardPage[] _pages;

	private int _pageCount;

	private List<PagesMetaCardViewDisplayInformation> _visibleItems = new List<PagesMetaCardViewDisplayInformation>();

	private float _currentPage;

	private float _currentVelocity;

	private int _targetPage;

	private bool _staggerCardUpdates;

	private float _scrollValue;

	private RectTransform _rectTransform;

	private Rect _lastRect;

	private float _lastTargetHeight;

	private float _lastMaxShrink;

	private int _scrollingInDirection;

	public float TARGET_HEIGHT_PERCENT_OF_PREFAB = 0.67f;

	public float MAX_SHRINK_PERCENT = 0.1f;

	private const float APPROXIMATELY_EQUAL_RANGE = 0.005f;

	private RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public int Rows { get; private set; }

	public int Columns { get; private set; }

	public void UpdateLayout()
	{
		_lastRect = RectTransform.rect;
		_lastTargetHeight = TARGET_HEIGHT_PERCENT_OF_PREFAB;
		_lastMaxShrink = MAX_SHRINK_PERCENT;
		float num = _cardPrefab.RectTransform.rect.height * _lastTargetHeight;
		Rows = Mathf.RoundToInt(_lastRect.height / num);
		if (Rows < 1)
		{
			Rows = 1;
		}
		float num2 = _lastRect.height / (float)Rows;
		float num3 = num2 / _cardPrefab.RectTransform.rect.height;
		float num4 = num3 * _cardPrefab.RectTransform.rect.width;
		float num5 = _lastRect.width / num4;
		Columns = Mathf.FloorToInt(num5);
		if (num5 - (float)Columns >= 1f - _lastMaxShrink)
		{
			int columns = Columns + 1;
			Columns = columns;
			num4 = _lastRect.width / (float)Columns;
			num3 = num4 / _cardPrefab.RectTransform.rect.width;
			num2 = num3 * _cardPrefab.RectTransform.rect.height;
		}
		if (Columns < 1)
		{
			Columns = 1;
		}
		int num6 = Rows * Columns;
		float num7 = -0.5f * num4 * (float)(Columns - 1);
		float num8 = 0.5f * num2 * (float)(Rows - 1);
		PagesMetaCardPage[] pages = _pages;
		foreach (PagesMetaCardPage pagesMetaCardPage in pages)
		{
			for (int i = pagesMetaCardPage.CardViews.Count; i < num6; i++)
			{
				PagesMetaCardView pagesMetaCardView = Object.Instantiate(_cardPrefab);
				pagesMetaCardView.Holder = this;
				pagesMetaCardView.Init(base.CardDatabase, base.CardViewBuilder);
				pagesMetaCardPage.CardViews.Add(pagesMetaCardView);
			}
			for (int j = 0; j < pagesMetaCardPage.CardViews.Count; j++)
			{
				PagesMetaCardView pagesMetaCardView2 = pagesMetaCardPage.CardViews[j];
				if (j < num6)
				{
					pagesMetaCardView2.transform.SetParent(pagesMetaCardPage.transform);
					pagesMetaCardView2.transform.localScale = num3 * Vector3.one;
					pagesMetaCardView2.transform.localRotation = Quaternion.Euler(Vector3.zero);
					int num9 = j % Columns;
					int num10 = j / Columns;
					pagesMetaCardView2.transform.localPosition = new Vector3(num7 + (float)num9 * num4, num8 + (float)num10 * (0f - num2));
				}
				else
				{
					pagesMetaCardView2.gameObject.UpdateActive(active: false);
				}
			}
		}
		UpdateScrollbarSettings();
		ForceRefreshPages();
		ScrollToTop();
	}

	protected override void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		base.Init(cardDatabase, cardViewBuilder);
		_pages = GetComponentsInChildren<PagesMetaCardPage>();
		if (_previousButton != null)
		{
			_previousButton.OnClick.AddListener(PreviousButton_OnClick);
			_previousButton.OnMouseover.AddListener(PreviousButton_OnHover);
		}
		if (_nextButton != null)
		{
			_nextButton.OnClick.AddListener(NextButton_OnClick);
			_nextButton.OnMouseover.AddListener(PreviousButton_OnHover);
		}
	}

	protected override void Activate(bool active)
	{
		if (_previousButton != null)
		{
			_previousButton.gameObject.SetActive(active);
		}
		if (_nextButton != null)
		{
			_nextButton.gameObject.SetActive(active);
		}
		if (_scrollbar != null)
		{
			_scrollbar.gameObject.SetActive(active);
		}
		_staggerCardUpdates = true;
	}

	public void SetCards(List<PagesMetaCardViewDisplayInformation> displayInfo)
	{
		_visibleItems = displayInfo;
		ForceRefreshPages();
		UpdateScrollbarSettings();
	}

	private void ForceRefreshPages()
	{
		PagesMetaCardPage[] pages = _pages;
		for (int i = 0; i < pages.Length; i++)
		{
			pages[i].LastPageIndex = -1;
		}
	}

	private void UpdateScrollbarSettings()
	{
		_pageCount = Mathf.Max(1, Mathf.CeilToInt((float)_visibleItems.Count / (float)(Rows * Columns)));
		if (_scrollbar != null)
		{
			_scrollbar.numberOfSteps = _pageCount;
			_scrollbar.size = 1f / (float)_pageCount;
		}
	}

	public void ScrollToTop()
	{
		_currentPage = 0f;
		_currentVelocity = 0f;
		SetScrollValue(0f);
	}

	private float GetScrollValue()
	{
		if (!(_scrollbar != null))
		{
			return _scrollValue;
		}
		return _scrollbar.value;
	}

	private void SetScrollValue(float value)
	{
		if (_scrollbar != null)
		{
			_scrollbar.value = value;
		}
		else
		{
			_scrollValue = value;
		}
	}

	public override void ClearCards()
	{
		_visibleItems.Clear();
		_pageCount = 1;
		_currentPage = 0f;
		_targetPage = 0;
		_currentVelocity = 0f;
	}

	private void PreviousButton_OnClick()
	{
		ScrollPages(-1);
	}

	private void NextButton_OnClick()
	{
		ScrollPages(1);
	}

	private void PreviousButton_OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
	}

	private void ScrollPages(int pagesToScroll)
	{
		float scrollValue = GetScrollValue();
		if ((scrollValue == 0f && pagesToScroll < 0) || (scrollValue == 1f && pagesToScroll > 0))
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_invalid, AudioManager.Default);
		}
		else
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_deck_page_turn, AudioManager.Default);
			_staggerCardUpdates = true;
		}
		SetScrollValue((_pageCount == 0) ? 0f : Mathf.Clamp01(Mathf.Max(scrollValue, 0.001f) + (float)pagesToScroll / (float)_pageCount));
	}

	private static bool IsApproximatelyEqual(float checkValue, float equalTo, float range = 0.005f)
	{
		return Mathf.Abs(checkValue - equalTo) <= range;
	}

	private void GetPages(int topPageIndex, out PagesMetaCardPage topPage, int bottomPageIndex, out PagesMetaCardPage bottomPage)
	{
		List<PagesMetaCardPage> list = _pages.ToList();
		topPage = list.FirstOrDefault((PagesMetaCardPage p) => topPageIndex == -1 || topPageIndex == p.LastPageIndex);
		if (topPage != null)
		{
			list.Remove(topPage);
		}
		bottomPage = list.FirstOrDefault((PagesMetaCardPage p) => bottomPageIndex == -1 || bottomPageIndex == p.LastPageIndex);
		if (bottomPage != null)
		{
			list.Remove(bottomPage);
		}
		if (topPage == null)
		{
			topPage = list[0];
			list.RemoveAt(0);
			if (topPageIndex != -1)
			{
				UpdatePageCards(topPage, topPageIndex);
			}
		}
		if (bottomPage == null)
		{
			bottomPage = list[0];
			list.RemoveAt(0);
			if (bottomPageIndex != -1)
			{
				UpdatePageCards(bottomPage, bottomPageIndex);
			}
		}
		_staggerCardUpdates = false;
	}

	private void UpdatePageCards(PagesMetaCardPage page, int pageIndex)
	{
		page.LastPageIndex = pageIndex;
		List<PagesMetaCardView> cardViews = page.CardViews;
		int num = Rows * Columns;
		int num2 = pageIndex * Rows * Columns;
		for (int i = 0; i < num; i++)
		{
			PagesMetaCardView pagesMetaCardView = cardViews[i];
			int num3 = i + num2;
			if (num3 >= 0 && num3 < _visibleItems.Count)
			{
				PagesMetaCardViewDisplayInformation displayInfo = _visibleItems[num3];
				int num4 = 0;
				if (_staggerCardUpdates)
				{
					num4 = i % Columns;
					if (_scrollingInDirection < 0)
					{
						num4 = Columns - 1 - num4;
					}
				}
				pagesMetaCardView.UpdateDisplayInfo(displayInfo, num4);
				pagesMetaCardView.gameObject.UpdateActive(active: true);
			}
			else
			{
				pagesMetaCardView.gameObject.UpdateActive(active: false);
			}
		}
	}

	private void Update()
	{
		if (_pages != null)
		{
			if (_lastRect != RectTransform.rect || _lastTargetHeight != TARGET_HEIGHT_PERCENT_OF_PREFAB || _lastMaxShrink != MAX_SHRINK_PERCENT)
			{
				UpdateLayout();
			}
			_targetPage = Mathf.Min(_pageCount - 1, Mathf.FloorToInt(GetScrollValue() * (float)_pageCount));
			bool flag = (float)_targetPage >= _currentPage;
			float currentVelocity = _currentVelocity;
			if (IsApproximatelyEqual(_currentPage, _targetPage))
			{
				_currentPage = _targetPage;
				_currentVelocity = 0f;
			}
			else
			{
				_currentPage = Mathf.SmoothDamp(_currentPage, _targetPage, ref _currentVelocity, _scrollTime);
			}
			if (currentVelocity == 0f && _currentVelocity != 0f)
			{
				_scrollingInDirection = (flag ? 1 : (-1));
				DisableCardInteractability();
			}
			else if (currentVelocity != 0f && _currentVelocity == 0f)
			{
				_scrollingInDirection = 0;
				EnableCardInteractability();
			}
			int num = Mathf.FloorToInt(_currentPage);
			float num2 = _currentPage - (float)num;
			int bottomPageIndex = (IsApproximatelyEqual(num2, 0f) ? (-1) : (num + 1));
			GetPages(num, out var topPage, bottomPageIndex, out var bottomPage);
			if (_scrollDir == PageScrollDirection.Vertical)
			{
				topPage.SetY(num2 * _pageHeight);
				bottomPage.SetY((num2 - 1f) * _pageHeight);
			}
			else if (_scrollDir == PageScrollDirection.Horizontal)
			{
				topPage.SetX(num2 * (0f - _pageWidth));
				bottomPage.SetX((num2 - 1f) * (0f - _pageWidth));
			}
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (!base.IsDragging && base.IsPointerOver)
		{
			ScrollPages((!(eventData.scrollDelta.y > 0f)) ? 1 : (-1));
			eventData.Use();
		}
	}

	private void DisableCardInteractability()
	{
		if (base.RolloverZoomView != null)
		{
			base.RolloverZoomView.Close();
		}
		base.RolloverZoomView.IsActive = false;
		PagesMetaCardPage[] pages = _pages;
		for (int i = 0; i < pages.Length; i++)
		{
			foreach (PagesMetaCardView cardView in pages[i].CardViews)
			{
				cardView.CardCollider.enabled = false;
			}
		}
	}

	private void EnableCardInteractability()
	{
		base.RolloverZoomView.IsActive = true;
		PagesMetaCardPage[] pages = _pages;
		for (int i = 0; i < pages.Length; i++)
		{
			foreach (PagesMetaCardView cardView in pages[i].CardViews)
			{
				cardView.CardCollider.enabled = true;
			}
		}
	}
}
