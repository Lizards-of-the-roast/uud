using UnityEngine;

namespace Wotc.Mtga;

public class NullTooltipDisplay : ITooltipDisplay
{
	public static readonly ITooltipDisplay Default = new NullTooltipDisplay();

	public void DisplayTooltip(GameObject source, TooltipData data, TooltipProperties properties = null)
	{
	}
}
