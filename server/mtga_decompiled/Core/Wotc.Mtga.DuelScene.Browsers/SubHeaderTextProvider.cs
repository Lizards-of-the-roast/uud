using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Browsers;

public class SubHeaderTextProvider : ISubHeaderTextProvider
{
	private readonly IClientLocProvider _clientLocProvider;

	private readonly IPromptTextProvider _promptTextProvider;

	private readonly IProvider<string> _payloadTextProvider;

	public SubHeaderTextProvider(IClientLocProvider clientLocProvider, IPromptTextProvider promptTextProvider, IProvider<string> payloadTextProvider)
	{
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
		_promptTextProvider = promptTextProvider ?? NullPromptTextProvider.Default;
		_payloadTextProvider = payloadTextProvider ?? NullProvider<string>.Default;
	}

	public string GetText(Prompt prompt = null, string defaultKey = "DuelScene/Browsers/Choose_Option_Text")
	{
		if (_payloadTextProvider.TryGet(out var result))
		{
			return result;
		}
		if (_promptTextProvider.TryGetPromptText(prompt, out var result2))
		{
			return result2;
		}
		return _clientLocProvider.GetLocalizedText(defaultKey);
	}
}
