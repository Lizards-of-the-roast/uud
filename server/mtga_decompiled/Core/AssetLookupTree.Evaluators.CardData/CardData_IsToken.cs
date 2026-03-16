using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_IsToken : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		if (bb.CardData.Instance == null)
		{
			if (bb.CardData.Printing != null)
			{
				return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Printing.IsToken);
			}
			return false;
		}
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Instance.ObjectType == GameObjectType.Token);
	}
}
