using System.Collections.Generic;
using MTGA.Loc;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class AverageQueueTimeComponent : EventComponent
{
	[SerializeField]
	private Localize _averageQueueLocalize;

	public void SetQueueTimeText(int minutes)
	{
		string value = ((minutes <= 1) ? "1" : ((minutes <= 5) ? "5" : ((minutes <= 10) ? "10" : ((minutes > 15) ? "20" : "15"))));
		List<MTGALocalizable.LocParam> parameters = new List<MTGALocalizable.LocParam>
		{
			new MTGALocalizable.LocParam
			{
				key = "minutes",
				value = value
			}
		};
		_averageQueueLocalize.SetText((minutes <= 20) ? "Draft/AverageQueueTime_Under" : "Draft/AverageQueueTime_Over", parameters);
	}
}
