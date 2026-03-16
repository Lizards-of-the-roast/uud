using UnityEngine;

namespace MovementSystem;

public abstract class SplineEventDuration : SplineEvent
{
	public readonly float Duration;

	public float EndTime => Time + Duration;

	public SplineEventDuration(float time, float duration)
		: base(time)
	{
		Duration = duration;
	}

	public sealed override void Update(float prev, float curr, IdealPoint currPoint)
	{
		if (CanUpdate())
		{
			if (CanActivate(prev, curr))
			{
				Activate(curr, currPoint);
			}
			else if (IsActive(curr))
			{
				Update(curr, currPoint);
			}
			else if (CanDeactivate(prev, curr))
			{
				Deactivate(curr, currPoint);
			}
		}
	}

	protected abstract void Activate(float progress, IdealPoint currPoint);

	protected abstract void Update(float progress, IdealPoint currPoint);

	protected abstract void Deactivate(float progress, IdealPoint currPoint);

	protected virtual bool CanUpdate()
	{
		return true;
	}

	private bool CanActivate(float prev, float curr)
	{
		if (!(prev < Time) || !(Time <= curr))
		{
			if (Mathf.Approximately(Time, 0f))
			{
				return Mathf.Approximately(prev, 0f);
			}
			return false;
		}
		return true;
	}

	private bool IsActive(float progress)
	{
		if (Time < progress)
		{
			return progress < EndTime;
		}
		return false;
	}

	private bool CanDeactivate(float prev, float curr)
	{
		if (prev < EndTime)
		{
			return EndTime <= curr;
		}
		return false;
	}
}
