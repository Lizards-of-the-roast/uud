using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_ManaSpecTypeFromAutoTapActions : EvaluatorBase_List<ManaSpecType>
{
	public ManaSpecType manaSpecType;

	public override bool Execute(IBlackboard bb)
	{
		IEnumerable<ManaSpecType> enumerable = new List<ManaSpecType>();
		if (bb.GreAction?.AutoTapSolution != null)
		{
			foreach (ManaPaymentOption item in bb.GreAction.AutoTapSolution.AutoTapActions.Select((AutoTapAction action) => action.ManaPaymentOption))
			{
				foreach (ManaInfo item2 in (IEnumerable<ManaInfo>)item.Mana)
				{
					IEnumerable<ManaSpecType> second = item2.Specs.Select((ManaInfo.Types.Spec spec) => spec.Type);
					enumerable = enumerable.Union(second);
				}
			}
		}
		if (bb.GreAction?.AutoTapSolution != null)
		{
			return EvaluatorBase_List<ManaSpecType>.GetResult(ExpectedValues, Operation, ExpectedResult, enumerable, MinCount, MaxCount);
		}
		return false;
	}
}
