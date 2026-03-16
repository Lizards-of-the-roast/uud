using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasPerpetualChanges : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return bb.CardData.HasPerpetualChanges();
	}
}
