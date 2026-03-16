using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public interface IPromptTextController
{
	void SetPrompt(Prompt prompt);

	void SetClientPrompt(string key, params (string, string)[] parameters);
}
