using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.Shared.Code;
using UnityEngine;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Unification.Models;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.Events;

public class AwsColorChallengeStrategy : CampaignGraphStrategy, IColorChallengeStrategy
{
	private const string SKIP_NODE_ID = "UnlockPlayMode";

	private Dictionary<string, AwsColorChallengeTrack> _tracks;

	private string _currentTrackName;

	private readonly string _personaId;

	private int _completedGames;

	private readonly List<string> _completedTracks = new List<string>();

	protected override string GraphName => "ColorChallenge";

	public string TemplateKey => "ColorChallenge";

	private ColorChallengePlayerEvent Event => Pantry.Get<EventManager>().ColorMasteryEvent;

	public ClientGraphDefinition Graph => _graph;

	public Dictionary<string, IColorChallengeTrack> Tracks { get; private set; }

	public string CurrentTrackName
	{
		get
		{
			string text = _currentTrackName;
			if (text == null)
			{
				string obj = Tracks.Keys.FirstOrDefault() ?? "white";
				string text2 = obj;
				_currentTrackName = obj;
				text = text2;
			}
			return text;
		}
		set
		{
			_currentTrackName = value;
		}
	}

	public IColorChallengeTrack CurrentTrack
	{
		get
		{
			Tracks.TryGetValue(CurrentTrackName, out var value);
			if (value == null)
			{
				return Tracks.Values.FirstOrDefault();
			}
			return value;
		}
	}

	public bool ColorChallengeSkipped
	{
		get
		{
			if (_state.NodeStates.TryGetValue("UnlockPlayMode", out var value))
			{
				return value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed;
			}
			throw new ArgumentOutOfRangeException("No node matching id UnlockPlayMode found!");
		}
	}

	public bool InPlayingMatchesModule => Event.InPlayingMatchesModule;

	public bool CurrentTrackCompleted => CurrentTrack?.Completed ?? false;

	public int CompletedGames
	{
		get
		{
			return _completedGames;
		}
		private set
		{
			if (_completedGames != value)
			{
				_completedGames = value;
				this.OnCompletedGamesChanged?.Invoke(_completedGames);
			}
		}
	}

	public int TotalGames { get; private set; }

	public List<string> CompletedTracks => _completedTracks;

	public event Action<int> OnCompletedGamesChanged;

	public void GoToNextNode()
	{
		string nextMatchNodeId = Event.CurrentMatchNode.NextMatchNodeId;
		if (!string.IsNullOrEmpty(nextMatchNodeId) && _graph.Nodes.TryGetValue(nextMatchNodeId, out var value) && value.GetStatus(_state.NodeStates) != Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Locked)
		{
			Event.SelectMatchNode(nextMatchNodeId);
		}
	}

	public Guid GetDeckIdForTrack(string trackName)
	{
		Guid result = Guid.Empty;
		if (_tracks.TryGetValue(trackName, out var value))
		{
			Client_DeckSummary deckSummary = value.DeckSummary;
			if (deckSummary != null)
			{
				_ = deckSummary.DeckId;
				if (true)
				{
					return value.DeckSummary.DeckId;
				}
			}
			foreach (Client_ColorChallengeMatchNode node in value.Nodes)
			{
				Guid? guid = node.DeckId();
				if (guid.HasValue)
				{
					result = guid.Value;
					break;
				}
			}
		}
		return result;
	}

	public Promise<ClientCampaignGraphState> Skip()
	{
		if (_graph.Nodes.TryGetValue("UnlockPlayMode", out var value))
		{
			return _manager.ProcessNode(_graph, value);
		}
		return Task.FromResult<ClientCampaignGraphState>(null).AsPromise();
	}

	protected override void PostGraphInit(ClientGraphDefinition definition)
	{
		ICardDataProvider cardDataProvider = Pantry.Get<ICardDataProvider>();
		if (_graph != null)
		{
			(Dictionary<string, AwsColorChallengeTrack>, Dictionary<string, IColorChallengeTrack>) tuple = CreateTracks(_graph, cardDataProvider);
			_tracks = tuple.Item1;
			Dictionary<string, IColorChallengeTrack> dictionary = (Tracks = tuple.Item2);
			TotalGames = 0;
			{
				foreach (var (_, awsColorChallengeTrack2) in _tracks)
				{
					TotalGames += awsColorChallengeTrack2.Nodes.Count;
				}
				return;
			}
		}
		Debug.LogError("No ColorChallenge node graph definition found!");
	}

	public string SwitchTrack(string trackName)
	{
		if (string.IsNullOrEmpty(trackName))
		{
			CurrentTrackName = Tracks.Keys.FirstOrDefault() ?? "";
			if (GetCompletedNodes() <= 0)
			{
				return "";
			}
		}
		if (trackName != null && Tracks.ContainsKey(trackName))
		{
			CurrentTrackName = trackName;
			if (!SelectLastSelectedMatchNode())
			{
				SelectLastAvailableMatchNode();
			}
		}
		else
		{
			Debug.LogError("Invalid ColorChallenge track name " + trackName);
		}
		return CurrentTrackName;
	}

	private int GetCompletedNodes()
	{
		int num = 0;
		foreach (string key in Tracks.Keys)
		{
			if (Tracks.TryGetValue(key, out var value))
			{
				num += value.UnlockedMatchNodeCount;
			}
		}
		return num;
	}

	private bool SelectLastSelectedMatchNode()
	{
		if (string.IsNullOrEmpty(_personaId))
		{
			return false;
		}
		string campaignEventSelectedNode = MDNPlayerPrefs.GetCampaignEventSelectedNode(_personaId, _graph.Id, CurrentTrack.Name);
		if (_state.NodeStates.TryGetValue(campaignEventSelectedNode, out var value) && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
		{
			Event.SelectMatchNode(campaignEventSelectedNode);
			return true;
		}
		return false;
	}

	private void SelectLastAvailableMatchNode()
	{
		string nodeId = string.Empty;
		foreach (Client_ColorChallengeMatchNode node in CurrentTrack.Nodes)
		{
			string id = node.Id;
			if (_state.NodeStates.TryGetValue(id, out var value) && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
			{
				nodeId = id;
				continue;
			}
			break;
		}
		Event.SelectMatchNode(nodeId);
	}

	public IEnumerator UpdateData()
	{
		yield return UpdateDataAsync(refresh: true).AsCoroutine();
	}

	protected override Promise<ClientCampaignGraphState> PostGraphStateInit(ClientGraphDefinition definition, ClientCampaignGraphState state)
	{
		List<string> list = new List<string>();
		foreach (ClientNodeDefinition item2 in definition.Nodes.Values.Where((ClientNodeDefinition x) => x.IsRewardNode() || x.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.Timer))
		{
			if (state.NodeStates.TryGetValue(item2.Id, out var value) && value.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available && (item2.IsRewardNode() || value.TimerState.UnlockTime < ServerGameTime.GameTime))
			{
				list.Add(item2.Id);
			}
		}
		Promise<ClientCampaignGraphState> promise = ((list.Count <= 0) ? new SimplePromise<ClientCampaignGraphState>(state) : _manager.ProcessNodes(_graph, list).Then(delegate(Promise<ClientCampaignGraphState> p)
		{
			_state = p.Result;
		}));
		return promise.IfSuccess(delegate
		{
			_completedTracks.Clear();
			CompletedGames = 0;
			foreach (var (item, awsColorChallengeTrack2) in _tracks)
			{
				awsColorChallengeTrack2.UpdateState(_state.NodeStates);
				if (awsColorChallengeTrack2.Completed)
				{
					_completedTracks.Add(item);
				}
				CompletedGames += awsColorChallengeTrack2.UnlockedMatchNodeCount;
			}
		});
	}

	private static (Dictionary<string, AwsColorChallengeTrack>, Dictionary<string, IColorChallengeTrack>) CreateTracks(ClientGraphDefinition graph, ICardDataProvider cardDataProvider)
	{
		Dictionary<string, AwsColorChallengeTrack> dictionary = new Dictionary<string, AwsColorChallengeTrack>();
		Dictionary<string, IColorChallengeTrack> dictionary2 = new Dictionary<string, IColorChallengeTrack>();
		foreach (ClientNodeGroupDefinition nodeGroup in graph.NodeGroups)
		{
			string name = nodeGroup.Name;
			AwsColorChallengeTrack value = (dictionary[name] = new AwsColorChallengeTrack(name, graph, nodeGroup.Nodes, cardDataProvider));
			dictionary2[name] = value;
		}
		return (dictionary, dictionary2);
	}

	public Promise<ClientCampaignGraphState> JoinNewMatchQueue(string nodeId)
	{
		if (!_graph.Nodes.TryGetValue(nodeId, out var value))
		{
			return new SimplePromise<ClientCampaignGraphState>(new Error(-1, "Error: Node not found: NodeId: " + nodeId));
		}
		if (!_state.NodeStates.TryGetValue(nodeId, out var value2) || value2.Status != Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
		{
			return new SimplePromise<ClientCampaignGraphState>(new Error(-1, "Error: Node state is not NodeStateStatus.Available. NodeId: " + nodeId));
		}
		if (value.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.Queue)
		{
			GraphClientPayload payload = new GraphClientPayload
			{
				QueuePayload = new QueueNodePayload
				{
					DeckId = Event.CourseData.CourseDeck.Id
				}
			};
			return _manager.ProcessNode(_graph, value, payload);
		}
		return _manager.ProcessNode(_graph, value);
	}

	public bool TryGetDeckUpgradePacket(out Client_DeckUpgrade deckUpgrade)
	{
		deckUpgrade = null;
		ClientNodeDefinition clientNodeDefinition = null;
		foreach (var (key, clientNodeState2) in _state.NodeStates)
		{
			if (_graph.Nodes.TryGetValue(key, out var value) && clientNodeState2.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available && value.Type == Wizards.Arena.Enums.CampaignGraph.NodeType.UpgradePacket)
			{
				clientNodeDefinition = value;
				break;
			}
		}
		if (clientNodeDefinition == null)
		{
			return false;
		}
		GraphClientPayload payload = new GraphClientPayload
		{
			QueuePayload = new QueueNodePayload
			{
				DeckId = Event.CourseData.CourseDeck.Id
			}
		};
		_manager.ProcessNode(_graph, clientNodeDefinition, payload).Then((Promise<ClientCampaignGraphState> _) => UpdateDataAsync(refresh: false));
		deckUpgrade = new Client_DeckUpgrade(clientNodeDefinition.Configuration.UpgradePacketConfig);
		return true;
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
}
