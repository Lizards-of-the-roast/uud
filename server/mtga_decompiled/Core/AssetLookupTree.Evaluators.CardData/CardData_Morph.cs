using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Morph : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Instance != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Instance.FaceDownState.ReasonFaceDown == ReasonFaceDown.Morph);
		}
		return false;
	}
}
