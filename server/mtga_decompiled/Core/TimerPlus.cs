using System;
using System.Timers;

public class TimerPlus : Timer
{
	private DateTime m_dueTime;

	public double TimeLeft => (m_dueTime - DateTime.Now).TotalMilliseconds;

	public TimerPlus()
	{
		base.Elapsed += ElapsedAction;
	}

	protected new void Dispose()
	{
		base.Elapsed -= ElapsedAction;
		base.Dispose();
	}

	public new void Start()
	{
		m_dueTime = DateTime.Now.AddMilliseconds(base.Interval);
		base.Start();
	}

	private void ElapsedAction(object sender, ElapsedEventArgs e)
	{
		if (base.AutoReset)
		{
			m_dueTime = DateTime.Now.AddMilliseconds(base.Interval);
		}
	}
}
