using System;
using System.Collections.Generic;
using GreClient.Rules;
using Pooling;

namespace Wotc.Mtga.DuelScene;

public class PlayerNumericAidController : IPlayerNumericAidController, IDisposable
{
	private readonly IObjectPool _objectPool;

	private readonly IEntityDialogControllerProvider _dialogControllerProvider;

	private readonly Dictionary<uint, Dictionary<uint, PlayerNumericAid>> _numericAidMap;

	public PlayerNumericAidController(IObjectPool objectPool, IEntityDialogControllerProvider dialogControllerProvider)
	{
		_objectPool = objectPool;
		_dialogControllerProvider = dialogControllerProvider;
		_numericAidMap = objectPool.PopObject<Dictionary<uint, Dictionary<uint, PlayerNumericAid>>>();
	}

	public void Add(uint playerId, PlayerNumericAid playerNumericAid)
	{
		Dictionary<uint, PlayerNumericAid> idMapByPlayerId = GetIdMapByPlayerId(playerId);
		bool flag = !idMapByPlayerId.ContainsKey(playerId);
		idMapByPlayerId[playerNumericAid.Id] = playerNumericAid;
		if (flag && _dialogControllerProvider.TryGetDialogControllerById(playerId, out var dialogController))
		{
			dialogController.ShowPlayerNumericAid(playerNumericAid.Value.ToString());
		}
	}

	public void Update(uint playerId, PlayerNumericAid playerNumericAid)
	{
		GetIdMapByPlayerId(playerId)[playerNumericAid.Id] = playerNumericAid;
		if (_dialogControllerProvider.TryGetDialogControllerById(playerId, out var dialogController))
		{
			dialogController.UpdatePlayerNumericAid(playerNumericAid.Value.ToString());
		}
	}

	public void Remove(uint playerId, PlayerNumericAid playerNumericAid)
	{
		Dictionary<uint, PlayerNumericAid> idMapByPlayerId = GetIdMapByPlayerId(playerId);
		idMapByPlayerId.Remove(playerNumericAid.Id);
		if (idMapByPlayerId.Count == 0)
		{
			_numericAidMap.Remove(playerId);
			_objectPool.PushObject(idMapByPlayerId, tryClear: false);
		}
		if (_dialogControllerProvider.TryGetDialogControllerById(playerId, out var dialogController))
		{
			dialogController.ClearPlayerNumericAid();
		}
	}

	private Dictionary<uint, PlayerNumericAid> GetIdMapByPlayerId(uint playerId)
	{
		if (_numericAidMap.TryGetValue(playerId, out var value))
		{
			return value;
		}
		Dictionary<uint, PlayerNumericAid> dictionary = _objectPool.PopObject<Dictionary<uint, PlayerNumericAid>>();
		return _numericAidMap[playerId] = dictionary;
	}

	public void Dispose()
	{
		foreach (KeyValuePair<uint, Dictionary<uint, PlayerNumericAid>> item in _numericAidMap)
		{
			item.Value.Clear();
			_objectPool.PushObject(item.Value);
		}
		_numericAidMap.Clear();
		_objectPool.PushObject(_numericAidMap);
	}
}
