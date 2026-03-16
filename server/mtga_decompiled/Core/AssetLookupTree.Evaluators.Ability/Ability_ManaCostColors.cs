using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_ManaCostColors : EvaluatorBase_List<ManaColor>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_List<ManaColor>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.ManaCost.Select((ManaQuantity x) => x.Color), MinCount, MaxCount);
		}
		return false;
	}
}
