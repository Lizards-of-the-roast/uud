using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_CopiedCardReasonFaceDown : EvaluatorBase_List<ReasonFaceDown>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<ReasonFaceDown>.GetResult(ExpectedValues, Operation, ExpectedResult, (bb.CardData?.Instance?.FaceDownState.CopiedCardsReasonFaceDown).GetValueOrDefault());
	}
}
