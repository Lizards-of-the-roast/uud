using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Extractors.Action;

public class Action_ActionType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.GreAction != null)
		{
			value = (int)bb.GreAction.ActionType;
			return true;
		}
		value = (int)bb.GreActionType;
		return bb.GreActionType != ActionType.None;
	}
}
