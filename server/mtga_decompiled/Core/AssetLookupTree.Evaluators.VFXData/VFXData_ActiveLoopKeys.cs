using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.VFX;

namespace AssetLookupTree.Evaluators.VFXData;

public class VFXData_ActiveLoopKeys : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, LoopingAnimationManager.ActiveLoopingKeys(), MinCount, MaxCount);
	}
}
