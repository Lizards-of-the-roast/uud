using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public class NullParameterInjector : IParameterizedInjector
{
	public static readonly IParameterizedInjector Default = new NullParameterInjector();

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		return string.Empty;
	}
}
