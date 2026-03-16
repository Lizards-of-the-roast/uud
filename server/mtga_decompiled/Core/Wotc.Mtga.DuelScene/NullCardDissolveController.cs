using System;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullCardDissolveController : ICardDissolveController
{
	public static readonly ICardDissolveController Default = new NullCardDissolveController();

	public void DissolveCard(DuelScene_CDC cardView, Action onComplete, ZoneTransferReason reason, CardData responsibleCard, MtgZone fromZone, MtgZone toZone)
	{
	}
}
