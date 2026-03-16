using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Companions;

public class NullCompanionViewProvider : ICompanionViewProvider
{
	public static readonly ICompanionViewProvider Default = new NullCompanionViewProvider();

	public AccessoryController GetCompanionByPlayerId(uint id)
	{
		return null;
	}

	public AccessoryController GetCompanionByPlayerType(GREPlayerNum playerType)
	{
		return null;
	}

	public IEnumerable<AccessoryController> GetAllCompanions()
	{
		return Array.Empty<AccessoryController>();
	}
}
