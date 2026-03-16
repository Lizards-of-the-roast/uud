namespace Wizards.Mtga.PrivateGame.Challenges;

public readonly struct ChallengeTypeInfo
{
	public readonly ChallengeSpinnerMatchTypeKey MatchType;

	public readonly string DeckFormatName;

	public readonly ChallengeSpinnerDeckTypeKey? DeckType;

	public ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey matchType, string deckFormatName, ChallengeSpinnerDeckTypeKey? deckType = null)
	{
		MatchType = matchType;
		DeckFormatName = deckFormatName;
		DeckType = deckType;
	}
}
