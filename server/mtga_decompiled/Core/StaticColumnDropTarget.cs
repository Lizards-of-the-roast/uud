using System;
using Core.Meta.Cards.Views;
using UnityEngine;
using Wizards.Mtga;

public class StaticColumnDropTarget : MetaCardHolder
{
	public bool NaturalColumnNumber;

	public float SpecificColumnNumber;

	public Texture2D PlusCursor;

	public Transform ColliderTransform;

	private bool _canDrop;

	private bool ShowPlusCursor => PlusCursor != null;

	public float CanDropOffset { get; set; }

	public void SetCards(CardCollection allCards, int grpIdForTheLastAdded = -1, DeckFormat format = null)
	{
		throw new NotImplementedException();
	}

	public override void ClearCards()
	{
		throw new NotImplementedException();
	}

	protected override void OnBeginDragCardOver(MetaCardView cardView)
	{
		if (ShowPlusCursor)
		{
			Cursor.SetCursor(PlusCursor, Vector2.zero, CursorMode.Auto);
		}
	}

	protected override void OnEndDragCardOver(MetaCardView cardView)
	{
		if (ShowPlusCursor)
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		}
	}

	public void OnEnable()
	{
		Pantry.Get<MetaCardViewDragState>().OnDragStateChange += OnDragStateChange;
	}

	public void OnDisable()
	{
		Pantry.Get<MetaCardViewDragState>().OnDragStateChange -= OnDragStateChange;
	}

	private void OnDragStateChange(MetaCardView draggingCard)
	{
		if (ColliderTransform != null && CanDropOffset != 0f)
		{
			bool flag = draggingCard != null;
			if (_canDrop != flag)
			{
				_canDrop = flag;
				Vector3 localPosition = ColliderTransform.localPosition;
				localPosition.z += (_canDrop ? 1f : (-1f)) * CanDropOffset;
				ColliderTransform.localPosition = localPosition;
			}
		}
	}

	public override DeckBuilderPile? GetParentDeckBuilderPile()
	{
		return DeckBuilderPile.MainDeck;
	}
}
