using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads;
using AssetLookupTree.Payloads.Card;
using AssetLookupTree.Payloads.Card.RulesText;
using GreClient.CardData;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Duel;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Cards.Parts.Textbox;

public abstract class CDCPart_Textbox_SuperBase : CDCPart
{
	private static readonly Vector3 DEFAULT_COLLIDER_OFFSET = new Vector3(0f, 0f, 0.2f);

	[SerializeField]
	protected ScrollRect _scrollRect;

	protected CardTextColorSettings _colorSettings;

	protected TMP_FontAsset _fontAsset;

	protected TextAlignmentOptions _textAlignment = TextAlignmentOptions.TopLeft;

	protected IReadOnlyList<float> _supportedFontSizes = Wotc.Mtga.Cards.Text.Constants.DEFAULT_FONT_SIZES;

	protected float _lineSpacing;

	protected bool _canShowFrameLanguage;

	protected string _languageOverride;

	private (string cachedLanguage, Material originalMaterial) _originalFontMaterial = (cachedLanguage: null, originalMaterial: null);

	private AssetLoader.AssetTracker<Material> _fontMaterialTracker = new AssetLoader.AssetTracker<Material>("CDCPart_Textbox_SuperBase");

	private GraphicRaycaster _textboxScrollRaycaster;

	protected override void HandleUpdateInternal()
	{
		_canShowFrameLanguage = FrameLanguageUtilities.CanShowFrameLanguage(_cachedModel, base.GetCurrentGameState?.Invoke(), _cachedViewMetadata.IsMouseOver ? MouseOverType.MouseOver : MouseOverType.None, _cachedCardHolderType, _cachedViewMetadata.IsHoverCopy, CDCFieldFillerFieldType.RulesText, Languages.CurrentLanguage);
		if (!_canShowFrameLanguage || (!FrameLanguageUtilities.IsMysticalArchiveLanguageOverride(_cachedModel, out _languageOverride) && !FrameLanguageUtilities.IsPhyrexianLanguageOverride(_cachedModel, out _languageOverride) && !FrameLanguageUtilities.IsEnglishLanguageOverride(_cachedModel, out _languageOverride)))
		{
			_languageOverride = null;
		}
		FillBlackboardForTextbox(_assetLookupSystem.Blackboard);
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FieldFont> loadedTree))
		{
			FieldFont payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				TMP_FontAsset tMP_FontAsset = AssetLoader.AcquireAndTrackAsset(_assetTracker, "FieldFont", payload.FontAssetReference);
				if ((object)tMP_FontAsset != null)
				{
					_fontAsset = tMP_FontAsset;
					goto IL_00f8;
				}
			}
		}
		_fontAsset = null;
		goto IL_00f8;
		IL_0272:
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<TextboxLineSpacing> loadedTree2))
		{
			TextboxLineSpacing payload2 = loadedTree2.GetPayload(_assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				_lineSpacing = payload2.LineSpacing;
				goto IL_02b8;
			}
		}
		_lineSpacing = 0f;
		goto IL_02b8;
		IL_02b8:
		_assetLookupSystem.Blackboard.Clear();
		return;
		IL_00f8:
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FieldTextColor> loadedTree3))
		{
			FieldTextColor payload3 = loadedTree3.GetPayload(_assetLookupSystem.Blackboard);
			if (payload3 != null)
			{
				FieldTextColorSettings fieldTextColorSettings = _cardColorCaches.GetFieldTextColorSettings(payload3.ColorSettingsRef);
				if ((object)fieldTextColorSettings != null)
				{
					_colorSettings = fieldTextColorSettings.Settings;
					goto IL_01e6;
				}
			}
		}
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<TextColor> loadedTree4))
		{
			TextColor payload4 = loadedTree4.GetPayload(_assetLookupSystem.Blackboard);
			if (payload4 != null)
			{
				CardTextColorTable cardTextColorTable = _cardColorCaches.GetCardTextColorTable(payload4.ColorTableRef);
				if ((object)cardTextColorTable != null)
				{
					_colorSettings = cardTextColorTable.FieldTypeOverrides.Find((CardTextColorTable.FieldTypeOverride x) => x.FieldType == CDCFieldFillerFieldType.RulesText)?.Settings ?? cardTextColorTable.DefaultSettings;
					goto IL_01e6;
				}
			}
		}
		_colorSettings = new CardTextColorSettings();
		goto IL_01e6;
		IL_022c:
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<TextAlignmentOverride> loadedTree5))
		{
			TextAlignmentOverride payload5 = loadedTree5.GetPayload(_assetLookupSystem.Blackboard);
			if (payload5 != null)
			{
				_textAlignment = payload5.AlignmentOptions;
				goto IL_0272;
			}
		}
		_textAlignment = TextAlignmentOptions.TopLeft;
		goto IL_0272;
		IL_01e6:
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<TextboxFontSizes> loadedTree6))
		{
			TextboxFontSizes payload6 = loadedTree6.GetPayload(_assetLookupSystem.Blackboard);
			if (payload6 != null)
			{
				_supportedFontSizes = payload6.FontSizes;
				goto IL_022c;
			}
		}
		_supportedFontSizes = Wotc.Mtga.Cards.Text.Constants.DEFAULT_FONT_SIZES;
		goto IL_022c;
	}

	public override void HandleCleanup()
	{
		_fontMaterialTracker.Cleanup();
		_colorSettings = null;
		base.HandleCleanup();
	}

	protected virtual string GetFormatForAbility(uint affectorId, AbilityPrintingData ability, AbilityState state, bool isPerpetual = false)
	{
		if (ability != null && ability.HasParseFailure())
		{
			return Wotc.Mtga.Cards.Text.Constants.PARSE_FAILURE_FORMAT;
		}
		if (ability != null && ability.IsModalAbilityChild())
		{
			return _colorSettings.DefaultFormat;
		}
		string currentFormat = (((state & (AbilityState.Removed | AbilityState.Exhausted)) != AbilityState.None) ? _colorSettings.RemovedFormat : (state switch
		{
			AbilityState.Normal => _colorSettings.DefaultFormat, 
			AbilityState.Added => isPerpetual ? _colorSettings.PerpetualFormat : _colorSettings.AddedFormat, 
			_ => throw new NotImplementedException(state.ToString()), 
		}));
		return GetTargetingFormat(currentFormat, affectorId, ability);
	}

	protected string GetTargetingFormat(string currentFormat, uint affectorId, AbilityPrintingData abilityPrintingData)
	{
		MtgGameState mtgGameState = base.GetCurrentGameState?.Invoke();
		if (mtgGameState != null)
		{
			List<TargetSpec> list = _genericObjectPool.PopObject<List<TargetSpec>>();
			foreach (TargetSpec item in mtgGameState.TargetInfo)
			{
				if (item.Affector == affectorId)
				{
					list.Add(item);
				}
			}
			string formatDueToTargeting = TargetingColorer.GetFormatDueToTargeting(abilityPrintingData.Id, _cardDatabase.AbilityDataProvider, list, _assetLookupSystem, _cachedModel);
			if (formatDueToTargeting != TargetingColorer.EmptyFormat)
			{
				currentFormat = formatDueToTargeting;
			}
			list.Clear();
			_genericObjectPool.PushObject(list, tryClear: false);
		}
		return currentFormat;
	}

	protected void UpdateLabelMaterial(TMP_Text label, string language = null)
	{
		string text = (string.IsNullOrEmpty(language) ? Languages.CurrentLanguage : language);
		if (!_originalFontMaterial.originalMaterial || text != _originalFontMaterial.cachedLanguage)
		{
			_originalFontMaterial = (cachedLanguage: text, originalMaterial: label.font.material);
		}
		FillBlackboardForTextbox(_assetLookupSystem.Blackboard);
		_assetLookupSystem.Blackboard.Language = text;
		_assetLookupSystem.Blackboard.Material = _originalFontMaterial.originalMaterial;
		_assetLookupSystem.Blackboard.Font = label.font;
		_assetLookupSystem.Blackboard.FontName = label.font.name;
		Material material = null;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FontMaterialSettings> loadedTree))
		{
			FontMaterialSettings payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				material = _fontMaterialTracker.Acquire(payload.MaterialReference);
				goto IL_0111;
			}
		}
		_fontMaterialTracker.Cleanup();
		material = _originalFontMaterial.originalMaterial;
		goto IL_0111;
		IL_0111:
		_assetLookupSystem.Blackboard.Clear();
		if (!material)
		{
			label.fontSharedMaterial = label.font.material;
		}
		else if (!material.name.StartsWith(label.font.name))
		{
			label.fontSharedMaterial = label.font.material;
		}
		else
		{
			label.fontSharedMaterial = material;
		}
	}

	public virtual void EnableTouchScroll()
	{
		if ((bool)_scrollRect)
		{
			EnableTouchScroll(_scrollRect.preferredHeight);
		}
	}

	public virtual void EnableTouchScroll(float totalHeight)
	{
		if (!_scrollRect)
		{
			return;
		}
		float num = Mathf.Abs(_scrollRect.viewport.rect.y);
		BASE_CDC componentInParent = GetComponentInParent<BASE_CDC>();
		if (totalHeight > num)
		{
			if (_textboxScrollRaycaster == null)
			{
				_textboxScrollRaycaster = base.gameObject.GetComponentInChildren<Canvas>().gameObject.AddComponent<GraphicRaycaster>();
			}
			else
			{
				_textboxScrollRaycaster.enabled = true;
			}
			Image componentInChildren = _scrollRect.GetComponentInChildren<Image>();
			if (componentInChildren != null)
			{
				componentInChildren.raycastTarget = true;
			}
			if (componentInParent != null)
			{
				componentInParent.Collider.center = DEFAULT_COLLIDER_OFFSET;
			}
		}
		else if (_textboxScrollRaycaster != null)
		{
			_textboxScrollRaycaster.enabled = false;
			if (componentInParent != null)
			{
				componentInParent.Collider.center = Vector3.zero;
			}
		}
	}

	public virtual bool ScrollTextbox(Vector2 delta)
	{
		return false;
	}

	protected static bool ScrollTextbox(Vector2 delta, ScrollRect scrollRect)
	{
		if ((bool)scrollRect && scrollRect.enabled)
		{
			delta.y *= -1f;
			float scrollSensitivity = scrollRect.scrollSensitivity;
			scrollSensitivity *= Mathf.Max(0.1f, MDNPlayerPrefs.TextboxScrollSensitivity);
			scrollRect.velocity += delta * scrollSensitivity;
			if (scrollRect.verticalNormalizedPosition > 0.99f && scrollRect.velocity.y < 0f)
			{
				scrollRect.velocity = Vector2.zero;
			}
			else if (scrollRect.verticalNormalizedPosition < 0.01f && scrollRect.velocity.y > 0f)
			{
				scrollRect.velocity = Vector2.zero;
			}
			return true;
		}
		return false;
	}

	protected bool HasALTRulesTextOverride(AbilityPrintingData ability, out string overrideText)
	{
		FillBlackboardForTextbox(_assetLookupSystem.Blackboard);
		_assetLookupSystem.Blackboard.Ability = ability;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<RulesTextOverride> loadedTree))
		{
			RulesTextOverride payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				List<(string, string)> list = _genericObjectPool.PopObject<List<(string, string)>>();
				foreach (ILocParameterProvider parameterProvider in payload.ParameterProviders)
				{
					if (parameterProvider.TryGetValue(_assetLookupSystem.Blackboard, out var paramValue))
					{
						list.Add((parameterProvider.GetKey(), paramValue));
					}
				}
				overrideText = _cardDatabase.ClientLocProvider.GetLocalizedTextForLanguage(payload.LocKey, _languageOverride, list.ToArray());
				list.Clear();
				_genericObjectPool.PushObject(list, tryClear: false);
				return true;
			}
		}
		_assetLookupSystem.Blackboard.Clear();
		overrideText = string.Empty;
		return false;
	}

	protected string GetAbilityTextByCardAbilityGrpId(uint abilityGrpId)
	{
		return GetAbilityTextByCardAbilityGrpId(abilityGrpId, checkForOverride: true);
	}

	protected string GetAbilityTextByCardAbilityGrpId(uint abilityGrpId, bool checkForOverride)
	{
		AbilityPrintingData abilityPrintingById = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(abilityGrpId);
		if (checkForOverride && HasALTRulesTextOverride(abilityPrintingById, out var overrideText))
		{
			return overrideText;
		}
		List<uint> list = _genericObjectPool.PopObject<List<uint>>();
		MtgCardInstance instance = _cachedModel.Instance;
		if (instance != null)
		{
			addGrpIdsForInstance(instance, list);
			MtgCardInstance parent = instance.Parent;
			if (parent != null)
			{
				addGrpIdsForInstance(parent, list);
			}
		}
		else
		{
			addGrpId(_cachedModel.GrpId, list);
		}
		string text = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(list, abilityGrpId, _cachedModel.AbilityIds, _cachedModel.TitleId, _languageOverride);
		list.Clear();
		_genericObjectPool.PushObject(list, tryClear: false);
		if (_cachedModel.Printing.TextChangeData.ChangedAbilityId == abilityGrpId)
		{
			MatchCollection matchCollection = Wotc.Mtga.Cards.Text.Constants.STRIKETHROUGH_MATCHER.Matches(text);
			if (matchCollection != null)
			{
				foreach (Match item in matchCollection)
				{
					text = text.Replace(item.Value, string.Format(_colorSettings.RemovedFormat, item.Value));
				}
			}
		}
		return text;
		static void addGrpId(uint grpId, List<uint> toList)
		{
			if (grpId != 0 && !toList.Contains(grpId))
			{
				toList.Add(grpId);
			}
		}
		static void addGrpIdsForInstance(MtgCardInstance cardInstance, List<uint> toList)
		{
			if (cardInstance.ObjectType != GameObjectType.Ability)
			{
				addGrpId(cardInstance.GrpId, toList);
				if (cardInstance.GrpId != cardInstance.BaseGrpId)
				{
					addGrpId(cardInstance.BaseGrpId, toList);
				}
			}
			addGrpId(cardInstance.ObjectSourceGrpId, toList);
			foreach (AddedAbilityData abilityAdder in cardInstance.AbilityAdders)
			{
				addGrpId(abilityAdder.SourceGrpId, toList);
				addGrpId(abilityAdder.AddedByGrpId, toList);
			}
			foreach (uint abilityOriginalCardGrpId in cardInstance.AbilityOriginalCardGrpIds)
			{
				addGrpId(abilityOriginalCardGrpId, toList);
			}
			if (cardInstance.MutationParent != null)
			{
				addGrpId(cardInstance.MutationParent.GrpId, toList);
			}
			foreach (MtgCardInstance mutationChild in cardInstance.MutationChildren)
			{
				addGrpId(mutationChild.GrpId, toList);
			}
		}
	}

	protected void FillBlackboardForTextbox(IBlackboard bb)
	{
		bb.Clear();
		bb.SetCardDataExtensive(_cachedModel);
		bb.SetCdcViewMetadata(_cachedViewMetadata);
		bb.CardHolderType = _cachedCardHolderType;
		bb.FieldFillerType = CDCFieldFillerFieldType.RulesText;
		bb.Language = Languages.CurrentLanguage;
		bb.FixedRulesTextSize = MDNPlayerPrefs.FixedRulesTextSize;
		if (_cachedModel.ObjectType == GameObjectType.Ability)
		{
			bb.Ability = CardUtilities.GetAbilityOfCardModel(_cachedModel, _cardDatabase.AbilityDataProvider, base.GetCurrentGameState?.Invoke());
		}
	}
}
