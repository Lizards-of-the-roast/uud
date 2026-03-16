namespace Wotc.Mtga.Cards.Text;

public class BasicTextEntry : ICardTextEntry
{
	private readonly string _rawContent;

	public BasicTextEntry(string rawContent)
	{
		_rawContent = rawContent;
	}

	public string GetText()
	{
		return _rawContent;
	}
}
