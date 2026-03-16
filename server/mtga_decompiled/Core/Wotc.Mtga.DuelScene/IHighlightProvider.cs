using System;

namespace Wotc.Mtga.DuelScene;

public interface IHighlightProvider
{
	event Action HighlightsUpdated;

	HighlightType GetHighlightForId(uint id);
}
