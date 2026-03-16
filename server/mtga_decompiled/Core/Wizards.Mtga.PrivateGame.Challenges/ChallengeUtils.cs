using System.Collections.Generic;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;

namespace Wizards.Mtga.PrivateGame.Challenges;

public static class ChallengeUtils
{
	public static readonly Dictionary<ChallengeMatchTypes, ChallengeTypeInfo> MatchTypeToInfo = new Dictionary<ChallengeMatchTypes, ChallengeTypeInfo>
	{
		{
			ChallengeMatchTypes.DirectGame,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.ChallengeMatch, "DirectGame", ChallengeSpinnerDeckTypeKey.Standard)
		},
		{
			ChallengeMatchTypes.DirectGameBrawl,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.ChallengeMatch, "DirectGameBrawlRebalanced", ChallengeSpinnerDeckTypeKey.Brawl)
		},
		{
			ChallengeMatchTypes.DirectGameLimited,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.ChallengeMatch, "DirectGameLimited", ChallengeSpinnerDeckTypeKey.Limited)
		},
		{
			ChallengeMatchTypes.DirectGameAlchemy,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.ChallengeMatch, "DirectGameAlchemy", ChallengeSpinnerDeckTypeKey.Alchemy)
		},
		{
			ChallengeMatchTypes.DirectGameTournamentMode,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.TournamentMatch, "TraditionalStandard")
		},
		{
			ChallengeMatchTypes.DirectGameTournamentLimited,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.LimitedTournamentMatch, "DirectGameLimited")
		},
		{
			ChallengeMatchTypes.DirectGameTournamentHistoric,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.HistoricTournamentMatch, "TraditionalHistoric")
		},
		{
			ChallengeMatchTypes.DirectGameTournamentAlchemy,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.AlchemyTournamentMatch, "TraditionalAlchemy")
		},
		{
			ChallengeMatchTypes.DirectGameTournamentExplorer,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.ExplorerTournamentMatch, "TraditionalExplorer")
		},
		{
			ChallengeMatchTypes.DirectGameTournamentTimeless,
			new ChallengeTypeInfo(ChallengeSpinnerMatchTypeKey.TimelessTournamentMatch, "TraditionalTimeless")
		}
	};
}
