using System.Collections.Generic;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.Graph;

namespace Wotc.Mtga.Network.ServiceWrappers;

public interface IAchievementServiceWrapper
{
	Promise<SetFavoriteGraphNodesResp> SetFavoriteGraphNodes(List<GraphIdNodeId> newFavorites);
}
