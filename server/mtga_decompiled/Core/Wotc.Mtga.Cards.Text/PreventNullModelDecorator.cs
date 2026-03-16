using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Cards.Text;

public class PreventNullModelDecorator : ITextEntryParser
{
	private readonly ITextEntryParser _nested;

	public PreventNullModelDecorator(ITextEntryParser nested)
	{
		_nested = nested ?? NullTextEntryParser.Default;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		if (ShouldPrevent(card))
		{
			return Array.Empty<ICardTextEntry>();
		}
		return _nested.ParseText(card, colorSettings, overrideLang);
	}

	private static bool ShouldPrevent(ICardDataAdapter card)
	{
		return card == null;
	}
}
