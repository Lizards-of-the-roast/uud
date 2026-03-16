using EventPage.Components.NetworkModels;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class DescriptionComponentController : TextComponentController
{
	public DescriptionComponentController(TextComponent component)
		: base(component)
	{
	}

	protected override LocalizedTextData GetTextData(IPlayerEvent playerEvent)
	{
		return playerEvent.EventUXInfo.EventComponentData.DescriptionText;
	}
}
