using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class InjectFakeNumeral : IParameterizedInjector
{
	private const string NUMERAL_PARAM = "{numeral}";

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		if (ability.FakeBaseIdNumeral != 0)
		{
			value = value.Replace("{numeral}", ability.FakeBaseIdNumeral.ToString());
		}
		return value;
	}
}
