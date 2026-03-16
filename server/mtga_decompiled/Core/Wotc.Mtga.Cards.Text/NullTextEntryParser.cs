using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Cards.Text;

public class NullTextEntryParser : ITextEntryParser
{
	public static readonly ITextEntryParser Default = new NullTextEntryParser();

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		yield break;
	}
}
