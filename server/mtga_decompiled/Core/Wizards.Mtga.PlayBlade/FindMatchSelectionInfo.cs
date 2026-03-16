using System;
using System.Collections.Generic;
using Wizards.Unification.Models.PlayBlade;

namespace Wizards.Mtga.PlayBlade;

public class FindMatchSelectionInfo
{
	public PlayBladeQueueType SelectedQueueType;

	public Dictionary<PlayBladeQueueType, BladeQueueInfo> SelectedBladeQueueInfoForQueueType = new Dictionary<PlayBladeQueueType, BladeQueueInfo>();

	public bool UseBO3;

	public Guid SelectedDeckId = Guid.Empty;

	public bool SelectedDeckIsInvalidForFormat;

	public BladeQueueInfo SelectedBladeQueueInfo
	{
		get
		{
			if (!SelectedBladeQueueInfoForQueueType.TryGetValue(SelectedQueueType, out var value))
			{
				return null;
			}
			return value;
		}
		set
		{
			SelectedBladeQueueInfoForQueueType[SelectedQueueType] = value;
		}
	}

	public BladeEventInfo SelectedEvent
	{
		get
		{
			if (!UseBO3)
			{
				return SelectedBladeQueueInfo?.EventInfo_BO1;
			}
			return SelectedBladeQueueInfo?.EventInfo_BO3;
		}
	}

	public string SelectedEventName => SelectedEvent?.EventName;
}
