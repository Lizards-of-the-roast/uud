using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class NullEmblemDataProvider : IEmblemDataProvider
{
	public static readonly IEmblemDataProvider Default = new NullEmblemDataProvider();

	public IReadOnlyList<EmblemData> GetAllEmblems()
	{
		return Array.Empty<EmblemData>();
	}
}
