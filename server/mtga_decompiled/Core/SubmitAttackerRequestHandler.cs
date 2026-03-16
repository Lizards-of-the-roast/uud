using GreClient.Rules;

public class SubmitAttackerRequestHandler : BaseUserRequestHandler<DeclareAttackerRequest>
{
	public SubmitAttackerRequestHandler(DeclareAttackerRequest request)
		: base(request)
	{
	}

	public override void HandleRequest()
	{
		_request.SubmitAttackers();
	}
}
