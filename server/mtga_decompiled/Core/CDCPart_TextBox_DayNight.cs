using System.Collections.Generic;
using System.Text;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.Parts.Textbox;

public class CDCPart_TextBox_DayNight : CDCPart_Textbox_SuperBase
{
	[SerializeField]
	private TMP_Text _upperTextLabel;

	[SerializeField]
	private TMP_Text _lowerTextLabel;

	[SerializeField]
	private List<uint> _abilityIdsForUpperLabel = new List<uint> { 147678u };

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		_upperTextLabel.font = _fontAsset;
		_lowerTextLabel.font = _fontAsset;
		_upperTextLabel.fontSize = _supportedFontSizes[0];
		_lowerTextLabel.fontSize = _supportedFontSizes[0];
		UpdateLabelMaterial(_upperTextLabel);
		UpdateLabelMaterial(_upperTextLabel);
		StringBuilder stringBuilder = _genericObjectPool.PopObject<StringBuilder>();
		foreach (AbilityPrintingData ability in _cachedModel.Abilities)
		{
			if (_abilityIdsForUpperLabel.Contains(ability.Id))
			{
				string formatForAbility = GetFormatForAbility(_cachedModel.InstanceId, ability, AbilityState.Normal);
				stringBuilder.AppendFormat(formatForAbility, _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(_cachedModel.GrpId, ability.Id, _cachedModel.AbilityIds, _cachedModel.TitleId));
				stringBuilder.AppendLine();
			}
		}
		_upperTextLabel.SetText(string.Format(_colorSettings.DefaultFormat, stringBuilder));
		stringBuilder.Clear();
		foreach (AbilityPrintingData ability2 in _cachedModel.Abilities)
		{
			if (!_abilityIdsForUpperLabel.Contains(ability2.Id))
			{
				string formatForAbility2 = GetFormatForAbility(_cachedModel.InstanceId, ability2, AbilityState.Normal);
				stringBuilder.AppendFormat(formatForAbility2, _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(_cachedModel.GrpId, ability2.Id, _cachedModel.AbilityIds, _cachedModel.TitleId));
				stringBuilder.AppendLine();
			}
		}
		_lowerTextLabel.SetText(string.Format(_colorSettings.DefaultFormat, stringBuilder));
		stringBuilder.Clear();
		_genericObjectPool.PushObject(stringBuilder, tryClear: false);
	}

	protected override void HandleDestructionInternal()
	{
		_upperTextLabel.gameObject.SetActive(!_cachedDestroyed);
		_lowerTextLabel.gameObject.SetActive(!_cachedDestroyed);
		if (!_cachedDestroyed)
		{
			Renderer[] componentsInChildren = _upperTextLabel.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
			componentsInChildren = _lowerTextLabel.GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
		base.HandleDestructionInternal();
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		if ((bool)_upperTextLabel)
		{
			_upperTextLabel.SetText(" ");
		}
		if ((bool)_lowerTextLabel)
		{
			_lowerTextLabel.SetText(" ");
		}
	}
}
