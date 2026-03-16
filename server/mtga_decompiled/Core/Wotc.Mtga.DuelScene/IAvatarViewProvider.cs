using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface IAvatarViewProvider
{
	IEnumerable<DuelScene_AvatarView> GetAllAvatars();

	DuelScene_AvatarView GetAvatarById(uint id);

	DuelScene_AvatarView GetAvatarByPlayerSide(GREPlayerNum playerType);
}
