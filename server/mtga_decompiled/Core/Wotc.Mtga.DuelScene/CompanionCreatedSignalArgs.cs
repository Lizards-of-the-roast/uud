namespace Wotc.Mtga.DuelScene;

public class CompanionCreatedSignalArgs : SignalArgs
{
	public readonly uint PlayerId;

	public readonly AccessoryController Companion;

	public CompanionCreatedSignalArgs(object dispatcher, uint playerId, AccessoryController companion)
		: base(dispatcher)
	{
		PlayerId = playerId;
		Companion = companion;
	}
}
