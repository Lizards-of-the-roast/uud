using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class Material_Name : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.MaterialName == null)
		{
			if ((object)bb.Material == null)
			{
				return false;
			}
			value = bb.Material.name;
			return true;
		}
		value = bb.MaterialName;
		return true;
	}
}
