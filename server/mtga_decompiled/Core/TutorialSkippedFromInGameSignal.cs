using Wotc;

public class TutorialSkippedFromInGameSignal : SignalBase<SignalArgs>
{
	public static TutorialSkippedFromInGameSignal Create()
	{
		return new TutorialSkippedFromInGameSignal();
	}
}
