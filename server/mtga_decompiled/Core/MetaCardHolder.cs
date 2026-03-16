using System;
using System.Collections.Generic;
using Core.Meta.Cards.Views;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public abstract class MetaCardHolder : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IDropHandler
{
	protected ICardRolloverZoom _rolloverZoomView;

	public GameObject HighlightObject;

	public ScrollRect ScrollRect;

	public float RequiredHoverTimeBeforeShowRollover;

	public float TimeToTurnOffRolloverAfterMouseOff;

	public bool SendDragEventsUp;

	private bool _initialized;

	private bool _isPointerOver;

	private readonly List<MetaCardView> _draggingCards = new List<MetaCardView>();

	public ICardRolloverZoom RolloverZoomView
	{
		get
		{
			return _rolloverZoomView;
		}
		set
		{
			_rolloverZoomView = value;
			if (_rolloverZoomView != null)
			{
				_rolloverZoomView.IsActive = true;
			}
		}
	}

	protected bool IsPointerOver
	{
		get
		{
			return _isPointerOver;
		}
		private set
		{
			if (_isPointerOver != value)
			{
				_isPointerOver = value;
				OnIsPointerOverChanged(_isPointerOver);
				OnHoverOrDragChanged();
			}
		}
	}

	private bool IsHoveringAndDragging
	{
		get
		{
			if (_isPointerOver)
			{
				return IsDragging;
			}
			return false;
		}
	}

	public bool IsDragging => _draggingCards.Count > 0;

	public Func<MetaCardView, bool> CanSingleClickCards { get; set; }

	public Func<MetaCardView, bool> CanDoubleClickCards { get; set; }

	public Func<MetaCardView, bool> CanDragCards { get; set; }

	public Func<MetaCardView, bool> CanDropCards { get; set; }

	public Func<MetaCardView, bool> ShowHighlight { get; set; }

	public Action<MetaCardView> OnCardClicked { get; set; }

	public Action<MetaCardView> OnCardRightClicked { get; set; }

	public Action<MetaCardView> OnCardDragged { get; set; }

	public Action<MetaCardView> OnEndCardDragged { get; set; }

	public Action<MetaCardView, MetaCardHolder> OnCardDropped { get; set; }

	public Action<MetaCardView, bool, bool, bool> CustomHighlightHandler { get; set; }

	public CardDatabase CardDatabase { get; private set; }

	public CardViewBuilder CardViewBuilder { get; private set; }

	public bool Default_CanSingleClickCards(MetaCardView cardView)
	{
		return false;
	}

	public bool Default_CanDoubleClickCards(MetaCardView cardView)
	{
		return false;
	}

	public bool Default_CanDragCards(MetaCardView cardView)
	{
		return false;
	}

	public bool Default_CanDropCards(MetaCardView cardView)
	{
		return false;
	}

	public bool Default_ShowHighlight(MetaCardView cardView)
	{
		if ((CanDragCards != null && CanDragCards(cardView)) || OnCardClicked != null || OnCardRightClicked != null || RolloverZoomView != null)
		{
			return true;
		}
		return false;
	}

	private void OnHoverOrDragChanged()
	{
		if (!IsHoveringAndDragging && HighlightObject != null && HighlightObject.activeSelf)
		{
			HighlightObject.SetActive(value: false);
		}
	}

	protected virtual void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		CardDatabase = cardDatabase;
		CardViewBuilder = cardViewBuilder;
		CanSingleClickCards = Default_CanSingleClickCards;
		CanDoubleClickCards = Default_CanDoubleClickCards;
		CanDragCards = Default_CanDragCards;
		CanDropCards = Default_CanDropCards;
		ShowHighlight = Default_ShowHighlight;
	}

	protected virtual void Activate(bool active)
	{
	}

	public abstract void ClearCards();

	protected virtual void OnIsPointerOverChanged(bool isOver)
	{
	}

	protected virtual void OnBeginDragCardOver(MetaCardView cardView)
	{
	}

	protected virtual void OnEndDragCardOver(MetaCardView cardView)
	{
	}

	public virtual void EnsureInit(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		if (!_initialized)
		{
			_initialized = true;
			if (HighlightObject != null)
			{
				HighlightObject.SetActive(value: false);
			}
			Init(cardDatabase, cardViewBuilder);
		}
	}

	public void SetActive(bool active)
	{
		if (base.gameObject.activeSelf != active)
		{
			base.gameObject.SetActive(active);
		}
		Activate(active);
	}

	public void SetScrollEnabled(bool enabled)
	{
		if (ScrollRect != null)
		{
			ScrollRect.enabled = enabled;
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus && IsPointerOver)
		{
			IsPointerOver = false;
			HighlightObject.UpdateActive(active: false);
			OnEndDragCardOver(Pantry.Get<MetaCardViewDragState>().DraggingCard);
		}
		else if (focus && !IsPointerOver && base.transform.RaycastIsPointerOver())
		{
			IsPointerOver = true;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		IsPointerOver = true;
		if (OnCardDropped != null)
		{
			MetaCardView draggingCard = MetaCardUtility.GetDraggingCard(eventData);
			if (!(draggingCard == null) && CanDropCards(draggingCard) && !(draggingCard.Holder == this) && draggingCard.Card != null)
			{
				HighlightObject.UpdateActive(active: true);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_card_rollover, base.gameObject);
				OnBeginDragCardOver(draggingCard);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsPointerOver = false;
		MetaCardView draggingCard = MetaCardUtility.GetDraggingCard(eventData);
		if (!(draggingCard == null))
		{
			Func<MetaCardView, bool> canDropCards = CanDropCards;
			if (canDropCards != null && canDropCards(draggingCard))
			{
				HighlightObject.UpdateActive(active: false);
				OnEndDragCardOver(draggingCard);
			}
		}
	}

	public virtual void OnDrop(PointerEventData eventData)
	{
		if (eventData.ConfirmOnlyButtonPressed(PointerEventData.InputButton.Left) && OnCardDropped != null)
		{
			MetaCardView draggingCard = MetaCardUtility.GetDraggingCard(eventData);
			if (!(draggingCard == null) && !(draggingCard.Holder == this) && draggingCard.Card != null && CanDropCards(draggingCard) && draggingCard.Holder.CanDragCards(draggingCard))
			{
				HighlightObject.SetActive(value: false);
				OnEndDragCardOver(draggingCard);
				OnCardDropped(draggingCard, this);
			}
		}
	}

	public void SetDragging(MetaCardView draggingCard)
	{
		_draggingCards.Add(draggingCard);
		OnHoverOrDragChanged();
	}

	public void ReleaseDragging(MetaCardView draggingCard)
	{
		_draggingCards.Remove(draggingCard);
		OnHoverOrDragChanged();
	}

	public void ReleaseAllDraggingCards()
	{
		if (!IsDragging)
		{
			return;
		}
		foreach (MetaCardView item in new List<MetaCardView>(_draggingCards))
		{
			item.OnEndDrag(new PointerEventData(EventSystem.current));
		}
		_draggingCards.Clear();
		OnHoverOrDragChanged();
	}

	public virtual DeckBuilderPile? GetParentDeckBuilderPile()
	{
		return null;
	}
}
