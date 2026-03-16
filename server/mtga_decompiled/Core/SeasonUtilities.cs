using System.Collections.Generic;
using Core.Shared.Code.ClientModels;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Loc;

public class SeasonUtilities
{
	public static string GetSeasonDisplayName(int ordinal)
	{
		return Languages.ActiveLocProvider.GetLocalizedText("MainNav/Season/Season" + ordinal + "Name");
	}

	public static int GetStepsForRank(RankInfo rankInfo, bool isConstructed, Client_SeasonAndRankInfo seasonDetails)
	{
		if (seasonDetails != null)
		{
			List<Client_RankDefinition> list = (isConstructed ? seasonDetails.constructedRankInfo : seasonDetails.limitedRankInfo);
			Client_RankDefinition client_RankDefinition = null;
			if (list != null)
			{
				client_RankDefinition = list.Find((Client_RankDefinition ri) => ri.rankClass == rankInfo.rankClass && ri.level == rankInfo.level);
			}
			return client_RankDefinition?.steps ?? 0;
		}
		return 0;
	}
}
