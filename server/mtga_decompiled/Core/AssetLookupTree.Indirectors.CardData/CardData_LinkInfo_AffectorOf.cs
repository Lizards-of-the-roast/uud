using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Indirectors.CardData;

public class CardData_LinkInfo_AffectorOf : IIndirector
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
		foreach (LinkInfoData affectorOfLinkInfo in bb.CardData.AffectorOfLinkInfos)
		{
			bb.LinkInfo = affectorOfLinkInfo;
			yield return bb;
		}
	}
}
