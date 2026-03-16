using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class Font_Name : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.FontName == null)
		{
			if ((object)bb.Font == null)
			{
				return false;
			}
			value = bb.Font.name;
			return true;
		}
		value = bb.FontName;
		return true;
	}
}
