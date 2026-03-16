using System;
using Core.Shared.Code.ClientModels;
using Wizards.Arena.Promises;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Code.Harnesses.OfflineHarnessServices;

public class HarnessRankServiceProvider : IPlayerRankServiceWrapper
{
	public CombinedRankInfo CombinedRank { get; set; }

	public RankProgress RankProgress { get; }

	public MythicRatingUpdated MythicRatingUpdated { get; }

	public event Action<CombinedRankInfo> OnCombinedRankUpdated;

	public Promise<CombinedRankInfo> GetPlayerRankInfo()
	{
		throw new NotImplementedException();
	}

	public Promise<Client_SeasonAndRankInfo> GetSeasonAndRankDetail()
	{
		throw new NotImplementedException();
	}

	public Promise<EventAndSeasonPayouts> GetEventAndSeasonPayouts()
	{
		throw new NotImplementedException();
	}
}
