using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_ManaSpecsCountOfType : EvaluatorBase_Int
{
	public ManaSpecType Type;

	public override bool Execute(IBlackboard bb)
	{
		if (bb.GreAction?.AutoTapSolution?.AutoTapActions == null)
		{
			return false;
		}
		return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, AutoTapActionsWorkflow.GetManaSpecCountInSolution(bb.GreAction.AutoTapSolution, Type));
	}
}
