using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Indirectors.Action;

public class Action_TimingSourceAbility : IIndirector
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
		if (bb.GreAction != null && bb.AbilityDataProvider.TryGetAbilityPrintingById(bb.GreAction.TimingSourceGrpid, out var ability))
		{
			bb.Ability = ability;
			yield return bb;
		}
	}
}
