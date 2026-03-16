using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors.Player;

public class Player_Designations : IIndirector
{
	private Designation? _cache;

	public void SetCache(IBlackboard bb)
	{
		_cache = bb.Designation;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Designation = _cache;
		_cache = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.Player == null)
		{
			yield break;
		}
		foreach (DesignationData designation in bb.Player.Designations)
		{
			bb.Designation = designation.Type;
			yield return bb;
		}
	}
}
