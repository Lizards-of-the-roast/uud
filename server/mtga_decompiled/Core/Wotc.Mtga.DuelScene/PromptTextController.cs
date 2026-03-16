using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class PromptTextController : IPromptTextController
{
	private readonly IClientLocProvider _clientLocProvider;

	private readonly IPromptTextProvider _promptTextProvider;

	private readonly ISignalDispatch<PromptTextUpdatedSignalArgs> _promptUpdatedSignal;

	public PromptTextController(IClientLocProvider clientLocProvider, IPromptTextProvider promptTextProvider, ISignalDispatch<PromptTextUpdatedSignalArgs> promptUpdatedSignal)
	{
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
		_promptTextProvider = promptTextProvider ?? NullPromptTextProvider.Default;
		_promptUpdatedSignal = promptUpdatedSignal;
	}

	public void SetPrompt(Prompt prompt)
	{
		UpdatePromptText(_promptTextProvider.GetPromptText(prompt));
	}

	public void SetClientPrompt(string key, params (string, string)[] parameters)
	{
		UpdatePromptText(_clientLocProvider.GetLocalizedText(key, parameters));
	}

	private void UpdatePromptText(string text)
	{
		_promptUpdatedSignal.Dispatch(new PromptTextUpdatedSignalArgs(this, text));
	}
}
