using System.Linq;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.ReplacementEffect;

public class ReplacementEffect_SourceIdsHasInstance : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ReplacementEffectData.SourceIds != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.ReplacementEffectData.SourceIds.Contains(bb.CardData.InstanceId));
		}
		return false;
	}
}
