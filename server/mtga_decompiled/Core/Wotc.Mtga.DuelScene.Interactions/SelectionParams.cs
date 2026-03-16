namespace Wotc.Mtga.DuelScene.Interactions;

public readonly struct SelectionParams
{
	public readonly int Min;

	public readonly uint Max;

	public readonly uint Current;

	public readonly uint Selectable;

	public SelectionParams(int min, uint max, uint current, uint selectable)
	{
		Min = min;
		Max = max;
		Current = current;
		Selectable = selectable;
	}
}
