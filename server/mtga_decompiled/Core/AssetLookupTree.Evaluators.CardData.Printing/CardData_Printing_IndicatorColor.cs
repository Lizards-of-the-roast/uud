using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_IndicatorColor : EvaluatorBase_List<CardColor>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing?.IndicatorColors != null)
		{
			return EvaluatorBase_List<CardColor>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.IndicatorColors, MinCount, MaxCount);
		}
		return false;
	}
}
