using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Code.Promises;
using Core.Shared.Code;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Arena.Promises;
using Wizards.MDN.NodeGraph;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Events;

namespace Core.NPEStitcher;

public class NodeNpeStrategy : INpeStrategy
{
	private const string NPE_NODE_GROUP = "NPE_Tutorial";

	private readonly CampaignGraphManager _graphManager;

	private readonly Wizards.Arena.Client.Logging.ILogger _logger;

	private ClientGraphDefinition _npeGraph;

	private Dictionary<string, ClientNodeState> _graphState;

	private Matchmaking _matchmaking;

	private bool _refreshedSinceMatch;

	public bool Initialized { get; private set; }

	public NpeModuleState State { get; private set; }

	public bool Available => true;

	public bool TutorialRequired
	{
		get
		{
			if (!OverridesConfiguration.Local.GetFeatureToggleValue("npe.force_tutorial"))
			{
				return (State & NpeModuleState.Complete) == 0;
			}
			return true;
		}
	}

	public int NextGameNumber { get; private set; }

	public NodeNpeStrategy(Wizards.Arena.Client.Logging.ILogger logger = null)
	{
		_graphManager = Pantry.Get<CampaignGraphManager>();
		_logger = logger ?? new UnityCrossThreadLogger();
		Pantry.Get<GlobalCoroutineExecutor>().StartGlobalCoroutine(Initialize());
	}

	public IEnumerator Initialize()
	{
		float timeout = Time.time + 15f;
		yield return new WaitUntil(() => _graphManager.Initialized || Time.time >= timeout);
		if (!_graphManager.Initialized)
		{
			_logger.Error("[NPE] Node Graph Manager failed to initialize!");
			State = NpeModuleState.Error;
			yield break;
		}
		Task<bool> getNpeGraphTask = getNpeGraph();
		yield return getNpeGraphTask.AsCoroutine();
		if (!getNpeGraphTask.Result)
		{
			_logger.Error("[NPE] No NPE node group defined!");
			State = NpeModuleState.Error;
		}
		else
		{
			yield return Refresh();
			Initialized = true;
		}
	}

	public void ResetNPEState()
	{
		foreach (ClientNodeDefinition value in _npeGraph.Nodes.Values)
		{
			if (value.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.PlayFamiliar)
			{
				_graphState.Remove(value.Id);
			}
		}
	}

	public IEnumerator Refresh()
	{
		if (!_refreshedSinceMatch)
		{
			_graphManager.Update(_npeGraph);
			while (!_graphManager.Ready)
			{
				yield return null;
			}
			_graphState = _graphManager.GetState(_npeGraph).NodeStates;
			if (_graphState == null)
			{
				_logger.Error("[NPE] Node Graph Manager returned null state!");
				_graphState = new Dictionary<string, ClientNodeState>();
			}
			configureFromState();
			_refreshedSinceMatch = true;
			RunAutomaticFlows();
		}
	}

	private void RunAutomaticFlows()
	{
		if ((State & NpeModuleState.HaveRewards) > NpeModuleState.Uninitialized)
		{
			ClaimRewards(delegate
			{
			});
		}
	}

	public void Join(Action<bool, Error> onComplete)
	{
		onComplete(arg1: true, Error.NoError);
	}

	public void PlayMatch()
	{
		foreach (ClientNodeDefinition value in _npeGraph.Nodes.Values)
		{
			if (value.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.PlayFamiliar && _graphState[value.Id].Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
			{
				_graphManager.ProcessNode(_npeGraph, value);
				_refreshedSinceMatch = false;
				if (_matchmaking == null)
				{
					_matchmaking = Pantry.Get<Matchmaking>();
				}
				_matchmaking.SetExpectedEvent(null);
				_matchmaking.SetupBotMatch(null, LoadSceneMode.Single);
				break;
			}
		}
	}

	public void SkipTutorial(Action onComplete)
	{
		foreach (ClientNodeDefinition value in _npeGraph.Nodes.Values)
		{
			if (value.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.ManualComplete && _graphState[value.Id].Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
			{
				_graphManager.ProcessNode(_npeGraph, value).Then(delegate
				{
					ClaimRewards(onComplete);
				});
				break;
			}
		}
	}

	public void ClaimRewards(Action onComplete)
	{
		foreach (ClientNodeDefinition value in _npeGraph.Nodes.Values)
		{
			if (value.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.PayOut && _graphState[value.Id].Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
			{
				_graphManager.ProcessNode(_npeGraph, value).Then(delegate
				{
					configureFromState();
				}).ThenOnMainThread(onComplete);
				return;
			}
		}
		MainThreadDispatcher.Instance.Add(onComplete);
	}

	public void ReplayTutorial(Action onComplete)
	{
		foreach (ClientNodeDefinition value in _npeGraph.Nodes.Values)
		{
			if (value.Type != Wizards.Arena.Enums.CampaignGraph.NodeType.Reset)
			{
				continue;
			}
			if (_graphState[value.Id].Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
			{
				ResetNPEState();
				_graphManager.ProcessNode(_npeGraph, value).Then(delegate
				{
					configureFromState();
				}).ThenOnMainThread(onComplete);
			}
			else
			{
				_logger.Error($"[NPE] Attempted to replay tutorial but Reset node was in state {_graphState[value.Id].Status}");
			}
			return;
		}
		_logger.Error("[NPE] Attempted to replay tutorial but no Reset node was found!");
	}

	private async Task<bool> getNpeGraph()
	{
		return (await _graphManager.GetDefinitions()).TryGetValue("NPE_Tutorial", out _npeGraph);
	}

	private void configureFromState()
	{
		State = NpeModuleState.Uninitialized;
		NextGameNumber = 0;
		int num = 0;
		foreach (ClientNodeDefinition value2 in _npeGraph.Nodes.Values)
		{
			if (!_graphState.TryGetValue(value2.Id, out var value))
			{
				value = new ClientNodeState
				{
					Status = Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Locked
				};
			}
			if (value2.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.PlayFamiliar)
			{
				if (value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed)
				{
					int nextGameNumber = NextGameNumber + 1;
					NextGameNumber = nextGameNumber;
					num++;
				}
				else if (value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
				{
					State |= NpeModuleState.CanPlay;
				}
				if (value.FamiliarMatchState != null)
				{
					num += value.FamiliarMatchState.MatchesPlayed;
				}
			}
			else if (value2.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.PayOut && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
			{
				State |= NpeModuleState.HaveRewards;
			}
		}
		if (num == 0)
		{
			State |= NpeModuleState.CanJoin;
		}
		if (_npeGraph.CanSkip(_graphManager))
		{
			State |= NpeModuleState.CanSkip;
		}
		if (_npeGraph.IsCompleted(_graphManager))
		{
			State |= NpeModuleState.Complete;
		}
		if (State == NpeModuleState.Uninitialized)
		{
			State = NpeModuleState.Error;
			_logger.Error("[NPE] Invalid state received for NPE graph!");
		}
	}
}
