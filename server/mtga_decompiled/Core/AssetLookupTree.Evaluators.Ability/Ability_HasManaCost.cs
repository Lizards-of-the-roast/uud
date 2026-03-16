using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_HasManaCost : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		bool expectedResult = ExpectedResult;
		AbilityPrintingData ability = bb.Ability;
		return EvaluatorBase_Boolean.GetResult(expectedResult, ability != null && ability.ManaCost?.Count > 0);
	}
}
