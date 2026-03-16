using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Cards.Text;

public class XChoiceParser : ITextEntryParser
{
	private const string ABIL_WORD_VALUEOFX = "ValueOfX";

	private const string ABIL_WORD_XVALUEFROMPARENT = "WhereXisValueFromParentAbility";

	private const string LOC_PARAM_XCHOICE = "xChoice";

	private readonly IClientLocProvider _locManager;

	public XChoiceParser(IClientLocProvider locManager)
	{
		_locManager = locManager ?? NullLocProvider.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		string xChoiceValue = GetXChoiceValue(card.Instance);
		if (!string.IsNullOrEmpty(xChoiceValue))
		{
			yield return new BasicTextEntry(string.Format(colorSettings.AddedFormat, _locManager.GetLocalizedTextForLanguage("DuelScene/RuleText/CastingTimeOption_XChoice", overrideLang, ("xChoice", xChoiceValue))));
		}
	}

	private string GetXChoiceValue(MtgCardInstance card)
	{
		if (card.ChooseXResult.HasValue)
		{
			return card.ChooseXResult.Value.ToString();
		}
		using (IEnumerator<AbilityWordData> enumerator = AbilityWordData.GetMatchingAbilityWords(card, "ValueOfX").GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.AdditionalDetail;
			}
		}
		using (IEnumerator<AbilityWordData> enumerator = AbilityWordData.GetMatchingAbilityWords(card, "WhereXisValueFromParentAbility").GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current.AdditionalDetail;
			}
		}
		return string.Empty;
	}
}
