using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Companions;

public class MutableCompanionViewProvider : ICompanionViewProvider
{
	public Dictionary<uint, AccessoryController> IdMap = new Dictionary<uint, AccessoryController>();

	public Dictionary<GREPlayerNum, AccessoryController> PlayerTypeMap = new Dictionary<GREPlayerNum, AccessoryController>();

	public HashSet<AccessoryController> AllCompanions = new HashSet<AccessoryController>();

	public AccessoryController GetCompanionByPlayerId(uint id)
	{
		if (!IdMap.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public AccessoryController GetCompanionByPlayerType(GREPlayerNum playerType)
	{
		if (!PlayerTypeMap.TryGetValue(playerType, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<AccessoryController> GetAllCompanions()
	{
		return AllCompanions;
	}
}
