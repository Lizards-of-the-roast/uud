using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IAvatarBuilder
{
	DuelScene_AvatarView Create(MtgPlayer player);

	bool Destroy(DuelScene_AvatarView avatar);
}
