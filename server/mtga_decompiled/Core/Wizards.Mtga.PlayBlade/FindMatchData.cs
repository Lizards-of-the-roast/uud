using System;
using System.Collections.Generic;
using Wizards.Unification.Models.PlayBlade;
using Wotc.Mtga.Extensions;

namespace Wizards.Mtga.PlayBlade;

public struct FindMatchData
{
	public PlayBladeQueueType QueueType;

	public Dictionary<PlayBladeQueueType, string> QueueIdForQueueType;

	public string QueueId;

	public bool UseBO3;

	public Guid DeckId;

	public override bool Equals(object obj)
	{
		if (obj is FindMatchData other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(FindMatchData other)
	{
		if (QueueType == other.QueueType && QueueIdForQueueType.ContainSame(other.QueueIdForQueueType) && QueueId == other.QueueId && UseBO3 == other.UseBO3)
		{
			return DeckId.Equals(other.DeckId);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine((int)QueueType, QueueIdForQueueType, QueueId, UseBO3, DeckId);
	}
}
