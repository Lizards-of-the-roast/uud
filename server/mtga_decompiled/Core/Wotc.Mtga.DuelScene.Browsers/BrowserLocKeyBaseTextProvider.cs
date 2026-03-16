using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Browser;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Browsers;

public class BrowserLocKeyBaseTextProvider<T> : IProvider<string> where T : BrowserLocKeyBase
{
	private readonly IClientLocProvider _locProvider;

	private readonly IBlackboard _blackboard;

	private readonly IPayloadProvider<T> _payloadProvider;

	public BrowserLocKeyBaseTextProvider(IClientLocProvider locProvider, IBlackboard blackboard, IPayloadProvider<T> payloadProvider)
	{
		_locProvider = locProvider ?? NullLocProvider.Default;
		_blackboard = blackboard;
		_payloadProvider = payloadProvider ?? NullPayloadProvider<T>.Default;
	}

	public string Get()
	{
		if (!_payloadProvider.TryGetPayload(_blackboard, out var payload))
		{
			return null;
		}
		return _locProvider.GetLocalizedText(payload.LocKey, payload.GetLocParams(_blackboard));
	}
}
