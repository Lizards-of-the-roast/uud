using System;
using System.Collections.Generic;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models.Graph;

namespace Core.MainNavigation.RewardTrack;

public class ClientRewardWebDiff
{
	public HashSet<int> currentUnlockedNodes = new HashSet<int>();

	public HashSet<int> currentAvailableNodes = new HashSet<int>();

	public HashSet<int> oldUnlockedNodes = new HashSet<int>();

	public HashSet<int> oldAvailableNodes = new HashSet<int>();

	public List<ClientInventoryUpdateReportItem> inventoryUpdates;

	public ClientRewardWebDiff(RewardWebDiff webDiff)
	{
		if (webDiff.currentUnlockedNodes != null)
		{
			foreach (int currentUnlockedNode in webDiff.currentUnlockedNodes)
			{
				currentUnlockedNodes.Add(currentUnlockedNode);
			}
		}
		if (webDiff.currentAvailableNodes != null)
		{
			foreach (int currentAvailableNode in webDiff.currentAvailableNodes)
			{
				currentAvailableNodes.Add(currentAvailableNode);
			}
		}
		if (webDiff.oldUnlockedNodes != null)
		{
			foreach (int oldUnlockedNode in webDiff.oldUnlockedNodes)
			{
				oldUnlockedNodes.Add(oldUnlockedNode);
			}
		}
		if (webDiff.oldAvailableNodes != null)
		{
			foreach (int oldAvailableNode in webDiff.oldAvailableNodes)
			{
				oldAvailableNodes.Add(oldAvailableNode);
			}
		}
		inventoryUpdates = new List<ClientInventoryUpdateReportItem>(webDiff.inventoryUpdates);
	}

	public ClientRewardWebDiff(ClientGraphDefinition masteryGraph, Dictionary<string, ClientNodeState> old)
	{
		if (masteryGraph.Configuration.SetMasteryConfiguration.OrbMappings == null)
		{
			return;
		}
		foreach (KeyValuePair<string, ClientNodeState> item in old)
		{
			if (masteryGraph.Configuration.SetMasteryConfiguration.OrbMappings.ContainsKey(item.Key))
			{
				if (item.Value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed)
				{
					oldUnlockedNodes.Add(GetOrbIdForNodeId(masteryGraph, item.Key));
				}
				else if (item.Value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
				{
					oldAvailableNodes.Add(GetOrbIdForNodeId(masteryGraph, item.Key));
				}
			}
		}
	}

	private int GetOrbIdForNodeId(ClientGraphDefinition masteryGraph, string nodeId)
	{
		if (!masteryGraph.Configuration.SetMasteryConfiguration.OrbMappings.TryGetValue(nodeId, out var value))
		{
			throw new Exception();
		}
		return value;
	}

	public void ConfigureCurrent(ClientGraphDefinition masteryGraph, Dictionary<string, ClientNodeState> current)
	{
		if (masteryGraph.Configuration.SetMasteryConfiguration.OrbMappings == null)
		{
			return;
		}
		foreach (KeyValuePair<string, ClientNodeState> item in current)
		{
			if (masteryGraph.Configuration.SetMasteryConfiguration.OrbMappings.ContainsKey(item.Key))
			{
				if (item.Value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed)
				{
					currentUnlockedNodes.Add(GetOrbIdForNodeId(masteryGraph, item.Key));
				}
				else if (item.Value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
				{
					currentAvailableNodes.Add(GetOrbIdForNodeId(masteryGraph, item.Key));
				}
			}
		}
	}
}
