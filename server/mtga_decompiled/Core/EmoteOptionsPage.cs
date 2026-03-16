using System.Collections.Generic;
using Wizards.Arena.Enums.Cosmetic;

public readonly struct EmoteOptionsPage
{
	public readonly EmotePage PageType;

	public readonly List<EmoteData> EmoteData;

	public EmoteOptionsPage(EmotePage pageType)
	{
		PageType = pageType;
		EmoteData = new List<EmoteData>();
	}
}
