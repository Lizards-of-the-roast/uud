using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Companions;

public interface ICompanionViewProvider
{
	AccessoryController GetCompanionByPlayerId(uint id);

	AccessoryController GetCompanionByPlayerType(GREPlayerNum playerType);

	IEnumerable<AccessoryController> GetAllCompanions();

	bool TryGetCompanionByPlayerType(GREPlayerNum playerType, out AccessoryController view)
	{
		view = GetCompanionByPlayerType(playerType);
		return view != null;
	}
}
