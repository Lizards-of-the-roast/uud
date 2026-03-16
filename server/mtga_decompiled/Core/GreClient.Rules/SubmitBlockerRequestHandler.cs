namespace GreClient.Rules;

public class SubmitBlockerRequestHandler : BaseUserRequestHandler<DeclareBlockersRequest>
{
	public SubmitBlockerRequestHandler(DeclareBlockersRequest request)
		: base(request)
	{
	}

	public override void HandleRequest()
	{
		_request.SubmitBlockers();
	}
}
