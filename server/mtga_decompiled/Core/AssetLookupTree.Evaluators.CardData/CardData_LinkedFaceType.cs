using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_LinkedFaceType : EvaluatorBase_List<LinkedFace>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<LinkedFace>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.LinkedFaceType);
		}
		return false;
	}
}
