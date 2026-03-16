namespace Wotc.Mtga.DuelScene.ZoneCounts;

public interface IZoneCountViewBuilder
{
	ZoneCountView Create(uint playerId);

	void Destroy(ZoneCountView zoneCountView);
}
