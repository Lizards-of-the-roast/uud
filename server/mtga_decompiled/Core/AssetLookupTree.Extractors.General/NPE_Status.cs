using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class NPE_Status : IExtractor<int>
{
	public enum NPEStatus
	{
		NpeIncomplete,
		NpeComplete
	}

	public bool Execute(IBlackboard bb, out int value)
	{
		value = (bb.IsNPEComplete ? 1 : 0);
		return true;
	}
}
