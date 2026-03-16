using System;
using System.Collections.Generic;
using GreClient.Network;
using Newtonsoft.Json.Linq;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public abstract class BotBattleTest
{
	public IHeadlessClientStrategy LocalPlayerStrategy;

	public IHeadlessClientStrategy OpponentStrategy;

	public DateTime StartTime;

	protected CardDatabase _cardDatabase;

	public BotBattleDSConfig DsConfig { get; private set; }

	public abstract JObject ToJObject();

	public abstract GreClient.Network.DeckConfig GetLocalPlayerDeck();

	public abstract GreClient.Network.DeckConfig GetOpponentDeck();

	public abstract bool IsComplete();

	public BotBattleTest(BotBattleDSConfig dsConfig, CardDatabase cardDatabase)
	{
		DsConfig = dsConfig;
		_cardDatabase = cardDatabase;
		StartTime = DateTime.Now;
	}

	public virtual void OnMatchCompleted()
	{
	}

	public GreClient.Network.MatchConfig CreateMatchConfig()
	{
		BotBattleSessionType sessionType = DsConfig.SessionType;
		bool flag = sessionType == BotBattleSessionType.SetTest || sessionType == BotBattleSessionType.CardTest;
		return new GreClient.Network.MatchConfig("Bot Battle", 1u, "LGS", GameType.Duel, GameVariant.Normal, MatchWinCondition.SingleElimination, (!flag) ? MulliganType.London : MulliganType.None, useSpecifiedSeed: false, Array.Empty<uint>(), 1u, 7u, ShuffleRestriction.None, MDNPlayerPrefs.UseTimers ? TimerPackage.V5 : TimerPackage.None, 1u, new List<GreClient.Network.TeamConfig>
		{
			new GreClient.Network.TeamConfig(new List<GreClient.Network.PlayerConfig>
			{
				new GreClient.Network.PlayerConfig("Local Player", PlayerType.Human, string.Empty, GetLocalPlayerDeck(), Array.Empty<(uint, string)>(), FamiliarStrategyType.None, ShuffleRestriction.None, 20u, 7u, flag && DsConfig.LocalPlayerCardsToTest.Count > 0, startingPlayer: true, Array.Empty<uint>(), string.Empty, "Avatar_Basic_AjaniGoldmane", string.Empty, (petId: string.Empty, variantId: string.Empty), "NoTitle", RankConfig.Default)
			}),
			new GreClient.Network.TeamConfig(new List<GreClient.Network.PlayerConfig>
			{
				new GreClient.Network.PlayerConfig("Opponent", PlayerType.Bot, string.Empty, GetOpponentDeck(), Array.Empty<(uint, string)>(), FamiliarStrategyType.None, ShuffleRestriction.None, 20u, 7u, flag && DsConfig.OpponentCardsToTest.Count > 0, startingPlayer: false, Array.Empty<uint>(), string.Empty, "Avatar_Basic_AjaniGoldmane", string.Empty, (petId: string.Empty, variantId: string.Empty), "NoTitle", RankConfig.Default)
			})
		});
	}
}
