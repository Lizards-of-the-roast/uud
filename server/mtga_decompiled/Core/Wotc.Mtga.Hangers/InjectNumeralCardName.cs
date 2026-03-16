using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Hangers;

public class InjectNumeralCardName : IParameterizedInjector
{
	private const string LOC_PARAM = "{cardName}";

	private readonly IGreLocProvider _greLocProvider;

	public InjectNumeralCardName(IGreLocProvider greLocProvider)
	{
		_greLocProvider = greLocProvider;
	}

	public static bool TryGetCardNameLoc(AbilityPrintingData ability, IGreLocProvider greLocProvider, out string locText)
	{
		locText = string.Empty;
		if (ability.BaseIdNumeral.HasValue && greLocProvider.TryGetLocalizedText(out locText, ability.BaseIdNumeral.Value))
		{
			locText = CardUtilities.FormatComplexTitle(locText);
			return true;
		}
		return false;
	}

	public string Inject(string value, ICardDataAdapter model, AbilityPrintingData ability)
	{
		if (TryGetCardNameLoc(ability, _greLocProvider, out var locText))
		{
			value = value.Replace("{cardName}", locText);
		}
		return value;
	}
}
