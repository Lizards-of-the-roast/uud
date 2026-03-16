using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_CardType : EvaluatorBase_List<CardType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<CardType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.Types, MinCount, MaxCount);
		}
		return false;
	}
}
