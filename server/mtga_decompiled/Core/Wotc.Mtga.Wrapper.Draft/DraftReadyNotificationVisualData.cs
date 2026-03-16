namespace Wotc.Mtga.Wrapper.Draft;

public readonly struct DraftReadyNotificationVisualData
{
	public readonly bool ShouldStartSparkTimer;

	public readonly bool ShouldStopSparkTimer;

	public readonly bool ShouldResetChronometer;

	public readonly string ChronometerContextKey;

	public readonly bool ShouldRevealIdentities;

	public readonly KnownSeatVisualData[] KnownSeatVisualDatas;

	public DraftReadyNotificationVisualData(bool shouldStartSparkTimer, bool shouldStopSparkTimer, bool shouldResetChronometer, string chronometerContextKey, bool shouldRevealIdentities, KnownSeatVisualData[] knownSeatVisualDatas)
	{
		ShouldStartSparkTimer = shouldStartSparkTimer;
		ShouldStopSparkTimer = shouldStopSparkTimer;
		ShouldResetChronometer = shouldResetChronometer;
		ChronometerContextKey = chronometerContextKey;
		ShouldRevealIdentities = shouldRevealIdentities;
		KnownSeatVisualDatas = knownSeatVisualDatas;
	}
}
