using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface IEntityDialogControllerProvider
{
	EntityDialogController GetDialogControllerByPlayerType(GREPlayerNum playerType);

	EntityDialogController GetDialogControllerById(uint id);

	IEnumerable<EntityDialogController> GetAllDialogControllers();

	bool TryGetDialogControllerByPlayerType(GREPlayerNum playerType, out EntityDialogController dialogController)
	{
		dialogController = GetDialogControllerByPlayerType(playerType);
		return dialogController != null;
	}

	bool TryGetDialogControllerById(uint id, out EntityDialogController dialogController)
	{
		dialogController = GetDialogControllerById(id);
		return dialogController != null;
	}
}
