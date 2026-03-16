using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class NullInstanceDecorator : IHangerConfigProvider
{
	private readonly IHangerConfigProvider _configProvider;

	public NullInstanceDecorator(IHangerConfigProvider configProvider)
	{
		_configProvider = configProvider ?? new NullConfigProvider();
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (model.Instance == null)
		{
			return Array.Empty<HangerConfig>();
		}
		return _configProvider.GetHangerConfigs(model);
	}
}
