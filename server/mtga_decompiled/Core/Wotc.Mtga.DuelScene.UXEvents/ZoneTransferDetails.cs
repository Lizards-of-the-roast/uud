using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public readonly struct ZoneTransferDetails
{
	public readonly uint OldId;

	public readonly uint NewId;

	public readonly ZoneTransferReason Reason;

	public readonly ZoneType ToZoneType;

	public readonly bool IsMergeBreakup;

	public bool HasIdChange => OldId != NewId;

	public bool ReasonIsSBA
	{
		get
		{
			ZoneTransferReason reason = Reason;
			if ((uint)(reason - 20) <= 6u || reason == ZoneTransferReason.Deathtouch)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsPendingCreation
	{
		get
		{
			if (Reason == ZoneTransferReason.CardCreated)
			{
				return ToZoneType == ZoneType.Pending;
			}
			return false;
		}
	}

	public ZoneTransferDetails(uint oldId, uint newId, ZoneTransferReason reason, ZoneType toZone, bool mergeBreakup)
	{
		OldId = oldId;
		NewId = newId;
		Reason = reason;
		ToZoneType = toZone;
		IsMergeBreakup = mergeBreakup;
	}

	public static ZoneTransferDetails Translate(ZoneTransferUXEvent zte)
	{
		return new ZoneTransferDetails(zte.OldId, zte.NewId, zte.Reason, zte.ToZoneType, zte.IsMergeBreakup);
	}
}
