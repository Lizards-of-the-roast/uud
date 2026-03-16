using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class LevelUpAbilityTextbox : TextboxSubComponentBase
{
	[SerializeField]
	private TMP_Text _levelRequirementLabel;

	[SerializeField]
	private TMP_Text _powerToughnessLabel;

	[SerializeField]
	private float _levelUpRequirementWidth = 0.682f;

	[SerializeField]
	private CDCMaterialFiller[] _materialFillers;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private StationAbilityColorTable _backgroundColorTable;

	[SerializeField]
	private Image _activeOverlay;

	public override void SetContent(ICardTextEntry content)
	{
		if (!(content is LevelUpTextEntry levelUpTextEntry))
		{
			throw new ArgumentException("Invalid content type (" + content.GetType().Name + ") supplied to " + GetType().Name, "content");
		}
		base.SetContent(content);
		_textLabel.text = levelUpTextEntry.GetText();
		_levelRequirementLabel.text = levelUpTextEntry.LevelRequirement;
		_powerToughnessLabel.text = GetPowerToughnessText(levelUpTextEntry.Power, levelUpTextEntry.Toughness);
		_background.color = _backgroundColorTable.GetColor(levelUpTextEntry.PresentationColor, levelUpTextEntry.IsFirstStationAbility);
		_activeOverlay.enabled = !levelUpTextEntry.IsActive;
	}

	private static string GetPowerToughnessText(StringBackedInt power, StringBackedInt toughness)
	{
		return power.RawText + "/" + toughness.RawText;
	}

	public override void CleanUp()
	{
		base.CleanUp();
		_levelRequirementLabel.text = " ";
		_powerToughnessLabel.text = " ";
	}

	public override void UpdateVisibility(RectTransform viewportTransform)
	{
		UpdateVisibilityForElement(viewportTransform, _levelRequirementLabel.gameObject);
		UpdateVisibilityForElement(viewportTransform, _powerToughnessLabel.gameObject);
	}

	private void UpdateVisibilityForElement(RectTransform viewportTransform, GameObject element)
	{
		Rect rect = viewportTransform.rect;
		Vector3 vector = viewportTransform.InverseTransformPoint(element.transform.position);
		bool active = vector.y <= rect.yMax && vector.y >= rect.yMin;
		element.UpdateActive(active);
	}

	public override IEnumerable<CDCMaterialFiller> GetCdcFillersOnNonLabelVisuals()
	{
		return _materialFillers;
	}

	public override void SetStripeEnabled(bool stripeEnabled)
	{
	}

	public override void SetAlignment(TextAlignmentOptions textAlignment)
	{
	}
}
