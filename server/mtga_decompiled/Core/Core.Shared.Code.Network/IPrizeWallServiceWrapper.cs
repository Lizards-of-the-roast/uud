using System.Collections.Generic;
using Core.Code.PrizeWall;
using Wizards.Arena.Promises;

namespace Core.Shared.Code.Network;

public interface IPrizeWallServiceWrapper
{
	Promise<List<Client_PrizeWall>> GetAllPrizeWalls();
}
