using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_Qualifications : IIndirector
{
	private QualificationData? _cacheQualificationData;

	public void SetCache(IBlackboard bb)
	{
		_cacheQualificationData = bb.Qualification;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Qualification = _cacheQualificationData;
		_cacheQualificationData = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.AffectedByQualifications.Count != 0)
		{
			for (int i = 0; i < bb.CardData.AffectedByQualifications.Count; i++)
			{
				bb.Qualification = bb.CardData.AffectedByQualifications[i];
				yield return bb;
			}
		}
	}
}
