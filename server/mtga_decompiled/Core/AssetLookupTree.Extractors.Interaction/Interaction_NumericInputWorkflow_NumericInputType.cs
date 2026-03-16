using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.Interactions;

namespace AssetLookupTree.Extractors.Interaction;

public class Interaction_NumericInputWorkflow_NumericInputType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.Interaction is NumericInputWorkflow numericInputWorkflow)
		{
			value = (int)numericInputWorkflow.NumericInputType;
			return true;
		}
		value = 0;
		return false;
	}
}
