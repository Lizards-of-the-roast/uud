using AssetLookupTree.Blackboard;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators.CardData.InstanceAndPrinting;

public class CardData_InstanceAndPrinting_HaveSameTypeLine : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Printing != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Supertypes.ContainSame(bb.CardData.Printing.Supertypes) && bb.CardData.CardTypes.ContainSame(bb.CardData.Printing.Types) && bb.CardData.Subtypes.ContainSame(bb.CardData.Printing.Subtypes));
		}
		return false;
	}
}
