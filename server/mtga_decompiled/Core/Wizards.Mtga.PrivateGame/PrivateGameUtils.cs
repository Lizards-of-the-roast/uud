using System.Collections.Generic;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using Wizards.Mtga.PrivateGame.Challenges;

namespace Wizards.Mtga.PrivateGame;

public static class PrivateGameUtils
{
	public static string GetDeckTypeFromSpinners(int matchType, int deckType)
	{
		return GetDeckTypeFromSpinners((ChallengeSpinnerMatchTypeKey)matchType, (ChallengeSpinnerDeckTypeKey)deckType);
	}

	private static string GetDeckTypeFromSpinners(ChallengeSpinnerMatchTypeKey matchType, ChallengeSpinnerDeckTypeKey deckType)
	{
		foreach (var (_, challengeTypeInfo2) in ChallengeUtils.MatchTypeToInfo)
		{
			if (matchType == challengeTypeInfo2.MatchType && (((ChallengeSpinnerMatchType)ChallengeSpinnerMatchTypes.All[matchType]).IsTournament || deckType == challengeTypeInfo2.DeckType))
			{
				return challengeTypeInfo2.DeckFormatName;
			}
		}
		return "";
	}

	public static ChallengeMatchTypes GetGameModeFromSpinners(int matchType, int deckType)
	{
		return GetGameModeFromSpinners((ChallengeSpinnerMatchTypeKey)matchType, (ChallengeSpinnerDeckTypeKey)deckType);
	}

	private static ChallengeMatchTypes GetGameModeFromSpinners(ChallengeSpinnerMatchTypeKey matchType, ChallengeSpinnerDeckTypeKey deckType)
	{
		foreach (var (result, challengeTypeInfo2) in ChallengeUtils.MatchTypeToInfo)
		{
			if (matchType == challengeTypeInfo2.MatchType && (((ChallengeSpinnerMatchType)ChallengeSpinnerMatchTypes.All[matchType]).IsTournament || deckType == challengeTypeInfo2.DeckType))
			{
				return result;
			}
		}
		return ChallengeMatchTypes.DirectGame;
	}
}
