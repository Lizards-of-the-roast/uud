using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class UnknownRequestHandler : BaseUserRequestHandler<BaseUserRequest>
{
	public UnknownRequestHandler(BaseUserRequest request)
		: base(request)
	{
	}

	public override void HandleRequest()
	{
		Debug.LogWarning("Request could not be handled appropriately, let GRE respond.");
		_request.AutoRespond();
	}
}
