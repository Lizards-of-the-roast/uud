using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_ManaSpecTypeInManaPaymentConditions : EvaluatorBase_List<ManaSpecType>
{
	public override bool Execute(IBlackboard bb)
	{
		IEnumerable<ManaSpecType> enumerable = new List<ManaSpecType>();
		if (bb.GreAction?.ManaPaymentConditions != null)
		{
			foreach (ManaPaymentCondition manaPaymentCondition in bb.GreAction.ManaPaymentConditions)
			{
				enumerable = enumerable.Union(manaPaymentCondition.Specs);
			}
		}
		if (bb.GreAction?.ManaPaymentConditions != null)
		{
			return EvaluatorBase_List<ManaSpecType>.GetResult(ExpectedValues, Operation, ExpectedResult, enumerable, MinCount, MaxCount);
		}
		return false;
	}
}
