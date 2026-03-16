using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Wizards.Mtga.PrivateGame.Challenges;

public static class ChallengeSpinnerDeckTypes
{
	public const int DefaultIndex = 0;

	public static readonly OrderedDictionary All = new OrderedDictionary
	{
		{
			ChallengeSpinnerDeckTypeKey.Standard,
			new ChallengeSpinnerDeckType
			{
				Label = "MainNav/PrivateGame/DeckType_60_Card",
				AllowBo3 = true
			}
		},
		{
			ChallengeSpinnerDeckTypeKey.Brawl,
			new ChallengeSpinnerDeckType
			{
				Label = "MainNav/PrivateGame/DeckType_Brawl",
				AllowBo3 = false
			}
		},
		{
			ChallengeSpinnerDeckTypeKey.Limited,
			new ChallengeSpinnerDeckType
			{
				Label = "MainNav/PrivateGame/DeckType_40_Card",
				AllowBo3 = true
			}
		},
		{
			ChallengeSpinnerDeckTypeKey.Alchemy,
			new ChallengeSpinnerDeckType
			{
				Label = "MainNav/PrivateGame/DeckType_60_Card_Alchemy",
				AllowBo3 = true
			}
		}
	};

	public static readonly List<string> Labels = (from ChallengeSpinnerDeckType x in All.Values
		select x.Label).ToList();
}
