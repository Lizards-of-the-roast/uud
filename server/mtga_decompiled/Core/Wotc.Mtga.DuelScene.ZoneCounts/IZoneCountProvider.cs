namespace Wotc.Mtga.DuelScene.ZoneCounts;

public interface IZoneCountProvider
{
	ZoneCountView GetForPlayer(uint playerId);

	bool TryGetForPlayer(uint playerId, out ZoneCountView zoneCountView)
	{
		zoneCountView = GetForPlayer(playerId);
		return zoneCountView != null;
	}
}
