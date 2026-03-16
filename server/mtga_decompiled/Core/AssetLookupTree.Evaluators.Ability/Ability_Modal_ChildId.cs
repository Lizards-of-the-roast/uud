using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_Modal_ChildId : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null && bb.Ability.IsModalAbility())
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.ModalAbilityChildren.Select((AbilityPrintingData x) => (int)x.Id), MinCount, MaxCount);
		}
		return false;
	}
}
