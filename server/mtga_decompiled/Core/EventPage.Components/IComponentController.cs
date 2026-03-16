using Wizards.MDN;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public interface IComponentController
{
	void Update(IPlayerEvent playerEvent);

	void OnEventPageOpen(EventContext eventContext);

	void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state);
}
