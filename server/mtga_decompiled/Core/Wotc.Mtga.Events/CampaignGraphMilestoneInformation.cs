using Wizards.Mtga;
using Wizards.Unification.Models.Graph;

namespace Wotc.Mtga.Events;

public struct CampaignGraphMilestoneInformation
{
	private string _graphName;

	private string _milestoneId;

	public CampaignGraphMilestoneInformation(string graphName, string milestoneId)
	{
		_graphName = graphName;
		_milestoneId = milestoneId;
	}

	public bool? MilestoneCompleted(CampaignGraphManager graphManager = null)
	{
		if (graphManager == null)
		{
			graphManager = Pantry.Get<CampaignGraphManager>();
		}
		graphManager.TryGetDefinitions(out var definitions);
		definitions.TryGetValue(_graphName, out var value);
		string milestoneId = _milestoneId;
		if (value == null || !value.Milestones.Exists((ClientGraphMilestone x) => x.Nodes.Count > 0 && x.Name == milestoneId))
		{
			return null;
		}
		graphManager.TryGetState(_graphName, out var state);
		state.MilestoneStates.TryGetValue(_milestoneId, out var value2);
		return value2;
	}
}
