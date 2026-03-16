using UnityEngine;

namespace Wotc.Mtga;

public interface ITooltipDisplay
{
	void DisplayTooltip(GameObject source, TooltipData data, TooltipProperties properties = null);
}
