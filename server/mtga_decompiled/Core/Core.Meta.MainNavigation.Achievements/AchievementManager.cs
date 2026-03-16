using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Achievements;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Achievements.Scripts;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Arena.Models.Achievements;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Events;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementManager : IAchievementManager, IDisposable
{
	private readonly ILogger _logger;

	private IAchievementDataProvider _achievementDataProvider;

	private IAchievementServiceWrapper _achievementServiceWrapper;

	private CampaignGraphManager _campaignGraphManager;

	private FrontDoorConnectionAWS _frontDoorConnectionAWS;

	private IAchievementsToastProvider _achievementToastsProvider;

	private IAchievementsProgressProvider _achievementsCache;

	private HomePageAchievements _tempHomePageAchievements;

	private readonly List<IClientAchievement> _favoriteAchievements = new List<IClientAchievement>();

	private readonly List<IClientAchievement> _claimable = new List<IClientAchievement>();

	private readonly List<IClientAchievement> _closeToComplete = new List<IClientAchievement>();

	private readonly List<IClientAchievement> _recentlyProgressed = new List<IClientAchievement>();

	private readonly List<IClientAchievement> _oneShots = new List<IClientAchievement>();

	private IAchievementDataProvider AchievementDataProvider => _achievementDataProvider ?? (_achievementDataProvider = Pantry.Get<IAchievementDataProvider>());

	private IAchievementServiceWrapper AchievementServiceWrapper => _achievementServiceWrapper ?? (_achievementServiceWrapper = Pantry.Get<IAchievementServiceWrapper>());

	private CampaignGraphManager CampaignGraphManager => _campaignGraphManager ?? (_campaignGraphManager = Pantry.Get<CampaignGraphManager>());

	private FrontDoorConnectionAWS FrontDoorConnectionAWS => _frontDoorConnectionAWS ?? (_frontDoorConnectionAWS = Pantry.Get<IFrontDoorConnectionServiceWrapper>().FDCAWS);

	private IAchievementsToastProvider AchievementToastsProvider => _achievementToastsProvider ?? (_achievementToastsProvider = Pantry.Get<IAchievementsToastProvider>());

	public bool CachePopulated
	{
		get
		{
			if (_achievementsCache != null)
			{
				return _achievementsCache.CachePopulated;
			}
			return false;
		}
	}

	public IReadOnlyCollection<IClientAchievement> FavoriteAchievements => _favoriteAchievements;

	public IReadOnlyList<IClientAchievementGroup> ClaimableAchievementGroups
	{
		get
		{
			if (_claimable != null && _claimable.Count != 0)
			{
				return (from x in _claimable.ToArray()
					where x != null
					select x.AchievementGroup).Distinct().ToList();
			}
			return Array.Empty<IClientAchievementGroup>();
		}
	}

	public int ClaimableAchievementCount => ClaimableAchievementGroups.Select((IClientAchievementGroup cag) => cag.ClaimableAchievementCount).Sum();

	public IEnumerable<IClientAchievementSet> AchievementSets => _achievementsCache.AchievementSets.Values.OrderBy((IClientAchievementSet a) => a.DisplayPriority);

	public IEnumerable<IClientAchievementGroup> AchievementGroups => _achievementsCache.AchievementGroups.Values;

	public IEnumerable<IClientAchievement> Achievements => _achievementsCache.Achievements.Values;

	public IEnumerable<IClientAchievement> UpNextAchievements => (from a in _favoriteAchievements.Concat(_closeToComplete).Concat(_recentlyProgressed).Concat(_oneShots)
		where !a.IsCompleted
		select a).Distinct();

	public event Action<IClientAchievement> OnHomePageAchievementsUpdated;

	public event Action<List<IClientAchievement>> OnFavoriteAchievementsUpdated;

	public event Action<IClientAchievement> OnAchievementUpdated;

	public static IAchievementManager Create()
	{
		return new AchievementManager();
	}

	public bool IsAchievementFavorited(IClientAchievement achievement)
	{
		return _favoriteAchievements.Contains(achievement);
	}

	public bool IsAchievementFavorited(GraphIdNodeId id)
	{
		return _favoriteAchievements.Exists((IClientAchievement x) => x.Id.Equals(id));
	}

	private AchievementManager()
	{
		_logger = new ConsoleLogger();
		_logger.Level = LoggerLevel.Debug;
		GetDependenciesForCachingAchievementDefinitions().AsPromise().ThenOnMainThread(delegate((IReadOnlyDictionary<string, ClientGraphDefinition> GraphDefs, List<string> AchievementSets) tuple)
		{
			_achievementsCache = new AchievementsProgressProvider(tuple.GraphDefs, tuple.AchievementSets);
			if (_tempHomePageAchievements != null)
			{
				PostHomePageAchievements(_tempHomePageAchievements);
				_tempHomePageAchievements = null;
			}
		}).IfError(delegate(Error error)
		{
			SimpleLog.LogErrorFormat("There was an error: {0}", error.Exception);
		});
		FrontDoorConnectionAWS.OnMsg_CampaignGraphDeltas += PostCampaignGraphDeltasAndToasts;
		AchievementDataProvider.OnHomePageAchievementsUpdated += PostHomePageAchievements;
	}

	private async Task<(IReadOnlyDictionary<string, ClientGraphDefinition> GraphDefs, List<string> AchievementSets)> GetDependenciesForCachingAchievementDefinitions()
	{
		IReadOnlyDictionary<string, ClientGraphDefinition> clientGraphDefinitions = await CampaignGraphManager.GetDefinitions();
		while (AchievementDataProvider.AchievementsMetadata == null)
		{
			await Task.Delay(10);
		}
		return (GraphDefs: clientGraphDefinitions, AchievementSets: AchievementDataProvider.AchievementsMetadata.AchievementSets);
	}

	public IEnumerable<IClientAchievement> GetQuestTrackerAchievements(int count)
	{
		return (from x in _claimable.Concat(_closeToComplete).Concat(_recentlyProgressed).Concat(_oneShots)
			where !_favoriteAchievements.Contains(x)
			select x).Take(count);
	}

	public void SetFavorite(GraphIdNodeId graphNode)
	{
		if (string.IsNullOrEmpty(graphNode.NodeId) || !_favoriteAchievements.Exists((IClientAchievement x) => x.Id.Equals(graphNode)))
		{
			List<GraphIdNodeId> favoriteGraphNodes = new List<GraphIdNodeId> { graphNode };
			AchievementServiceWrapper.SetFavoriteGraphNodes(favoriteGraphNodes).ThenOnMainThreadIfSuccess(delegate(SetFavoriteGraphNodesResp resp)
			{
				PostFavoritesUpdate(resp.Favorites);
			});
		}
	}

	public void RemoveFavorite(GraphIdNodeId graphNode)
	{
		if (string.IsNullOrEmpty(graphNode.NodeId) || !_favoriteAchievements.All((IClientAchievement x) => x.Id.NodeId != graphNode.NodeId))
		{
			List<GraphIdNodeId> favoriteGraphNodes = new List<GraphIdNodeId>();
			AchievementServiceWrapper.SetFavoriteGraphNodes(favoriteGraphNodes).ThenOnMainThreadIfSuccess(delegate(SetFavoriteGraphNodesResp resp)
			{
				PostFavoritesUpdate(resp.Favorites);
			});
		}
	}

	public void PostFavoritesUpdate(List<GraphIdNodeId> favorites)
	{
		if (favorites == null || favorites.SequenceEqual(_favoriteAchievements.Select((IClientAchievement x) => x.Id)))
		{
			return;
		}
		List<IClientAchievement> list = new List<IClientAchievement>();
		foreach (IClientAchievement item in favorites.Select((GraphIdNodeId x) => _achievementsCache.GetAchievement(x)).Union(_favoriteAchievements))
		{
			list.Add(item);
		}
		_favoriteAchievements.ForEach(delegate(IClientAchievement x)
		{
			x.UpNextReason = AchievementUpNextReason.None;
		});
		_favoriteAchievements.Clear();
		_favoriteAchievements.AddRange(favorites.Select((GraphIdNodeId x) => _achievementsCache.GetAchievement(x)));
		_favoriteAchievements.ForEach(delegate(IClientAchievement x)
		{
			x.UpNextReason = AchievementUpNextReason.IsFavorited;
		});
		foreach (IClientAchievement item2 in list)
		{
			this.OnAchievementUpdated?.Invoke(item2);
		}
		this.OnFavoriteAchievementsUpdated?.Invoke(_favoriteAchievements);
	}

	public void RemoveClaimedAchievementFromClaimables(IClientAchievement achievement)
	{
		_claimable.Remove(achievement);
	}

	private void PostHomePageAchievements(HomePageAchievements homePageAchievements)
	{
		if (homePageAchievements == null)
		{
			return;
		}
		if (_achievementsCache == null)
		{
			_tempHomePageAchievements = homePageAchievements;
			return;
		}
		_claimable.Clear();
		if (homePageAchievements.Claimable != null)
		{
			_claimable.AddRange(from x in homePageAchievements.Claimable
				select _achievementsCache.GetAchievement(x) into x
				where x != null
				select x);
		}
		_closeToComplete.Clear();
		if (homePageAchievements.CloseToComplete != null)
		{
			_closeToComplete.AddRange(from x in homePageAchievements.CloseToComplete
				select _achievementsCache.GetAchievement(x) into x
				where x != null
				select x);
			_closeToComplete.ForEach(delegate(IClientAchievement x)
			{
				x.UpNextReason = AchievementUpNextReason.IsCloseToComplete;
			});
		}
		_recentlyProgressed.Clear();
		if (homePageAchievements.RecentlyProgressed != null)
		{
			_recentlyProgressed.AddRange(from x in homePageAchievements.RecentlyProgressed
				select _achievementsCache.GetAchievement(x) into x
				where x != null
				select x);
			_recentlyProgressed.ForEach(delegate(IClientAchievement x)
			{
				x.UpNextReason = AchievementUpNextReason.IsRecentlyProgressed;
			});
		}
		_oneShots.Clear();
		if (homePageAchievements.OneShots != null)
		{
			_oneShots.AddRange(from x in homePageAchievements.OneShots
				select _achievementsCache.GetAchievement(x) into x
				where x != null
				select x);
			_oneShots.ForEach(delegate(IClientAchievement x)
			{
				x.UpNextReason = AchievementUpNextReason.IsOneShot;
			});
		}
		PostFavoritesUpdate(homePageAchievements.Favorites);
	}

	private void PostCampaignGraphDeltasAndToasts(CampaignGraphDeltas campaignGraphDeltas)
	{
		if (campaignGraphDeltas.Deltas == null)
		{
			return;
		}
		foreach (KeyValuePair<string, Dictionary<string, CampaignGraphDeltaNodeDelta>> delta in campaignGraphDeltas.Deltas)
		{
			delta.Deconstruct(out var key, out var value);
			string graphId = key;
			foreach (KeyValuePair<string, CampaignGraphDeltaNodeDelta> item in value)
			{
				item.Deconstruct(out key, out var value2);
				string nodeId = key;
				CampaignGraphDeltaNodeDelta campaignGraphDeltaNodeDelta = value2;
				GraphIdNodeId achievementId = GraphIdNodeId.From(graphId, nodeId);
				IClientAchievement clientAchievement = _achievementsCache?.GetAchievement(achievementId);
				if (clientAchievement != null)
				{
					clientAchievement.UpdateStateWithDeltas(campaignGraphDeltaNodeDelta.PreState, campaignGraphDeltaNodeDelta.PostState);
					if (campaignGraphDeltaNodeDelta.PostState.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed && !_claimable.Contains(clientAchievement))
					{
						_claimable.Add(clientAchievement);
					}
				}
			}
		}
		((Promise<Unit>)new Until(() => CachePopulated)).ThenOnMainThread((Action)delegate
		{
			PostCampaignGraphDeltasAndToastsPostCachePopulated(campaignGraphDeltas);
		});
	}

	private void PostCampaignGraphDeltasAndToastsPostCachePopulated(CampaignGraphDeltas campaignGraphDeltas)
	{
		IEnumerable<GraphIdNodeId> achievementToasts = campaignGraphDeltas.AchievementToasts;
		foreach (GraphIdNodeId item in achievementToasts ?? Enumerable.Empty<GraphIdNodeId>())
		{
			IClientAchievement achievement = _achievementsCache.GetAchievement(item);
			AchievementToastsProvider.AddAchievementToast(campaignGraphDeltas, achievement);
		}
	}

	private void ReleaseUnmanagedResources()
	{
		FrontDoorConnectionAWS.OnMsg_CampaignGraphDeltas -= PostCampaignGraphDeltasAndToasts;
		AchievementDataProvider.OnHomePageAchievementsUpdated -= PostHomePageAchievements;
		_achievementDataProvider = null;
		_achievementServiceWrapper = null;
		_campaignGraphManager = null;
		_frontDoorConnectionAWS = null;
		_achievementToastsProvider = null;
	}

	public void Dispose()
	{
		ReleaseUnmanagedResources();
		GC.SuppressFinalize(this);
	}

	~AchievementManager()
	{
		ReleaseUnmanagedResources();
	}
}
