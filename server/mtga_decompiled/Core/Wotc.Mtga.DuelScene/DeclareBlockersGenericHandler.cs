using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class DeclareBlockersGenericHandler : BaseUserRequestHandler<DeclareBlockersRequest>
{
	public DeclareBlockersGenericHandler(DeclareBlockersRequest request)
		: base(request)
	{
	}

	public override void HandleRequest()
	{
		_request.SubmitBlockers();
	}
}
