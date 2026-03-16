using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class AverageQueueTimeComponentController : IComponentController
{
	private const int SECONDS_IN_MINUTE = 60;

	private AverageQueueTimeComponent _component;

	public AverageQueueTimeComponentController(AverageQueueTimeComponent component)
	{
		_component = component;
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
		bool active = false;
		PlayerEventModule currentModule = playerEvent.CourseData.CurrentModule;
		if ((uint)(currentModule - 1) <= 2u || currentModule == PlayerEventModule.HumanDraft)
		{
			AvgQueueTimeDisplayData avgQueueTimeDisplay = playerEvent.EventUXInfo.EventComponentData.AvgQueueTimeDisplay;
			if (avgQueueTimeDisplay != null)
			{
				int queueTimeText = avgQueueTimeDisplay.Seconds / 60 + ((playerEvent.AvgPodmakingSec % 60 > 0) ? 1 : 0);
				_component.SetQueueTimeText(queueTimeText);
				active = true;
			}
		}
		_component.gameObject.SetActive(active);
	}
}
