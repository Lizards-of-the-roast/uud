using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_Toughness : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		ICardDataAdapter cardData = bb.CardData;
		if (cardData != null)
		{
			CardPrintingData printing = cardData.Printing;
			if (printing != null && printing.Toughness.DefinedValue.HasValue)
			{
				return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.CardData.Printing.Toughness.DefinedValue.Value);
			}
		}
		return false;
	}
}
