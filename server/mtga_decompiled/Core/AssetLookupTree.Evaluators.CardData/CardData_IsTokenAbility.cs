using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_IsTokenAbility : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue: false);
		}
		if (bb.CardData.Instance == null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue: false);
		}
		if (bb.CardData.Instance.ObjectType != GameObjectType.Ability)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue: false);
		}
		if (bb.CardData.Instance.Parent == null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue: false);
		}
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Instance.Parent.ObjectType == GameObjectType.Token);
	}
}
