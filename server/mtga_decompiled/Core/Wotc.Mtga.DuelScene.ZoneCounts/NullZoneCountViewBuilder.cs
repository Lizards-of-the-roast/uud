namespace Wotc.Mtga.DuelScene.ZoneCounts;

public class NullZoneCountViewBuilder : IZoneCountViewBuilder
{
	public static readonly IZoneCountViewBuilder Default = new NullZoneCountViewBuilder();

	public ZoneCountView Create(uint playerId)
	{
		return null;
	}

	public void Destroy(ZoneCountView zoneCountView)
	{
	}
}
