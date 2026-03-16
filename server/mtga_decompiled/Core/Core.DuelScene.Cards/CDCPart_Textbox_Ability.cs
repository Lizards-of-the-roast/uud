using System.Text;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Parts.Textbox;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Core.DuelScene.Cards;

public class CDCPart_Textbox_Ability : CDCPart_Textbox_SuperBase
{
	[SerializeField]
	private TMP_Text _LoyaltyAndLoreTextLabel;

	[SerializeField]
	private DefaultAbilityTextbox _defaultTextbox;

	[SerializeField]
	private LoyaltyAbilityTextbox _loyaltyTextboxPositive;

	[SerializeField]
	private LoyaltyAbilityTextbox _loyaltyTextboxNeutral;

	[SerializeField]
	private LoyaltyAbilityTextbox _loyaltyTextboxNegative;

	[SerializeField]
	private LoreAbilityTextbox _chapterTextbox;

	[SerializeField]
	private TableAbilityTextbox _tableTextbox;

	[SerializeField]
	private LoyaltyTableAbilityTextbox _loyaltyTableTextbox;

	private TextboxSubComponentBase _activeSubComponent;

	private float _lastCalculatedHeight;

	private ITextEntryParser _preAbilityTextSupplementalTextParser = NullTextEntryParser.Default;

	private ITextEntryParser _postAbilityTextSupplementalTextParser = NullTextEntryParser.Default;

	protected override void OnInit()
	{
		base.OnInit();
		_preAbilityTextSupplementalTextParser = new TextParserAggregate(new CommaSeparatorAggregate(new XChoiceParser(_cardDatabase.ClientLocProvider), new ManaSpentToCastParser(_cardDatabase.ClientLocProvider)), new LinkedInfoTextParser(_cardDatabase.GreLocProvider, _cardDatabase.ClientLocProvider, _assetLookupSystem), new LinkedInfoTitleTextParser(_cardDatabase.GreLocProvider), new DelayedTriggerParser(_cardDatabase.ClientLocProvider, _assetLookupSystem, base.GetCurrentGameState));
		_postAbilityTextSupplementalTextParser = new ParenthesisFormatingParser(ParenthesisFormatingParser.ParenthesisFormatType.Added, new TextParserAggregate(new CommaSeparatorAggregate(new AddedBackupAbilitiesTextParser(_cardDatabase.GreLocProvider, _cardDatabase.AbilityDataProvider)), new AppendDamagesEntityText(base.EntityNameProvider, _cardDatabase.AbilityDataProvider, _cardDatabase.ClientLocProvider, base.GetCurrentGameState)));
	}

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		ICardTextEntry textBoxContentForAbility = GetTextBoxContentForAbility(_cachedModel);
		GetSubComponent(textBoxContentForAbility).SetContent(textBoxContentForAbility);
		FitTextInTextbox();
	}

	private ICardTextEntry GetTextBoxContentForAbility(ICardDataAdapter cardData)
	{
		AbilityPrintingData abilityOfCardModel = CardUtilities.GetAbilityOfCardModel(cardData, _cardDatabase.AbilityDataProvider, base.GetCurrentGameState?.Invoke());
		string textForAbility = GetTextForAbility(cardData, abilityOfCardModel);
		if (LoyaltyTableTextEntry.TryParse(abilityOfCardModel, textForAbility, cardData.Instance?.DieRollResults, _colorSettings, out var textEntry))
		{
			return textEntry;
		}
		if (abilityOfCardModel.PaymentType == AbilityPaymentType.Loyalty)
		{
			return new LoyaltyTextEntry(abilityOfCardModel.LoyaltyCost.RawText, textForAbility);
		}
		if (abilityOfCardModel.BaseId == 166 && abilityOfCardModel.BaseIdNumeral.HasValue)
		{
			return new ChapterTextEntry(new uint[1] { abilityOfCardModel.BaseIdNumeral.Value }, textForAbility);
		}
		if (TableTextEntry.TryGetTableTextEntry(abilityOfCardModel, textForAbility, cardData.Instance?.DieRollResults, _colorSettings, out var tableTextEntry))
		{
			return tableTextEntry;
		}
		return new BasicTextEntry(textForAbility);
	}

	private string GetTextForAbility(ICardDataAdapter cardData, AbilityPrintingData abilityPrintingData)
	{
		StringBuilder stringBuilder = _genericObjectPool.PopObject<StringBuilder>();
		foreach (ICardTextEntry item in _preAbilityTextSupplementalTextParser.ParseText(cardData, _colorSettings, _languageOverride))
		{
			stringBuilder.AppendLine(item.GetText());
		}
		if (HasALTRulesTextOverride(abilityPrintingData, out var overrideText))
		{
			stringBuilder.AppendLine(overrideText);
		}
		else if (cardData.RulesTextOverride != null)
		{
			string rulesText = cardData.RulesTextOverride.GetOverride(_colorSettings);
			rulesText = Utilities.SanitizeParentheticalText(rulesText);
			stringBuilder.AppendLine(rulesText);
		}
		else if (abilityPrintingData != null)
		{
			string formatForAbility = GetFormatForAbility(cardData.InstanceId, abilityPrintingData, AbilityState.Normal);
			string abilityTextByCardAbilityGrpId = GetAbilityTextByCardAbilityGrpId(abilityPrintingData.Id, checkForOverride: false);
			string text = string.Format(formatForAbility, abilityTextByCardAbilityGrpId);
			text = text.Substring(text.IndexOf('—') + 1);
			stringBuilder.AppendLine(text);
		}
		foreach (ICardTextEntry item2 in _postAbilityTextSupplementalTextParser.ParseText(cardData, _colorSettings, _languageOverride))
		{
			stringBuilder.AppendLine(item2.GetText());
		}
		string result = stringBuilder.ToString();
		stringBuilder.Clear();
		_genericObjectPool.PushObject(stringBuilder, tryClear: false);
		return result;
	}

	private TextboxSubComponentBase GetSubComponent(ICardTextEntry content)
	{
		TextboxSubComponentBase activeSubComponent = ((content is LoyaltyTextEntry loyaltyTextEntry) ? (loyaltyTextEntry.GetValence() switch
		{
			LoyaltyValence.Positive => _loyaltyTextboxPositive, 
			LoyaltyValence.Neutral => _loyaltyTextboxNeutral, 
			LoyaltyValence.Negative => _loyaltyTextboxNegative, 
			_ => _defaultTextbox, 
		}) : ((content is ChapterTextEntry) ? _chapterTextbox : ((content is TableTextEntry) ? _tableTextbox : ((!(content is LoyaltyTableTextEntry)) ? ((TextboxSubComponentBase)_defaultTextbox) : ((TextboxSubComponentBase)_loyaltyTableTextbox)))));
		_activeSubComponent = activeSubComponent;
		_loyaltyTextboxPositive.gameObject.UpdateActive(_activeSubComponent == _loyaltyTextboxPositive);
		_loyaltyTextboxNeutral.gameObject.UpdateActive(_activeSubComponent == _loyaltyTextboxNeutral);
		_loyaltyTextboxNegative.gameObject.UpdateActive(_activeSubComponent == _loyaltyTextboxNegative);
		_chapterTextbox.gameObject.UpdateActive(_activeSubComponent == _chapterTextbox);
		_tableTextbox.gameObject.UpdateActive(_activeSubComponent == _tableTextbox);
		_defaultTextbox.gameObject.UpdateActive(_activeSubComponent == _defaultTextbox);
		_loyaltyTableTextbox.gameObject.UpdateActive(_activeSubComponent == _loyaltyTableTextbox);
		_LoyaltyAndLoreTextLabel.gameObject.UpdateActive(_activeSubComponent == _loyaltyTextboxPositive || _activeSubComponent == _loyaltyTextboxNeutral || _activeSubComponent == _loyaltyTextboxNegative || _activeSubComponent == _chapterTextbox);
		_activeSubComponent.SetFont(_fontAsset);
		_activeSubComponent.SetAlignment(_textAlignment);
		_activeSubComponent.SetLineSpacing(_lineSpacing);
		_activeSubComponent.SetMaterial(base.UpdateLabelMaterial, _languageOverride);
		_activeSubComponent.gameObject.SetLayer(base.gameObject.layer);
		foreach (CDCMaterialFiller cdcFillersOnNonLabelVisual in _activeSubComponent.GetCdcFillersOnNonLabelVisuals())
		{
			cdcFillersOnNonLabelVisual.Init(_cardMaterialBuilder, _cardDatabase);
			cdcFillersOnNonLabelVisual.UpdateMaterials(_cachedModel, _cachedCardHolderType, base.GetCurrentGameState, _cachedViewMetadata.IsDimmed, _cachedViewMetadata.IsMouseOver);
		}
		return _activeSubComponent;
	}

	private void FitTextInTextbox()
	{
		_lastCalculatedHeight = 0f;
		_scrollRect.velocity = Vector2.zero;
		float num = Mathf.Abs(_scrollRect.viewport.rect.y);
		bool flag = false;
		foreach (float supportedFontSize in _supportedFontSizes)
		{
			_activeSubComponent.SetFontSize(supportedFontSize);
			_lastCalculatedHeight = _activeSubComponent.GetPreferredHeight();
			if (_lastCalculatedHeight <= num)
			{
				flag = true;
				break;
			}
		}
		_scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, _lastCalculatedHeight);
		Vector3 localPosition = _scrollRect.content.localPosition;
		if (flag)
		{
			_scrollRect.enabled = false;
			_scrollRect.verticalScrollbar.gameObject.UpdateActive(active: false);
			float num2 = num - _lastCalculatedHeight;
			localPosition.y = (0f - num2) * 0.5f;
		}
		else
		{
			_scrollRect.enabled = true;
			_scrollRect.verticalScrollbar.gameObject.UpdateActive(active: true);
			localPosition.y = 0f;
		}
		_scrollRect.content.localPosition = localPosition;
		LayoutRebuilder.MarkLayoutForRebuild(_scrollRect.content);
	}

	public override void EnableTouchScroll()
	{
		EnableTouchScroll(_lastCalculatedHeight);
	}

	public override bool ScrollTextbox(Vector2 delta)
	{
		return CDCPart_Textbox_SuperBase.ScrollTextbox(delta, _scrollRect);
	}
}
