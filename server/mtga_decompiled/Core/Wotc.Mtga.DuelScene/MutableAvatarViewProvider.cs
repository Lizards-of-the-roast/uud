using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class MutableAvatarViewProvider : IAvatarViewProvider
{
	public readonly HashSet<DuelScene_AvatarView> AllAvatars = new HashSet<DuelScene_AvatarView>();

	public readonly Dictionary<uint, DuelScene_AvatarView> AvatarViewsById = new Dictionary<uint, DuelScene_AvatarView>();

	public readonly Dictionary<GREPlayerNum, DuelScene_AvatarView> AvatarViewsByEnum = new Dictionary<GREPlayerNum, DuelScene_AvatarView>();

	public IEnumerable<DuelScene_AvatarView> GetAllAvatars()
	{
		return AllAvatars;
	}

	public DuelScene_AvatarView GetAvatarById(uint id)
	{
		if (!AvatarViewsById.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public DuelScene_AvatarView GetAvatarByPlayerSide(GREPlayerNum playerType)
	{
		if (!AvatarViewsByEnum.TryGetValue(playerType, out var value))
		{
			return null;
		}
		return value;
	}
}
