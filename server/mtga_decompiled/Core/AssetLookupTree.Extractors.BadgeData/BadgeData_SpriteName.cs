using System.IO;
using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.BadgeData;

public class BadgeData_SpriteName : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.BadgeData?.SpriteRef?.RelativePath == null)
		{
			return false;
		}
		value = Path.GetFileNameWithoutExtension(bb.BadgeData.SpriteRef.RelativePath);
		return true;
	}
}
