using AssetLookupTree.Blackboard;
using Wizards.Mtga.FrontDoorModels;

namespace AssetLookupTree.Evaluators.Event;

public class Event_Type : EvaluatorBase_List<MDNEFormatType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Event?.PlayerEvent?.EventInfo != null)
		{
			return EvaluatorBase_List<MDNEFormatType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Event.PlayerEvent.EventInfo.FormatType);
		}
		return false;
	}
}
