using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Cards.Text;

public class ColorsSpentToCastParser : ITextEntryParser
{
	public const string ABIL_WORD_COLORSSPENTTOCAST = "ColorsSpentToCast";

	private const string LOC_PARAM_COLORS_SPENT_TO_CAST = "colorsSpentToCast";

	private readonly IClientLocProvider _locManager;

	public ColorsSpentToCastParser(IClientLocProvider locManager)
	{
		_locManager = locManager ?? NullLocProvider.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		string text = string.Join(", ", GetLocalizedColorText(card.Instance, overrideLang));
		if (!string.IsNullOrWhiteSpace(text))
		{
			yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, text));
		}
	}

	public IEnumerable<string> GetLocalizedColorText(MtgCardInstance card, string overrideLang = null)
	{
		foreach (AbilityWordData colorSpentAbilityWordDatum in GetColorSpentAbilityWordData(card))
		{
			yield return _locManager.GetLocalizedTextForLanguage("DuelScene/RuleText/ColorsSpentToCast", overrideLang, ("colorsSpentToCast", GenerateManaText(colorSpentAbilityWordDatum.Colors)));
		}
	}

	private static string GenerateManaText(List<int> colors)
	{
		return ManaUtilities.ConvertManaSymbols(ManaUtilities.ConvertToOldSchoolManaText(ManaUtilities.ConvertManaIndexesToManaQuantities(colors)));
	}

	private static IEnumerable<AbilityWordData> GetColorSpentAbilityWordData(MtgCardInstance instance)
	{
		if (instance == null)
		{
			yield break;
		}
		foreach (AbilityWordData matchingAbilityWord in AbilityWordData.GetMatchingAbilityWords(instance, "ColorsSpentToCast"))
		{
			if (matchingAbilityWord.Colors != null && matchingAbilityWord.Colors.Count > 0)
			{
				yield return matchingAbilityWord;
			}
		}
	}
}
