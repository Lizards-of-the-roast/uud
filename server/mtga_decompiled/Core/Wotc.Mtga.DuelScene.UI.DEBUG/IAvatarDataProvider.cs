using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public interface IAvatarDataProvider
{
	IReadOnlyList<string> GetAllAvatars();
}
