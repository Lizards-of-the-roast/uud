using System;
using GreClient.Rules;

public class ActionsAvailableRequestBotBattleStrategyHandler : ActionsAvailableRequestRandomHandler
{
	private readonly SetTest _setTest;

	public ActionsAvailableRequestBotBattleStrategyHandler(ActionsAvailableRequest request, Random random, SetTest setTest)
		: base(request, random)
	{
		_setTest = setTest;
	}

	public override void HandleRequest()
	{
		if (_setTest.IsComplete())
		{
			_request.Concede();
		}
		else if (!_setTest.TryHandleSelectActionRequest(_request))
		{
			base.HandleRequest();
		}
	}
}
