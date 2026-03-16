using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Wizards.Mtga.PrivateGame.Challenges;

public static class ChallengeSpinnerMatchTypes
{
	public static readonly OrderedDictionary All = new OrderedDictionary
	{
		{
			ChallengeSpinnerMatchTypeKey.ChallengeMatch,
			new ChallengeSpinnerMatchType
			{
				Label = "MainNav/PrivateGame/ChallengeMatch",
				TournamentText = null
			}
		},
		{
			ChallengeSpinnerMatchTypeKey.TournamentMatch,
			new ChallengeSpinnerMatchType
			{
				Label = "MainNav/PrivateGame/TournamentMatch",
				TournamentText = "MainNav/PrivateGame/TraditionalStandardCards"
			}
		},
		{
			ChallengeSpinnerMatchTypeKey.LimitedTournamentMatch,
			new ChallengeSpinnerMatchType
			{
				Label = "MainNav/PrivateGame/LimitedTournamentMatch",
				TournamentText = "MainNav/PrivateGame/TraditionalLimitedCards"
			}
		},
		{
			ChallengeSpinnerMatchTypeKey.HistoricTournamentMatch,
			new ChallengeSpinnerMatchType
			{
				Label = "MainNav/PrivateGame/HistoricTournamentMatch",
				TournamentText = "MainNav/PrivateGame/HistoricCards"
			}
		},
		{
			ChallengeSpinnerMatchTypeKey.AlchemyTournamentMatch,
			new ChallengeSpinnerMatchType
			{
				Label = "MainNav/PrivateGame/AlchemyTournamentMatch",
				TournamentText = "MainNav/PrivateGame/AlchemyCards"
			}
		},
		{
			ChallengeSpinnerMatchTypeKey.ExplorerTournamentMatch,
			new ChallengeSpinnerMatchType
			{
				Label = "MainNav/PrivateGame/ExplorerTournamentMatch",
				TournamentText = "MainNav/PrivateGame/ExplorerCards"
			}
		},
		{
			ChallengeSpinnerMatchTypeKey.TimelessTournamentMatch,
			new ChallengeSpinnerMatchType
			{
				Label = "MainNav/PrivateGame/TimelessTournamentMatch",
				TournamentText = "MainNav/PrivateGame/TimelessCards"
			}
		}
	};

	public static readonly int DefaultIndex = 0;

	public static readonly IEnumerable<string> Labels = (from ChallengeSpinnerMatchType x in All.Values
		select x.Label).ToList();
}
