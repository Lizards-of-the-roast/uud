using GreClient.CardData;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Hangers;

public class InjectX : IParameterizedInjector
{
	private const string NUMERAL_PARAM = "{numeral}";

	private readonly IClientLocProvider _clientLocProvider;

	public InjectX(IClientLocProvider clientLocProvider)
	{
		_clientLocProvider = clientLocProvider;
	}

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		value = value.Replace("{numeral}", _clientLocProvider.GetLocalizedText("AbilityHanger/Keyword/X"));
		return value;
	}
}
