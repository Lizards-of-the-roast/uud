using System;
using System.Threading.Tasks;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;

namespace Wotc.Mtga.Events;

public abstract class CampaignGraphStrategy
{
	protected readonly CampaignGraphManager _manager;

	protected ClientGraphDefinition _graph;

	private string _personaId;

	protected ClientCampaignGraphState _state;

	private readonly Task _initializedTask;

	private Promise<ClientCampaignGraphState> _refreshPromise;

	protected abstract string GraphName { get; }

	public bool Initialized
	{
		get
		{
			if (_initializedTask.IsCompleted)
			{
				return _state != null;
			}
			return false;
		}
	}

	protected CampaignGraphStrategy()
	{
		_manager = Pantry.Get<CampaignGraphManager>();
		_personaId = Pantry.Get<IAccountClient>().AccountInformation?.PersonaID;
		_initializedTask = AsyncInit();
	}

	private async Task AsyncInit()
	{
		_graph = await GetGraph();
		PostGraphInit(_graph);
		_state = _manager.GetState(_graph);
		await UpdateDataAsync(_state == null).AsTask;
	}

	public async Task WaitUntilInitialized()
	{
		await _initializedTask;
	}

	protected virtual void PostGraphInit(ClientGraphDefinition definition)
	{
	}

	protected virtual Promise<ClientCampaignGraphState> PostGraphStateInit(ClientGraphDefinition definition, ClientCampaignGraphState state)
	{
		return new SimplePromise<ClientCampaignGraphState>(_state);
	}

	private async Task<ClientGraphDefinition> GetGraph()
	{
		(await _manager.GetDefinitions()).TryGetValue(GraphName, out var value);
		return value;
	}

	public Promise<ClientCampaignGraphState> UpdateDataAsync(bool refresh)
	{
		if (_refreshPromise != null)
		{
			return _refreshPromise;
		}
		if (refresh)
		{
			Action<Promise<ClientCampaignGraphState>> onComplete = delegate
			{
				_refreshPromise = null;
			};
			_refreshPromise = _manager.Update(_graph).Then(delegate(Promise<ClientCampaignGraphState> p)
			{
				if (!p.Successful)
				{
					return new SimplePromise<ClientCampaignGraphState>(_state);
				}
				_state = p.Result;
				return PostGraphStateInit(_graph, _state);
			}).Then(onComplete);
			return _refreshPromise;
		}
		_state = _manager.GetState(_graph);
		return PostGraphStateInit(_graph, _state);
	}
}
