namespace MovementSystem;

public abstract class SplineEventTrigger : SplineEvent
{
	public bool HasTriggered { get; private set; }

	public SplineEventTrigger(float time)
		: base(time)
	{
	}

	public sealed override void Update(float prev, float curr, IdealPoint currPoint)
	{
		if (CanUpdate() && CanTrigger(curr) && !HasTriggered)
		{
			Trigger(curr);
			HasTriggered = true;
		}
	}

	private bool CanTrigger(float progress)
	{
		return progress >= Time;
	}

	protected virtual bool CanUpdate()
	{
		return true;
	}

	protected abstract void Trigger(float progress);
}
