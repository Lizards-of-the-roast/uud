using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class DeclareAttackersGenericHandler : BaseUserRequestHandler<DeclareAttackerRequest>
{
	public DeclareAttackersGenericHandler(DeclareAttackerRequest request)
		: base(request)
	{
	}

	public override void HandleRequest()
	{
		if (_request.DeclaredAttackers.Count > 0)
		{
			_request.SubmitAttackers();
		}
		else
		{
			_request.DeclareAllAttackers(_request.Attackers[0].LegalDamageRecipients[0]);
		}
	}
}
