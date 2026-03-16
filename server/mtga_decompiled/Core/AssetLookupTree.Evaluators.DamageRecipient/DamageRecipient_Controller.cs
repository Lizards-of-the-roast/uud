using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.DamageRecipient;

public class DamageRecipient_Controller : EvaluatorBase_List<GREPlayerNum>
{
	public override bool Execute(IBlackboard bb)
	{
		GREPlayerNum inValue = GREPlayerNum.Invalid;
		MtgEntity damageRecipientEntity = bb.DamageRecipientEntity;
		if (!(damageRecipientEntity is MtgCardInstance mtgCardInstance))
		{
			if (damageRecipientEntity is MtgPlayer mtgPlayer)
			{
				inValue = mtgPlayer.ClientPlayerEnum;
			}
		}
		else
		{
			inValue = mtgCardInstance.Controller.ClientPlayerEnum;
		}
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<GREPlayerNum>.GetResult(ExpectedValues, Operation, ExpectedResult, inValue);
		}
		return false;
	}
}
