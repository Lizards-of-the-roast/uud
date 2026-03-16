using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Loc;

public class RankUtilities
{
	private const string UNKNOWN_RANK_NAME = "Unknown";

	public static string GetClassDisplayName(RankingClassType rankClassEnum)
	{
		IClientLocProvider activeLocProvider = Languages.ActiveLocProvider;
		string text = RankLocKey(rankClassEnum);
		if (activeLocProvider != null && !string.IsNullOrEmpty(text))
		{
			return activeLocProvider.GetLocalizedText(text);
		}
		return "Unknown";
	}

	private static string RankLocKey(RankingClassType rankClassEnum)
	{
		return rankClassEnum switch
		{
			RankingClassType.Spark => "Rank/Rank_Spark", 
			RankingClassType.Bronze => "Rank/Rank_Bronze", 
			RankingClassType.Silver => "Rank/Rank_Silver", 
			RankingClassType.Gold => "Rank/Rank_Gold", 
			RankingClassType.Diamond => "Rank/Rank_Diamond", 
			RankingClassType.Platinum => "Rank/Rank_Platinum", 
			RankingClassType.Master => "Rank/Rank_Master", 
			RankingClassType.Mythic => "Rank/Rank_Mythic", 
			_ => string.Empty, 
		};
	}
}
