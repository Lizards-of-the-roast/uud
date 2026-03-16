namespace Wizards.Mtga.PrivateGame.Challenges;

public struct ChallengeSpinnerMatchType
{
	public string Label;

	public string TournamentText;

	public bool IsTournament => TournamentText != null;
}
