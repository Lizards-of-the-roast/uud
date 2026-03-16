using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card.RulesText;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Parts.Textbox;
using Wotc.Mtga.Cards.Text;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class CDCPart_TextBox_Rules : CDCPart_Textbox_SuperBase
{
	public enum ExtraSpaceDistribution
	{
		None = -1,
		EitherEnd,
		DistributeEvenly,
		DistributeProportionally
	}

	public enum AbilityFiltering
	{
		None,
		ExclusiveChapterAbilities,
		FilterOutChapterAbilities
	}

	[SerializeField]
	protected RectTransform _subComponentRoot;

	[SerializeField]
	protected float _abilitySpacing = 0.035f;

	[SerializeField]
	protected ExtraSpaceDistribution _extraSpaceDistribution;

	[SerializeField]
	private AbilityFiltering _filterOptions;

	private IFlavorTextProvider _flavorTextProvider = NullFlavorTextProvider.Default;

	private ITextEntryParser _preRulesSupplementalText = NullTextEntryParser.Default;

	private ITextEntryParser _postRulesSupplementalText = NullTextEntryParser.Default;

	private ITextEntryParser _rulesTextOverrideParser = NullTextEntryParser.Default;

	private readonly List<TextboxSubComponentBase> _activeSubComponents = new List<TextboxSubComponentBase>(5);

	private float _lastCalculatedHeight;

	public bool DisplayingFlavorText { get; private set; }

	protected override void OnInit()
	{
		base.OnInit();
		_scrollRect.onValueChanged.AddListener(OnScrollViewChanged);
		if (_filterOptions != AbilityFiltering.ExclusiveChapterAbilities)
		{
			_flavorTextProvider = new DefaultFlavorTextProvider(_cardDatabase.GreLocProvider, _cardDatabase.AltFlavorTextKeyProvider, _cardDatabase.ClientLocProvider);
			_preRulesSupplementalText = new PreventNullInstanceDecorator(new TextParserAggregate(new RingBearerTextParser(_localizationManager, _genericObjectPool, _assetLookupSystem), new AbilityWordSupplementalTextParser(_localizationManager, _cardDatabase.AbilityDataProvider, _assetLookupSystem), new LinkedInfoTextParser(_cardDatabase.GreLocProvider, _localizationManager, _assetLookupSystem), new AlternateCostTextParser(_localizationManager, _cardDatabase.GreLocProvider), new LinkedInfoTitleTextParser(_cardDatabase.GreLocProvider), new ReplacementEffectParser(_cardDatabase.CardTitleProvider, _genericObjectPool, base.GetCurrentGameState), new DelayedTriggerParser(_localizationManager, _assetLookupSystem, base.GetCurrentGameState), new CommaSeparatorAggregate(new XChoiceParser(_localizationManager), new ManaSpentToCastParser(_localizationManager)), new ColorsSpentToCastParser(_localizationManager), new MutationsParser(_cardDatabase.CardTitleProvider, _localizationManager), new BoonInfoTextParser(_localizationManager), new AdditionalCostOptionTextParser(_localizationManager, _cardDatabase), new TokenFakeAbilityTextParser(_cardDatabase.GreLocProvider)));
			_postRulesSupplementalText = new PreventNullModelDecorator(new MeldsWithTextParser(_cardDatabase.GreLocProvider));
			_rulesTextOverrideParser = new PreventNullModelDecorator(new TextParserAggregate(_preRulesSupplementalText, new RulesTextOverrideParser()));
		}
	}

	public override bool ScrollTextbox(Vector2 delta)
	{
		return CDCPart_Textbox_SuperBase.ScrollTextbox(delta, _scrollRect);
	}

	private void OnScrollViewChanged(Vector2 _)
	{
		UpdateSubComponentVisibility();
	}

	public override void EnableTouchScroll()
	{
		EnableTouchScroll(_lastCalculatedHeight);
	}

	private void UpdateSubComponentVisibility()
	{
		foreach (TextboxSubComponentBase activeSubComponent in _activeSubComponents)
		{
			activeSubComponent.UpdateVisibility(_scrollRect.viewport);
		}
	}

	private void SetContentForSubComponents(IEnumerable<ICardTextEntry> textEntries)
	{
		foreach (ICardTextEntry textEntry in textEntries)
		{
			GetSubComponentForContent(textEntry).SetContent(textEntry);
		}
	}

	private static void SetBackgroundStripesForSubComponents(IReadOnlyList<TextboxSubComponentBase> textboxSubComponents)
	{
		if (textboxSubComponents.Count <= 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < textboxSubComponents.Count; i++)
		{
			TextboxSubComponentBase textboxSubComponentBase = textboxSubComponents[i];
			textboxSubComponentBase.SetStripeEnabled(flag);
			if (i + 1 < textboxSubComponents.Count)
			{
				flag = ((!(textboxSubComponents[i + 1] is LoyaltyAbilityTextbox)) ? ((textboxSubComponentBase is LoyaltyAbilityTextbox) ? (!flag) : flag) : (!flag));
			}
		}
	}

	private void FitTextInTextbox()
	{
		float bottomPadding = GetBottomPadding();
		_lastCalculatedHeight = 0f;
		_scrollRect.velocity = Vector2.zero;
		float num = Mathf.Abs(_scrollRect.viewport.rect.y) - bottomPadding;
		bool flag = false;
		for (int i = 0; i < _supportedFontSizes.Count; i++)
		{
			float fontSize = _supportedFontSizes[i];
			_lastCalculatedHeight = _abilitySpacing * (float)(_activeSubComponents.Count - 1);
			foreach (TextboxSubComponentBase activeSubComponent in _activeSubComponents)
			{
				activeSubComponent.SetFontSize(fontSize);
				_lastCalculatedHeight += activeSubComponent.GetPreferredHeight();
			}
			if (_lastCalculatedHeight <= num)
			{
				flag = true;
				break;
			}
		}
		Vector3 labelLocalPosition = _subComponentRoot.localPosition;
		if (flag)
		{
			float extraHeight = num - _lastCalculatedHeight;
			PositionAbilitiesStatic(ref labelLocalPosition, num, _lastCalculatedHeight, extraHeight);
		}
		else
		{
			PositionAbilitiesScrolling(ref labelLocalPosition, _lastCalculatedHeight, bottomPadding);
		}
		_subComponentRoot.localPosition = labelLocalPosition;
		UpdateSubComponentVisibility();
	}

	private float GetBottomPadding()
	{
		if (_filterOptions == AbilityFiltering.ExclusiveChapterAbilities)
		{
			return 0f;
		}
		FillBlackboardForTextbox(_assetLookupSystem.Blackboard);
		float result = 0f;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<TextBoxPadding> loadedTree))
		{
			TextBoxPadding payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				result = payload.BottomPadding;
			}
		}
		_assetLookupSystem.Blackboard.Clear();
		return result;
	}

	private void PositionAbilitiesStatic(ref Vector3 labelLocalPosition, float viewportSize, float totalHeight, float extraHeight)
	{
		float num = 0f;
		if (_extraSpaceDistribution == ExtraSpaceDistribution.EitherEnd)
		{
			num = -0.5f * extraHeight;
		}
		foreach (TextboxSubComponentBase activeSubComponent in _activeSubComponents)
		{
			float preferredHeight = activeSubComponent.GetPreferredHeight();
			float num2 = 0f;
			switch (_extraSpaceDistribution)
			{
			case ExtraSpaceDistribution.DistributeEvenly:
			{
				int count = _activeSubComponents.Count;
				num2 = extraHeight / (float)count;
				break;
			}
			case ExtraSpaceDistribution.DistributeProportionally:
			{
				float num3 = preferredHeight / totalHeight;
				num2 = extraHeight * num3;
				break;
			}
			}
			preferredHeight += num2;
			activeSubComponent.RectTransform.sizeDelta = new Vector2(activeSubComponent.RectTransform.ParentSize().x, preferredHeight);
			activeSubComponent.RectTransform.anchoredPosition3D = new Vector2(activeSubComponent.RectTransform.pivot.x, num);
			num -= _abilitySpacing;
			num -= preferredHeight;
		}
		_subComponentRoot.sizeDelta = new Vector2(_subComponentRoot.sizeDelta.x, viewportSize);
		_scrollRect.enabled = false;
		_scrollRect.verticalScrollbar.gameObject.UpdateActive(active: false);
		labelLocalPosition.y = 0f;
	}

	private void PositionAbilitiesScrolling(ref Vector3 labelLocalPosition, float totalHeight, float bottomPadding)
	{
		float num = 0f;
		foreach (TextboxSubComponentBase activeSubComponent in _activeSubComponents)
		{
			float preferredHeight = activeSubComponent.GetPreferredHeight();
			activeSubComponent.RectTransform.sizeDelta = new Vector2(activeSubComponent.RectTransform.ParentSize().x, preferredHeight);
			activeSubComponent.RectTransform.anchoredPosition3D = new Vector2(activeSubComponent.RectTransform.pivot.x, num);
			num -= _abilitySpacing;
			num -= preferredHeight;
		}
		_subComponentRoot.sizeDelta = new Vector2(_subComponentRoot.sizeDelta.x, totalHeight + bottomPadding);
		_scrollRect.enabled = true;
		_scrollRect.verticalScrollbar.gameObject.UpdateActive(active: true);
		labelLocalPosition.y = 0f;
	}

	protected override void HandleDestructionInternal()
	{
		if (_subComponentRoot != null)
		{
			_subComponentRoot.gameObject.UpdateActive(!_cachedDestroyed);
		}
		base.HandleDestructionInternal();
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		ReleaseAllSubComponents();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)_scrollRect)
		{
			_scrollRect.onValueChanged.RemoveListener(OnScrollViewChanged);
		}
		DisplayingFlavorText = false;
	}

	private string TryGetBoldedAbilityText(AbilityPrintingData abilityPrinting, string localizedText)
	{
		if (abilityPrinting.IsKeyword())
		{
			return Utilities.GetBoldedAbilityText(localizedText);
		}
		return localizedText;
	}

	private IEnumerable<ICardTextEntry> GetTextFromAllAbilitiesEntries(IReadOnlyList<KeyValuePair<AbilityPrintingData, AbilityState>> abilities)
	{
		List<AbilityTextData> abilityTextDatas = _genericObjectPool.PopObject<List<AbilityTextData>>();
		PopulateAbilityTextDataList(abilities, ref abilityTextDatas);
		AbilityTextData abilityTextData = null;
		foreach (AbilityTextData abilityTextData2 in abilityTextDatas)
		{
			if (!abilityTextData2.HideText)
			{
				LoyaltyTableTextEntry textEntry;
				TableTextEntry tableTextEntry;
				if (abilityTextData2.IsChapterAbility)
				{
					yield return new DividerTextEntry();
					yield return new ChapterTextEntry(abilityTextData2.Chapters, abilityTextData2.FormattedLocalizedText);
				}
				else if (LoyaltyTableTextEntry.TryParse(abilityTextData2, _cachedModel?.Instance?.DieRollResults, _colorSettings, out textEntry))
				{
					yield return textEntry;
				}
				else if (abilityTextData2.IsLoyaltyAbility)
				{
					yield return new LoyaltyTextEntry(abilityTextData2.Printing.LoyaltyCost.RawText, abilityTextData2.FormattedLocalizedText);
				}
				else if (abilityTextData2.IsClassLevelAbility)
				{
					string format = _colorSettings.FormatForAbilityState(abilityTextData2.State);
					ManaUtilities.ParseClassLevelCost(abilityTextData2.RawLocalizedText, out var costText, out var nameText);
					yield return new LevelTextEntry(string.Format(format, costText), string.Format(format, nameText));
				}
				else if ((abilityTextData2.IsToSolveAbility && (abilityTextData == null || !abilityTextData.IsToSolveAbility)) || (abilityTextData2.IsSolvedAbility && (abilityTextData == null || !abilityTextData.IsSolvedAbility)))
				{
					yield return new DividerTextEntry();
					yield return new BasicTextEntry(abilityTextData2.FormattedLocalizedText);
				}
				else if (TableTextEntry.TryGetTableTextEntry(abilityTextData2.Printing, abilityTextData2.FormattedLocalizedText, _cachedModel?.Instance?.DieRollResults, _colorSettings, out tableTextEntry, abilityTextData2.State))
				{
					yield return tableTextEntry;
				}
				else if (abilityTextData2.IsStationAbility)
				{
					StationAbilityData stationData = abilityTextData2.StationData;
					yield return new StationTextEntry(abilityTextData2.FormattedLocalizedText, stationData.LevelRequirement, stationData.Power, stationData.Toughness, stationData.IsFirstStationAbility, stationData.PresentationColor, stationData.IsActive);
				}
				else if (abilityTextData2.IsLevelUpAbility)
				{
					LevelUpAbilityData levelUpData = abilityTextData2.LevelUpData;
					yield return new LevelUpTextEntry(abilityTextData2.FormattedLocalizedText, levelUpData.LevelRequirement, levelUpData.Power, levelUpData.Toughness, levelUpData.IsFirstGrantedAbility, levelUpData.PresentationColor, levelUpData.IsActive);
				}
				else
				{
					yield return new BasicTextEntry(abilityTextData2.FormattedLocalizedText);
				}
				abilityTextData = abilityTextData2;
			}
		}
		abilityTextDatas.Clear();
		_genericObjectPool.PushObject(abilityTextDatas, tryClear: false);
	}

	private void PopulateAbilityTextDataList(IEnumerable<KeyValuePair<AbilityPrintingData, AbilityState>> sourceList, ref List<AbilityTextData> abilityTextDatas)
	{
		HashSet<AddedAbilityData> hashSet = _genericObjectPool.PopObject<HashSet<AddedAbilityData>>();
		foreach (KeyValuePair<AbilityPrintingData, AbilityState> source in sourceList)
		{
			AbilityPrintingData abilityPrintingData = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(source.Key.Id) ?? source.Key;
			AbilityState abilityState = source.Value;
			if (IgnoreAbilityEntry(abilityPrintingData, abilityState, _cachedModel, _filterOptions, _genericObjectPool))
			{
				continue;
			}
			if (abilityPrintingData.BaseId == 202 && _cachedModel.Instance?.Owner != null && !_cachedModel.Instance.Owner.Designations.Exists(_cachedModel.Instance.GrpId, (DesignationData x, uint grpId) => x.Type == Designation.Companion && x.GrpId == grpId))
			{
				abilityState = AbilityState.Removed;
			}
			bool isMutation = false;
			if (abilityState == AbilityState.Added && !CardViewUtilities.ShouldAddedAbilityAppearAdded(abilityPrintingData, _cachedModel?.Instance, _cardDatabase, out isMutation))
			{
				abilityState = AbilityState.Normal;
			}
			if (_cachedModel.Instance != null && abilityPrintingData.EmbeddedActiveAbilityIds.Any() && !abilityPrintingData.EmbeddedActiveAbilityIds.Intersect(_cachedModel.Instance.AbilityAdders.Select((AddedAbilityData x) => x.AbilityId)).Any() && _cachedModel.Instance.Zone.Type == ZoneType.Battlefield)
			{
				abilityState = AbilityState.Removed;
			}
			bool isPerpetual = false;
			if (abilityState == AbilityState.Added)
			{
				AddedAbilityData addedAbilityData = _cachedModel.Instance.AbilityAdders.Find(abilityPrintingData.Id, hashSet, (AddedAbilityData item, uint abilityId, HashSet<AddedAbilityData> addedAbilities) => item.AbilityId == abilityId && !addedAbilities.Contains(item));
				hashSet.Add(addedAbilityData);
				isPerpetual = addedAbilityData.IsPerpetualAddedAbility(_cachedModel.Instance);
			}
			string formatForAbility = GetFormatForAbility(_cachedModel.InstanceId, source.Key, abilityState, isPerpetual);
			string text = TryGetBoldedAbilityText(localizedText: GetAbilityTextByCardAbilityGrpId(source.Key.Id), abilityPrinting: source.Key);
			string formattedLocalizedText = string.Format(formatForAbility, text);
			AbilityPrintingData printing = _cardDatabase.AbilityDataProvider.GetAbilityPrintingById(source.Key.Id) ?? AbilityPrintingData.InvalidAbility(source.Key.Id);
			abilityTextDatas.Add(new AbilityTextData
			{
				Printing = printing,
				RawLocalizedText = text,
				FormattedLocalizedText = formattedLocalizedText,
				State = abilityState,
				IsKeyword = source.Key.IsKeyword(),
				IsGroupable = source.Key.IsGroupable(),
				OmitDuplicates = source.Key.GetOmitDuplicates(),
				IsPerpetual = isPerpetual,
				IsMutation = isMutation
			});
		}
		hashSet.Clear();
		_genericObjectPool.PushObject(hashSet, tryClear: false);
		AbilityTextData.ProcessAbilityTextDatas(abilityTextDatas, _cachedModel, _colorSettings, base.GetAbilityTextByCardAbilityGrpId, GetFormatForModalChildAbility, _cardDatabase, _assetLookupSystem, (!_canShowFrameLanguage || !FrameLanguageUtilities.IsPhyrexianLanguageOverride(_cachedModel, out var _)) ? ", " : " ");
	}

	public static bool IgnoreAbilityEntry(AbilityPrintingData ability, AbilityState abilityState, ICardDataAdapter cardData, AbilityFiltering filterOptions, IObjectPool genericObjectPool)
	{
		if (filterOptions != AbilityFiltering.None)
		{
			bool flag = ability.BaseId == 166;
			if ((filterOptions == AbilityFiltering.ExclusiveChapterAbilities && !flag) || (filterOptions == AbilityFiltering.FilterOutChapterAbilities && flag))
			{
				return true;
			}
		}
		if (abilityState == AbilityState.Removed && cardData.Instance != null && !cardData.Printing.Abilities.Exists(ability.Id, (AbilityPrintingData a, uint id) => a != null && a.Id == id) && !cardData.Instance.AbilityAdders.Exists(ability.Id, (AddedAbilityData b, uint id) => b.AbilityId == id))
		{
			return true;
		}
		if (ability.BaseId == 203)
		{
			MtgCardInstance instance = cardData.Instance;
			if (instance != null && instance.MutationChildrenIds.Count > 0)
			{
				return true;
			}
		}
		if (ability.SubCategory == AbilitySubCategory.ClassAbilityGranting)
		{
			return true;
		}
		if (ability.EmbeddedIntoAbilityIds.Count > 0 && ability.EmbeddedIntoAbilityIds.Exists(cardData, (uint id, ICardDataAdapter model) => model.Printing.Abilities.ContainsId(id)))
		{
			return true;
		}
		List<AbilityPrintingData> list = genericObjectPool.PopObject<List<AbilityPrintingData>>();
		foreach (AbilityPrintingData ability2 in cardData.Abilities)
		{
			if (ability2.BaseId == ability.BaseId && ability2 is DynamicAbilityPrintingData item)
			{
				list.Add(item);
			}
		}
		bool flag2 = false;
		if (list.Count() >= 2)
		{
			string[] array = ability.OldSchoolManaText.Split('o');
			foreach (DynamicAbilityPrintingData item2 in list)
			{
				if (array.Length > item2.OldSchoolManaText.Split('o').Length)
				{
					flag2 = true;
					break;
				}
			}
		}
		list.Clear();
		genericObjectPool.PushObject(list, tryClear: false);
		if (flag2)
		{
			return true;
		}
		if (cardData.AbilityIds.ContainsId(103u) && ability.Id != 103)
		{
			return true;
		}
		return false;
	}

	private string GetFormatForModalChildAbility(ICardDataAdapter affector, AbilityPrintingData abilityPrintingData, bool chosen)
	{
		string currentFormat = ((affector.ZoneType != ZoneType.Battlefield) ? (chosen ? _colorSettings.DefaultFormat : _colorSettings.RemovedFormat) : (ModalAbilityIsExhausted(affector, abilityPrintingData.Id) ? _colorSettings.RemovedFormat : _colorSettings.DefaultFormat));
		return GetTargetingFormat(currentFormat, affector.InstanceId, abilityPrintingData);
	}

	private bool ModalAbilityIsExhausted(ICardDataAdapter cardData, uint abilityId)
	{
		if (cardData == null)
		{
			return false;
		}
		MtgCardInstance instance = cardData.Instance;
		if (instance == null)
		{
			return false;
		}
		foreach (MtgAbilityInstance abilityInstance in instance.AbilityInstances)
		{
			if (abilityInstance.ModalAbilityIsExhausted(abilityId))
			{
				return true;
			}
		}
		return false;
	}

	[Conditional("UNITY_EDITOR")]
	[ContextMenu("Fit Text In Textbox")]
	private void EDITOR_FitTextInTextbox()
	{
		FitTextInTextbox();
	}

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		ReleaseAllSubComponents();
		if (_cachedModel.RulesTextOverride != null)
		{
			foreach (ICardTextEntry item in _rulesTextOverrideParser.ParseText(_cachedModel, _colorSettings, _languageOverride))
			{
				GetSubComponentForContent(item).SetContent(item);
			}
		}
		else
		{
			foreach (ICardTextEntry item2 in _preRulesSupplementalText.ParseText(_cachedModel, _colorSettings, _languageOverride))
			{
				GetSubComponentForContent(item2).SetContent(item2);
			}
			SetContentForSubComponents(GetSubComponentContentFromAbilities(_cachedModel));
			SetBackgroundStripesForSubComponents(_activeSubComponents);
			foreach (ICardTextEntry item3 in _postRulesSupplementalText.ParseText(_cachedModel, _colorSettings, _languageOverride))
			{
				GetSubComponentForContent(item3).SetContent(item3);
			}
		}
		if (_activeSubComponents.Count == 0 && _flavorTextProvider.TryGetFlavorText(_cachedModel, out var flavorText))
		{
			DisplayingFlavorText = true;
			TextAlignmentOptions alignment = (_cachedModel.PrintedTypes.Contains(CardType.Land) ? TextAlignmentOptions.Center : TextAlignmentOptions.Left);
			ICardTextEntry content = new BasicTextEntry(string.Format(_colorSettings.DefaultFormat, flavorText));
			TextboxSubComponentBase subComponentForContent = GetSubComponentForContent(content);
			subComponentForContent.SetAlignment(alignment);
			subComponentForContent.SetContent(content);
		}
		else
		{
			if (FrameLanguageUtilities.IsPhyrexianLanguageOverride(_cachedModel, out var languageOverride) && _canShowFrameLanguage && _cachedModel.FlavorTextId != 0)
			{
				string localizedText = _cardDatabase.GreLocProvider.GetLocalizedText(_cachedModel.FlavorTextId, languageOverride, formatted: false);
				if (localizedText != null && !string.IsNullOrEmpty(localizedText))
				{
					DisplayingFlavorText = true;
					ICardTextEntry content2 = new BasicTextEntry(string.Format(_colorSettings.DefaultFormat, Regex.Unescape(localizedText)));
					TextboxSubComponentBase subComponentForContent2 = GetSubComponentForContent(content2);
					subComponentForContent2.SetAlignment(TextAlignmentOptions.Left);
					subComponentForContent2.SetContent(content2);
					goto IL_023d;
				}
			}
			DisplayingFlavorText = false;
		}
		goto IL_023d;
		IL_023d:
		FitTextInTextbox();
	}

	private IEnumerable<ICardTextEntry> GetSubComponentContentFromAbilities(ICardDataAdapter cardModel)
	{
		if (cardModel == null || (CardUtilities.IsBasicLand(cardModel) && cardModel.AddedAbilities.Count <= 0))
		{
			yield break;
		}
		if (cardModel.InstanceId == 0 && cardModel.AllAbilities.Count == 0 && cardModel.RulesTextOverride != null)
		{
			yield return new BasicTextEntry(cardModel.RulesTextOverride.GetOverride(_colorSettings));
			yield break;
		}
		foreach (ICardTextEntry textFromAllAbilitiesEntry in GetTextFromAllAbilitiesEntries(cardModel.AllAbilities))
		{
			yield return textFromAllAbilitiesEntry;
		}
	}

	private TextboxSubComponentBase GetSubComponentForContent(ICardTextEntry content)
	{
		FillBlackboardForTextbox(_assetLookupSystem.Blackboard);
		_assetLookupSystem.Blackboard.CardTextEntry = content;
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<RulesTextComponent> loadedTree))
		{
			RulesTextComponent payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				TextboxSubComponentBase component = _unityObjectPool.PopObject(payload.PrefabPath, _subComponentRoot).GetComponent<TextboxSubComponentBase>();
				_assetLookupSystem.Blackboard.Clear();
				component.SetFont(_fontAsset);
				component.SetAlignment(_textAlignment);
				component.SetLineSpacing(_lineSpacing);
				component.SetMaterial(base.UpdateLabelMaterial, _languageOverride);
				component.gameObject.SetLayer(base.gameObject.layer);
				component.RectTransform.anchorMin = Vector2.up;
				component.RectTransform.anchorMax = Vector2.up;
				component.RectTransform.localPosition = Vector3.zero;
				component.RectTransform.localRotation = Quaternion.identity;
				component.RectTransform.localScale = Vector3.one;
				component.RectTransform.sizeDelta = new Vector2(_subComponentRoot.rect.width, component.RectTransform.rect.height);
				foreach (CDCMaterialFiller cdcFillersOnNonLabelVisual in component.GetCdcFillersOnNonLabelVisuals())
				{
					cdcFillersOnNonLabelVisual.Init(_cardMaterialBuilder, _cardDatabase);
					cdcFillersOnNonLabelVisual.UpdateMaterials(_cachedModel, _cachedCardHolderType, base.GetCurrentGameState, _cachedViewMetadata.IsDimmed, _cachedViewMetadata.IsMouseOver);
				}
				_activeSubComponents.Add(component);
				return component;
			}
		}
		throw new ArgumentException("Content type (" + content.GetType().Name + ") has no corresponding TextboxSubComponentBase.");
	}

	private void ReleaseAllSubComponents()
	{
		TextboxSubComponentBase removed;
		while (_activeSubComponents.TryRemoveAt(0, out removed))
		{
			removed.CleanUp();
			_unityObjectPool.PushObject(removed.gameObject);
		}
	}
}
