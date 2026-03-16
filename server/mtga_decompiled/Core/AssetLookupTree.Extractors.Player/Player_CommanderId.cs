using System.Collections.Generic;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Player;

public class Player_CommanderId : IExtractor<List<uint>>
{
	public bool Execute(IBlackboard bb, out List<uint> value)
	{
		if (bb.Player != null)
		{
			value = bb.Player.CommanderIds;
			return value != null;
		}
		value = null;
		return false;
	}
}
