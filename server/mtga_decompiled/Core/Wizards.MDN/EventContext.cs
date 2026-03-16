using Wizards.Arena.Enums.UILayout;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Events;

namespace Wizards.MDN;

public class EventContext
{
	public enum DeckSelectSceneContext
	{
		SelectDeck,
		InspectDeck
	}

	public IPlayerEvent PlayerEvent;

	public DeckSelectSceneContext DeckSelectContext;

	public PostMatchContext PostMatchContext;

	public bool DeckIsFixed
	{
		get
		{
			if (PlayerEvent.EventUXInfo.EventComponentData?.SelectedDeckWidget == null)
			{
				return PlayerEvent.EventInfo.FormatType == MDNEFormatType.Constructed;
			}
			return PlayerEvent.EventUXInfo.EventComponentData.SelectedDeckWidget.DeckButtonBehavior == LayoutDeckButtonBehavior.Fixed;
		}
	}

	public EventContext(EventContext context)
	{
		PlayerEvent = context.PlayerEvent;
		PostMatchContext = context.PostMatchContext?.GetCopy() ?? null;
	}

	public EventContext()
	{
		PlayerEvent = null;
		PostMatchContext = null;
	}
}
