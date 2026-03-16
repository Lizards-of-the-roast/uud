using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Evaluators.Event;

public class Event_State : EvaluatorBase_List<MDNEventState>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Event?.PlayerEvent?.EventInfo != null)
		{
			return EvaluatorBase_List<MDNEventState>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Event.PlayerEvent.EventInfo.EventState);
		}
		return false;
	}
}
