using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public readonly struct BadgeActivationCalculatorInput
{
	public readonly ICardDataAdapter CardData;

	public readonly AbilityPrintingData Ability;

	public readonly MtgGameState GameState;

	public BadgeActivationCalculatorInput(ICardDataAdapter cardData, AbilityPrintingData ability, MtgGameState gameState)
	{
		CardData = cardData;
		Ability = ability;
		GameState = gameState;
	}
}
