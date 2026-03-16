using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_FaceDownVisualCard : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance == null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue: false);
		}
		bool flag = bb.CardData.Instance.FaceDownState.IsFaceDown || bb.CardData.Instance.FaceDownState.IsCopiedFaceDown;
		bool flag2 = !bb.CardData.IsDisplayedFaceDown;
		bool flag3 = bb.CardData.ZoneType != ZoneType.Exile;
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, flag && flag2 && flag3);
	}
}
