using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.Player;

public class Player_AffectedByQualifications : IIndirector
{
	private QualificationData? _cache;

	public void SetCache(IBlackboard bb)
	{
		_cache = bb.Qualification;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Qualification = _cache;
		_cache = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.Player == null)
		{
			yield break;
		}
		foreach (QualificationData affectedByQualification in bb.Player.AffectedByQualifications)
		{
			bb.Qualification = affectedByQualification;
			yield return bb;
		}
	}
}
