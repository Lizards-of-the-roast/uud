using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_LinkInfo_AffectedBy : IIndirector
{
	private LinkInfoData _cache;

	public void SetCache(IBlackboard bb)
	{
		_cache = bb.LinkInfo;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.LinkInfo = _cache;
		_cache = default(LinkInfoData);
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			yield break;
		}
		foreach (LinkInfoData affectedByLinkInfo in bb.CardData.AffectedByLinkInfos)
		{
			bb.LinkInfo = affectedByLinkInfo;
			yield return bb;
		}
	}
}
