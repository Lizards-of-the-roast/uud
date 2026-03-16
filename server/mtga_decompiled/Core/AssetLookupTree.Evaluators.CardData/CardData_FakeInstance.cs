using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_FakeInstance : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		bool inValue = bb.CardData != null && bb.CardData.Instance != null && bb.CardData.InstanceId == 0;
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, inValue);
	}
}
