using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public struct NumericBadgeCalculatorInput
{
	public ICardDataAdapter CardData;

	public AbilityPrintingData Ability;

	public MtgGameState GameState;
}
