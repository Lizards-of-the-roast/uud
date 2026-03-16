using TMPro;
using UnityEngine;

public class MinSizeByTextAdjuster : MinSizeByTextAdjusterImpl
{
	[Tooltip("Required: By default on the same object but you may asign a remote one.")]
	public TextMeshProUGUI text;

	private TextMeshProUGUI[] _texts;

	protected override void Reset()
	{
		if (!text)
		{
			text = GetComponent<TextMeshProUGUI>();
		}
		base.Reset();
	}

	protected override void Start()
	{
		if (text != null)
		{
			_texts = new TextMeshProUGUI[1] { text };
		}
		base.Start();
	}

	protected override TextMeshProUGUI[] GetTexts()
	{
		return _texts;
	}
}
