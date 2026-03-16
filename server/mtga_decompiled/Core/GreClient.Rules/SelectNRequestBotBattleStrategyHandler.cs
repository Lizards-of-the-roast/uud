using System;
using Wotc.Mtga.Cards.Database;

namespace GreClient.Rules;

public class SelectNRequestBotBattleStrategyHandler : BaseUserRequestHandler<SelectNRequest>
{
	private readonly Random _random;

	private readonly SetTest _setTest;

	private readonly ICardDatabaseAdapter _cardDatabase;

	public SelectNRequestBotBattleStrategyHandler(SelectNRequest request, SetTest setTest, Random random, ICardDatabaseAdapter cardDatabase)
		: base(request)
	{
		_setTest = setTest;
		_random = random;
		_cardDatabase = cardDatabase;
	}

	public override void HandleRequest()
	{
		if (!_setTest.TryHandleWish(_request))
		{
			new SelectNRequestRandomHandler(_request, _random, _cardDatabase).HandleRequest();
		}
	}
}
