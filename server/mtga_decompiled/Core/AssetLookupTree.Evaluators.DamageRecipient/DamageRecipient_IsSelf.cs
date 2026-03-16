using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.DamageRecipient;

public class DamageRecipient_IsSelf : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.DamageRecipientEntity != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.DamageRecipientEntity is MtgCardInstance mtgCardInstance && bb.CardData?.Instance?.InstanceId == mtgCardInstance.InstanceId);
		}
		return false;
	}
}
