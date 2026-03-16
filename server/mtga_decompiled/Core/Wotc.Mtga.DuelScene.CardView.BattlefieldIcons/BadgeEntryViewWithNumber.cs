using TMPro;
using UnityEngine;

namespace Wotc.Mtga.DuelScene.CardView.BattlefieldIcons;

public class BadgeEntryViewWithNumber : BadgeEntryView
{
	[SerializeField]
	private TMP_Text _label;

	[SerializeField]
	private TMP_Text _modifierLabel;

	public void Init(int number, string modifier, BadgeEntryStatus badgeEntryStatus)
	{
		Init(number, badgeEntryStatus);
		if ((bool)_modifierLabel && !string.IsNullOrWhiteSpace(modifier))
		{
			_modifierLabel.enabled = true;
			_modifierLabel.text = modifier;
		}
	}

	public void Init(int number, BadgeEntryStatus badgeEntryStatus)
	{
		base.Init(badgeEntryStatus);
		_label.text = number.ToString();
		_label.enabled = true;
		if ((bool)_modifierLabel)
		{
			_modifierLabel.enabled = false;
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		_label.enabled = false;
		if ((bool)_modifierLabel)
		{
			_modifierLabel.enabled = true;
		}
	}
}
