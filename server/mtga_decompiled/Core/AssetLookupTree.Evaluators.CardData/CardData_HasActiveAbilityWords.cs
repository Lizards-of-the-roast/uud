using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasActiveAbilityWords : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.ActiveAbilityWords.Select((AbilityWordData x) => x.AbilityWord), MinCount, MaxCount);
		}
		return false;
	}
}
