using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class NullSleeveDataProvider : ISleeveDataProvider
{
	public static readonly ISleeveDataProvider Default = new NullSleeveDataProvider();

	public IReadOnlyList<string> GetAllSleeves()
	{
		return Array.Empty<string>();
	}
}
