using System;
using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Events;

public class AwsColorChallengeTrack : IColorChallengeTrack
{
	private readonly ClientGraphDefinition _graph;

	private readonly List<ClientNodeDefinition> _nodes = new List<ClientNodeDefinition>();

	private Dictionary<string, ClientNodeState> _state = new Dictionary<string, ClientNodeState>();

	public string Name { get; }

	public List<Client_ColorChallengeMatchNode> Nodes { get; } = new List<Client_ColorChallengeMatchNode>();

	public bool Completed { get; private set; }

	public int UnlockedMatchNodeCount { get; private set; }

	public Client_DeckSummary DeckSummary => GetDeckSummary(Nodes);

	public Client_ColorChallengeMatchNode CurrentMatchNode(string lastSelectedMatchNodeId)
	{
		return Nodes.FirstOrDefault((Client_ColorChallengeMatchNode x) => x.Id == lastSelectedMatchNodeId) ?? Nodes.FirstOrDefault((Client_ColorChallengeMatchNode x) => IsNodeNextToUnlock(x.Id)) ?? Nodes[0];
	}

	public bool IsNodeNextToUnlock(string id)
	{
		if (id != null && _state.TryGetValue(id, out var value))
		{
			if (value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
			{
				return !IsNodeCompleted(id);
			}
			return false;
		}
		return false;
	}

	public bool IsNodeCompleted(string id)
	{
		if (id != null && _state.TryGetValue(id, out var value) && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available && _graph.Nodes.TryGetValue(id, out var value2))
		{
			return IsCompleteNode(_graph, _state, value2);
		}
		return false;
	}

	public AwsColorChallengeTrack(string name, ClientGraphDefinition graph, List<string> nodeIds, ICardDataProvider cardDataProvider)
	{
		Name = name;
		_graph = graph;
		foreach (string nodeId in nodeIds)
		{
			if (graph.Nodes.TryGetValue(nodeId, out var value))
			{
				AddNode(value, cardDataProvider);
			}
		}
	}

	private void AddNode(ClientNodeDefinition node, ICardDataProvider cardDatabase)
	{
		_nodes.Add(node);
		Client_ColorChallengeMatchNode item = new Client_ColorChallengeMatchNode(node, _graph, cardDatabase);
		Nodes.Add(item);
	}

	public void UpdateState(Dictionary<string, ClientNodeState> state)
	{
		_state = state;
		UnlockedMatchNodeCount = 0;
		Completed = false;
		foreach (ClientNodeDefinition node in _nodes)
		{
			if (state.TryGetValue(node.Id, out var value) && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available && IsCompleteNode(_graph, state, node))
			{
				UnlockedMatchNodeCount++;
			}
		}
		Completed = UnlockedMatchNodeCount == _nodes.Count;
	}

	private static bool IsCompleteNode(ClientGraphDefinition graph, Dictionary<string, ClientNodeState> nodeStates, ClientNodeDefinition nodeDef)
	{
		ClientNodeDefinition clientNodeDefinition = nodeDef.GetChildren(graph, recursive: false, (ClientNodeDefinition _) => ClientNodeDefinitionExtensions.PayoutNodeTypes.Contains(_.Type)).FirstOrDefault();
		if (clientNodeDefinition != null && nodeStates.TryGetValue(clientNodeDefinition.Id, out var value))
		{
			return value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed;
		}
		switch (nodeDef.Type)
		{
		case Wizards.Arena.Enums.CampaignGraph.NodeType.PlayFamiliar:
		{
			ClientNodeState value3;
			return nodeDef.GetChildren(graph, recursive: false, (ClientNodeDefinition _) => _.IsMatchNode()).All((ClientNodeDefinition _) => nodeStates.TryGetValue(_.Id, out value3) && value3.Status != Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Locked);
		}
		case Wizards.Arena.Enums.CampaignGraph.NodeType.Queue:
		{
			int result;
			if (nodeStates.TryGetValue(nodeDef.Id, out var value2))
			{
				DTO_QueueNodeState queueMatchState = value2.QueueMatchState;
				result = ((queueMatchState != null && queueMatchState.SuccessfulAttempts > 0) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}
		default:
			return false;
		}
	}

	public static Client_DeckSummary GetDeckSummary(List<Client_ColorChallengeMatchNode> nodes, IPreconDeckServiceWrapper preconDeckServiceWrapper = null, IEventManager eventManager = null)
	{
		Guid? guid = nodes.LastOrDefault()?.DeckId(eventManager);
		if (guid.HasValue)
		{
			if (preconDeckServiceWrapper == null)
			{
				preconDeckServiceWrapper = Pantry.Get<IPreconDeckServiceWrapper>();
			}
			return preconDeckServiceWrapper.GetPreconDeck(guid.Value)?.Summary;
		}
		return null;
	}
}
