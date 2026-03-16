using GreClient.CardData;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;

public class StoreCardView : CDCMetaCardView
{
	[FormerlySerializedAs("_isFakeStyleCard")]
	public bool IsFakeStyleCard;

	public MetaCardHolder CardHolder;

	public ICardRolloverZoom ZoomView;

	private ScrollRect _scrollRect;

	public StoreCardView _copiedCard { get; private set; }

	protected override bool ShowHighlight => false;

	private void Start()
	{
		if (PlatformUtils.IsHandheld() && _scrollRect == null)
		{
			_scrollRect = GetComponentInParent<ScrollRect>();
		}
	}

	protected override Bounds GetBounds()
	{
		return _cardCollider?.bounds ?? new Bounds(Vector3.zero, Vector3.zero);
	}

	public void SetZoomHandler(ICardRolloverZoom zoomHandler, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		ZoomView = zoomHandler;
		if (CardHolder != null)
		{
			CardHolder.EnsureInit(cardDatabase, cardViewBuilder);
			CardHolder.RolloverZoomView = ZoomView;
			base.Holder = CardHolder;
		}
		if (_copiedCard != null)
		{
			_copiedCard.SetZoomHandler(zoomHandler, cardDatabase, cardViewBuilder);
		}
	}

	public StoreCardView CreateCard(CardData data, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, bool flipSleeved = false, bool useDim = false, bool isClickable = true, uint waitFrames = 0u)
	{
		_isClickable = isClickable;
		data.IsFakeStyleCard = IsFakeStyleCard;
		if (_copiedCard != null)
		{
			Object.Destroy(_copiedCard.gameObject);
		}
		_copiedCard = Object.Instantiate(this, base.transform.parent);
		if (CardHolder != null && ZoomView != null && !flipSleeved)
		{
			CardHolder.EnsureInit(cardDatabase, cardViewBuilder);
			CardHolder.RolloverZoomView = ZoomView;
			base.Holder = CardHolder;
			_copiedCard.Holder = CardHolder;
		}
		_copiedCard.Init(cardDatabase, cardViewBuilder);
		_copiedCard.SetDataAndWait(data, CardHolderType.Store, waitFrames);
		_copiedCard.SetScrollView(_scrollRect);
		if (useDim)
		{
			_copiedCard.CardView.SetDimmed(CDCMetaCardView.GRAY_OUT_COLOR_VALUE);
		}
		else
		{
			_copiedCard.CardView.SetDimmed(null);
		}
		_copiedCard.gameObject.SetActive(value: true);
		return _copiedCard;
	}

	public void DestroyCard()
	{
		if (_copiedCard != null)
		{
			Object.Destroy(_copiedCard.gameObject);
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (PlatformUtils.IsHandheld())
		{
			GetComponentInParent<StoreItemBase>()?.PointerExit();
		}
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		base.IsDragDetected = true;
		_scrollRect?.OnBeginDrag(eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		base.IsDragDetected = true;
		_scrollRect?.OnDrag(eventData);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		base.IsDragDetected = false;
		_scrollRect?.OnEndDrag(eventData);
	}

	public override void StartZoom()
	{
		base.StartZoom();
		if (_scrollRect != null)
		{
			_scrollRect.enabled = false;
		}
	}

	public override void CancelZoom()
	{
		if (_scrollRect != null)
		{
			_scrollRect.enabled = true;
		}
	}

	protected override void OnDestroy()
	{
		Cleanup();
	}

	protected void SetScrollView(ScrollRect scrollrect)
	{
		_scrollRect = scrollrect;
	}
}
