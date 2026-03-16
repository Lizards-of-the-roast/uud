using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Extractors.LinkInfo;

public class LinkInfo_Type : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.LinkInfo.LinkType;
		return !bb.LinkInfo.Equals(default(LinkInfoData));
	}
}
