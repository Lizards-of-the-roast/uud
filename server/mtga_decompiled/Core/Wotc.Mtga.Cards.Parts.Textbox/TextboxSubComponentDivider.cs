using System.Collections.Generic;
using TMPro;
using Wotc.Mtga.Cards.Text;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class TextboxSubComponentDivider : TextboxSubComponentBase
{
	public override void CleanUp()
	{
	}

	public override void SetContent(ICardTextEntry content)
	{
	}

	public override void SetFont(TMP_FontAsset fontAsset)
	{
	}

	public override void SetAlignment(TextAlignmentOptions textAlignment)
	{
	}

	public override void SetFontSize(float newSize)
	{
	}

	public override float GetPreferredHeight()
	{
		return _minimumHeight;
	}

	public override IEnumerable<CDCMaterialFiller> GetCdcFillersOnNonLabelVisuals()
	{
		yield break;
	}

	public override void SetStripeEnabled(bool stripeEnabled)
	{
	}

	public override void SetLineSpacing(float lineSpacing)
	{
	}
}
