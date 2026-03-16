using System;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class Date_Month : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.DateTimeUtc == default(DateTime))
		{
			return false;
		}
		value = bb.DateTimeUtc.Month;
		return true;
	}
}
