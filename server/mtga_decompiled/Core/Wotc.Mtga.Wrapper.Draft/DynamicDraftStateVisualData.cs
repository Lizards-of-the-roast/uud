namespace Wotc.Mtga.Wrapper.Draft;

public readonly struct DynamicDraftStateVisualData
{
	public readonly bool PassDirectionIsLeft;

	public readonly uint PackNumber;

	public readonly uint PickNumber;

	public readonly int NumberOfCardsToPick;

	public readonly CollationMapping[] PacksOnLocalSeat;

	public readonly CollationMapping[] PacksOnLeftSeat;

	public readonly CollationMapping[] PacksOnRightSeat;

	public DynamicDraftStateVisualData(bool passDirectionIsLeft, uint packNumber, uint pickNumber, int numberOfCardsToPick, CollationMapping[] packsOnLocalSeat, CollationMapping[] packsOnLeftSeat, CollationMapping[] packsOnRightSeat)
	{
		PassDirectionIsLeft = passDirectionIsLeft;
		PickNumber = pickNumber;
		PackNumber = packNumber;
		NumberOfCardsToPick = numberOfCardsToPick;
		PacksOnLocalSeat = packsOnLocalSeat;
		PacksOnLeftSeat = packsOnLeftSeat;
		PacksOnRightSeat = packsOnRightSeat;
	}
}
