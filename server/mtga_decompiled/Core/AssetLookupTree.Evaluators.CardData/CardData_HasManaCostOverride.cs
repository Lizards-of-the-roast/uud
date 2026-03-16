using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasManaCostOverride : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		bool inValue = bb.CardData != null && bb.CardData.ManaCostOverride != null && bb.CardData.ManaCostOverride.Count > 0;
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue);
	}
}
