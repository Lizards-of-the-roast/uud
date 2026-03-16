using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class NullAvatarDataProvider : IAvatarDataProvider
{
	public static readonly IAvatarDataProvider Default = new NullAvatarDataProvider();

	public IReadOnlyList<string> GetAllAvatars()
	{
		return Array.Empty<string>();
	}
}
