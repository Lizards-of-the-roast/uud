using System;
using Wizards.Arena.Promises;
using Wizards.Models;

namespace Core.Shared.Code.Network;

public interface IPlayerInboxServiceWrapper
{
	Promise<GetPlayerInboxResp> GetPlayerInbox();

	Promise<MarkLetterReadResp> MarkLetterRead(Guid letterId);

	Promise<ClaimAttachmentResp> ClaimLetterAttachment(Guid letterId);
}
