using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_Modal_ParentId : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null && bb.Ability.IsModalAbilityChild())
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Ability.ModalAbilityParents.Select((AbilityPrintingData x) => (int)x.Id), MinCount, MaxCount);
		}
		return false;
	}
}
