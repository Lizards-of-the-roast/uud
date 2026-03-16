using Wizards.Arena.Models.Network;
using Wotc.Mtga.Providers;

namespace Core.Shared.Code.PVPChallenge;

public static class ChallengeDataUtils
{
	public static string GetTitleLocKey(string titleId, CosmeticsProvider cosmeticsProvider)
	{
		foreach (CosmeticTitleEntry playerOwnedTitle in cosmeticsProvider.PlayerOwnedTitles)
		{
			if (playerOwnedTitle.Id == titleId)
			{
				return playerOwnedTitle.LocKey;
			}
		}
		return "";
	}
}
