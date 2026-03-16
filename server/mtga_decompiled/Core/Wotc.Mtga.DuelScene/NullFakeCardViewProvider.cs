using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullFakeCardViewProvider : IFakeCardViewProvider
{
	public static readonly IFakeCardViewProvider Default = new NullFakeCardViewProvider();

	public IEnumerable<DuelScene_CDC> GetAllFakeCards()
	{
		return Array.Empty<DuelScene_CDC>();
	}

	public DuelScene_CDC GetFakeCard(string key)
	{
		return null;
	}
}
