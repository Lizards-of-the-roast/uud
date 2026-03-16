using Wizards.Mtga.FrontDoorModels;

public class PostMatchContext
{
	public bool GameEndAffectsQuest = true;

	public bool WonGame;

	public int GamesWon;

	public bool MatchesOfThisEventTypeCanAffectDailyWeeklyWins = true;

	public PostMatchClientUpdate PostMatchClientUpdate;

	public PostMatchContext GetCopy()
	{
		return new PostMatchContext
		{
			WonGame = WonGame,
			GameEndAffectsQuest = GameEndAffectsQuest,
			GamesWon = GamesWon,
			MatchesOfThisEventTypeCanAffectDailyWeeklyWins = MatchesOfThisEventTypeCanAffectDailyWeeklyWins,
			PostMatchClientUpdate = PostMatchClientUpdate
		};
	}
}
