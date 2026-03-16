using System;

namespace Wotc.Mtga.DuelScene;

public class NullHighlightProvider : IHighlightProvider
{
	public static readonly IHighlightProvider Default = new NullHighlightProvider();

	public event Action HighlightsUpdated
	{
		add
		{
		}
		remove
		{
		}
	}

	public HighlightType GetHighlightForId(uint id)
	{
		return HighlightType.None;
	}
}
