using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public class StationAbilityTextbox : TextboxSubComponentBase
{
	[SerializeField]
	private TMP_Text _chargeCountLabel;

	[SerializeField]
	private TMP_Text _powerToughnessLabel;

	[SerializeField]
	private float _powerToughnessWidth = 0.682f;

	[SerializeField]
	private float _padding;

	[SerializeField]
	private CDCMaterialFiller[] _materialFillers;

	[SerializeField]
	private Image _background;

	[SerializeField]
	private StationAbilityColorTable _backgroundColorTable;

	[SerializeField]
	private Image _activeOverlay;

	private bool _displayPowerToughness;

	public override void SetContent(ICardTextEntry content)
	{
		if (!(content is StationTextEntry stationTextEntry))
		{
			throw new ArgumentException("Invalid content type (" + content.GetType().Name + ") supplied to " + GetType().Name, "content");
		}
		base.SetContent(content);
		_textLabel.text = stationTextEntry.GetText();
		_chargeCountLabel.text = stationTextEntry.LevelRequirement;
		_displayPowerToughness = stationTextEntry.DisplayPowerToughness;
		_powerToughnessLabel.gameObject.UpdateActive(_displayPowerToughness);
		_background.color = _backgroundColorTable.GetColor(stationTextEntry.PresentationColor, stationTextEntry.IsFirstStationAbility);
		_activeOverlay.enabled = !stationTextEntry.IsActive;
		if (_displayPowerToughness)
		{
			_textLabel.margin = new Vector4(0f, 0f, _powerToughnessWidth, 0f);
			_powerToughnessLabel.text = GetPowerToughnessText(stationTextEntry.Power, stationTextEntry.Toughness);
		}
		else
		{
			_textLabel.margin = Vector4.zero;
		}
	}

	private static string GetPowerToughnessText(StringBackedInt power, StringBackedInt toughness)
	{
		return power.RawText + "/" + toughness.RawText;
	}

	public override void CleanUp()
	{
		base.CleanUp();
		_displayPowerToughness = false;
		_chargeCountLabel.text = " ";
		_powerToughnessLabel.text = " ";
	}

	public override void UpdateVisibility(RectTransform viewportTransform)
	{
		UpdateVisibilityForElement(viewportTransform, _chargeCountLabel.gameObject);
		if (_displayPowerToughness)
		{
			UpdateVisibilityForElement(viewportTransform, _powerToughnessLabel.gameObject);
		}
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

	public override float GetPreferredHeight()
	{
		return base.GetPreferredHeight() + _padding;
	}
}
