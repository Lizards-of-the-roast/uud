using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Event;

public class Event_MatchmakingName : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Event?.PlayerEvent?.MatchMakingName != null)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.Event.PlayerEvent.MatchMakingName);
		}
		return false;
	}
}
