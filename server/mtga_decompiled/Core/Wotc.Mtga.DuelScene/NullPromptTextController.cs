using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class NullPromptTextController : IPromptTextController
{
	public static readonly IPromptTextController Default = new NullPromptTextController();

	public void SetPrompt(Prompt prompt)
	{
	}

	public void SetClientPrompt(string key, params (string, string)[] parameters)
	{
	}
}
