namespace Wotc.Mtga.DuelScene.ZoneCounts;

public class NullZoneCountManager : IZoneCountManager, IZoneCountProvider, IZoneCountController
{
	public static readonly IZoneCountManager Default = new NullZoneCountManager();

	private static readonly IZoneCountProvider _provider = NullZoneCountProvider.Default;

	private static readonly IZoneCountController _controller = NullZoneCountController.Default;

	public ZoneCountView GetForPlayer(uint playerId)
	{
		return _provider.GetForPlayer(playerId);
	}

	public ZoneCountView CreateZoneCount(uint playerId)
	{
		return _controller.CreateZoneCount(playerId);
	}

	public void DeleteZoneCount(uint playerId)
	{
		_controller.DeleteZoneCount(playerId);
	}
}
