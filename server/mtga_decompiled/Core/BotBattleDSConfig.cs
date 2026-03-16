using System.Collections.Generic;

public class BotBattleDSConfig
{
	public string FileName;

	public BotBattleSessionType SessionType;

	public BotBattleStrategyType LocalPlayerStrategy;

	public BotBattleStrategyType OpponentStrategy;

	public int MatchesToPlay;

	public List<List<uint>> LocalPlayerCardsToTest = new List<List<uint>>();

	public List<List<uint>> OpponentCardsToTest = new List<List<uint>>();
}
