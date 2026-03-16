using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IAvatarViewController
{
	DuelScene_AvatarView CreateAvatarView(MtgPlayer player);

	bool DeleteAvatar(uint playerId);
}
