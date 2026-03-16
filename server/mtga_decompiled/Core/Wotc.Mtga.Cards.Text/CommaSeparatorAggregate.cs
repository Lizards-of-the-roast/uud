using System;
using System.Collections.Generic;
using System.Text;
using GreClient.CardData;

namespace Wotc.Mtga.Cards.Text;

public class CommaSeparatorAggregate : ITextEntryParser
{
	private readonly ITextEntryParser[] _parsers;

	private readonly StringBuilder _stringBuilder = new StringBuilder();

	private const string COMMA_SEPARATOR_STRING = ", ";

	public CommaSeparatorAggregate(params ITextEntryParser[] parsers)
	{
		_parsers = parsers ?? Array.Empty<ITextEntryParser>();
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		_stringBuilder.Clear();
		ITextEntryParser[] parsers = _parsers;
		for (int i = 0; i < parsers.Length; i++)
		{
			foreach (ICardTextEntry item in parsers[i].ParseText(card, colorSettings, overrideLang))
			{
				if (_stringBuilder.Length != 0)
				{
					_stringBuilder.Append(string.Format(colorSettings.AddedFormat, ", "));
				}
				_stringBuilder.Append(item.GetText());
			}
		}
		if (_stringBuilder.Length != 0)
		{
			yield return new BasicTextEntry(_stringBuilder.ToString());
		}
		_stringBuilder.Clear();
	}
}
