using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Interaction.NumericInput;

public class Interaction_NumericInputWorkflow_NumericInputType : EvaluatorBase_List<NumericInputType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Interaction is NumericInputWorkflow numericInputWorkflow)
		{
			return EvaluatorBase_List<NumericInputType>.GetResult(ExpectedValues, Operation, ExpectedResult, numericInputWorkflow.NumericInputType);
		}
		return false;
	}
}
