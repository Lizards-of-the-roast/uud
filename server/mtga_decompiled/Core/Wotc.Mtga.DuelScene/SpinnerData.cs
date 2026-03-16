namespace Wotc.Mtga.DuelScene;

public readonly struct SpinnerData
{
	public readonly uint InstanceId;

	public readonly int Amount;

	public readonly int Min;

	public readonly int Max;

	public SpinnerData(uint id, int initialAmount, int min, int max)
	{
		InstanceId = id;
		Amount = initialAmount;
		Min = min;
		Max = max;
	}
}
