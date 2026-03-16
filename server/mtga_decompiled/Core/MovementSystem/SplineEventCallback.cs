using System;

namespace MovementSystem;

public class SplineEventCallback : SplineEventTrigger
{
	public readonly Action<float> Callback;

	public SplineEventCallback(float time, Action<float> callback)
		: base(time)
	{
		Callback = callback;
	}

	protected override bool CanUpdate()
	{
		return Callback != null;
	}

	protected override void Trigger(float progress)
	{
		Callback(progress);
	}
}
