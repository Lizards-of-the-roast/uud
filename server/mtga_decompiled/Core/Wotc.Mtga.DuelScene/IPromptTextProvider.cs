using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public interface IPromptTextProvider
{
	string GetPromptText(Prompt prompt);

	bool TryGetPromptText(Prompt prompt, out string result)
	{
		result = GetPromptText(prompt);
		return !string.IsNullOrEmpty(result);
	}
}
