using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Request;

public class Request_EffectCost_CostSelection_SelectableCard : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null)
		{
			return false;
		}
		if (!(bb.Request is EffectCostRequest { CostSelection: var costSelection }))
		{
			return false;
		}
		if (costSelection == null)
		{
			return false;
		}
		foreach (uint id in costSelection.Ids)
		{
			if (bb.CardData.InstanceId == id)
			{
				return true;
			}
		}
		return false;
	}
}
