using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.Companions;

public class NullCompanionViewManager : ICompanionViewManager, ICompanionViewProvider, ICompanionViewController
{
	public static readonly ICompanionViewManager Default = new NullCompanionViewManager();

	private static readonly ICompanionViewProvider _provider = NullCompanionViewProvider.Default;

	private static readonly ICompanionViewController _controller = NullCompanionViewController.Default;

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
		return _controller.CreateCompanionForPlayer(player);
	}
}
