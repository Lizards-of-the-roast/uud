using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Vorthos;

namespace AssetLookupTree.Extractors.Vorthos;

public class Vorthos_TimeOfDay : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)ItsVorthosTime.Get(bb.DateTimeUtc);
		return true;
	}
}
