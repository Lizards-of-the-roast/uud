using System;

namespace Core.Shared.Code.DebugTools;

public class InvokeAtInterval
{
	private readonly int _interval;

	private readonly Action _action;

	private int _ticks;

	public InvokeAtInterval(int interval, Action action)
	{
		_interval = interval;
		_action = action;
	}

	public void Increment()
	{
		_ticks++;
		if (_ticks > _interval)
		{
			_ticks = 0;
			_action?.Invoke();
		}
	}
}
