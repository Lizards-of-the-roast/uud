using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Events;

public class Client_ColorChallengeMatchNode
{
	public readonly string Id;

	public readonly string OpponentAvatar;

	public readonly RewardDisplayData Reward;

	public readonly Client_DeckUpgrade DeckUpgradeData;

	public readonly bool IsPvpMatch;

	public readonly List<string> Children;

	public readonly string NextMatchNodeId;

	private readonly ClientNodeConfig _config;

	private readonly ClientGraphDefinition _graph;

	public Client_ColorChallengeMatchNode(ClientNodeDefinition node, ClientGraphDefinition graph, ICardDataProvider cardDatabase)
	{
		Id = node.Id;
		IsPvpMatch = node.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.Queue;
		OpponentAvatar = node.Configuration?.FamiliarMatchConfig?.OpponentAvatar ?? "Avatar_Basic_NPE";
		Children = node.Children;
		_config = node.Configuration;
		_graph = graph;
		NextMatchNodeId = node.GetChildren(graph, recursive: true, (ClientNodeDefinition _) => ClientNodeDefinitionExtensions.MatchNodeTypes.Contains(_.Type)).FirstOrDefault()?.Id;
		foreach (ClientNodeDefinition child in node.GetChildren(graph, recursive: true, (ClientNodeDefinition _) => ClientNodeDefinitionExtensions.PayoutNodeTypes.Contains(_.Type)))
		{
			switch (child.Type)
			{
			case Wizards.Arena.Enums.CampaignGraph.NodeType.UpgradePacket:
				DeckUpgradeData = new Client_DeckUpgrade(child.Configuration.UpgradePacketConfig);
				break;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.AutomaticPayout:
				if (child?.Configuration?.AutomaticPayoutNode?.ChestDescription != null && Reward == null)
				{
					Reward = TempRewardTranslation.ChestDescriptionToDisplayData(ServiceWrapperHelpers.ToClientChestDescription(child.Configuration.AutomaticPayoutNode.ChestDescription), cardDatabase, Pantry.Get<CardMaterialBuilder>());
				}
				else if (Reward != null)
				{
					Debug.LogError("Color challenge node has null automatic payout");
				}
				break;
			case Wizards.Arena.Enums.CampaignGraph.NodeType.PayOut:
				if (child?.Configuration?.PayoutNodeConfig?.ChestDescription != null)
				{
					Reward = TempRewardTranslation.ChestDescriptionToDisplayData(ServiceWrapperHelpers.ToClientChestDescription(child.Configuration.PayoutNodeConfig.ChestDescription), cardDatabase, Pantry.Get<CardMaterialBuilder>());
					return;
				}
				Debug.LogError("Color challenge node has null payout");
				break;
			}
		}
	}

	public Guid? DeckId(IEventManager eventManager = null)
	{
		if (IsPvpMatch)
		{
			Guid? preferredDeckIDFromQueue = GetPreferredDeckIDFromQueue(_config?.QueueNodeConfig?.QueueName, eventManager);
			if (preferredDeckIDFromQueue.HasValue)
			{
				return preferredDeckIDFromQueue;
			}
		}
		return _config?.FamiliarMatchConfig?.PlayerPreconDeckId;
	}

	private static Guid? GetPreferredDeckIDFromQueue(string queueName, IEventManager eventManager = null)
	{
		if (queueName == null)
		{
			Debug.LogError("No queue available from PvP node");
			return null;
		}
		if (eventManager == null)
		{
			eventManager = Pantry.Get<EventManager>();
		}
		return eventManager.GetEventContext(queueName)?.PlayerEvent?.EventUXInfo?.EventComponentData?.InspectPreconDecksWidget?.DeckIds?.FirstOrDefault();
	}
}
