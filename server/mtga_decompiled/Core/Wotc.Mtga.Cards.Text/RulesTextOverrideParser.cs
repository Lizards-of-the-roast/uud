using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Cards.Text;

public class RulesTextOverrideParser : ITextEntryParser
{
	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		string rulesText = card.RulesTextOverride.GetOverride(colorSettings);
		rulesText = Utilities.SanitizeParentheticalText(rulesText);
		yield return new BasicTextEntry(rulesText);
	}
}
