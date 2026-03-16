using System.Collections.Generic;

namespace MovementSystem;

public class SplineEventData
{
	public readonly List<SplineEvent> Events = new List<SplineEvent>();

	public SplineEventData(params SplineEvent[] events)
	{
		Events.AddRange(events);
	}

	public void UpdateEvents(float prevTime, float currTime, IdealPoint currPoint)
	{
		foreach (SplineEvent @event in Events)
		{
			@event?.Update(prevTime, currTime, currPoint);
		}
	}
}
