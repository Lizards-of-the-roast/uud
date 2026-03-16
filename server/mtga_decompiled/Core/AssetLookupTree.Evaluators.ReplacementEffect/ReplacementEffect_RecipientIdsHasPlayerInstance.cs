using System.Linq;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.ReplacementEffect;

public class ReplacementEffect_RecipientIdsHasPlayerInstance : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ReplacementEffectData.RecipientIds != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.ReplacementEffectData.RecipientIds.Contains(bb.Player.InstanceId));
		}
		return false;
	}
}
