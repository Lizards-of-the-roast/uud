using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.AvatarView;

namespace AssetLookupTree.Evaluators.Avatar;

public class Avatar_FramePartType : EvaluatorBase_List<AvatarFramePartType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<AvatarFramePartType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.AvatarFramePart);
	}
}
