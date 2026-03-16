using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface IAvatarLayout
{
	void LayoutAvatars(IEnumerable<DuelScene_AvatarView> avatars);
}
