using HasbroGo.Social.Models;

namespace MTGA.Social;

public class SocialUtilities
{
	public static string GetInvitePlayerName(IncomingFriendInvite sdkInvite)
	{
		string result = "unknown";
		if (sdkInvite.DisplayName != null && sdkInvite.DisplayName.IsValid)
		{
			result = sdkInvite.DisplayName.ToString();
		}
		return result;
	}

	public static string GetInvitePlayerName(OutgoingFriendInvite sdkInvite)
	{
		string result = "unknown";
		if (sdkInvite.ReceiverDisplayName != null && sdkInvite.ReceiverDisplayName.IsValid)
		{
			result = sdkInvite.ReceiverDisplayName.ToString();
		}
		else if (sdkInvite.ReceiverEmail != null)
		{
			result = sdkInvite.ReceiverEmail.ToString();
		}
		return result;
	}
}
