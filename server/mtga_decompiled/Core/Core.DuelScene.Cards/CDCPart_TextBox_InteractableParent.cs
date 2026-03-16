using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.Parts.Textbox;
using Wotc.Mtga.Loc;

namespace Core.DuelScene.Cards;

public class CDCPart_TextBox_InteractableParent : CDCPart_Textbox_SuperBase
{
	[SerializeField]
	protected List<InteractableTextBox> _textBoxes;

	private string _lastLanguageUsed = string.Empty;

	private readonly List<(string, string)> _cachedLabelStrings = new List<(string, string)>();

	protected IReadOnlyCollection<AbilityPrintingData> _overrideAbilities;

	private IReadOnlyCollection<AbilityPrintingData> _abilities => _overrideAbilities ?? _cachedModel.PrintingAbilities;

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		if (Languages.CurrentLanguage != _lastLanguageUsed)
		{
			_lastLanguageUsed = Languages.CurrentLanguage;
			_cachedLabelStrings.Clear();
			foreach (AbilityPrintingData ability in _abilities)
			{
				string formatForAbility = base.GetFormatForAbility(_cachedModel.InstanceId, ability, AbilityState.Normal);
				string abilityTextByCardAbilityGrpId = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(_cachedModel.GrpId, ability.Id, _cachedModel.AbilityIds);
				string[] array = abilityTextByCardAbilityGrpId.Split('—');
				if (array.Length == 2)
				{
					string item = string.Format(formatForAbility, array[0]);
					string item2 = string.Format(formatForAbility, array[1]);
					_cachedLabelStrings.Add((item, item2));
				}
				else
				{
					string item3 = string.Format(formatForAbility, abilityTextByCardAbilityGrpId);
					_cachedLabelStrings.Add((string.Empty, item3));
				}
			}
		}
		TMP_FontAsset titleFont = GetTitleFont();
		TMP_FontAsset fontAsset = _fontAsset;
		for (int i = 0; i < _cachedLabelStrings.Count && i < _textBoxes.Count; i++)
		{
			(string, string) tuple = _cachedLabelStrings[i];
			string item4 = tuple.Item1;
			string item5 = tuple.Item2;
			InteractableTextBox interactableTextBox = _textBoxes[i];
			interactableTextBox.SetFont(titleFont, fontAsset);
			interactableTextBox.SetLabels(item4, item5);
		}
		SetTextBoxStatus(_cachedModel);
		FitTextInTextbox();
	}

	private void FitTextInTextbox()
	{
		foreach (InteractableTextBox textBox in _textBoxes)
		{
			textBox.FitTextInTextbox(_supportedFontSizes);
		}
	}

	private TMP_FontAsset GetTitleFont()
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(_cachedModel);
		_assetLookupSystem.Blackboard.SetCdcViewMetadata(_cachedViewMetadata);
		_assetLookupSystem.Blackboard.GameState = base.GetCurrentGameState?.Invoke();
		_assetLookupSystem.Blackboard.CardHolderType = _cachedCardHolderType;
		_assetLookupSystem.Blackboard.Language = Languages.CurrentLanguage;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FieldFont> loadedTree))
		{
			FieldFont payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				TMP_FontAsset tMP_FontAsset = AssetLoader.AcquireAndTrackAsset(_assetTracker, "FieldFont", payload.FontAssetReference);
				if ((object)tMP_FontAsset != null)
				{
					return tMP_FontAsset;
				}
			}
		}
		return null;
	}

	protected virtual void SetTextBoxStatus(ICardDataAdapter cardData)
	{
	}
}
