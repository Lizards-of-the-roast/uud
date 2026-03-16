using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Cards.Text;

public interface ITextEntryParser
{
	IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null);
}
