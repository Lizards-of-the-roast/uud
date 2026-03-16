using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Meta.MainNavigation.SocialV2;

public class GatheringManager
{
	private readonly IGatheringServiceWrapper _gatheringServiceWrapper = Pantry.Get<IGatheringServiceWrapper>();

	private readonly List<Gathering> _gatherings = new List<Gathering>();

	public IReadOnlyCollection<Gathering> Gatherings => _gatherings;

	public void CreateGathering(string gatheringName, string gatheringPassword)
	{
		gatheringPassword = "abc123";
		GatheringCreateReq req = new GatheringCreateReq
		{
			GatheringName = gatheringName,
			GatheringPassword = gatheringPassword
		};
		_gatheringServiceWrapper.Create(req).Then(UpdateGatheringsPromise);
	}

	public void LeaveGathering(string gatheringId)
	{
		GatheringExitReq req = new GatheringExitReq
		{
			GatheringId = gatheringId
		};
		_gatheringServiceWrapper.Exit(req).Then(UpdateGatheringsPromise);
	}

	public void CloseGathering(string gatheringId, string playerId)
	{
		Gathering gathering = _gatherings.FirstOrDefault((Gathering gathering2) => gathering2.Id == gatheringId);
		if (!string.IsNullOrEmpty(gathering.Id) && !(gathering.OwnerId != playerId))
		{
			GatheringCloseReq req = new GatheringCloseReq
			{
				GatheringId = gatheringId
			};
			_gatheringServiceWrapper.Close(req).Then(UpdateGatheringsPromise);
		}
	}

	public void JoinGathering(string gatheringName)
	{
		GatheringJoinReq req = new GatheringJoinReq
		{
			GatheringId = gatheringName
		};
		_gatheringServiceWrapper.Join(req).Then(UpdateGatheringsPromise);
	}

	public void UpdateGatherings()
	{
		_gatheringServiceWrapper.ReconnectAll().Then(delegate(Promise<List<Wizards.Arena.Models.Network.Gathering>> gatherings)
		{
			if (gatherings.Successful)
			{
				List<Gathering> collection = gatherings.Result.ConvertAll((Wizards.Arena.Models.Network.Gathering gathering) => (Gathering)gathering);
				_gatherings.Clear();
				_gatherings.AddRange(collection);
			}
			else
			{
				SimpleLog.LogError($"Error reconnecting all the gatherings.\nError Code: {gatherings.Error.Code}\nMessage: {gatherings.Error.Message}\nException: {gatherings.Error.Exception}");
			}
		});
	}

	private void UpdateGatheringsPromise<T>(Promise<T> promise)
	{
		if (promise.Successful)
		{
			UpdateGatherings();
		}
	}
}
