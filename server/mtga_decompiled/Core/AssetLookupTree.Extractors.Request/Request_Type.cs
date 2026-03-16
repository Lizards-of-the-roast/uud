using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Request;

public class Request_Type : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb?.Request == null)
		{
			return false;
		}
		value = (int)bb.Request.Type;
		return true;
	}
}
