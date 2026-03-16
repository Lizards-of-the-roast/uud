using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullAvatarBuilder : IAvatarBuilder
{
	public static readonly IAvatarBuilder Default = new NullAvatarBuilder();

	public DuelScene_AvatarView Create(MtgPlayer player)
	{
		return null;
	}

	public bool Destroy(DuelScene_AvatarView avatar)
	{
		return false;
	}
}
