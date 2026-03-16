using AssetLookupTree.Blackboard;
using AssetLookupTree.Evaluators;

namespace Core.Code.AssetLookupTree.AssetLookup.Evaluators;

public class Emote_Page : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.EmotePrefabData != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.EmotePrefabData.Page);
		}
		return false;
	}
}
