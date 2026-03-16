using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_DesignationValue : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Instance != null)
		{
			foreach (DesignationData designation in bb.CardData.Instance.Designations)
			{
				if (designation.Value.HasValue)
				{
					return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)designation.Value.Value);
				}
			}
		}
		return false;
	}
}
