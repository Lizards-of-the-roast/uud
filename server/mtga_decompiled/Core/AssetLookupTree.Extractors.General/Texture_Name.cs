using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class Texture_Name : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.TextureName == null)
		{
			if ((object)bb.Texture == null)
			{
				return false;
			}
			value = bb.Texture.name;
			return true;
		}
		value = bb.TextureName;
		return true;
	}
}
