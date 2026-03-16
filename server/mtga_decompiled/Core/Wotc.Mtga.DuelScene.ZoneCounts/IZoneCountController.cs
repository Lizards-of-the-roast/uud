namespace Wotc.Mtga.DuelScene.ZoneCounts;

public interface IZoneCountController
{
	ZoneCountView CreateZoneCount(uint playerId);

	void DeleteZoneCount(uint playerId);
}
