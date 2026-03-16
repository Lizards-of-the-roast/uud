using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.ReplacementEffect;

public class ReplacementEffect_SpawnerType : EvaluatorBase_List<ReplacementEffectSpawnerType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<ReplacementEffectSpawnerType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ReplacementEffectData.SpawnerType, MinCount, MaxCount);
	}
}
