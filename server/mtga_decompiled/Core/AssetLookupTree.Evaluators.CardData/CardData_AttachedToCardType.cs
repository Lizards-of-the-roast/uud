using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_AttachedToCardType : EvaluatorBase_List<CardType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null || bb.CardData.Instance == null || bb.GameState == null || bb.CardData.Instance.AttachedToId == 0)
		{
			return !ExpectedResult;
		}
		if (!bb.GameState.TryGetEntity(bb.CardData.Instance.AttachedToId, out var mtgEntity))
		{
			return !ExpectedResult;
		}
		if (mtgEntity is MtgCardInstance instance)
		{
			GreClient.CardData.CardData cardData = CardDataExtensions.CreateWithDatabase(instance, bb.CardDatabase);
			return EvaluatorBase_List<CardType>.GetResult(ExpectedValues, Operation, ExpectedResult, cardData.CardTypes, MinCount, MaxCount);
		}
		return !ExpectedResult;
	}
}
