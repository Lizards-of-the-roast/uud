using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullAvatarLayout : IAvatarLayout
{
	public static readonly IAvatarLayout Default = new NullAvatarLayout();

	public void LayoutAvatars(IEnumerable<DuelScene_AvatarView> avatars)
	{
	}
}
