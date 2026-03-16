using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_TargetSelection_TargetingHighlightType : EvaluatorBase_List<Wotc.Mtgo.Gre.External.Messaging.HighlightType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<Wotc.Mtgo.Gre.External.Messaging.HighlightType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.TargetSelectionParams.TargetingHighlightType);
	}
}
