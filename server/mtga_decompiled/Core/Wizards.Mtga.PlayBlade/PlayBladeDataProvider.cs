using System.Collections.Generic;
using AssetLookupTree;
using Wizards.MDN;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Unification.Models.PlayBlade;

namespace Wizards.Mtga.PlayBlade;

public class PlayBladeDataProvider
{
	private EventManager _eventManager;

	private AssetLookupSystem _assetLookupSystem;

	private DeckDataProvider _deckDataProvider;

	private CombinedRankInfo _combinedRankInfo;

	private RecentlyPlayedDataProvider _recentlyPlayedDataProvider;

	private SparkyTourState _sparkyTourState;

	private ClientPlayerInventory _playerInventory;

	private ViewedEventsDataProvider _viewedEventsDataProvider;

	private readonly PlayBladeConfigDataProvider _playBladeConfigDataProvider;

	public PlayBladeDataProvider(EventManager eventManager, AssetLookupSystem assetLookupSystem, DeckDataProvider deckDataProvider, CombinedRankInfo combinedRankInfo, SparkyTourState sparkyTourState, RecentlyPlayedDataProvider recentlyPlayedDataProvider, ClientPlayerInventory inventory, ViewedEventsDataProvider viewedEventsDataProvider)
	{
		_eventManager = eventManager;
		_assetLookupSystem = assetLookupSystem;
		_deckDataProvider = deckDataProvider;
		_combinedRankInfo = combinedRankInfo;
		_sparkyTourState = sparkyTourState;
		_recentlyPlayedDataProvider = recentlyPlayedDataProvider;
		_playerInventory = inventory;
		_viewedEventsDataProvider = viewedEventsDataProvider;
		_playBladeConfigDataProvider = Pantry.Get<PlayBladeConfigDataProvider>();
	}

	~PlayBladeDataProvider()
	{
		_eventManager = null;
		_deckDataProvider = null;
		_combinedRankInfo = null;
		_sparkyTourState = null;
		_recentlyPlayedDataProvider = null;
	}

	public BladeData GetBladeData()
	{
		List<PlayBladeQueueEntry> playBladeConfig = _playBladeConfigDataProvider.GetPlayBladeConfig();
		return new BladeData(_eventManager, _assetLookupSystem, _deckDataProvider, _combinedRankInfo, _sparkyTourState, _recentlyPlayedDataProvider, _playerInventory, playBladeConfig, _viewedEventsDataProvider);
	}

	public void SetRankInfo(CombinedRankInfo rankInfo)
	{
		if (rankInfo != null)
		{
			_combinedRankInfo = rankInfo;
		}
	}
}
