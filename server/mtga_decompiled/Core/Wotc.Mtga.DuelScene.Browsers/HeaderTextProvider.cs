using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Browsers;

public class HeaderTextProvider : IHeaderTextProvider
{
	private readonly IProvider<string> _payloadTextProvider;

	private readonly IClientLocProvider _clientLocProvider;

	public HeaderTextProvider(IClientLocProvider clientLocProvider, IProvider<string> payloadTextProvider)
	{
		_payloadTextProvider = payloadTextProvider ?? NullProvider<string>.Default;
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public string GetText()
	{
		if (!_payloadTextProvider.TryGet(out var result))
		{
			return _clientLocProvider.GetLocalizedText("DuelScene/Browsers/Choose_Option_Title");
		}
		return result;
	}
}
