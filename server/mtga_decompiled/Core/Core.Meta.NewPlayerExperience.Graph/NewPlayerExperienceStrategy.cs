using System.Collections.Generic;
using System.Threading.Tasks;
using Wotc.Mtga.Events;

namespace Core.Meta.NewPlayerExperience.Graph;

public class NewPlayerExperienceStrategy : CampaignGraphStrategy
{
	private const string UNLOCK_PLAY_MODES_NODE_ID = "UnlockPlayModes";

	protected override string GraphName => "NewPlayerExperience";

	public Task<bool> OpenedDualColorPreconEvent => GetMilestoneStatus("OpenDualColorPreconEvent");

	public Task<bool> SparkQueueOpened => GetMilestoneStatus("OpenSparkQueue");

	public Task<bool> GraduatedSparkQueue => GetMilestoneStatus("GraduateSparkRank");

	public Task<bool> NpeCompleted => GetMilestoneStatus("NPE_Completed");

	public static NewPlayerExperienceStrategy Create()
	{
		return new NewPlayerExperienceStrategy();
	}

	public async Task<Dictionary<string, bool>> GetNpeGraphMilestones()
	{
		await WaitUntilInitialized();
		if (!base.Initialized)
		{
			return null;
		}
		return _state?.MilestoneStates;
	}

	public async Task<bool> GetMilestoneStatus(string milestoneName)
	{
		await WaitUntilInitialized();
		if (!base.Initialized)
		{
			return false;
		}
		bool value = false;
		_state?.MilestoneStates?.TryGetValue(milestoneName, out value);
		return value;
	}

	public async Task Skip()
	{
		await WaitUntilInitialized();
		if (base.Initialized && _graph.Nodes.TryGetValue("UnlockPlayModes", out var value))
		{
			_manager.ProcessNode(_graph, value);
		}
	}
}
