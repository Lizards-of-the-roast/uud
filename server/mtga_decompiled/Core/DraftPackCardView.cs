using System;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class DraftPackCardView : CDCMetaCardView
{
	public bool IgnoreRepositionNextEndDrag;

	[SerializeField]
	private GameObject _autoSelectTagObject;

	[SerializeField]
	public Button undoPickButtonObject;

	public Action<DraftPackCardView> UndoAction;

	public Collider Collider => _cardCollider;

	public ICardCollectionItem CurrentCard { get; private set; }

	public bool UseButtonOverlay { get; private set; }

	public override void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		IgnoreRepositionNextEndDrag = false;
		base.Init(cardDatabase, cardViewBuilder);
		undoPickButtonObject.onClick.AddListener(PerformUndo);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)undoPickButtonObject)
		{
			undoPickButtonObject.onClick.RemoveListener(PerformUndo);
		}
	}

	public void ActivatePickTag(bool activate)
	{
		ActivateFirstTag(activate);
		if (activate)
		{
			_autoSelectTagObject.UpdateActive(active: false);
		}
	}

	public void SetDataIncludingBans(ICardCollectionItem card)
	{
		CurrentCard = card;
		SetData(card.Card);
		SetBanDisplay(card.Card);
	}

	private void SetBanDisplay(CardData cardData)
	{
		Color? dimmed = (Pantry.Get<IEmergencyCardBansProvider>().IsTitleIdEmergencyBanned(cardData.Printing.TitleId) ? new Color?(CDCMetaCardView.BANNED_COLOR_VALUE) : ((Color?)null));
		_cardView.SetDimmed(dimmed);
	}

	private void OnDisable()
	{
		_autoSelectTagObject.UpdateActive(active: false);
		ActivateFirstTag(isFirst: false);
	}

	public void ActivateAutoSelectTag(bool activate)
	{
		_autoSelectTagObject.UpdateActive(activate);
		if (activate)
		{
			ActivateFirstTag(isFirst: false);
		}
	}

	public void ActivateUndoButton(bool activate)
	{
		UseButtonOverlay = activate;
		undoPickButtonObject.gameObject.SetActive(activate);
		_cardView.gameObject.UpdateActive(!activate);
	}

	public void PerformUndo()
	{
		if (UseButtonOverlay)
		{
			UndoAction?.Invoke(this);
		}
	}

	protected override void BeginDragCard(PointerEventData eventData)
	{
		if (!UseButtonOverlay)
		{
			base.BeginDragCard(eventData);
		}
	}

	protected override void DragCard()
	{
		if (!UseButtonOverlay)
		{
			base.DragCard();
		}
	}

	protected override void EndDragCard()
	{
		if (UseButtonOverlay || !IsCardViewEnabled())
		{
			return;
		}
		if (_cardView == null)
		{
			Debug.Log("EndDragCard: WARNING! Was not initialized.");
			return;
		}
		if (_originalLayer != -1)
		{
			_cardView.transform.SetLayerRecursive(_originalLayer);
		}
		_cardCollider.enabled = true;
		if (IgnoreRepositionNextEndDrag)
		{
			IgnoreRepositionNextEndDrag = false;
		}
		else
		{
			RestoreDragOriginalPosition(_cardView.Root);
		}
	}
}
