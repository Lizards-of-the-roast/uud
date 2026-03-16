using System;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Rules;

public class PayCostsRequestBotBattleStrategyHandler : BaseUserRequestHandler<PayCostsRequest>
{
	private const uint TOC_WAIVE_COST_GRPID = 993u;

	private readonly Random _random;

	public PayCostsRequestBotBattleStrategyHandler(PayCostsRequest decision, Random random)
		: base(decision)
	{
		_random = random;
	}

	public override void HandleRequest()
	{
		if (_request.PaymentActions != null)
		{
			foreach (Wotc.Mtgo.Gre.External.Messaging.Action action in _request.PaymentActions.Actions)
			{
				if (action.AbilityGrpId == 993)
				{
					_request.PaymentActions.SubmitAction(action);
					return;
				}
			}
		}
		new PayCostsRequestRandomHandler(_request, _random).HandleRequest();
	}
}
