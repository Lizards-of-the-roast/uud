namespace Wotc.Mtga.DuelScene;

public class ZoneCountCreatedSignalArgs : SignalArgs
{
	public readonly uint PlayerId;

	public readonly ZoneCountView ZoneCount;

	public ZoneCountCreatedSignalArgs(object dispatcher, uint playerId, ZoneCountView zoneCount)
		: base(dispatcher)
	{
		PlayerId = playerId;
		ZoneCount = zoneCount;
	}
}
