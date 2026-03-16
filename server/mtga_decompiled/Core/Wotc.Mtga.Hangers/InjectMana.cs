using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class InjectMana : IParameterizedInjector
{
	private const string ABILITY_COST_STRING = "{abilityCost}";

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		string text = ManaUtilities.ConvertToOldSchoolManaText(ability.ManaCost);
		if (string.IsNullOrEmpty(text) && ability is DynamicAbilityPrintingData dynamicAbilityPrintingData)
		{
			text = dynamicAbilityPrintingData.Cost;
		}
		return value.Replace("{abilityCost}", text);
	}
}
