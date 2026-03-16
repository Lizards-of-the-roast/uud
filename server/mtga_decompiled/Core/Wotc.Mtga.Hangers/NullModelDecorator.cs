using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class NullModelDecorator : IHangerConfigProvider
{
	private readonly IHangerConfigProvider _configProvider;

	public NullModelDecorator(IHangerConfigProvider configProvider)
	{
		_configProvider = configProvider ?? new NullConfigProvider();
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (model == null)
		{
			return Array.Empty<HangerConfig>();
		}
		return _configProvider.GetHangerConfigs(model);
	}
}
