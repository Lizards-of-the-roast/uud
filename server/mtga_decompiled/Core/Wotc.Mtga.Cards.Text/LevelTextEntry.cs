namespace Wotc.Mtga.Cards.Text;

public class LevelTextEntry : ICardTextEntry
{
	private readonly string _cost;

	private readonly string _header;

	public LevelTextEntry(string cost, string header)
	{
		_cost = cost ?? string.Empty;
		_header = header ?? string.Empty;
	}

	public string GetText()
	{
		return _header;
	}

	public string GetCost()
	{
		return _cost;
	}
}
