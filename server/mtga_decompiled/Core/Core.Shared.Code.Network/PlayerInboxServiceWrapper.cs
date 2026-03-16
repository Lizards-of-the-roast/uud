using System;
using MTGA.FrontDoorConnection;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Models;
using Wizards.Unification.Models.FrontDoor;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Shared.Code.Network;

public class PlayerInboxServiceWrapper : IPlayerInboxServiceWrapper
{
	private readonly IFrontDoorConnection _fdc;

	private readonly IInventoryServiceWrapper _inventoryServiceWrapper;

	public PlayerInboxServiceWrapper(IFrontDoorConnection fdcAws, IInventoryServiceWrapper inventoryServiceWrapper)
	{
		_fdc = fdcAws;
		_inventoryServiceWrapper = inventoryServiceWrapper;
	}

	public Promise<GetPlayerInboxResp> GetPlayerInbox()
	{
		return _fdc.SendMessage<GetPlayerInboxResp>(CmdType.GetPlayerInbox, (object)null).IfError(delegate(Promise<GetPlayerInboxResp> p)
		{
			PromiseExtensions.Logger.Error(p.Error.Message);
		});
	}

	public Promise<MarkLetterReadResp> MarkLetterRead(Guid letterId)
	{
		MarkLetterReadReq request = new MarkLetterReadReq
		{
			LetterId = letterId
		};
		return _fdc.SendMessage<MarkLetterReadResp>(CmdType.MarkLetterRead, request).IfError(delegate(Promise<MarkLetterReadResp> p)
		{
			PromiseExtensions.Logger.Error(p.Error.Message);
		});
	}

	public Promise<ClaimAttachmentResp> ClaimLetterAttachment(Guid letterId)
	{
		ClaimAttachmentReq request = new ClaimAttachmentReq
		{
			LetterId = letterId
		};
		return _fdc.SendMessage<ClaimAttachmentResp>(CmdType.ClaimAttachment, request).IfSuccess(delegate(Promise<ClaimAttachmentResp> p)
		{
			if (p.Result != null)
			{
				_inventoryServiceWrapper.OnInventoryInfoUpdated_AWS(AWSInventoryConversions.ConvertInventoryInfo(p.Result.DTO_InventoryInfo));
			}
		});
	}
}
