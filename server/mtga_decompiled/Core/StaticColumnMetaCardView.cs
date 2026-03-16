using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class StaticColumnMetaCardView : CDCMetaCardView
{
	[NonSerialized]
	public bool RecentlyCreated;

	[NonSerialized]
	public bool FrontOfColumn;

	[SerializeField]
	private GameObject _quantityObject;

	[SerializeField]
	private TMP_Text _quantityText;

	private Canvas _quantityCanvas;

	private bool _forceHideQuantity;

	private int _quantity = 1;

	public bool ForceHideQuantity
	{
		get
		{
			return _forceHideQuantity;
		}
		set
		{
			_forceHideQuantity = value;
			UpdateQuantityActive();
		}
	}

	public int Quantity
	{
		get
		{
			return _quantity;
		}
		set
		{
			_quantity = value;
			_quantityText.text = $"x{_quantity}";
			UpdateQuantityActive();
		}
	}

	private void UpdateQuantityActive()
	{
		_quantityObject.UpdateActive(_quantity > 1 && !ForceHideQuantity);
	}

	public void SetErrors(bool banned, bool unowned, bool invalid, bool suggested = false)
	{
		if (banned)
		{
			_cardView.SetDimmed(CDCMetaCardView.BANNED_COLOR_VALUE);
		}
		else if (unowned)
		{
			_cardView.SetDimmed(CDCMetaCardView.GRAY_OUT_COLOR_VALUE);
		}
		else if (suggested)
		{
			_cardView.SetDimmed(CDCMetaCardView.SUGGESTED_COLOR_VALUE);
		}
		else
		{
			_cardView.SetDimmed(null);
		}
		if (invalid && _baseHighlight != HighlightType.Invalid)
		{
			_baseHighlight = HighlightType.Invalid;
			UpdateHighlight();
		}
		else if (!invalid && _baseHighlight == HighlightType.Invalid)
		{
			_baseHighlight = HighlightType.None;
			UpdateHighlight();
		}
	}

	public override void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		base.Init(cardDatabase, cardViewBuilder);
		if (_quantityObject.transform.parent != _cardView.PartsRoot)
		{
			_quantityObject.transform.SetParent(_cardView.PartsRoot);
		}
		DisableQuantityCanvas();
		Quantity = 1;
	}

	protected override void BeginDragCard(PointerEventData eventData)
	{
		base.BeginDragCard(eventData);
		EnableQuantityCanvas();
	}

	protected override void EndDragCard()
	{
		DisableQuantityCanvas();
		base.EndDragCard();
	}

	private void EnableQuantityCanvas()
	{
		if (_quantityCanvas == null)
		{
			_quantityCanvas = _quantityObject.GetComponent<Canvas>();
			if (_quantityCanvas == null)
			{
				_quantityCanvas = _quantityObject.AddComponent<Canvas>();
			}
		}
		_quantityCanvas.overrideSorting = true;
		_quantityCanvas.sortingOrder = 5;
	}

	private void DisableQuantityCanvas()
	{
		if (_quantityCanvas != null)
		{
			_quantityCanvas.overrideSorting = false;
		}
	}
}
