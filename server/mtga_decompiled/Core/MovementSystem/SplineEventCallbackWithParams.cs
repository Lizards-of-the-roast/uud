using System;

namespace MovementSystem;

public class SplineEventCallbackWithParams<T> : SplineEventTrigger
{
	private readonly T _callbackParams;

	private readonly Action<float, T> _callback;

	public SplineEventCallbackWithParams(float time, T callbackParams, Action<float, T> callback)
		: base(time)
	{
		_callbackParams = callbackParams;
		_callback = callback;
	}

	protected override bool CanUpdate()
	{
		return _callback != null;
	}

	protected override void Trigger(float progress)
	{
		_callback(progress, _callbackParams);
	}
}
