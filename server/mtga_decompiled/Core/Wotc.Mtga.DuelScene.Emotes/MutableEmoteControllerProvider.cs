using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Emotes;

public class MutableEmoteControllerProvider : IEmoteControllerProvider, IDisposable
{
	private readonly Dictionary<GREPlayerNum, IEmoteController> _playerTypeMap = new Dictionary<GREPlayerNum, IEmoteController>();

	private readonly Dictionary<uint, IEmoteController> _playerIdMap = new Dictionary<uint, IEmoteController>();

	private readonly HashSet<IEmoteController> _allControllers = new HashSet<IEmoteController>();

	public void Add(MtgPlayer player, IEmoteController controller)
	{
		if (player != null && controller != null)
		{
			_playerTypeMap[player.ClientPlayerEnum] = controller;
			_playerIdMap[player.InstanceId] = controller;
			_allControllers.Add(controller);
		}
	}

	public IEmoteController GetEmoteControllerByPlayerType(GREPlayerNum playerType)
	{
		if (!_playerTypeMap.TryGetValue(playerType, out var value))
		{
			return null;
		}
		return value;
	}

	public IEmoteController GetEmoteControllerById(uint id)
	{
		if (!_playerIdMap.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<IEmoteController> GetAllEmoteControllers()
	{
		return _allControllers;
	}

	public void Dispose()
	{
		foreach (IEmoteController allController in _allControllers)
		{
			allController.Dispose();
		}
		_allControllers.Clear();
		_playerTypeMap.Clear();
		_playerIdMap.Clear();
	}
}
