using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.Interactions.SelectFromGroups;

namespace AssetLookupTree.Evaluators.Interaction.SelectFromGroups;

public class Interaction_SelectFromGroups_SelectableCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Interaction is SelectFromGroupsWorkflow_Browser selectFromGroupsWorkflow_Browser)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, selectFromGroupsWorkflow_Browser.SelectableCount);
		}
		return false;
	}
}
