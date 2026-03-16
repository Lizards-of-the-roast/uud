using UnityEngine;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class TextComponent : EventComponent
{
	[SerializeField]
	private Localize _text;

	public void SetText(string locKey)
	{
		_text.SetText(new MTGALocalizedString
		{
			Key = locKey
		});
	}

	public void SetTextNoLoc(string text)
	{
		_text.SetText("", null, text);
	}
}
