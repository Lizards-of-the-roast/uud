using System;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.Text;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class ClassAbilityTextbox : TextboxSubComponentBase
{
	[SerializeField]
	private RectTransform _header;

	[SerializeField]
	private TMP_Text _costLabel;

	public override float GetPreferredHeight()
	{
		return _header.rect.height;
	}

	public override void SetContent(ICardTextEntry content)
	{
		if (!(content is LevelTextEntry levelTextEntry))
		{
			throw new ArgumentException("Invalid content type (" + content.GetType().Name + ") supplied to " + GetType().Name, "content");
		}
		base.SetContent(content);
		_textLabel.text = levelTextEntry.GetText();
		_costLabel.text = levelTextEntry.GetCost();
	}

	public override void CleanUp()
	{
		base.CleanUp();
		_costLabel.text = " ";
	}

	public override void SetStripeEnabled(bool stripeEnabled)
	{
	}

	public override void SetFontSize(float newSize)
	{
	}

	public override void SetAlignment(TextAlignmentOptions textAlignment)
	{
	}
}
