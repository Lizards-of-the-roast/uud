using System;
using Core.Meta.Cards.Views;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.GeneralUtilities;
using Wizards.Mtga;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;

public abstract class MetaCardView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IScrollHandler
{
	private enum PointerZone
	{
		Entered,
		EnteredButOutsideCDC,
		Exited
	}

	[SerializeField]
	private float _dragZOffset;

	[SerializeField]
	private Vector2 _popupOffset;

	[SerializeField]
	public Vector2 _maxDragOffset = new Vector2(50f, 50f);

	[SerializeField]
	public Vector2 _minDragOffset = new Vector2(-50f, -50f);

	private CardData _cardData;

	private CardData _visualCard;

	private PointerZone _lastPointerZone = PointerZone.Exited;

	public bool ShowUnCollectedTreatment;

	public bool ShowBannedTreatment;

	public bool ShowInvalidTreatment;

	public bool ShowDisabledTreatment;

	public HangerSituation HangerSituation;

	public bool AllowRollOver = true;

	private float _pendingRolloverOnTimer;

	private float _pendingRollverOffTimer;

	private float _scrollDelay;

	protected bool _isClickable = true;

	public bool SendDragEventsUp;

	private Vector3 _originalPosition;

	private Vector3 _originalScale;

	private Quaternion _originalRotation;

	private Vector3 _dragOffset;

	private Vector2 _angle = Vector2.zero;

	private Vector2 _prevScreenPosition;

	private PointerEventData _lastPointerData;

	public MetaCardHolder Holder { get; set; }

	public CardData Card
	{
		get
		{
			return _cardData;
		}
		set
		{
			_cardData = value;
		}
	}

	public CardData VisualCard
	{
		get
		{
			return _visualCard ?? _cardData;
		}
		set
		{
			_visualCard = value;
		}
	}

	protected virtual bool UsesOutsideCdcZone => false;

	protected bool IsMouseDown { get; set; }

	protected bool IsMouseOver { get; set; }

	protected bool IsDragging { get; set; }

	public bool IsDragDetected { get; protected set; }

	private MetaCardViewDragState DragState => Pantry.Get<MetaCardViewDragState>();

	protected MetaCardView DraggingCard
	{
		get
		{
			return DragState.DraggingCard;
		}
		set
		{
			DragState.DraggingCard = value;
		}
	}

	public Action<MetaCardView> OnClicked { get; set; }

	protected virtual bool ShowHighlight => true;

	private static Vector2 MousePointScreen => Input.mousePosition;

	private Camera MousePointCamera => CurrentCamera.Value;

	private Vector3 MousePointWorld => MousePointCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));

	protected abstract void UpdateHighlight();

	protected abstract Bounds GetBounds();

	protected abstract void BeginDragCard(PointerEventData eventData);

	protected abstract void DragCard();

	protected abstract void EndDragCard();

	private void Update()
	{
		if (_pendingRolloverOnTimer > 0f)
		{
			_pendingRolloverOnTimer -= Time.deltaTime;
			if (_pendingRolloverOnTimer < 0f)
			{
				Holder.RolloverZoomView.CardRolledOver(VisualCard, GetBounds(), HangerSituation);
				_pendingRolloverOnTimer = 0f;
			}
		}
		if (_pendingRollverOffTimer > 0f)
		{
			_pendingRollverOffTimer -= Time.deltaTime;
			if (_pendingRollverOffTimer < 0f)
			{
				Holder.RolloverZoomView.CardRolledOff(VisualCard);
				_pendingRollverOffTimer = 0f;
			}
		}
		if (_scrollDelay > 0f)
		{
			_scrollDelay -= Time.deltaTime;
		}
		if (IsDragging)
		{
			DragCard();
		}
		if (UsesOutsideCdcZone && _lastPointerZone == PointerZone.EnteredButOutsideCDC)
		{
			PointerEventData lastPointerEventData = CustomInputModule.GetLastPointerEventData();
			if (lastPointerEventData != null && !IsOutsideCardCDC(lastPointerEventData))
			{
				OnPointerEnter(lastPointerEventData);
			}
		}
		else if (UsesOutsideCdcZone && _lastPointerZone == PointerZone.Entered)
		{
			PointerEventData lastPointerEventData2 = CustomInputModule.GetLastPointerEventData();
			if (IsOutsideCardCDC(lastPointerEventData2))
			{
				OnPointerExit(lastPointerEventData2);
				_lastPointerZone = PointerZone.EnteredButOutsideCDC;
			}
		}
		if (ShowHighlight)
		{
			UpdateHighlight();
		}
	}

	protected virtual bool IsOutsideCardCDC(PointerEventData eventData)
	{
		return false;
	}

	public virtual bool IsCardViewEnabled()
	{
		return true;
	}

	public virtual void SetSelected(bool isSelected)
	{
	}

	protected virtual void OnDestroy()
	{
		if (DraggingCard == this)
		{
			DraggingCard = null;
			EndDragCard();
		}
		if (Holder != null)
		{
			Holder.ReleaseDragging(this);
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus && IsMouseDown)
		{
			PointerEventData lastPointerEventData = CustomInputModule.GetLastPointerEventData();
			if (lastPointerEventData != null)
			{
				OnPointerUp(lastPointerEventData);
				lastPointerEventData.pointerDrag = null;
			}
		}
		if (DraggingCard == this)
		{
			OnEndDrag(new PointerEventData(EventSystem.current));
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (eventData.dragging)
		{
			return;
		}
		if (UsesOutsideCdcZone && IsOutsideCardCDC(eventData))
		{
			_lastPointerZone = PointerZone.EnteredButOutsideCDC;
			return;
		}
		_lastPointerZone = PointerZone.Entered;
		if (!IsCardViewEnabled() || Holder == null || !AllowRollOver)
		{
			return;
		}
		IsMouseOver = true;
		_scrollDelay = 0.25f;
		if (Holder.RolloverZoomView == null)
		{
			return;
		}
		if (Holder.RolloverZoomView.LastRolloverModel == VisualCard)
		{
			_pendingRollverOffTimer = 0f;
			return;
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_card_rollover, base.gameObject);
		_pendingRolloverOnTimer = Holder.RequiredHoverTimeBeforeShowRollover;
		if (_pendingRolloverOnTimer <= 0f)
		{
			Holder.RolloverZoomView.CardRolledOver(VisualCard, GetBounds(), HangerSituation, _popupOffset);
			_pendingRolloverOnTimer = 0f;
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		IsMouseOver = false;
		_lastPointerZone = PointerZone.Exited;
		if (!IsCardViewEnabled())
		{
			return;
		}
		_pendingRolloverOnTimer = 0f;
		if (Holder != null && Holder.RolloverZoomView != null)
		{
			_pendingRollverOffTimer = Holder.TimeToTurnOffRolloverAfterMouseOff;
			if (_pendingRollverOffTimer <= 0f)
			{
				Holder.RolloverZoomView.CardRolledOff(VisualCard);
				_pendingRollverOffTimer = 0f;
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!_isClickable)
		{
			eventData.PassEventToNextClickableItem(base.gameObject);
		}
		else
		{
			if (!IsCardViewEnabled() || eventData.dragging)
			{
				return;
			}
			OnClicked?.Invoke(this);
			if (Holder == null)
			{
				return;
			}
			IsDragDetected = false;
			Action<MetaCardView> action;
			if (Holder.OnCardClicked != null && eventData.ConfirmOnlyButtonPressed(PointerEventData.InputButton.Left))
			{
				action = Holder.OnCardClicked;
			}
			else
			{
				if (Holder.OnCardRightClicked == null || !eventData.ConfirmOnlyButtonPressed(PointerEventData.InputButton.Right))
				{
					return;
				}
				action = Holder.OnCardRightClicked;
			}
			if (Holder.CanSingleClickCards(this) || (Holder.CanDoubleClickCards(this) && eventData.clickCount % 2 == 0))
			{
				action(this);
			}
		}
	}

	public virtual void OnBeginDrag(PointerEventData eventData)
	{
		if (!IsCardViewEnabled() || (bool)DraggingCard)
		{
			return;
		}
		if (!IsMouseDown)
		{
			eventData.pointerDrag = null;
			eventData.dragging = false;
			return;
		}
		IsDragDetected = true;
		SendEventDataUpwards(eventData, "OnBeginDrag");
		if (!(Holder == null) && Holder.CanDragCards(this) && eventData.ConfirmOnlyButtonPressed(PointerEventData.InputButton.Left))
		{
			IsDragging = true;
			DraggingCard = this;
			Holder.OnCardDragged?.Invoke(this);
			if (Holder.RolloverZoomView != null)
			{
				Holder.RolloverZoomView.Close();
			}
			Holder.SetDragging(this);
			AudioManager.PlayAudio((this is DraftPackCardView) ? WwiseEvents.sfx_ui_deckbuilding_card_pickup : WwiseEvents.sfx_ui_main_pull_card, base.gameObject);
			BeginDragCard(eventData);
		}
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
		IsDragDetected = true;
		_lastPointerData = eventData;
		SendEventDataUpwards(eventData, "OnDrag");
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
		IsDragDetected = false;
		SendEventDataUpwards(eventData, "OnEndDrag");
		if (Holder == null || !Holder.CanDragCards(this) || !eventData.ConfirmOnlyButtonPressed(PointerEventData.InputButton.Left))
		{
			return;
		}
		_lastPointerData = null;
		eventData.pointerDrag = null;
		eventData.dragging = false;
		IsDragging = false;
		DraggingCard = null;
		if (IsCardViewEnabled())
		{
			Holder.OnEndCardDragged?.Invoke(this);
			Holder.ReleaseDragging(this);
			if (this is DraftPackCardView)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_deckbuilding_card_place, base.gameObject);
			}
			else
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_return_card, base.gameObject);
			}
			EndDragCard();
		}
	}

	private void SendEventDataUpwards(PointerEventData eventData, string message)
	{
		bool num = SendDragEventsUp || (Holder != null && Holder.SendDragEventsUp);
		bool flag = base.transform.parent != null;
		if (num && flag)
		{
			base.transform.parent.SendMessageUpwards(message, eventData, SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void StartZoom()
	{
		IsDragDetected = false;
		if (_lastPointerData != null)
		{
			_lastPointerData.pointerDrag = null;
			_lastPointerData.dragging = false;
		}
	}

	public virtual void CancelZoom()
	{
	}

	protected void StoreDragFields(PointerEventData eventData, Transform trans)
	{
		_originalPosition = trans.localPosition;
		_originalScale = trans.localScale;
		_originalRotation = trans.localRotation;
		Vector3 position = trans.position;
		float z = eventData.pressEventCamera.WorldToScreenPoint(position).z;
		Vector3 vector = position - eventData.pressEventCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, z));
		_dragOffset = new Vector3(Mathf.Clamp(vector.x, _minDragOffset.x, _maxDragOffset.x), Mathf.Clamp(vector.y, _minDragOffset.y, _maxDragOffset.y), _dragZOffset);
	}

	protected void ApplyDragFields(Transform trans)
	{
		Vector2 vector = MousePointScreen - _prevScreenPosition;
		_angle += vector * 1f;
		_angle = Vector2.Lerp(_angle, Vector2.zero, 5f * Time.deltaTime);
		_angle.x = Mathf.Clamp(_angle.x, -15f, 15f);
		_angle.y = Mathf.Clamp(_angle.y, -15f, 15f);
		float t = Time.deltaTime * 40f;
		Vector3 mousePointWorld = MousePointWorld;
		trans.position = Vector3.Lerp(trans.position, new Vector3(mousePointWorld.x, mousePointWorld.y, _originalPosition.z) + _dragOffset, t);
		trans.localScale = Vector3.Lerp(trans.localScale, _originalScale * 1.2f, t);
		trans.rotation = Quaternion.Lerp(trans.rotation, Quaternion.Euler(_angle.y, 0f - _angle.x, 0f), t);
		_prevScreenPosition = MousePointScreen;
	}

	protected void RestoreDragOriginalPosition(Transform trans)
	{
		trans.localPosition = _originalPosition;
		trans.localScale = _originalScale;
		trans.localRotation = _originalRotation;
	}

	public virtual void OnPointerDown(PointerEventData eventData)
	{
		IsMouseDown = true;
		IsDragDetected = false;
		if (Holder != null && Holder.RolloverZoomView != null)
		{
			Holder.RolloverZoomView.CardPointerDown(eventData.button, VisualCard, this, HangerSituation);
		}
		eventData.Use();
	}

	public virtual void OnPointerUp(PointerEventData eventData)
	{
		IsMouseDown = false;
		IsDragDetected = false;
		if (Holder != null && Holder.RolloverZoomView != null)
		{
			Holder.RolloverZoomView.CardPointerUp(eventData.button, VisualCard, this);
		}
		eventData.Use();
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (Holder != null && Holder.RolloverZoomView != null && _scrollDelay <= 0f && Holder.RolloverZoomView.CardScrolled(eventData.scrollDelta))
		{
			eventData.Use();
		}
		else if (base.transform.parent != null)
		{
			base.transform.parent.SendMessageUpwards("OnScroll", eventData, SendMessageOptions.DontRequireReceiver);
		}
	}
}
