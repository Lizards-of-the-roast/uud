namespace Core.Rewards;

public readonly struct RewardDisplayContext
{
	public readonly int ChildIndex;

	public readonly bool AutoFlipping;

	public RewardDisplayContext(int childIndex, bool autoFlipping)
	{
		ChildIndex = childIndex;
		AutoFlipping = autoFlipping;
	}
}
