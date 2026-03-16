using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullAvatarViewManager : IAvatarViewManager, IAvatarViewProvider, IAvatarViewController
{
	public static readonly IAvatarViewManager Default = new NullAvatarViewManager();

	private static IAvatarViewProvider Provider = NullAvatarViewProvider.Default;

	private static IAvatarViewController Controller = NullAvatarViewController.Default;

	public IEnumerable<DuelScene_AvatarView> GetAllAvatars()
	{
		return Provider.GetAllAvatars();
	}

	public DuelScene_AvatarView GetAvatarById(uint id)
	{
		return Provider.GetAvatarById(id);
	}

	public DuelScene_AvatarView GetAvatarByPlayerSide(GREPlayerNum playerType)
	{
		return Provider.GetAvatarByPlayerSide(playerType);
	}

	public DuelScene_AvatarView CreateAvatarView(MtgPlayer player)
	{
		return Controller.CreateAvatarView(player);
	}

	public bool DeleteAvatar(uint playerId)
	{
		return Controller.DeleteAvatar(playerId);
	}
}
