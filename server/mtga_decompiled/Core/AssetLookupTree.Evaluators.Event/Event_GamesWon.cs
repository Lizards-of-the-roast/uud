using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Event;

public class Event_GamesWon : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Event?.PostMatchContext != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.Event.PostMatchContext.GamesWon);
		}
		return false;
	}
}
