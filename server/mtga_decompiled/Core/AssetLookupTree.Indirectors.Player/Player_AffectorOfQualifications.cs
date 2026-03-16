using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.Player;

public class Player_AffectorOfQualifications : IIndirector
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
		foreach (QualificationData affectorOfQualification in bb.Player.AffectorOfQualifications)
		{
			bb.Qualification = affectorOfQualification;
			yield return bb;
		}
	}
}
