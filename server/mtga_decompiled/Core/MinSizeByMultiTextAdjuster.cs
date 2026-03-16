using TMPro;
using UnityEngine;

public class MinSizeByMultiTextAdjuster : MinSizeByTextAdjusterImpl
{
	[Tooltip("Select multiple TMPs")]
	public TextMeshProUGUI[] texts;

	protected override void Reset()
	{
		if (texts == null || texts.Length == 0)
		{
			texts = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		}
		base.Reset();
	}

	protected override TextMeshProUGUI[] GetTexts()
	{
		return texts;
	}
}
