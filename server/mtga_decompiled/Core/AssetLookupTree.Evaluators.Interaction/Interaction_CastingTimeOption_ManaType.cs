using System.Linq;
using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Interaction;

public class Interaction_CastingTimeOption_ManaType : EvaluatorBase_List<ManaColor>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Interaction is CastingTimeOption_ManaTypeWorkflow castingTimeOption_ManaTypeWorkflow)
		{
			return EvaluatorBase_List<ManaColor>.GetResult(ExpectedValues, Operation, ExpectedResult, castingTimeOption_ManaTypeWorkflow.SelectionPairs.SelectMany((CastingTimeOption_ManaTypeWorkflow.SelectionPair x) => x.Options), MinCount, MaxCount);
		}
		return false;
	}
}
