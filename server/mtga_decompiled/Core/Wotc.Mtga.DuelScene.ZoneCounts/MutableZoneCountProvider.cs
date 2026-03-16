using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.ZoneCounts;

public class MutableZoneCountProvider : IZoneCountProvider
{
	public readonly Dictionary<uint, ZoneCountView> PlayerIdToZoneCount = new Dictionary<uint, ZoneCountView>();

	public ZoneCountView GetForPlayer(uint playerId)
	{
		if (!PlayerIdToZoneCount.TryGetValue(playerId, out var value))
		{
			return null;
		}
		return value;
	}
}
