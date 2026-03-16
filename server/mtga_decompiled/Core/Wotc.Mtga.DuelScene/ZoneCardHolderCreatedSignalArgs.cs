using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class ZoneCardHolderCreatedSignalArgs : SignalArgs
{
	public readonly ICardHolder CardHolder;

	public readonly MtgZone Zone;

	public ZoneCardHolderCreatedSignalArgs(object dispatcher, ICardHolder cardHolder, MtgZone zone)
		: base(dispatcher)
	{
		CardHolder = cardHolder;
		Zone = zone;
	}
}
