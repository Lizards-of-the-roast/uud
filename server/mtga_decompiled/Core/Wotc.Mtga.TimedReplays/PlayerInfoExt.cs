using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;

namespace Wotc.Mtga.TimedReplays;

public static class PlayerInfoExt
{
	public static void SetCosmetics(this MatchManager.PlayerInfo info, PlayerCosmetics cosmetics)
	{
		info.ScreenName = cosmetics.ScreenName ?? "";
		info.RankingClass = (RankingClassType)cosmetics.RankingClass;
		info.RankingTier = cosmetics.RankingTier;
		info.MythicPercentile = cosmetics.MythicPercentile;
		info.MythicPlacement = cosmetics.MythicPlacement;
		info.AvatarSelection = cosmetics.AvatarSelection;
		if (cosmetics.PetSelectionName != null)
		{
			info.PetSelection = new ClientPetSelection
			{
				name = cosmetics.PetSelectionName,
				variant = cosmetics.PetSelectionVariant
			};
		}
		info.CommanderGrpIds = cosmetics.CommanderGrpIds;
		info.EmoteSelection = cosmetics.EmoteSelection;
		info.SleeveSelection = cosmetics.SleeveSelection;
		info.TitleSelection = cosmetics.TitleSelection;
	}
}
