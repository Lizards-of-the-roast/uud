using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public class NullSubHeaderTextProvider : ISubHeaderTextProvider
{
	public static readonly ISubHeaderTextProvider Default = new NullSubHeaderTextProvider();

	public string GetText(Prompt prompt = null, string defaultKey = null)
	{
		return string.Empty;
	}
}
