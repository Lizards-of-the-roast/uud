using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Code.Promises;
using Core.Shared.Code;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Wotc.Mtga.Events;

public class CampaignGraphManager : IDisposable
{
	private static ConcurrentBag<GraphIdNodeId> PendingManualCompleteNodes = new ConcurrentBag<GraphIdNodeId>();

	private int _refreshing;

	private Task _currentInitialization;

	private readonly Dictionary<string, ClientGraphDefinition> _emptyDefinitions = new Dictionary<string, ClientGraphDefinition>();

	private Dictionary<string, ClientGraphDefinition> _definitions;

	private readonly Dictionary<ClientGraphDefinition, ClientCampaignGraphState> _states = new Dictionary<ClientGraphDefinition, ClientCampaignGraphState>();

	private readonly INodeGraphServiceWrapper _wrapper;

	public bool Ready
	{
		get
		{
			if (Initialized)
			{
				return _refreshing == 0;
			}
			return false;
		}
	}

	public bool Initialized
	{
		get
		{
			Dictionary<string, ClientGraphDefinition> definitions = _definitions;
			if (definitions == null)
			{
				return false;
			}
			return definitions.Count > 0;
		}
	}

	public event Action<string, Dictionary<string, bool>> OnUpdateMilestoneStates;

	public event Action<ClientGraphDefinition> OnNodeStatesUpdated;

	public static CampaignGraphManager Create()
	{
		return new CampaignGraphManager();
	}

	public async Task<IReadOnlyDictionary<string, ClientGraphDefinition>> GetDefinitions()
	{
		await Init();
		if (!Initialized)
		{
			SimpleLog.LogError("Graph Manager not initialized when getting graph definitions!");
			return _emptyDefinitions;
		}
		return _definitions ?? _emptyDefinitions;
	}

	public bool TryGetDefinitions(out IReadOnlyDictionary<string, ClientGraphDefinition> definitions)
	{
		if (!Initialized)
		{
			SimpleLog.LogError("Graph Manager not initialized when try-getting graph definitions!");
		}
		definitions = _definitions ?? _emptyDefinitions;
		return _definitions != null;
	}

	public bool TryGetGraphDefinition(string graphId, out ClientGraphDefinition graphDefinition)
	{
		graphDefinition = null;
		if (!Initialized)
		{
			SimpleLog.LogError("Graph Manager not initialized when try-getting graph definition!");
			return false;
		}
		if (string.IsNullOrEmpty(graphId))
		{
			SimpleLog.LogErrorFormat("Graph ID passed in was null or empty.");
			return false;
		}
		if (_definitions == null)
		{
			SimpleLog.LogError("Graph Manager does not have the definitions setup yet.");
			return false;
		}
		return _definitions.TryGetValue(graphId, out graphDefinition);
	}

	public bool TryGetNodeDefinition(GraphIdNodeId nodeId, out ClientNodeDefinition nodeDefinition)
	{
		nodeDefinition = null;
		if (!Initialized)
		{
			SimpleLog.LogError("Graph Manager not initialized when try-getting node definition!");
			return false;
		}
		if (string.IsNullOrEmpty(nodeId.GraphId) || string.IsNullOrEmpty(nodeId.NodeId))
		{
			SimpleLog.LogErrorFormat("Node ID passed in was incomplete: {0}(graph ID) {1}(Node ID)", nodeId.GraphId, nodeId.NodeId);
		}
		if (_definitions == null)
		{
			SimpleLog.LogError("Graph Manager does not have the definitions setup yet.");
			return false;
		}
		if (!_definitions.TryGetValue(nodeId.GraphId, out var value))
		{
			SimpleLog.LogErrorFormat("Graph Manager had no definition of graph ID {0}.", nodeId.GraphId);
			return false;
		}
		return value.Nodes.TryGetValue(nodeId.NodeId, out nodeDefinition);
	}

	public bool TryGetNodeState(GraphIdNodeId nodeId, out ClientNodeState nodeState)
	{
		nodeState = null;
		if (!Initialized)
		{
			SimpleLog.LogError("Graph Manager not initialized when try-getting node state!");
			return false;
		}
		if (string.IsNullOrEmpty(nodeId.GraphId) || string.IsNullOrEmpty(nodeId.NodeId))
		{
			SimpleLog.LogErrorFormat("Node ID passed in was incomplete: {0}(graph ID) {1}(Node ID)", nodeId.GraphId, nodeId.NodeId);
		}
		if (TryGetState(nodeId.GraphId, out var state))
		{
			return state.NodeStates.TryGetValue(nodeId.NodeId, out nodeState);
		}
		return false;
	}

	private CampaignGraphManager()
	{
		_wrapper = Pantry.Get<INodeGraphServiceWrapper>();
		IFrontDoorConnectionServiceWrapper frontDoorConnectionServiceWrapper = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		if (frontDoorConnectionServiceWrapper != null && frontDoorConnectionServiceWrapper.FDCAWS != null)
		{
			frontDoorConnectionServiceWrapper.FDCAWS.OnMsg_CampaignGraphDeltas += PostCampaignGraphDeltasAndToasts;
		}
		Pantry.Get<GlobalCoroutineExecutor>().StartGlobalCoroutine(Init().AsCoroutine());
	}

	public Promise<ClientCampaignGraphState> Update(ClientGraphDefinition graph)
	{
		return RefreshGraph(graph).Convert((ClientCampaignGraphState _) => _states[graph]);
	}

	public bool TryGetState(string graphId, out ClientCampaignGraphState state)
	{
		state = null;
		if (!Initialized)
		{
			return false;
		}
		foreach (KeyValuePair<ClientGraphDefinition, ClientCampaignGraphState> state2 in _states)
		{
			if (state2.Key.Id == graphId)
			{
				state = state2.Value;
				return true;
			}
		}
		return false;
	}

	public ClientCampaignGraphState GetState(ClientGraphDefinition graph)
	{
		if (!Initialized)
		{
			SimpleLog.LogError("Graph Manager not initialized but looking for state data!");
			return null;
		}
		if (graph == null)
		{
			SimpleLog.LogError("Graph Manager tried to get the state of a null ClientGraphDefinition!");
			return null;
		}
		_states.TryGetValue(graph, out var value);
		return value;
	}

	public Promise<ClientCampaignGraphState> ProcessNodes(ClientGraphDefinition graph, List<string> ids, GraphClientPayload payload = null)
	{
		_refreshing++;
		return _wrapper.ProcessNodes(graph.Id, ids, payload).IfSuccess((Promise<ProcessNodePayloadV2> p) => UpdateStates(p.Result.GraphStates).AsPromise().Convert((Unit _) => p.Result)).Convert((ProcessNodePayloadV2 _) => _states[graph])
			.Then(delegate
			{
				_refreshing--;
			});
	}

	public Promise<ClientCampaignGraphState> ProcessNode(ClientGraphDefinition graph, ClientNodeDefinition node, GraphClientPayload payload = null)
	{
		_refreshing++;
		return _wrapper.ProcessNode(graph.Id, node.Id, payload).IfSuccess((Promise<ProcessNodePayloadV2> p) => UpdateStates(p.Result.GraphStates).AsPromise().Convert((Unit _) => p.Result)).Convert((ProcessNodePayloadV2 _) => _states[graph])
			.Then(delegate
			{
				_refreshing--;
			});
	}

	public Promise<ClientCampaignGraphState> ProcessNode(string graphId, string nodeId, GraphClientPayload payload = null)
	{
		if (!_definitions.TryGetValue(graphId, out var value))
		{
			return new SimplePromise<ClientCampaignGraphState>(null);
		}
		if (!value.Nodes.TryGetValue(nodeId, out var value2))
		{
			return new SimplePromise<ClientCampaignGraphState>(null);
		}
		return ProcessNode(value, value2, payload);
	}

	public Promise<ClientCampaignGraphState> PurchaseNode(ClientGraphDefinition graph, ClientNodeDefinition node, PurchasePayload purchasePayload)
	{
		_refreshing++;
		GraphClientPayload payload = new GraphClientPayload
		{
			PurchasePayload = purchasePayload
		};
		return _wrapper.ProcessNode(graph.Id, node.Id, payload).IfSuccess((Promise<ProcessNodePayloadV2> p) => UpdateStates(p.Result.GraphStates).AsPromise().Convert((Unit _) => p.Result)).Convert((ProcessNodePayloadV2 _) => _states[graph])
			.Then(delegate
			{
				_refreshing--;
			});
	}

	public Promise<ClientCampaignGraphState> PurchaseNodes(ClientGraphDefinition graph, List<ClientNodeDefinition> nodes, PurchasePayload purchasePayload)
	{
		_refreshing++;
		GraphClientPayload payload = new GraphClientPayload
		{
			PurchasePayload = purchasePayload
		};
		return _wrapper.ProcessNodes(graph.Id, nodes.Select((ClientNodeDefinition _) => _.Id).ToList(), payload).IfSuccess((Promise<ProcessNodePayloadV2> p) => UpdateStates(p.Result.GraphStates).AsPromise().Convert((Unit _) => p.Result)).Convert((ProcessNodePayloadV2 _) => _states[graph])
			.Then(delegate
			{
				_refreshing--;
			});
	}

	private async Task Init()
	{
		if (!Initialized)
		{
			if (_currentInitialization == null)
			{
				_currentInitialization = InnerInit();
			}
			await _currentInitialization;
			_currentInitialization = null;
		}
	}

	private async Task InnerInit()
	{
		await new Until(() => _wrapper.Ready).AsTask;
		_refreshing++;
		Promise<ClientGraphDefinitionsResponse> promise = await _wrapper.GetAllNodeGraphs().AsTask;
		if (promise.Successful)
		{
			_definitions = promise.Result?.GraphDefinitions?.ToDictionary((ClientGraphDefinition gd) => gd.Id, (ClientGraphDefinition gd) => gd);
		}
		if (_definitions == null)
		{
			SimpleLog.LogError("[GraphMan] Graph Definitions are null!");
		}
		_refreshing--;
	}

	private Promise<ClientCampaignGraphState> RefreshGraph(ClientGraphDefinition graph)
	{
		if (graph == null)
		{
			SimpleLog.LogError("Null ClientGraphDefinition!");
			return new SimplePromise<ClientCampaignGraphState>(new Error(-1, "Null ClientGraphDefinition!"));
		}
		_refreshing++;
		return _wrapper.GetStateForGraph(graph.Id).IfSuccess(delegate(Promise<ClientCampaignGraphState> p)
		{
			UpdateState(graph, p.Result);
		}).IfError(delegate(Promise<ClientCampaignGraphState> p)
		{
			SimpleLog.LogError(p.Error.Message);
		})
			.Then(delegate
			{
				_refreshing--;
			});
	}

	public void UpdateLoadedStatesWithDeltas(Dictionary<string, Dictionary<string, CampaignGraphDeltaNodeDelta>> deltas)
	{
		if (deltas == null)
		{
			return;
		}
		foreach (var (key, source) in deltas)
		{
			if (_definitions.TryGetValue(key, out var value) && _states.TryGetValue(value, out var value2))
			{
				Dictionary<string, ClientNodeState> newNodeState = source.ToDictionary((KeyValuePair<string, CampaignGraphDeltaNodeDelta> kvp) => kvp.Key, (KeyValuePair<string, CampaignGraphDeltaNodeDelta> kvp) => kvp.Value.PostState);
				UpdateNodeStates(value, value2, newNodeState, removeMissing: false);
			}
		}
	}

	public async Task UpdateStates(IEnumerable<KeyValuePair<string, ClientCampaignGraphState>> graphIdsAndStates)
	{
		foreach (var (graphId, state) in graphIdsAndStates ?? Enumerable.Empty<KeyValuePair<string, ClientCampaignGraphState>>())
		{
			await UpdateState(graphId, state);
		}
	}

	private async Task UpdateState(string graphId, ClientCampaignGraphState state)
	{
		if (graphId != null && state != null && (await GetDefinitions()).TryGetValue(graphId, out var value))
		{
			UpdateState(value, state);
		}
	}

	public ClientCampaignGraphState UpdateState(ClientGraphDefinition graph, ClientCampaignGraphState state)
	{
		if (_states.TryGetValue(graph, out var value))
		{
			UpdateNodeStates(graph, value, state.NodeStates, removeMissing: true);
			UpdateMilestoneStates(graph.Id, value, state);
		}
		else
		{
			value = state;
			_states[graph] = value;
		}
		if (value != null)
		{
			this.OnUpdateMilestoneStates?.Invoke(graph.Id, value.MilestoneStates);
		}
		return value;
	}

	private void UpdateNodeStates(ClientGraphDefinition graph, ClientCampaignGraphState existingState, Dictionary<string, ClientNodeState> newNodeState, bool removeMissing)
	{
		foreach (string key in newNodeState.Keys)
		{
			if (existingState.NodeStates.TryGetValue(key, out var value))
			{
				ClientNodeState clientNodeState = newNodeState[key];
				value.Status = clientNodeState.Status;
				value.TimerState = clientNodeState.TimerState;
				value.AccumulativePayoutState = clientNodeState.AccumulativePayoutState;
				value.ProgressNodeState = clientNodeState.ProgressNodeState;
				value.QuestNodeState = clientNodeState.QuestNodeState;
				value.TierRewardNodeState = clientNodeState.TierRewardNodeState;
				value.FamiliarMatchState = clientNodeState.FamiliarMatchState;
				value.ProgressionHistoryStateDataState = clientNodeState.ProgressionHistoryStateDataState;
				value.MatchAchievementNodeState = clientNodeState.MatchAchievementNodeState;
				value.ThresholdNodeState = clientNodeState.ThresholdNodeState;
			}
			else
			{
				existingState.NodeStates[key] = newNodeState[key];
			}
		}
		if (removeMissing)
		{
			foreach (string item in existingState.NodeStates.Keys.Except(newNodeState.Keys).ToList())
			{
				existingState.NodeStates.Remove(item);
			}
		}
		MainThreadDispatcher.Dispatch(delegate
		{
			this.OnNodeStatesUpdated?.Invoke(graph);
		});
	}

	private void UpdateMilestoneStates(string graphId, ClientCampaignGraphState existingState, ClientCampaignGraphState newState)
	{
		foreach (var (key, value) in newState.MilestoneStates)
		{
			existingState.MilestoneStates[key] = value;
		}
		foreach (string item in existingState.MilestoneStates.Keys.Except(newState.MilestoneStates.Keys).ToList())
		{
			existingState.MilestoneStates.Remove(item);
		}
	}

	public static Promise<StartData> UpdateCampaignGraphStates(Promise<StartData> respDataPromise)
	{
		CampaignGraphManager campaignGraphManager = Pantry.Get<CampaignGraphManager>();
		return respDataPromise.IfSuccess((Promise<StartData> p) => campaignGraphManager.UpdateStates(p.Result.UpdatedGraphs).AsPromise().Convert((Unit _) => p.Result));
	}

	public static void AddPendingManualCompleteNode(GraphIdNodeId pmcn)
	{
		PendingManualCompleteNodes.Add(pmcn);
	}

	public static IEnumerator SendPendingManualCompleteNodes()
	{
		if (PendingManualCompleteNodes.Count() == 0)
		{
			yield break;
		}
		IEnumerable<GraphIdNodeId> enumerable = PendingManualCompleteNodes.Distinct().ToList();
		PendingManualCompleteNodes.Clear();
		CampaignGraphManager campaignGraphManager = Pantry.Get<CampaignGraphManager>();
		foreach (GraphIdNodeId item in enumerable)
		{
			if (campaignGraphManager.TryGetGraphDefinition(item.GraphId, out var graphDefinition) && campaignGraphManager.TryGetNodeDefinition(item, out var nodeDefinition) && nodeDefinition.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.ManualComplete)
			{
				ClientCampaignGraphState state = campaignGraphManager.GetState(graphDefinition);
				if (state == null || (state.NodeStates.TryGetValue(item.NodeId, out var value) && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available))
				{
					yield return campaignGraphManager.ProcessNode(graphDefinition, nodeDefinition).AsCoroutine();
				}
			}
		}
	}

	private void PostCampaignGraphDeltasAndToasts(CampaignGraphDeltas deltas)
	{
		if (Initialized && deltas.Deltas != null)
		{
			UpdateLoadedStatesWithDeltas(deltas.Deltas);
		}
	}

	private void ReleaseUnmanagedResources()
	{
		IFrontDoorConnectionServiceWrapper frontDoorConnectionServiceWrapper = Pantry.Get<IFrontDoorConnectionServiceWrapper>();
		if (frontDoorConnectionServiceWrapper != null && frontDoorConnectionServiceWrapper.FDCAWS != null)
		{
			frontDoorConnectionServiceWrapper.FDCAWS.OnMsg_CampaignGraphDeltas -= PostCampaignGraphDeltasAndToasts;
		}
		this.OnUpdateMilestoneStates = null;
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~CampaignGraphManager()
	{
		ReleaseUnmanagedResources();
	}
}
