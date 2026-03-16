using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Extractors.General;

public class DamageType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.DamageType;
		return bb.DamageType != Wotc.Mtgo.Gre.External.Messaging.DamageType.None;
	}
}
