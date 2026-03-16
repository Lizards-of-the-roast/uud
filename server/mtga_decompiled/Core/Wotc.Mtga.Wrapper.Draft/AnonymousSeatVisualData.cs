namespace Wotc.Mtga.Wrapper.Draft;

public readonly struct AnonymousSeatVisualData
{
	public readonly bool IsVisible;

	public readonly string StatusKey;

	public readonly bool IsReady;

	public AnonymousSeatVisualData(bool isVisible, string statusKey = "", bool isReady = false)
	{
		IsVisible = isVisible;
		StatusKey = statusKey;
		IsReady = isReady;
	}
}
