using System;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.Quests;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Arena.Enums.Quest;
using Wizards.MDN.Objectives;
using Wizards.Unification.Models.Graph;
using Wizards.Unification.Models.Player;
using Wizards.Unification.Models.Quest;

namespace Wotc.Mtga.Events;

public static class ClientNodeDefinitionExtensions
{
	private static HashSet<Wizards.Arena.Enums.CampaignGraph.NodeType> rewardTypes = new HashSet<Wizards.Arena.Enums.CampaignGraph.NodeType>
	{
		Wizards.Arena.Enums.CampaignGraph.NodeType.AccumulativePayout,
		Wizards.Arena.Enums.CampaignGraph.NodeType.AutomaticPayout,
		Wizards.Arena.Enums.CampaignGraph.NodeType.PayOut
	};

	public static readonly HashSet<Wizards.Arena.Enums.CampaignGraph.NodeType> MatchNodeTypes = new HashSet<Wizards.Arena.Enums.CampaignGraph.NodeType>
	{
		Wizards.Arena.Enums.CampaignGraph.NodeType.PlayFamiliar,
		Wizards.Arena.Enums.CampaignGraph.NodeType.Queue
	};

	public static readonly HashSet<Wizards.Arena.Enums.CampaignGraph.NodeType> PayoutNodeTypes = new HashSet<Wizards.Arena.Enums.CampaignGraph.NodeType>
	{
		Wizards.Arena.Enums.CampaignGraph.NodeType.PayOut,
		Wizards.Arena.Enums.CampaignGraph.NodeType.AutomaticPayout,
		Wizards.Arena.Enums.CampaignGraph.NodeType.UpgradePacket
	};

	public static bool GetObjectiveBubbleRewardDisplayData(this ClientNodeDefinition node, ClientChestDescription chestDescription, out RewardDisplayData rewardDisplayData)
	{
		rewardDisplayData = new RewardDisplayData();
		if (node.UXInfo?.ObjectiveBubbleUXInfo == null)
		{
			return false;
		}
		Client_ChestData chest = null;
		if (chestDescription != null)
		{
			chest = new Client_ChestData(chestDescription);
		}
		rewardDisplayData = new RewardDisplayData(node.UXInfo.ObjectiveBubbleUXInfo, chest, WrapperController.Instance.CardDatabase.CardDataProvider, WrapperController.Instance.CardMaterialBuilder);
		return true;
	}

	public static bool TryGetRewardChest(this ClientNodeDefinition node, ClientGraphDefinition graphDefinition, ClientCampaignGraphState graphState, out ClientChestDescription chest)
	{
		chest = null;
		if (node.UXInfo.ObjectiveBubbleUXInfo?.RewardNodeId == null)
		{
			return false;
		}
		if (!graphDefinition.Nodes.TryGetValue(node.UXInfo.ObjectiveBubbleUXInfo.RewardNodeId, out var value))
		{
			return false;
		}
		switch (value.Type)
		{
		case Wizards.Arena.Enums.CampaignGraph.NodeType.PayOut:
			if (value.Configuration.PayoutNodeConfig == null)
			{
				return false;
			}
			chest = value.Configuration.PayoutNodeConfig.ChestDescription;
			break;
		case Wizards.Arena.Enums.CampaignGraph.NodeType.PayoutV2:
			return false;
		case Wizards.Arena.Enums.CampaignGraph.NodeType.AutomaticPayout:
			if (value.Configuration.AutomaticPayoutNode == null)
			{
				return false;
			}
			chest = value.Configuration.AutomaticPayoutNode.ChestDescription;
			break;
		case Wizards.Arena.Enums.CampaignGraph.NodeType.AccumulativePayout:
		case Wizards.Arena.Enums.CampaignGraph.NodeType.AccumulativePayoutV2:
		{
			if (value.Configuration.AccumulativePayoutConfig == null)
			{
				return false;
			}
			if (!graphState.NodeStates.TryGetValue(value.Id, out var value2))
			{
				value2 = new ClientNodeState();
				value2.AccumulativePayoutState = new DTO_AccumulativePayoutNodeState
				{
					Count = 0
				};
			}
			Dictionary<int, ClientChestDescription> chestDescriptions = node.Configuration.AccumulativePayoutConfig.ChestDescriptions;
			int key = Math.Min(value2.AccumulativePayoutState.Count + 1, chestDescriptions.Count);
			chest = chestDescriptions[key];
			break;
		}
		case Wizards.Arena.Enums.CampaignGraph.NodeType.TieredReward:
		{
			if (value.Configuration.TieredNode == null)
			{
				return false;
			}
			Dictionary<string, ClientChestDescription>.ValueCollection.Enumerator enumerator = value.Configuration.TieredNode.ChestDescriptions.Values.GetEnumerator();
			if (!enumerator.MoveNext())
			{
				return false;
			}
			chest = enumerator.Current;
			break;
		}
		case Wizards.Arena.Enums.CampaignGraph.NodeType.UpgradePacket:
			return false;
		default:
			return false;
		}
		return true;
	}

	public static bool IsRewardNode(this ClientNodeDefinition node)
	{
		return rewardTypes.Contains(node.Type);
	}

	public static bool IsMatchNode(this ClientNodeDefinition node)
	{
		if (node.Type != Wizards.Arena.Enums.CampaignGraph.NodeType.Queue)
		{
			return node.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.PlayFamiliar;
		}
		return true;
	}

	public static IEnumerable<ClientNodeDefinition> GetChildren(this ClientNodeDefinition node, ClientGraphDefinition graph, bool recursive = true, Func<ClientNodeDefinition, bool> filter = null)
	{
		if (filter == null)
		{
			filter = (ClientNodeDefinition _) => true;
		}
		foreach (string child in node.Children)
		{
			ClientNodeDefinition childNode = graph.Nodes[child];
			if (!filter(childNode))
			{
				continue;
			}
			yield return childNode;
			if (!recursive)
			{
				continue;
			}
			foreach (ClientNodeDefinition child2 in childNode.GetChildren(graph, recursive: true, filter))
			{
				yield return child2;
			}
		}
	}

	public static Wizards.Arena.Enums.CampaignGraph.NodeStateStatus GetStatus(this ClientNodeDefinition node, Dictionary<string, ClientNodeState> state)
	{
		Wizards.Arena.Enums.CampaignGraph.NodeStateStatus result = Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Locked;
		if (state.TryGetValue(node.Id, out var value))
		{
			result = value.Status;
		}
		return result;
	}

	public static int GetProgress(this ClientNodeDefinition nodeDefinition, ClientNodeState state)
	{
		try
		{
			switch (nodeDefinition.Type)
			{
			case Wizards.Arena.Enums.CampaignGraph.NodeType.EventCourse:
				if (state?.ProgressNodeState == null)
				{
					break;
				}
				goto IL_006a;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.Progress:
				if (state?.ProgressNodeState == null)
				{
					break;
				}
				goto IL_006a;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.AccumulativePayout:
				if (state?.AccumulativePayoutState == null)
				{
					break;
				}
				goto IL_0099;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.AccumulativePayoutV2:
				if (state?.AccumulativePayoutState == null)
				{
					break;
				}
				goto IL_0099;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.Quest:
				if (state?.QuestNodeState != null)
				{
					return state.QuestNodeState.CurrentProgress;
				}
				break;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.MatchResult:
				if (state?.ProgressNodeState != null)
				{
					return state.ProgressNodeState.CurrentProgress;
				}
				break;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.MatchAchievement:
				{
					if (state?.MatchAchievementNodeState != null)
					{
						return state.MatchAchievementNodeState.CurrentProgress;
					}
					break;
				}
				IL_006a:
				return state.ProgressNodeState.CurrentProgress;
				IL_0099:
				return state.AccumulativePayoutState.Count;
			}
			return (state != null && state.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed) ? nodeDefinition.GetMaxProgress() : 0;
		}
		catch (Exception ex)
		{
			SimpleLog.LogErrorFormat("Error parsing the node definition/state for node ID ({0}) of type {1}.\nException: {2}", nodeDefinition.Id, nodeDefinition.Type, ex);
			return -1;
		}
	}

	public static int GetMaxProgress(this ClientNodeDefinition nodeDefinition)
	{
		try
		{
			switch (nodeDefinition.Type)
			{
			case Wizards.Arena.Enums.CampaignGraph.NodeType.EventCourse:
				return nodeDefinition.Configuration.EventCourseConfig.Threshold;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.Progress:
				return nodeDefinition.Configuration.ProgressNode.Threshold;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.AccumulativePayout:
			case Wizards.Arena.Enums.CampaignGraph.NodeType.AccumulativePayoutV2:
				return nodeDefinition.Configuration.AccumulativePayoutConfig.ChestDescriptions.Keys.Max();
			case Wizards.Arena.Enums.CampaignGraph.NodeType.Quest:
				return nodeDefinition.Configuration.QuestConfig.MetricValue;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.MatchResult:
				return nodeDefinition.Configuration.MatchResultConfig.Threshold;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.MatchAchievement:
				return nodeDefinition.Configuration.MatchAchievementNodeConfig.Target;
			default:
				return 1;
			}
		}
		catch (Exception ex)
		{
			SimpleLog.LogErrorFormat("Error parsing the node definition for node ID ({0}) of type {1}.\nException: {2}", nodeDefinition.Id, nodeDefinition.Type, ex);
			return -1;
		}
	}

	public static Client_QuestData GenerateClient_QuestData(this ClientNodeDefinition nodeDefinition, ClientNodeState nodeState, int progress, int goal, ClientChestDescription chest = null)
	{
		return new Client_QuestData(new ClientQuestDescription
		{
			canSwap = false,
			endingProgress = progress,
			goal = goal,
			questId = Guid.NewGuid(),
			inventoryUpdate = null,
			locKey = (string.IsNullOrEmpty(chest?.descriptionLocKey) ? nodeDefinition.UXInfo.ObjectiveBubbleUXInfo.PopupUXInfo.DescriptionLocKey : chest.descriptionLocKey),
			questTrack = QuestPool.Promotional,
			chestDescription = chest,
			startingProgress = progress
		});
	}
}
