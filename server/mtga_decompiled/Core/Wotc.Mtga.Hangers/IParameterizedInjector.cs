using GreClient.CardData;

namespace Wotc.Mtga.Hangers;

public interface IParameterizedInjector
{
	string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability);
}
