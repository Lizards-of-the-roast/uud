using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class NullPromptTextProvider : IPromptTextProvider
{
	public static readonly IPromptTextProvider Default = new NullPromptTextProvider();

	public string GetPromptText(Prompt prompt)
	{
		return string.Empty;
	}
}
