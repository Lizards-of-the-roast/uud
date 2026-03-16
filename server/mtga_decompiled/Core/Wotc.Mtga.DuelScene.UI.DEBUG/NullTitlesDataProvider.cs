using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class NullTitlesDataProvider : ITitlesDataProvider
{
	public static readonly ITitlesDataProvider Default = new NullTitlesDataProvider();

	public IReadOnlyList<string> GetAllTitles()
	{
		return Array.Empty<string>();
	}
}
