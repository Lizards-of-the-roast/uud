using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Interaction;

public class Interaction_RequestType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb?.Interaction == null)
		{
			return false;
		}
		value = (int)bb.Interaction.Type;
		return true;
	}
}
