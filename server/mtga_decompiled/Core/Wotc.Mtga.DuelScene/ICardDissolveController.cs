using System;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface ICardDissolveController
{
	void DissolveCard(DuelScene_CDC cardView, Action onComplete, ZoneTransferReason reason, CardData responsibleCard, MtgZone fromZone, MtgZone toZone);
}
