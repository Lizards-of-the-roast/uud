using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_PropertyType : EvaluatorBase_List<PropertyType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.UpdatedProperties != null)
		{
			return EvaluatorBase_List<PropertyType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.UpdatedProperties, MinCount, MaxCount);
		}
		return false;
	}
}
