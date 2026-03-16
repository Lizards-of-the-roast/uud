using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers;

public class ZoneSpecificDecorator : IHangerConfigProvider
{
	private readonly ZoneType _zoneType;

	private readonly IHangerConfigProvider _configProvider;

	public ZoneSpecificDecorator(ZoneType zoneType, IHangerConfigProvider provider)
	{
		_zoneType = zoneType;
		_configProvider = provider ?? new NullConfigProvider();
	}

	public IEnumerable<HangerConfig> GetHangerConfigs(ICardDataAdapter model)
	{
		if (model.ZoneType == _zoneType)
		{
			return _configProvider.GetHangerConfigs(model);
		}
		return Array.Empty<HangerConfig>();
	}
}
