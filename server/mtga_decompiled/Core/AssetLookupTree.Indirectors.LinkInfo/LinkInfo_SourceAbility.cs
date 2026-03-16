using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Indirectors.LinkInfo;

public class LinkInfo_SourceAbility : IIndirector
{
	private AbilityPrintingData _cacheAbility;

	public void SetCache(IBlackboard bb)
	{
		_cacheAbility = bb.Ability;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Ability = _cacheAbility;
		_cacheAbility = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.AbilityDataProvider != null && bb.LinkInfo.SourceAbilityId != 0 && bb.AbilityDataProvider.TryGetAbilityPrintingById(bb.LinkInfo.SourceAbilityId, out var ability))
		{
			bb.Ability = ability;
			yield return bb;
		}
	}
}
