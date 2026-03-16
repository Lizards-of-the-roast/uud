namespace MovementSystem;

public abstract class SplineEvent
{
	public readonly float Time;

	public SplineEvent(float time)
	{
		Time = time;
	}

	public abstract void Update(float prev, float curr, IdealPoint currPoint);
}
