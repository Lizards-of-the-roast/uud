using System;

namespace Wotc.Mtga.DuelScene;

public class PlayerPresenceMediator : IDisposable
{
	private readonly IPlayerPresenceController _playerPresenceController;

	private readonly UIMessageHandler _uiMessageHandler;

	public PlayerPresenceMediator(UIMessageHandler uiMessageHandler, IPlayerPresenceController playerPresenceController)
	{
		_uiMessageHandler = uiMessageHandler;
		_playerPresenceController = playerPresenceController ?? NullPlayerPresenceController.Default;
		_uiMessageHandler.CardHoverChanged += _playerPresenceController.SetHoveredCardId;
	}

	public void Dispose()
	{
		_uiMessageHandler.CardHoverChanged -= _playerPresenceController.SetHoveredCardId;
	}
}
