using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.Cards.Text;

public class ParenthesisFormatingParser : ITextEntryParser
{
	public enum ParenthesisFormatType
	{
		Default,
		Added,
		Removed,
		Perpetual
	}

	private const string PARENTHESIS_FORMAT = "({0})";

	private readonly ITextEntryParser _nestedParser;

	private readonly ParenthesisFormatType _formatting;

	public ParenthesisFormatingParser(ParenthesisFormatType formatting, ITextEntryParser nestedParser)
	{
		_nestedParser = nestedParser ?? NullTextEntryParser.Default;
		_formatting = formatting;
	}

	public IEnumerable<ICardTextEntry> ParseText(ICardDataAdapter card, CardTextColorSettings colorSettings, string overrideLang = null)
	{
		foreach (ICardTextEntry item in _nestedParser.ParseText(card, colorSettings, overrideLang))
		{
			yield return new BasicTextEntry(string.Format(FormattedParenthesis(_formatting, colorSettings), item.GetText()));
		}
	}

	private static string FormattedParenthesis(ParenthesisFormatType format, CardTextColorSettings colorSettings)
	{
		return format switch
		{
			ParenthesisFormatType.Default => string.Format(colorSettings.DefaultFormat, "({0})"), 
			ParenthesisFormatType.Added => string.Format(colorSettings.AddedFormat, "({0})"), 
			ParenthesisFormatType.Removed => string.Format(colorSettings.RemovedFormat, "({0})"), 
			ParenthesisFormatType.Perpetual => string.Format(colorSettings.PerpetualFormat, "({0})"), 
			_ => string.Format(colorSettings.DefaultFormat, "({0})"), 
		};
	}
}
