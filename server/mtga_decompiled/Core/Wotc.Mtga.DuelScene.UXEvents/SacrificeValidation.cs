using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class SacrificeValidation : ISequenceValidator
{
	public bool ValidateSequence(in int idx, ref List<UXEvent> events, out uint length)
	{
		bool flag = IsSacrificeZoneTransfer(events[idx]);
		length = (flag ? 1u : 0u);
		return flag;
	}

	private bool IsSacrificeZoneTransfer(UXEvent evt)
	{
		if (evt is ZoneTransferGroup zoneTransferGroup && zoneTransferGroup._zoneTransfers.Count == 1)
		{
			return zoneTransferGroup._zoneTransfers[0].Reason == ZoneTransferReason.Sacrifice;
		}
		return false;
	}

	bool ISequenceValidator.ValidateSequence(in int startIdx, ref List<UXEvent> events, out uint length)
	{
		return ValidateSequence(in startIdx, ref events, out length);
	}
}
