using System.Collections.Generic;
using Wizards.Arena.Models;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Unification.Models.FrontDoor;

namespace Wotc.Mtga.Network.ServiceWrappers;

public class AwsPreferredPrintingServiceWrapper : IPreferredPrintingServiceWrapper
{
	private FrontDoorConnectionAWS _frontDoorConnection;

	public AwsPreferredPrintingServiceWrapper(FrontDoorConnectionAWS frontDoorConnection)
	{
		_frontDoorConnection = frontDoorConnection;
	}

	public Promise<Dictionary<int, DTO_PreferredPrintingWithStyle>> GetAllPreferredPrintings()
	{
		return sendGetAllPreferredPrintingsReq();
	}

	public Promise<bool> SetPreferredPrinting(int titleId, int grpId, string styleCode)
	{
		return sendSetPreferredPrintingReq(titleId, grpId, styleCode);
	}

	public Promise<bool> RemovePreferredPrinting(int titleId)
	{
		return sendRemovePreferredPrintingReq(titleId);
	}

	private Promise<Dictionary<int, DTO_PreferredPrintingWithStyle>> sendGetAllPreferredPrintingsReq()
	{
		return _frontDoorConnection.SendMessage<Dictionary<int, DTO_PreferredPrintingWithStyle>>(CmdType.GetAllPreferredPrintings, (object)null);
	}

	private Promise<bool> sendSetPreferredPrintingReq(int titleId, int grpId, string styleCode)
	{
		SetPreferredPrintingReq request = new SetPreferredPrintingReq
		{
			TitleId = titleId,
			GrpId = grpId,
			StyleCode = styleCode
		};
		return _frontDoorConnection.SendMessage<bool>(CmdType.SetPreferredPrinting, request);
	}

	private Promise<bool> sendRemovePreferredPrintingReq(int titleId)
	{
		RemovePreferredPrintingReq request = new RemovePreferredPrintingReq
		{
			TitleId = titleId
		};
		return _frontDoorConnection.SendMessage<bool>(CmdType.RemovePreferredPrinting, request);
	}
}
