namespace Wotc.Mtga.DuelScene.Browsers;

public class NullHeaderTextProvider : IHeaderTextProvider
{
	public static readonly IHeaderTextProvider Default = new NullHeaderTextProvider();

	public string GetText()
	{
		return string.Empty;
	}
}
