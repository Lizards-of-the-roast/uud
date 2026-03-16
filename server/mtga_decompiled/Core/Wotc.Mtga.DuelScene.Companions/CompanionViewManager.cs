using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Companions;

public class CompanionViewManager : ICompanionViewManager, ICompanionViewProvider, ICompanionViewController, IDisposable
{
	private readonly MutableCompanionViewProvider _provider;

	private readonly ICompanionViewController _controller;

	private readonly IEntityDialogControllerProvider _dialogueProvider;

	public CompanionViewManager(MutableCompanionViewProvider provider, ICompanionViewController controller)
	{
		_provider = provider ?? new MutableCompanionViewProvider();
		_controller = controller ?? NullCompanionViewController.Default;
	}

	public AccessoryController GetCompanionByPlayerId(uint id)
	{
		return _provider.GetCompanionByPlayerId(id);
	}

	public AccessoryController GetCompanionByPlayerType(GREPlayerNum playerType)
	{
		return _provider.GetCompanionByPlayerType(playerType);
	}

	public IEnumerable<AccessoryController> GetAllCompanions()
	{
		return _provider.GetAllCompanions();
	}

	public AccessoryController CreateCompanionForPlayer(MtgPlayer player)
	{
		AccessoryController accessoryController = _controller.CreateCompanionForPlayer(player);
		if (accessoryController == null)
		{
			return null;
		}
		_provider.IdMap[player.InstanceId] = accessoryController;
		_provider.PlayerTypeMap[player.ClientPlayerEnum] = accessoryController;
		_provider.AllCompanions.Add(accessoryController);
		return accessoryController;
	}

	public void Dispose()
	{
		foreach (AccessoryController allCompanion in _provider.GetAllCompanions())
		{
			allCompanion.Cleanup();
		}
		_provider.AllCompanions.Clear();
		_provider.IdMap.Clear();
		_provider.PlayerTypeMap.Clear();
	}
}
