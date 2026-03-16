using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class MutableEntityDialogControllerProvider : IEntityDialogControllerProvider, IDisposable
{
	public readonly Dictionary<GREPlayerNum, EntityDialogController> PlayerTypeMap = new Dictionary<GREPlayerNum, EntityDialogController>();

	public readonly Dictionary<uint, EntityDialogController> PlayerIdMap = new Dictionary<uint, EntityDialogController>();

	private readonly HashSet<EntityDialogController> _allControllers = new HashSet<EntityDialogController>();

	public void Add(MtgPlayer player, EntityDialogController controller)
	{
		if (player != null && controller != null)
		{
			PlayerTypeMap[player.ClientPlayerEnum] = controller;
			PlayerIdMap[player.InstanceId] = controller;
			_allControllers.Add(controller);
		}
	}

	public EntityDialogController GetDialogControllerByPlayerType(GREPlayerNum playerType)
	{
		if (!PlayerTypeMap.TryGetValue(playerType, out var value))
		{
			return null;
		}
		return value;
	}

	public EntityDialogController GetDialogControllerById(uint id)
	{
		if (!PlayerIdMap.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<EntityDialogController> GetAllDialogControllers()
	{
		return _allControllers;
	}

	public void Dispose()
	{
		foreach (EntityDialogController allController in _allControllers)
		{
			allController.Dispose();
		}
		_allControllers.Clear();
		PlayerTypeMap.Clear();
		PlayerIdMap.Clear();
	}
}
