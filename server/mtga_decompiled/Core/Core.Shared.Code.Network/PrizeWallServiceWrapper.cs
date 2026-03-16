using System.Collections.Generic;
using Core.Code.PrizeWall;
using MTGA.FrontDoorConnection;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Models.PrizeWall;

namespace Core.Shared.Code.Network;

public class PrizeWallServiceWrapper : IPrizeWallServiceWrapper
{
	private readonly IFrontDoorConnection _fdc;

	public PrizeWallServiceWrapper(IFrontDoorConnection fdcAws)
	{
		_fdc = fdcAws;
	}

	public Promise<List<Client_PrizeWall>> GetAllPrizeWalls()
	{
		return _fdc.SendMessage<GetAllPrizeWallsResp>(CmdType.GetAllPrizeWalls, (object)null).Convert((GetAllPrizeWallsResp p) => Client_PrizeWall.ConvertFromDTOList(p.ActivePrizeWalls));
	}
}
