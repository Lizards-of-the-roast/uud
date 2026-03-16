using System;

namespace Wotc.Mtga.DuelScene;

public class CardDragStartedMediator : IDisposable
{
	private readonly CardHoverController _hoverController;

	private readonly CardDragController _dragController;

	public CardDragStartedMediator(CardHoverController hoverController, CardDragController dragController)
	{
		_hoverController = hoverController;
		_dragController = dragController;
		_dragController.CardDragStarted += _hoverController.OnCardBeginDrag;
	}

	public void Dispose()
	{
		_dragController.CardDragStarted -= _hoverController.OnCardBeginDrag;
	}
}
