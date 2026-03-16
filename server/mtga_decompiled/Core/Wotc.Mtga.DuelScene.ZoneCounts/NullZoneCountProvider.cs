namespace Wotc.Mtga.DuelScene.ZoneCounts;

public class NullZoneCountProvider : IZoneCountProvider
{
	public static readonly IZoneCountProvider Default = new NullZoneCountProvider();

	public ZoneCountView GetForPlayer(uint playerId)
	{
		return null;
	}
}
