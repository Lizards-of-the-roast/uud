using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_NumericAid : EvaluatorBase_List<NumericAid>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_List<NumericAid>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.NumericAid);
		}
		return false;
	}
}
