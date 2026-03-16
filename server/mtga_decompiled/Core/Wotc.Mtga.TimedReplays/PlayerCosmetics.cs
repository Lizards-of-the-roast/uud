using System.Collections.Generic;

namespace Wotc.Mtga.TimedReplays;

public class PlayerCosmetics
{
	public string ScreenName;

	public int RankingClass;

	public int RankingTier;

	public float MythicPercentile;

	public int MythicPlacement;

	public string AvatarSelection;

	public string PetSelectionName;

	public string PetSelectionVariant;

	public IReadOnlyList<uint> CommanderGrpIds;

	public string SleeveSelection;

	public string TitleSelection;

	public List<string> EmoteSelection;

	public PlayerCosmetics()
	{
	}

	public PlayerCosmetics(MatchManager.PlayerInfo info)
	{
		ScreenName = info.ScreenName;
		RankingClass = (int)info.RankingClass;
		RankingTier = info.RankingTier;
		MythicPercentile = info.MythicPercentile;
		MythicPlacement = info.MythicPlacement;
		AvatarSelection = info.AvatarSelection;
		PetSelectionName = info.PetSelection?.name;
		PetSelectionVariant = info.PetSelection?.variant;
		CommanderGrpIds = info.CommanderGrpIds;
		EmoteSelection = info.EmoteSelection;
		SleeveSelection = info.SleeveSelection;
		TitleSelection = info.TitleSelection;
	}
}
