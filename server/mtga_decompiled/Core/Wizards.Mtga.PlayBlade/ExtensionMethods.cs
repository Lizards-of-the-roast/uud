using System.Collections.Generic;
using System.Linq;
using Wizards.Unification.Models.PlayBlade;

namespace Wizards.Mtga.PlayBlade;

public static class ExtensionMethods
{
	public static BladeQueueInfo GetBladeQueueInfoByEvent(this Dictionary<PlayBladeQueueType, List<BladeQueueInfo>> dict, BladeEventInfo eventInfo)
	{
		return dict.SelectMany((KeyValuePair<PlayBladeQueueType, List<BladeQueueInfo>> d) => d.Value).FirstOrDefault((BladeQueueInfo bqi) => bqi.EventInfo_BO1 == eventInfo || bqi.EventInfo_BO3 == eventInfo);
	}
}
