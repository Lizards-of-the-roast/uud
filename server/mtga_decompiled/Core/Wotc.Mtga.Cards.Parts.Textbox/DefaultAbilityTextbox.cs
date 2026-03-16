using System;
using Wotc.Mtga.Cards.Text;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class DefaultAbilityTextbox : TextboxSubComponentBase
{
	public override void SetContent(ICardTextEntry content)
	{
		if (!(content is BasicTextEntry basicTextEntry))
		{
			throw new ArgumentException("Invalid content type (" + content.GetType().Name + ") supplied to " + GetType().Name, "content");
		}
		base.SetContent(content);
		_textLabel.text = basicTextEntry.GetText();
	}
}
