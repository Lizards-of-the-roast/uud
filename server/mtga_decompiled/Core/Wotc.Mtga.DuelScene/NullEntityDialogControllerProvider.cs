using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullEntityDialogControllerProvider : IEntityDialogControllerProvider
{
	public static readonly IEntityDialogControllerProvider Default = new NullEntityDialogControllerProvider();

	public EntityDialogController GetDialogControllerByPlayerType(GREPlayerNum playerType)
	{
		return null;
	}

	public EntityDialogController GetDialogControllerById(uint id)
	{
		return null;
	}

	public IEnumerable<EntityDialogController> GetAllDialogControllers()
	{
		return Array.Empty<EntityDialogController>();
	}
}
