using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullAvatarViewController : IAvatarViewController
{
	public static readonly IAvatarViewController Default = new NullAvatarViewController();

	public DuelScene_AvatarView CreateAvatarView(MtgPlayer player)
	{
		return null;
	}

	public bool DeleteAvatar(uint playerId)
	{
		return false;
	}
}
