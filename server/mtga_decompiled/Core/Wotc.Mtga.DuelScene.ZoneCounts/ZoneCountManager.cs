using System;

namespace Wotc.Mtga.DuelScene.ZoneCounts;

public class ZoneCountManager : IZoneCountManager, IZoneCountProvider, IZoneCountController, IDisposable
{
	private readonly MutableZoneCountProvider _provider;

	private readonly IZoneCountViewBuilder _builder;

	public ZoneCountManager(MutableZoneCountProvider provider, IZoneCountViewBuilder builder)
	{
		_provider = provider ?? new MutableZoneCountProvider();
		_builder = builder ?? NullZoneCountViewBuilder.Default;
	}

	public ZoneCountView GetForPlayer(uint playerId)
	{
		return _provider.GetForPlayer(playerId);
	}

	public ZoneCountView CreateZoneCount(uint playerId)
	{
		if (_provider.PlayerIdToZoneCount.TryGetValue(playerId, out var value))
		{
			return value;
		}
		return _provider.PlayerIdToZoneCount[playerId] = _builder.Create(playerId);
	}

	public void DeleteZoneCount(uint playerId)
	{
		if (_provider.PlayerIdToZoneCount.TryGetValue(playerId, out var value))
		{
			_builder.Destroy(value);
		}
	}

	public void Dispose()
	{
		_provider.PlayerIdToZoneCount.Clear();
	}
}
