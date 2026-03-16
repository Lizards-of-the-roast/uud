namespace Wizards.Mtga.Credits;

public class NullCreditsTextProvider : ICreditsTextProvider
{
	public static readonly ICreditsTextProvider Default = new NullCreditsTextProvider();

	public string GetCreditsText()
	{
		return string.Empty;
	}

	public string GetUniversesBeyondHeaderText()
	{
		return string.Empty;
	}
}
