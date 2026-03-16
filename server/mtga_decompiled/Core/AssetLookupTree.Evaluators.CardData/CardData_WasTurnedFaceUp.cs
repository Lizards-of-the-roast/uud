using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_WasTurnedFaceUp : EvaluatorBase_List<ReasonFaceDown>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			return EvaluatorBase_List<ReasonFaceDown>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Instance.FaceDownState.WasTurnedFaceUpFrom);
		}
		return false;
	}
}
