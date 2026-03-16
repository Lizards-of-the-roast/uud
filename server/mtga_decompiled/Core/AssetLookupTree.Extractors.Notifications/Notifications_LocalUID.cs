using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Notifications;

public class Notifications_LocalUID : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (string.IsNullOrEmpty(bb.LocalNotificationUID))
		{
			return false;
		}
		value = bb.LocalNotificationUID;
		return true;
	}
}
