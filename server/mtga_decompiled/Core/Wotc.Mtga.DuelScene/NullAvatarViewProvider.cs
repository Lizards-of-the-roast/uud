using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullAvatarViewProvider : IAvatarViewProvider
{
	public static readonly IAvatarViewProvider Default = new NullAvatarViewProvider();

	public IEnumerable<DuelScene_AvatarView> GetAllAvatars()
	{
		return Array.Empty<DuelScene_AvatarView>();
	}

	public DuelScene_AvatarView GetAvatarById(uint id)
	{
		return null;
	}

	public DuelScene_AvatarView GetAvatarByPlayerSide(GREPlayerNum playerType)
	{
		return null;
	}
}
