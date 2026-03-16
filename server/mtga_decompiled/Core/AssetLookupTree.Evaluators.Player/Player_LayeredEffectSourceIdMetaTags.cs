using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Evaluators.Player;

public class Player_LayeredEffectSourceIdMetaTags : EvaluatorBase_List<MetaDataTag>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Player == null || bb.Player.LayeredEffects == null)
		{
			return !ExpectedResult;
		}
		List<MetaDataTag> list = new List<MetaDataTag>();
		foreach (LayeredEffectData layeredEffect in bb.Player.LayeredEffects)
		{
			if (!bb.CardDatabase.AbilityDataProvider.TryGetAbilityPrintingById(layeredEffect.SourceAbilityId, out var ability))
			{
				continue;
			}
			foreach (MetaDataTag tag in ability.Tags)
			{
				list.Add(tag);
			}
		}
		return EvaluatorBase_List<MetaDataTag>.GetResult(ExpectedValues, Operation, ExpectedResult, list, MinCount, MaxCount);
	}
}
