using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_CanShowDynamicFrame : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, DynamicCardFrameUtil.CanShow(bb.CardData, bb.GameState, bb.MouseOverType, bb.CardHolderType, bb.IsHoverCopy));
		}
		return false;
	}
}
