using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_IsChosenCompanion : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Controller?.Designations == null)
		{
			return false;
		}
		uint titleId = bb.CardData.TitleId;
		MtgPlayer controller = bb.CardData.Controller;
		bool inValue = false;
		foreach (DesignationData designation in controller.Designations)
		{
			if (designation.Type == Designation.Companion && (!bb.CardDataProvider.TryGetCardPrintingById(designation.GrpId, out var card) || (card != null && card.TitleId == titleId)))
			{
				inValue = true;
				break;
			}
		}
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue);
	}
}
