using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Cards.Text;

public class TextParserAggregate : ITextEntryParser
{
	private readonly ITextEntryParser[] _parsers;

	public TextParserAggregate(params ITextEntryParser[] parsers)
	{
		_parsers = parsers ?? Array.Empty<ITextEntryParser>();
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		ITextEntryParser[] parsers = _parsers;
		foreach (ITextEntryParser textEntryParser in parsers)
		{
			foreach (ICardTextEntry item in textEntryParser.ParseText(card, colorSettings, overrideLang))
			{
				yield return item;
			}
		}
	}
}
