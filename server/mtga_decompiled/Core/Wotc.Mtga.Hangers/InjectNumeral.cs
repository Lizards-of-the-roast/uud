using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class InjectNumeral : IParameterizedInjector
{
	private const string NUMERAL_PARAM = "{numeral}";

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		if (ability.BaseIdNumeral != 0)
		{
			value = value.Replace("{numeral}", ability.BaseIdNumeral.ToString());
		}
		return value;
	}
}
