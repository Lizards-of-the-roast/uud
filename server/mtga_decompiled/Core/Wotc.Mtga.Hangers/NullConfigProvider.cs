using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class NullConfigProvider : IHangerConfigProvider
{
	public static readonly IHangerConfigProvider Default = new NullConfigProvider();

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		return Array.Empty<HangerConfig>();
	}
}
