using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Event;

public class Event_InternalName : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CampaignGraphNodeName != null)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.CampaignGraphNodeName);
		}
		if (bb.Event?.PlayerEvent?.EventInfo?.InternalEventName != null)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.Event.PlayerEvent.EventInfo.InternalEventName);
		}
		return false;
	}
}
