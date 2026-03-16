namespace Wotc.Mtga.DuelScene;

public static class IAvatarViewProviderExtensions
{
	public static bool TryGetAvatarById(this IAvatarViewProvider provider, uint id, out DuelScene_AvatarView avatar)
	{
		avatar = provider.GetAvatarById(id);
		return avatar != null;
	}

	public static bool TryGetAvatarByPlayerSide(this IAvatarViewProvider provider, GREPlayerNum playerType, out DuelScene_AvatarView avatar)
	{
		avatar = provider.GetAvatarByPlayerSide(playerType);
		return avatar != null;
	}
}
