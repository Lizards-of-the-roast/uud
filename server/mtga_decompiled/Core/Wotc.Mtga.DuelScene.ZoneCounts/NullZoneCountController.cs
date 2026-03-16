namespace Wotc.Mtga.DuelScene.ZoneCounts;

public class NullZoneCountController : IZoneCountController
{
	public static readonly IZoneCountController Default = new NullZoneCountController();

	public ZoneCountView CreateZoneCount(uint playerId)
	{
		return null;
	}

	public void DeleteZoneCount(uint playerId)
	{
	}
}
