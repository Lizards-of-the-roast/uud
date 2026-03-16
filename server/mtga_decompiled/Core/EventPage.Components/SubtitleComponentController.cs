using EventPage.Components.NetworkModels;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class SubtitleComponentController : TextComponentController
{
	public SubtitleComponentController(TextComponent component)
		: base(component)
	{
	}

	protected override LocalizedTextData GetTextData(IPlayerEvent playerEvent)
	{
		return playerEvent.EventUXInfo.EventComponentData.SubtitleText;
	}
}
