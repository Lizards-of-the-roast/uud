using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_PendingSpecializationColor : EvaluatorBase_List<CardColor>
{
	public override bool Execute(IBlackboard bb)
	{
		ICardDataAdapter cardData = bb.CardData;
		if (cardData == null)
		{
			return false;
		}
		MtgCardInstance instance = cardData.Instance;
		if (!instance.HasPendingSpecialization())
		{
			return false;
		}
		CardColor? pendingSpecializationColor = instance.PendingSpecializationColor;
		if (!pendingSpecializationColor.HasValue)
		{
			return false;
		}
		return EvaluatorBase_List<CardColor>.GetResult(ExpectedValues, Operation, ExpectedResult, pendingSpecializationColor.Value);
	}
}
