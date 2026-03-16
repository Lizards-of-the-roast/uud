using AssetLookupTree.Blackboard;
using AssetLookupTree.Evaluators;

namespace Core.Code.AssetLookupTree.AssetLookup.Evaluators;

public class Emote_Id : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.EmotePrefabData != null)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.EmotePrefabData.Id);
		}
		return false;
	}
}
