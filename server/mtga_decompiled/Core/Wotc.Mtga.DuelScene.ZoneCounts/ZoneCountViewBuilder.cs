using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.ZoneCounts;

public class ZoneCountViewBuilder : IZoneCountViewBuilder
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly ISignalDispatch<ZoneCountCreatedSignalArgs> _createEvent;

	private readonly ISignalDispatch<ZoneCountDeletedSignalArgs> _deleteEvent;

	private readonly ICanvasRootProvider _canvasRootProvider;

	private readonly AssetLookupSystem _assetLookupSystem;

	public ZoneCountViewBuilder(IGameStateProvider gameStateProvider, ISignalDispatch<ZoneCountCreatedSignalArgs> createEvent, ISignalDispatch<ZoneCountDeletedSignalArgs> deleteEvent, ICanvasRootProvider canvasRootProvider, AssetLookupSystem assetLookupSystem)
	{
		_gameStateProvider = gameStateProvider;
		_createEvent = createEvent;
		_deleteEvent = deleteEvent;
		_canvasRootProvider = canvasRootProvider;
		_assetLookupSystem = assetLookupSystem;
	}

	public ZoneCountView Create(uint playerId)
	{
		ZoneCountView zoneCountView = AssetLoader.Instantiate<ZoneCountView>(GetPrefabPath(playerId), _canvasRootProvider.GetCanvasRoot(CanvasLayer.ScreenSpace_Default));
		_createEvent.Dispatch(new ZoneCountCreatedSignalArgs(this, playerId, zoneCountView));
		return zoneCountView;
	}

	private string GetPrefabPath(uint playerId)
	{
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		MtgPlayer player;
		return GetPrefabPath(mtgGameState.TryGetPlayer(playerId, out player) ? player : null);
	}

	private string GetPrefabPath(MtgPlayer player)
	{
		return GetPrefabPath(player?.ClientPlayerEnum ?? GREPlayerNum.Invalid);
	}

	private string GetPrefabPath(GREPlayerNum playerType)
	{
		if (playerType != GREPlayerNum.Opponent)
		{
			return _assetLookupSystem.GetPrefabPath<LocalZoneCountViewPrefab, ZoneCountView>();
		}
		return _assetLookupSystem.GetPrefabPath<OpponentZoneCountViewPrefab, ZoneCountView>();
	}

	public void Destroy(ZoneCountView zoneCountView)
	{
		if (!(zoneCountView == null))
		{
			_deleteEvent.Dispatch(new ZoneCountDeletedSignalArgs(this, zoneCountView));
			Object.Destroy(zoneCountView.gameObject);
		}
	}
}
