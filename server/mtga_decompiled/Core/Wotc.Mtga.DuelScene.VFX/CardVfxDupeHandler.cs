using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.VFX;

public class CardVfxDupeHandler
{
	private (string matchId, uint gameNumber) _dedupeGameInfo;

	private (uint turnNumber, HashSet<string> prefabs) _vfxPerTurn;

	private (uint turnNumber, Dictionary<string, HashSet<uint>> prefabToIdMap) _vfxPerInstance;

	public CardVfxDupeHandler()
	{
		_dedupeGameInfo = default((string, uint));
		_vfxPerTurn = (turnNumber: 0u, prefabs: new HashSet<string>());
		_vfxPerInstance = (turnNumber: 0u, prefabToIdMap: new Dictionary<string, HashSet<uint>>());
	}

	public CardVfxDupeHandler((string matchId, uint gameNumber) dedupeGameInfo, uint turnNumber, Dictionary<string, HashSet<uint>> dictionary, HashSet<string> set)
	{
		_dedupeGameInfo = dedupeGameInfo;
		_vfxPerInstance = (turnNumber: turnNumber, prefabToIdMap: dictionary);
		_vfxPerTurn = (turnNumber: turnNumber, prefabs: set);
	}

	private bool HasTurnReset(MtgGameState currentGameState, uint lastVfxTurnNumber)
	{
		string matchID = currentGameState.GameInfo.MatchID;
		uint gameNumber = currentGameState.GameInfo.GameNumber;
		if (!(matchID != _dedupeGameInfo.matchId) && gameNumber == _dedupeGameInfo.gameNumber)
		{
			return currentGameState.GameWideTurn != lastVfxTurnNumber;
		}
		return true;
	}

	public bool IsTransformDuplicate(Transform parentTransform, string prefabPath)
	{
		foreach (Transform item in parentTransform)
		{
			if (item.TryGetComponent<FreeAssetReferencesOnDestroy>(out var component) && component.AssetPaths.Contains(prefabPath))
			{
				return true;
			}
			if (item.name.Replace("(Clone)", string.Empty).Trim().Equals(prefabPath))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsTurnDuplicate(MtgGameState currentGameState, string prefabPath)
	{
		if (currentGameState == null)
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(prefabPath))
		{
			return false;
		}
		if (HasTurnReset(currentGameState, _vfxPerTurn.turnNumber))
		{
			_dedupeGameInfo = (matchId: currentGameState.GameInfo.MatchID, gameNumber: currentGameState.GameInfo.GameNumber);
			_vfxPerTurn.prefabs.Clear();
			_vfxPerTurn.turnNumber = currentGameState.GameWideTurn;
		}
		if (_vfxPerTurn.prefabs.Contains(prefabPath))
		{
			return true;
		}
		_vfxPerTurn.prefabs.Add(prefabPath);
		return false;
	}

	public bool IsTurnDuplicateForInstance(uint instanceId, MtgGameState currentGameState, string prefabPath)
	{
		if (currentGameState == null)
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(prefabPath))
		{
			return false;
		}
		if (HasTurnReset(currentGameState, _vfxPerInstance.turnNumber))
		{
			_dedupeGameInfo = (matchId: currentGameState.GameInfo.MatchID, gameNumber: currentGameState.GameInfo.GameNumber);
			_vfxPerInstance.prefabToIdMap.Clear();
			_vfxPerInstance.turnNumber = currentGameState.GameWideTurn;
		}
		if (_vfxPerInstance.prefabToIdMap.TryGetValue(prefabPath, out var value) && value.Contains(instanceId))
		{
			return true;
		}
		if (_vfxPerInstance.prefabToIdMap.TryGetValue(prefabPath, out var value2))
		{
			value2.Add(instanceId);
		}
		else
		{
			_vfxPerInstance.prefabToIdMap[prefabPath] = new HashSet<uint> { instanceId };
		}
		return false;
	}
}
