using System.Collections.Generic;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.FrontDoor;
using Wizards.Unification.Models.Graph;

namespace Wotc.Mtga.Network.ServiceWrappers;

public class AchievementServiceWrapper : IAchievementServiceWrapper
{
	private readonly FrontDoorConnectionAWS _fdc;

	private static IFrontDoorConnectionServiceWrapper FrontDoorConnection => Pantry.Get<IFrontDoorConnectionServiceWrapper>();

	public static IAchievementServiceWrapper Create()
	{
		return new AchievementServiceWrapper(FrontDoorConnection.FDCAWS);
	}

	public AchievementServiceWrapper(FrontDoorConnectionAWS fdcAws)
	{
		_fdc = fdcAws;
	}

	public Promise<SetFavoriteGraphNodesResp> SetFavoriteGraphNodes(List<GraphIdNodeId> newFavorites)
	{
		SetFavoriteGraphNodesReq request = new SetFavoriteGraphNodesReq
		{
			Favorites = newFavorites
		};
		return _fdc.SendMessage<SetFavoriteGraphNodesResp>(CmdType.GraphSetFavoriteNodes, request);
	}
}
