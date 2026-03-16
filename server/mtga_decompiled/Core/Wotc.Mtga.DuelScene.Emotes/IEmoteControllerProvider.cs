using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Emotes;

public interface IEmoteControllerProvider
{
	IEmoteController GetEmoteControllerByPlayerType(GREPlayerNum playerType);

	IEmoteController GetEmoteControllerById(uint id);

	IEnumerable<IEmoteController> GetAllEmoteControllers();

	bool TryGetEmoteControllerByPlayerType(GREPlayerNum playerType, out IEmoteController dialogController)
	{
		dialogController = GetEmoteControllerByPlayerType(playerType);
		return dialogController != null;
	}

	bool TryEmoteControllerById(uint id, out IEmoteController dialogController)
	{
		dialogController = GetEmoteControllerById(id);
		return dialogController != null;
	}
}
