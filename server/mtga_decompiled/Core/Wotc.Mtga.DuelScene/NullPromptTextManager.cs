using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class NullPromptTextManager : IPromptTextManager, IPromptTextProvider, IPromptTextController
{
	public static readonly IPromptTextManager Default = new NullPromptTextManager();

	private static readonly IPromptTextProvider _provider = NullPromptTextProvider.Default;

	private static readonly IPromptTextController _controller = NullPromptTextController.Default;

	public string GetPromptText(Prompt prompt)
	{
		return _provider.GetPromptText(prompt);
	}

	public void SetPrompt(Prompt prompt)
	{
		_controller.SetPrompt(prompt);
	}

	public void SetClientPrompt(string key, params (string, string)[] parameters)
	{
		_controller.SetClientPrompt(key, parameters);
	}

	public void UpdateLanguage()
	{
	}
}
