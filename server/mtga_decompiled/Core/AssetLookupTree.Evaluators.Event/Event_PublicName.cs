using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Event;

public class Event_PublicName : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CampaignGraphNodeName != null)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.CampaignGraphNodeName);
		}
		if (bb.Event?.PlayerEvent?.EventUXInfo?.PublicEventName != null)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.Event.PlayerEvent.EventUXInfo.PublicEventName);
		}
		return false;
	}
}
