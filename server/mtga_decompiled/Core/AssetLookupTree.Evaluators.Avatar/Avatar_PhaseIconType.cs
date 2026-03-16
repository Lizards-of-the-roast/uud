using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.AvatarView;

namespace AssetLookupTree.Evaluators.Avatar;

public class Avatar_PhaseIconType : EvaluatorBase_List<PhaseIconType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<PhaseIconType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.PhaseIconType);
	}
}
