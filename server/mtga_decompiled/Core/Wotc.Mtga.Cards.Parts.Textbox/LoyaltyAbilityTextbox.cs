using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class LoyaltyAbilityTextbox : TextboxSubComponentBase
{
	[SerializeField]
	private TMP_Text _loyaltyLabel;

	[SerializeField]
	private GameObject _iconBadge;

	[SerializeField]
	private CDCMaterialFiller _materialFiller;

	public override void SetContent(ICardTextEntry content)
	{
		if (!(content is ILoyaltyTextEntry loyaltyTextEntry))
		{
			throw new ArgumentException("Invalid content type (" + content.GetType().Name + ") supplied to " + GetType().Name, "content");
		}
		base.SetContent(content);
		_textLabel.text = loyaltyTextEntry.GetText();
		_loyaltyLabel.text = loyaltyTextEntry.GetCost();
		_iconBadge.UpdateActive(active: true);
	}

	public override void UpdateVisibility(RectTransform viewportTransform)
	{
		Rect rect = viewportTransform.rect;
		Vector3 vector = viewportTransform.InverseTransformPoint(_iconBadge.transform.position);
		bool active = vector.y <= rect.yMax && vector.y >= rect.yMin;
		_iconBadge.UpdateActive(active);
	}

	public override IEnumerable<CDCMaterialFiller> GetCdcFillersOnNonLabelVisuals()
	{
		if ((bool)_materialFiller)
		{
			yield return _materialFiller;
		}
	}
}
