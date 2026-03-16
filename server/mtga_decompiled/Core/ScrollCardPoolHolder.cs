using System;
using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class ScrollCardPoolHolder : CardPoolHolder
{
	private List<PagesMetaCardView> _cardViews = new List<PagesMetaCardView>();

	private int _lastCardCount;

	private int _currentColumn;

	private int _wantedSmallestCardIndex;

	private int _loadedSmallestCardIndex;

	private int _wantedLargestCardIndex;

	private int _loadedLargestCardIndex;

	private bool _scrollingRight;

	private int _velocityOffset;

	private int _numCardViewsChanged;

	private float _cardHeight;

	private float _cardWidth;

	private float _startY;

	private float _startX;

	private int _bufferSize;

	private PagesMetaCardViewDisplayInformation _commanderDisplayInfo;

	private PagesMetaCardViewDisplayInformation _partnerDisplayInfo;

	private PagesMetaCardViewDisplayInformation _companionDisplayInfo;

	[SerializeField]
	private float _maxVelocity;

	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private float _bufferMultiplier;

	[SerializeField]
	private float _paddingX;

	[SerializeField]
	private float _cardWidthShift;

	[SerializeField]
	private float _paddingTop;

	[SerializeField]
	private float _paddingBottom;

	private List<PagesMetaCardViewDisplayInformation> _bellaRenameMe;

	private bool _bellaShouldWeSnap;

	private int _bellaOldCardCount;

	protected override void Awake()
	{
		_usesPaging = false;
		base.Awake();
	}

	protected override void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		Vector2 offsetMin = base.RectTransform.offsetMin;
		Vector2 offsetMax = base.RectTransform.offsetMax;
		offsetMin.y = _paddingBottom;
		offsetMax.y = _paddingTop;
		base.RectTransform.offsetMin = offsetMin;
		base.RectTransform.offsetMax = offsetMax;
		base.Init(cardDatabase, cardViewBuilder);
	}

	public override void ClearCards()
	{
		_cardDisplayInfos = new List<PagesMetaCardViewDisplayInformation>();
		ResetWidth();
		DisableAllCards();
		_cardsDirty = true;
		_currentColumn = 0;
		InvokePageChange(0);
		_loadedSmallestCardIndex = 0;
		_loadedLargestCardIndex = 0;
	}

	public override void OnScroll(PointerEventData eventData)
	{
		_scrollRect.velocity = eventData.scrollDelta.y * _maxVelocity * Vector2.right;
	}

	public override void ScrollToTop()
	{
		ResetWidth();
		DisableAllCards();
		_cardsDirty = true;
		_pageParent.transform.localPosition = Vector2.zero;
		_currentColumn = 0;
		_loadedSmallestCardIndex = 0;
		_loadedLargestCardIndex = 0;
		InvokePageChange(0);
	}

	public override bool ScrollForCollapsedIfNeeded(PagesMetaCardView cardView)
	{
		int num = _currentColumn * base.Rows;
		int num2 = num + base.Rows * Columns;
		int num3 = _cardDisplayInfos.FindIndex((PagesMetaCardViewDisplayInformation _) => _?.Card?.TitleId == cardView.TitleId);
		if (num > num3 || num3 > num2)
		{
			_pageParent.transform.localPosition = new Vector2((float)(0.0 - (double)_cardWidth * Math.Ceiling((float)(num3 / base.Rows))), _pageParent.transform.localPosition.y);
			cardView.ForceCollapseAnimation();
			return true;
		}
		return false;
	}

	public override void SetCommanderCards(PagesMetaCardViewDisplayInformation di)
	{
	}

	public override void SetCompanionCards(PagesMetaCardViewDisplayInformation di)
	{
		_companionDisplayInfo = di;
	}

	protected override void Update()
	{
		if (_cardDisplayInfos.Count != _lastCardCount)
		{
			float x = _pageParent.anchoredPosition.x;
			UpdateLayout();
			_pageParent.anchoredPosition = new Vector2(x, _pageParent.anchoredPosition.y);
			_lastCardCount = _cardDisplayInfos.Count;
		}
	}

	private void LateUpdate()
	{
		int num;
		if (!(_lastRect != base.RectTransform.rect))
		{
			num = ((_lastRows != base.Rows) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0035;
			}
		}
		else
		{
			num = 1;
		}
		UpdateLayout();
		goto IL_0035;
		IL_0035:
		ClampVelocity();
		if (!_cardsDirty)
		{
			int num2 = CalculateCurrentColumn(_pageParent.anchoredPosition);
			if (num2 != _currentColumn && num2 >= 0)
			{
				_scrollingRight = num2 > _currentColumn;
				_currentColumn = num2;
				InvokePageChange(_currentColumn / Columns);
				_cardsDirty = true;
			}
		}
		bool cardsDirty = _cardsDirty;
		if (_cardsDirty)
		{
			UpdateCards();
		}
		if (num == 0 && !cardsDirty)
		{
			UpdateOffscreenCards();
		}
	}

	private void ClampVelocity()
	{
		if (_scrollRect.velocity.magnitude > _maxVelocity)
		{
			_scrollRect.velocity = _scrollRect.velocity.normalized * _maxVelocity;
		}
	}

	private int CalculateCurrentColumn(Vector2 anchoredPosition)
	{
		int num = (int)Math.Floor((0f - anchoredPosition.x - _startX) / _cardWidth) - 1;
		_velocityOffset = (int)Mathf.Clamp(Mathf.Floor((0f - _scrollRect.velocity.x) / _cardWidth), -Columns / 2, Columns / 2);
		return num + _velocityOffset;
	}

	private void UpdateLayout()
	{
		_cardsDirty = true;
		_lastRect = base.RectTransform.rect;
		_lastRows = base.Rows;
		_cardHeight = _lastRect.height / (float)base.Rows;
		Rect rect = _cardPrefab.RectTransform.rect;
		float num = _cardHeight / rect.height;
		_cardWidth = num * rect.width + _cardWidthShift;
		float f = _lastRect.width / _cardWidth;
		Columns = Mathf.CeilToInt(f);
		if (Columns < 1)
		{
			Columns = 1;
		}
		int num2 = base.Rows * Columns;
		_startX = _cardWidth * 0.5f + _paddingX;
		_currentColumn = 0;
		_loadedSmallestCardIndex = 0;
		_loadedLargestCardIndex = 0;
		_bufferSize = (int)Math.Floor(_bufferMultiplier * (float)num2);
		int num3 = num2 + _bufferSize * 2;
		_startY = (0f - _cardHeight) / 2f;
		_pageParent.transform.localPosition = Vector2.zero;
		ResetWidth();
		InvokePageChange(0);
		for (int i = _cardViews.Count; i < num3; i++)
		{
			PagesMetaCardView pagesMetaCardView = UnityEngine.Object.Instantiate(_cardPrefab);
			pagesMetaCardView.Holder = this;
			pagesMetaCardView.Init(base.CardDatabase, base.CardViewBuilder);
			ICardRolloverZoom rolloverZoomView = pagesMetaCardView.Holder.RolloverZoomView;
			rolloverZoomView.OnRolloverStart = (Action<ICardDataAdapter>)Delegate.Combine(rolloverZoomView.OnRolloverStart, new Action<ICardDataAdapter>(base.DismissNew));
			pagesMetaCardView.OnPrefPrintExpansionToggled = (Action<PagesMetaCardView>)Delegate.Combine(pagesMetaCardView.OnPrefPrintExpansionToggled, new Action<PagesMetaCardView>(base.CardExpansionToggled));
			pagesMetaCardView.OnPreferredPrintingToggleClicked = (Action<PagesMetaCardView, bool>)Delegate.Combine(pagesMetaCardView.OnPreferredPrintingToggleClicked, new Action<PagesMetaCardView, bool>(base.CardPreferredPrintingToggleClicked));
			_cardViews.Add(pagesMetaCardView);
		}
		for (int num4 = _cardViews.Count - 1; num4 > num3; num4--)
		{
			PagesMetaCardView pagesMetaCardView2 = _cardViews[num4];
			_cardViews.RemoveAt(num4);
			pagesMetaCardView2.Cleanup();
			UnityEngine.Object.Destroy(pagesMetaCardView2.gameObject);
		}
		for (int j = 0; j < _cardViews.Count; j++)
		{
			PagesMetaCardView pagesMetaCardView3 = _cardViews[j];
			if (j < num3)
			{
				Transform obj = pagesMetaCardView3.transform;
				obj.SetParent(_pageParent);
				obj.localScale = num * Vector3.one;
				obj.localRotation = Quaternion.Euler(Vector3.zero);
				obj.localPosition = GetCardPositionFromIndex(j);
			}
			else
			{
				pagesMetaCardView3.gameObject.UpdateActive(active: false);
				pagesMetaCardView3.transform.localScale = num * Vector3.one;
			}
		}
	}

	private Vector3 GetCardPositionFromIndex(int index)
	{
		int num = index / base.Rows;
		int num2 = index % base.Rows;
		return new Vector3(_startX + (float)num * _cardWidth, _startY + (float)num2 * (0f - _cardHeight));
	}

	private void UpdateCards()
	{
		_cardsDirty = false;
		int num = base.Rows * (Columns + 2 + Math.Abs(_velocityOffset));
		int num2 = Mathf.Max(_currentColumn - 1 - ((_velocityOffset > 0) ? _velocityOffset : 0), 0);
		for (int i = num2 * base.Rows; i < num2 * base.Rows + num; i++)
		{
			UpdateCardView(_cardViews, i);
		}
		_forceCardsLocRefresh = false;
		UpdateBounds();
	}

	private void UpdateBounds()
	{
		int num = base.Rows * Columns;
		_wantedSmallestCardIndex = Math.Max(_currentColumn * base.Rows - _bufferSize, 0);
		_wantedLargestCardIndex = _currentColumn * base.Rows + (num + _bufferSize);
		_loadedSmallestCardIndex = Math.Max(Math.Min(_loadedSmallestCardIndex, _currentColumn * base.Rows), _wantedSmallestCardIndex);
		_loadedLargestCardIndex = Math.Min(Math.Max(_loadedLargestCardIndex, _currentColumn * base.Rows + num - 1), _wantedLargestCardIndex);
	}

	private void UpdateOffscreenCards()
	{
		_offscreenLoadTimer += Time.deltaTime;
		if (!(_offscreenLoadTimer >= _offscreenLoadPulse))
		{
			return;
		}
		_offscreenLoadTimer = 0f;
		while (true)
		{
			if (_loadedLargestCardIndex < _wantedLargestCardIndex && (_scrollingRight || _loadedSmallestCardIndex <= _wantedSmallestCardIndex))
			{
				if (UpdateCardView(_cardViews, _loadedLargestCardIndex++))
				{
					break;
				}
			}
			else if (_loadedSmallestCardIndex < _wantedSmallestCardIndex || UpdateCardView(_cardViews, _loadedSmallestCardIndex--))
			{
				break;
			}
		}
	}

	private bool UpdateCardView(List<PagesMetaCardView> cardViews, int cardIndex)
	{
		bool result = false;
		PagesMetaCardView pagesMetaCardView = cardViews[cardIndex % cardViews.Count];
		if (cardIndex >= 0 && cardIndex < _cardDisplayInfos.Count)
		{
			PagesMetaCardViewDisplayInformation pagesMetaCardViewDisplayInformation = _cardDisplayInfos[cardIndex];
			result = pagesMetaCardView.UpdateDisplayInfo(pagesMetaCardViewDisplayInformation, 0, _forceCardsLocRefresh);
			pagesMetaCardView.gameObject.UpdateActive(active: true);
			pagesMetaCardView.transform.localPosition = GetCardPositionFromIndex(cardIndex);
			pagesMetaCardView.ShowCommanderFrame(pagesMetaCardViewDisplayInformation == _commanderDisplayInfo);
			pagesMetaCardView.ShowCommanderFrame(pagesMetaCardViewDisplayInformation == _partnerDisplayInfo);
			pagesMetaCardView.ShowCompanionFrame(pagesMetaCardViewDisplayInformation == _companionDisplayInfo);
		}
		else
		{
			if (pagesMetaCardView.Holder.RolloverZoomView.IsActive)
			{
				pagesMetaCardView.Holder.RolloverZoomView.CardRolledOff(pagesMetaCardView.VisualCard);
			}
			pagesMetaCardView.gameObject.UpdateActive(active: false);
		}
		return result;
	}

	private void ResetWidth()
	{
		int num = _cardDisplayInfos.Count / base.Rows + 1;
		_pageParent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _startX + (float)num * _cardWidth);
		_cardsDirty = true;
	}

	private void DisableAllCards()
	{
		foreach (PagesMetaCardView cardView in _cardViews)
		{
			if (cardView.Holder.RolloverZoomView.IsActive)
			{
				cardView.Holder.RolloverZoomView.CardRolledOff(cardView.VisualCard);
			}
			cardView.gameObject.UpdateActive(active: false);
		}
	}
}
