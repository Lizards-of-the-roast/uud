using System;
using GreClient.Network;
using Newtonsoft.Json.Linq;
using Wotc.Mtga.Cards.Database;

public class DeckTest : BotBattleTest
{
	private uint _matchesCompleted;

	private DeckConfig _localPlayerDeck;

	private DeckConfig _opponentDeck;

	public DeckTest(BotBattleDSConfig dsConfig, CardDatabase cardDatabase)
		: base(dsConfig, cardDatabase)
	{
		_localPlayerDeck = new DeckConfig(string.Empty, base.DsConfig.LocalPlayerCardsToTest[0], Array.Empty<uint>(), Array.Empty<uint>(), 0u);
		_localPlayerDeck = new DeckConfig(string.Empty, base.DsConfig.LocalPlayerCardsToTest[1], Array.Empty<uint>(), Array.Empty<uint>(), 0u);
		LocalPlayerStrategy = getStrategyForStratType(base.DsConfig.LocalPlayerStrategy);
		OpponentStrategy = getStrategyForStratType(base.DsConfig.OpponentStrategy);
		IHeadlessClientStrategy getStrategyForStratType(BotBattleStrategyType botBattleStrategyType)
		{
			return botBattleStrategyType switch
			{
				BotBattleStrategyType.Random => new RequestHandlerStrategy(RandomRequestHandlers.Create(_cardDatabase)), 
				BotBattleStrategyType.Goldfish => new GoldfishStrategy(10u), 
				_ => null, 
			};
		}
	}

	public override DeckConfig GetLocalPlayerDeck()
	{
		return _localPlayerDeck;
	}

	public override DeckConfig GetOpponentDeck()
	{
		return _opponentDeck;
	}

	public override JObject ToJObject()
	{
		return new JObject { ["MatchesComplete"] = _matchesCompleted };
	}

	public override bool IsComplete()
	{
		if (0 > base.DsConfig.MatchesToPlay)
		{
			return false;
		}
		if (_matchesCompleted < base.DsConfig.MatchesToPlay)
		{
			return false;
		}
		return true;
	}

	public override void OnMatchCompleted()
	{
		_matchesCompleted++;
	}
}
