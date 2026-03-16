using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Universal;

public struct ValidatorBlackboard
{
	public ICardDataAdapter CardData;

	public IReadOnlyList<MtgPlayer> Players;

	public bool IsFocusPlayer;
}
