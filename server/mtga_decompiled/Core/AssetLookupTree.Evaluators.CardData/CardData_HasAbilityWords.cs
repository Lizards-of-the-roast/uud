using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasAbilityWords : EvaluatorBase_List<AbilityWord>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<AbilityWord>.GetResult(ExpectedValues, Operation, ExpectedResult, from x in bb.CardData.Abilities
				where x.AbilityWord != AbilityWord.None
				select x.AbilityWord, MinCount, MaxCount);
		}
		return false;
	}
}
