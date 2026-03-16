using AssetLookupTree.Blackboard;
using Wotc.Mtga.Events;

namespace AssetLookupTree.Evaluators.Event;

public class Event_TimerState : EvaluatorBase_List<EventTimerState>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Event?.PlayerEvent?.CourseData != null && bb.Event?.PlayerEvent?.EventInfo != null)
		{
			return EvaluatorBase_List<EventTimerState>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Event.PlayerEvent.GetTimerState());
		}
		return false;
	}
}
