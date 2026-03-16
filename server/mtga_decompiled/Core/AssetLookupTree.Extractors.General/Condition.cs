using AssetLookupTree.Blackboard;
using Wotc.Mtga.Hangers;

namespace AssetLookupTree.Extractors.General;

public class Condition : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.Condition;
		return bb.Condition != ConditionType.None;
	}
}
