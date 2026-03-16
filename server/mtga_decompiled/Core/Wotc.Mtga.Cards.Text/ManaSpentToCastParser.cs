using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Text;

public class ManaSpentToCastParser : ITextEntryParser
{
	private const string ABIL_WORD_MANAPAYMENTCONDITION = "ManaPaymentCondition";

	private const string LOC_PARAM_TREASURESPENT = "treasureManaSpent";

	private const string LOC_PARAM_SPENTTOCAST = "spentToCast";

	private const string LOC_PARAM_MANASPENTTOCAST = "manaSpentToCast";

	private readonly IClientLocProvider _locManager;

	public ManaSpentToCastParser(IClientLocProvider locManager)
	{
		_locManager = locManager ?? NullLocProvider.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLanguage = null)
	{
		string text = string.Join(", ", GetLocalizedManaText(card.Instance, overrideLanguage));
		if (!string.IsNullOrWhiteSpace(text))
		{
			yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, text));
		}
	}

	private IEnumerable<string> GetLocalizedManaText(MtgCardInstance card, string overrideLanguage = null)
	{
		foreach (AbilityWordData matchingAbilityWord in AbilityWordData.GetMatchingAbilityWords(card, "ManaPaymentCondition"))
		{
			if (matchingAbilityWord.ManaSpecTypes != null && matchingAbilityWord.ManaSpecTypes.Contains(ManaSpecType.FromTreasure))
			{
				yield return _locManager.GetLocalizedTextForLanguage("DuelScene/RuleText/TreasureSpentToCast", overrideLanguage, ("treasureManaSpent", matchingAbilityWord.ManaSpecTypes.Count((ManaSpecType m) => m == ManaSpecType.FromTreasure).ToString()));
				continue;
			}
			List<int> colors = matchingAbilityWord.Colors;
			if (colors != null && colors.Count != 0)
			{
				if (colors.Contains(13))
				{
					yield return _locManager.GetLocalizedTextForLanguage("DuelScene/RuleText/SpentToCast", overrideLanguage, ("spentToCast", GenerateManaText(colors)));
				}
				else
				{
					yield return _locManager.GetLocalizedTextForLanguage("DuelScene/RuleText/ManaSpentToCast", overrideLanguage, ("manaSpentToCast", GenerateManaText(colors)));
				}
			}
		}
	}

	private static string GenerateManaText(List<int> colors)
	{
		return ManaUtilities.ConvertManaSymbols(ManaUtilities.ConvertToOldSchoolManaText(ManaUtilities.ConvertManaIndexesToManaQuantities(colors.OrderBy((int x) => x))));
	}
}
