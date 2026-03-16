using System.Collections.Generic;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Events;

namespace Wizards.MDN.NodeGraph;

public static class NodeGraphUtil
{
	private const string COMPLETED_GROUP = "Completed_Group";

	private const string SKIP_GROUP = "Skip_Group";

	private const string MATCH_GROUP = "Match_Group";

	public static bool IsCompleted(this ClientGraphDefinition graph, CampaignGraphManager manager)
	{
		Dictionary<string, ClientNodeState> nodeStates = manager.GetState(graph).NodeStates;
		if (nodeStates != null)
		{
			foreach (string item in graph.GetCompletedGroup()?.Nodes ?? new List<string>())
			{
				if (nodeStates.TryGetValue(item, out var value) && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CanSkip(this ClientGraphDefinition graph, CampaignGraphManager manager)
	{
		Dictionary<string, ClientNodeState> nodeStates = manager.GetState(graph).NodeStates;
		if (nodeStates != null)
		{
			foreach (string item in graph.GetSkipGroup()?.Nodes ?? new List<string>())
			{
				if (nodeStates.TryGetValue(item, out var value) && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static ClientNodeGroupDefinition GetCompletedGroup(this ClientGraphDefinition graph)
	{
		return graph.getGroup("Completed_Group");
	}

	public static ClientNodeGroupDefinition GetMatchGroup(this ClientGraphDefinition graph)
	{
		return graph.getGroup("Match_Group");
	}

	public static ClientNodeGroupDefinition GetSkipGroup(this ClientGraphDefinition graph)
	{
		return graph.getGroup("Skip_Group");
	}

	private static ClientNodeGroupDefinition getGroup(this ClientGraphDefinition graph, string groupName)
	{
		foreach (ClientNodeGroupDefinition nodeGroup in graph.NodeGroups)
		{
			if (nodeGroup.Name == groupName)
			{
				return nodeGroup;
			}
		}
		return null;
	}
}
