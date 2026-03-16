using System;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Extensions;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Printing_RelativeLinkedPrintingIndex : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		ICardDataAdapter cardData = bb.CardData;
		if (cardData == null)
		{
			return false;
		}
		CardPrintingData printing = cardData.Printing;
		if (printing == null)
		{
			return false;
		}
		int num = -1;
		foreach (CardPrintingData item in printing.LinkedFacePrintings ?? Array.Empty<CardPrintingData>())
		{
			num = item.LinkedFacePrintings.FindIndex(printing, (CardPrintingData x, CardPrintingData cardPrintingData) => x.GrpId == cardPrintingData.GrpId);
			if (num != -1)
			{
				break;
			}
		}
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, num);
	}
}
