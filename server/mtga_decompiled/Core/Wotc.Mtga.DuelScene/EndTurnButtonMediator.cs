using System;

namespace Wotc.Mtga.DuelScene;

public class EndTurnButtonMediator : IDisposable
{
	private readonly EndTurnButton _endTurnButton;

	private readonly CardDragController _cardDragController;

	private readonly AutoResponseManager _autoResponseManager;

	public EndTurnButtonMediator(EndTurnButton endTurnButton, CardDragController cardDragController, AutoResponseManager autoResponseManager)
	{
		_endTurnButton = endTurnButton;
		_cardDragController = cardDragController;
		_autoResponseManager = autoResponseManager;
		_endTurnButton.Clicked += _cardDragController.EndDrag;
		_endTurnButton.Clicked += _autoResponseManager.ToggleAutoPass;
		_autoResponseManager.SettingsUpdated += _endTurnButton.OnSettingsUpdated;
	}

	public void Dispose()
	{
		_autoResponseManager.SettingsUpdated -= _endTurnButton.OnSettingsUpdated;
		_endTurnButton.Clicked -= _autoResponseManager.ToggleAutoPass;
		_endTurnButton.Clicked -= _cardDragController.EndDrag;
	}
}
