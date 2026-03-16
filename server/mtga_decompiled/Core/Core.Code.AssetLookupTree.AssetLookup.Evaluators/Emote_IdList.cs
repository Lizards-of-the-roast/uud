using AssetLookupTree.Blackboard;
using AssetLookupTree.Evaluators;

namespace Core.Code.AssetLookupTree.AssetLookup.Evaluators;

public class Emote_IdList : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.EmotePrefabData != null)
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.EmotePrefabData.Id);
		}
		return false;
	}
}
