using AssetLookupTree.Blackboard;
using Wotc.Mtga.Cards;

namespace AssetLookupTree.Evaluators.CardData;

public class MouseOver : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.MouseOverType != MouseOverType.None);
	}
}
