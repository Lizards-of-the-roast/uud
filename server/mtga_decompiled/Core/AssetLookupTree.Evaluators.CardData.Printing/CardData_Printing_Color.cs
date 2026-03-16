using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_Color : EvaluatorBase_List<CardColor>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<CardColor>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.Colors, MinCount, MaxCount);
		}
		return false;
	}
}
