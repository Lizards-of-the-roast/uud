using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

public class OptionalActionMessageRequestNPEStrategyHandler : BaseUserRequestHandler<OptionalActionMessageRequest>
{
	public OptionalActionMessageRequestNPEStrategyHandler(OptionalActionMessageRequest request)
		: base(request)
	{
	}

	public override void HandleRequest()
	{
		OptionResponse response = OptionResponse.AllowYes;
		_request.SubmitResponse(response);
	}
}
