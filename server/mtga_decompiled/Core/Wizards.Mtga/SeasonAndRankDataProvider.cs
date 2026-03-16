using Core.Shared.Code.ClientModels;
using Wizards.Arena.Promises;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wizards.Mtga;

public class SeasonAndRankDataProvider
{
	private IPlayerRankServiceWrapper _rankServiceWrapper;

	public bool Initialized { get; private set; }

	public Client_SeasonAndRankInfo SeasonInfo { get; private set; }

	public static SeasonAndRankDataProvider Create()
	{
		return new SeasonAndRankDataProvider(Pantry.Get<IPlayerRankServiceWrapper>());
	}

	public SeasonAndRankDataProvider(IPlayerRankServiceWrapper rankServiceWrapper)
	{
		_rankServiceWrapper = rankServiceWrapper;
	}

	public Promise<Client_SeasonAndRankInfo> Refresh()
	{
		Initialized = false;
		SeasonInfo = null;
		return _rankServiceWrapper.GetSeasonAndRankDetail().IfSuccess(delegate(Promise<Client_SeasonAndRankInfo> promise)
		{
			SeasonInfo = promise.Result;
		}).Then(delegate
		{
			Initialized = true;
		});
	}
}
