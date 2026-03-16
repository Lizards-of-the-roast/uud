using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullRelatedCardIdProvider : IRelatedCardIdProvider
{
	public static IRelatedCardIdProvider Default = new NullRelatedCardIdProvider();

	public IEnumerable<uint> GetRelatedIds(DuelScene_CDC card)
	{
		return Array.Empty<uint>();
	}
}
