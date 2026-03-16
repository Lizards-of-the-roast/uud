namespace Wotc.Mtga.DuelScene;

public class ZoneCountDeletedSignalArgs : SignalArgs
{
	public readonly ZoneCountView ZoneCount;

	public ZoneCountDeletedSignalArgs(object dispatcher, ZoneCountView zoneCount)
		: base(dispatcher)
	{
		ZoneCount = zoneCount;
	}
}
