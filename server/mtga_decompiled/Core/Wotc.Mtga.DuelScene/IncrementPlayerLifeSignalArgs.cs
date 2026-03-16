namespace Wotc.Mtga.DuelScene;

public class IncrementPlayerLifeSignalArgs : SignalArgs
{
	public readonly uint PlayerId;

	public readonly int Amount;

	public IncrementPlayerLifeSignalArgs(object dispatcher, uint playerId, int amount)
		: base(dispatcher)
	{
		PlayerId = playerId;
		Amount = amount;
	}
}
