using AssetLookupTree.Blackboard;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators.CardData.InstanceAndPrinting;

public class CardData_InstanceAndPrinting_HaveSameSubTypes : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Printing != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Subtypes.ContainSame(bb.CardData.Printing.Subtypes));
		}
		return false;
	}
}
