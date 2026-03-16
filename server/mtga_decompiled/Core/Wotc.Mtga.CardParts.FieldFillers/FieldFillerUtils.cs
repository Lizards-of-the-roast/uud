using System;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.CardParts.FieldFillers;

public static class FieldFillerUtils
{
	[Flags]
	public enum PowerToughnessFlags
	{
		None = 0,
		Temporary = 1,
		Perpetual = 2,
		Damaged = 4,
		Backbone = 8,
		Frontbone = 0x10
	}

	private const string PHY_LOCALIZATION_SENTENCE_BEGIN = "^";

	private const string PHY_LOCALIZATION_SENTENCE_APPEND = ",";

	private const string PHY_LOCALIZATION_SENTENCE_END = ".";

	public static string MDFCHintText(CardPrintingData printing, IGreLocProvider locProvider)
	{
		if (printing == null)
		{
			return string.Empty;
		}
		if (locProvider == null)
		{
			return string.Empty;
		}
		if (printing.Subtypes.Count > 0)
		{
			return locProvider.GetLocalizedTextForEnumValue(printing.Subtypes[printing.Subtypes.Count - 1]);
		}
		if (printing.Types.Count > 0)
		{
			return locProvider.GetLocalizedTextForEnumValue(printing.Types[0]);
		}
		return string.Empty;
	}

	public static TMP_FontAsset FindFont(AssetLookupSystem als, AssetTracker tracker, out bool canSwapMaterial)
	{
		if (als.TreeLoader.TryLoadTree(out AssetLookupTree<FieldFont> loadedTree))
		{
			FieldFont payload = loadedTree.GetPayload(als.Blackboard);
			if (payload != null)
			{
				TMP_FontAsset tMP_FontAsset = AssetLoader.AcquireAndTrackAsset(tracker, "FieldFont", payload.FontAssetReference);
				if ((object)tMP_FontAsset != null)
				{
					canSwapMaterial = payload.CanSwapMaterial;
					return tMP_FontAsset;
				}
			}
		}
		canSwapMaterial = true;
		return null;
	}

	public static Material FindMaterial(AssetLookupSystem als, AssetTracker tracker)
	{
		if (als.TreeLoader.TryLoadTree(out AssetLookupTree<FontMaterialSettings> loadedTree))
		{
			FontMaterialSettings payload = loadedTree.GetPayload(als.Blackboard);
			if (payload != null)
			{
				Material material = AssetLoader.AcquireAndTrackAsset(tracker, "FieldMaterial", payload.MaterialReference);
				if ((object)material != null)
				{
					return material;
				}
			}
		}
		return null;
	}

	public static Material CheckForInvalidMaterial(ICardDataAdapter model, Material material, TMP_FontAsset font)
	{
		if (!material.name.StartsWith(font.name))
		{
			Debug.LogError($"Attempting to use material \"{font.name}\" on font \"{material.name}\" which don't share a name, this is likely to cause a visual error. Context:\n{model}");
			material = font.material;
		}
		return material;
	}

	public static CardTextColorSettings FindColor(AssetLookupSystem als, CardColorCaches cardColorCaches)
	{
		if (als.TreeLoader.TryLoadTree(out AssetLookupTree<FieldTextColor> loadedTree))
		{
			FieldTextColor payload = loadedTree.GetPayload(als.Blackboard);
			if (payload != null)
			{
				FieldTextColorSettings fieldTextColorSettings = cardColorCaches.GetFieldTextColorSettings(payload.ColorSettingsRef);
				if ((object)fieldTextColorSettings != null)
				{
					return fieldTextColorSettings.Settings;
				}
			}
		}
		if (als.TreeLoader.TryLoadTree(out AssetLookupTree<TextColor> loadedTree2))
		{
			TextColor payload2 = loadedTree2.GetPayload(als.Blackboard);
			if (payload2 != null)
			{
				CardTextColorTable cardTextColorTable = cardColorCaches.GetCardTextColorTable(payload2.ColorTableRef);
				if ((object)cardTextColorTable != null)
				{
					foreach (CardTextColorTable.FieldTypeOverride fieldTypeOverride in cardTextColorTable.FieldTypeOverrides)
					{
						if (fieldTypeOverride.FieldType == als.Blackboard.FieldFillerType)
						{
							return fieldTypeOverride.Settings;
						}
					}
					return cardTextColorTable.DefaultSettings;
				}
			}
		}
		return null;
	}

	private static string GetText_Title_Wildcard(ICardDataAdapter model, IClientLocProvider clientLocMan)
	{
		return model.Rarity switch
		{
			CardRarity.Common => clientLocMan.GetLocalizedText("MainNav/General/CommonWildcard"), 
			CardRarity.Uncommon => clientLocMan.GetLocalizedText("MainNav/General/UncommonWildcard"), 
			CardRarity.Rare => clientLocMan.GetLocalizedText("MainNav/General/RareWildcard"), 
			CardRarity.MythicRare => clientLocMan.GetLocalizedText("MainNav/General/MythicRareWildcard"), 
			_ => string.Empty, 
		};
	}

	private static string GetText_Title_Default(uint titleId, IGreLocProvider greLocMan)
	{
		if (titleId == 0)
		{
			return string.Empty;
		}
		return greLocMan.GetLocalizedText(titleId);
	}

	private static string GetText_Title_FrameLanguage(ICardDataAdapter model, uint titleId, IGreLocProvider greLocMan)
	{
		if (FrameLanguageUtilities.IsPhyrexianLanguageOverride(model, out var languageOverride) && titleId != 0 && greLocMan.TryGetLocalizedText(out var text, titleId, languageOverride))
		{
			text = string.Format("{0}{1}{2}", "^", text, ".");
			return CardUtilities.FormatComplexTitle(text);
		}
		if (FrameLanguageUtilities.IsMysticalArchiveLanguageOverride(model, out languageOverride) && titleId != 0 && greLocMan.TryGetLocalizedText(out text, titleId, languageOverride))
		{
			return CardUtilities.FormatComplexTitleVertical(text);
		}
		if (FrameLanguageUtilities.IsEnglishLanguageOverride(model, out languageOverride) && titleId != 0 && greLocMan.TryGetLocalizedText(out text, titleId, languageOverride))
		{
			return text;
		}
		return " ";
	}

	public static string GetText_Title(ICardDataAdapter model, ICardDatabaseAdapter cdb, bool canShowFrameLanguage)
	{
		string text = (model.IsWildcard ? GetText_Title_Wildcard(model, cdb.ClientLocProvider) : ((!(model.Printing.HasFrameLanguage() && canShowFrameLanguage)) ? GetText_Title_Default(model.TitleId, cdb.GreLocProvider) : GetText_Title_FrameLanguage(model, model.TitleId, cdb.GreLocProvider)));
		return CardUtilities.FormatComplexTitle(text);
	}

	public static string GetText_AltTitle(ICardDataAdapter model, ICardDatabaseAdapter cdb, bool canShowFrameLanguage)
	{
		CardPrintingData printing = model.Printing;
		if (printing != null && printing.AltTitleId != 0)
		{
			string text = ((!(model.Printing.HasFrameLanguage() && canShowFrameLanguage)) ? GetText_Title_Default(model.Printing.AltTitleId, cdb.GreLocProvider) : GetText_Title_FrameLanguage(model, model.Printing.AltTitleId, cdb.GreLocProvider));
			return CardUtilities.FormatComplexTitle(text);
		}
		return string.Empty;
	}

	public static string GetText_TypeLine(ICardDataAdapter model, ICardDatabaseAdapter cdb, bool canShowFrameLanguage, CardTextColorSettings colorSettings, AssetLookupSystem assetLookupSystem)
	{
		string languageOverride;
		string text;
		if (model.IsSplitCard(ignoreInstances: true))
		{
			ICardDataAdapter linkedFaceAtIndex = model.GetLinkedFaceAtIndex(0, ignoreInstance: false, cdb.CardDataProvider);
			ICardDataAdapter linkedFaceAtIndex2 = model.GetLinkedFaceAtIndex(1, ignoreInstance: false, cdb.CardDataProvider);
			string text_TypeLine = GetText_TypeLine(linkedFaceAtIndex, cdb, canShowFrameLanguage, colorSettings, assetLookupSystem);
			string text_TypeLine2 = GetText_TypeLine(linkedFaceAtIndex2, cdb, canShowFrameLanguage, colorSettings, assetLookupSystem);
			text = text_TypeLine + "/" + text_TypeLine2;
		}
		else if (canShowFrameLanguage && model.TitleId != 0 && FrameLanguageUtilities.IsPhyrexianLanguageOverride(model, out languageOverride))
		{
			string text_CardType = GetText_CardType(model, cdb, colorSettings, languageOverride);
			string text_SubType = GetText_SubType(model, cdb, colorSettings, languageOverride);
			text = string.Format("{0}{1}{2}{3}{4}", "^", text_CardType, ",", text_SubType, ".");
		}
		else if (canShowFrameLanguage && model.TitleId != 0 && (FrameLanguageUtilities.IsMysticalArchiveLanguageOverride(model, out languageOverride) || FrameLanguageUtilities.IsEnglishLanguageOverride(model, out languageOverride)))
		{
			text = cdb.CardTypeProvider.GetTypelineText(model, colorSettings, languageOverride);
			text = CardUtilities.FormatComplexTitle(text);
		}
		else
		{
			text = cdb.CardTypeProvider.GetTypelineText(model, colorSettings);
			text = CardUtilities.FormatComplexTitle(text);
		}
		if (ColorIndicatorUtil.TryGetIndicator(model, assetLookupSystem, out var indicatorType))
		{
			text = ColorIndicatorUtil.GetIndicatorSprite(indicatorType, model) + " " + text;
		}
		return text;
	}

	public static string GetText_CardType(ICardDataAdapter model, ICardDatabaseAdapter cdb, CardTextColorSettings colorSettings, string overrideLanguageCode = null)
	{
		return cdb.CardTypeProvider.GetCardTypeText(model, colorSettings, overrideLanguageCode);
	}

	public static string GetText_SubType(ICardDataAdapter model, ICardDatabaseAdapter cdb, CardTextColorSettings colorSettings, string overrideLanguageCode = null)
	{
		return cdb.CardTypeProvider.GetSubTypeText(model, colorSettings, overrideLanguageCode);
	}

	public static string GetText_PowerToughness(ICardDataAdapter model, MtgGameState gameState, CardTextColorSettings colorSettings, CardHolderType cardHolderType, bool isMouseOver)
	{
		bool num = model.ZoneType == ZoneType.Battlefield && (cardHolderType == CardHolderType.Battlefield || isMouseOver);
		Phase phase = gameState?.CurrentPhase ?? Phase.None;
		bool flag = num && phase == Phase.Combat && model.AffectedByQualifications.Exists((QualificationData x) => x.Type == QualificationType.AssignCombatDamageWithToughness);
		bool flag2 = num && model.AffectedByQualifications.Exists((QualificationData x) => x.Type == QualificationType.LethalDeterminedByPower);
		PowerToughnessFlags powerToughnessFlags = PowerToughnessFlags.None;
		PowerToughnessFlags powerToughnessFlags2 = PowerToughnessFlags.None;
		StringBackedInt stringBackedInt;
		StringBackedInt stringBackedInt2;
		if (UsePowerToughnessValues(model, out var powerValue, out var toughnessValue))
		{
			stringBackedInt = (flag ? toughnessValue : powerValue);
			stringBackedInt2 = (flag2 ? powerValue : toughnessValue);
			if (HasNonPerpetualPowerChanges(model))
			{
				powerToughnessFlags |= PowerToughnessFlags.Temporary;
			}
			if (HasPerpetualPowerChanges(model))
			{
				powerToughnessFlags |= PowerToughnessFlags.Perpetual;
			}
			if (HasNonPerpetualToughnessChanges(model))
			{
				powerToughnessFlags2 |= PowerToughnessFlags.Temporary;
			}
			if (HasPerpetualToughnessChanges(model))
			{
				powerToughnessFlags2 |= PowerToughnessFlags.Perpetual;
			}
			if (flag)
			{
				powerToughnessFlags |= PowerToughnessFlags.Backbone;
			}
			if (flag2)
			{
				powerToughnessFlags2 |= PowerToughnessFlags.Frontbone;
			}
			if (model.Damaged)
			{
				stringBackedInt2 = new StringBackedInt(stringBackedInt2.Value - (int)model.Damage);
				powerToughnessFlags2 |= PowerToughnessFlags.Damaged;
			}
		}
		else
		{
			stringBackedInt = model.Printing.Power;
			stringBackedInt2 = model.Printing.Toughness;
		}
		string format = PowerFormat(powerToughnessFlags, colorSettings);
		return string.Concat(str2: string.Format(ToughnessFormat(powerToughnessFlags2, colorSettings), stringBackedInt2.RawText), str0: string.Format(format, stringBackedInt.RawText), str1: "/");
	}

	private static bool UsePowerToughnessValues(ICardDataAdapter model, out StringBackedInt powerValue, out StringBackedInt toughnessValue)
	{
		if (model.CardTypes.Contains(CardType.Creature))
		{
			powerValue = model.Power;
			toughnessValue = model.Toughness;
			return true;
		}
		powerValue = ((!model.SuppressedPower.Equals(StringBackedInt.UNDEFINED)) ? model.SuppressedPower : model.Printing.Power);
		toughnessValue = ((!model.SuppressedToughness.Equals(StringBackedInt.UNDEFINED)) ? model.SuppressedToughness : model.Printing.Power);
		if (model.SuppressedPower.Equals(StringBackedInt.UNDEFINED))
		{
			return !model.SuppressedToughness.Equals(StringBackedInt.UNDEFINED);
		}
		return true;
	}

	public static string PowerFormat(PowerToughnessFlags powerState, CardTextColorSettings colorSettings)
	{
		return PowerFormat_Internal(powerState, colorSettings ?? CardTextColorSettings.DEFAULT);
	}

	private static string PowerFormat_Internal(PowerToughnessFlags powerState, CardTextColorSettings colorSettings)
	{
		if ((powerState & (PowerToughnessFlags.Temporary | PowerToughnessFlags.Backbone)) > PowerToughnessFlags.None)
		{
			return colorSettings.AddedFormat;
		}
		if ((powerState & PowerToughnessFlags.Perpetual) > PowerToughnessFlags.None)
		{
			return colorSettings.PerpetualFormat;
		}
		return colorSettings.DefaultFormat;
	}

	public static string ToughnessFormat(PowerToughnessFlags toughnessState, CardTextColorSettings colorSettings)
	{
		return ToughnessFormat_Internal(toughnessState, colorSettings ?? CardTextColorSettings.DEFAULT);
	}

	private static string ToughnessFormat_Internal(PowerToughnessFlags toughnessState, CardTextColorSettings colorSettings)
	{
		if ((toughnessState & PowerToughnessFlags.Damaged) > PowerToughnessFlags.None)
		{
			return colorSettings.RemovedFormat;
		}
		if ((toughnessState & (PowerToughnessFlags.Temporary | PowerToughnessFlags.Frontbone)) > PowerToughnessFlags.None)
		{
			return colorSettings.AddedFormat;
		}
		if ((toughnessState & PowerToughnessFlags.Perpetual) > PowerToughnessFlags.None)
		{
			return colorSettings.PerpetualFormat;
		}
		return colorSettings.DefaultFormat;
	}

	private static bool HasNonPerpetualPowerChanges(ICardDataAdapter model)
	{
		if (model != null)
		{
			return HasNonPerpetualPowerChanges(model.Instance);
		}
		return false;
	}

	public static bool HasNonPerpetualPowerChanges(MtgCardInstance card)
	{
		if (card != null)
		{
			if (!card.LayeredEffects.Exists((LayeredEffectData x) => x.IsPowerModification && !x.IsPerpetualPowerChange()))
			{
				return card.CounterDatas.Exists((CounterData x) => x.ModifiesPower());
			}
			return true;
		}
		return false;
	}

	private static bool HasNonPerpetualToughnessChanges(ICardDataAdapter model)
	{
		if (model != null)
		{
			return HasNonPerpetualToughnessChanges(model.Instance);
		}
		return false;
	}

	public static bool HasNonPerpetualToughnessChanges(MtgCardInstance card)
	{
		if (card != null)
		{
			if (!card.LayeredEffects.Exists((LayeredEffectData x) => x.IsToughnessModification && !x.IsPerpetualToughnessChange()))
			{
				return card.CounterDatas.Exists((CounterData x) => x.ModifiesToughness());
			}
			return true;
		}
		return false;
	}

	private static bool HasPerpetualPowerChanges(ICardDataAdapter model)
	{
		if (model != null)
		{
			return HasPerpetualPowerChanges(model.Instance);
		}
		return false;
	}

	public static bool HasPerpetualPowerChanges(MtgCardInstance card)
	{
		return card?.LayeredEffects.Exists((LayeredEffectData x) => x.IsPerpetualPowerChange()) ?? false;
	}

	private static bool HasPerpetualToughnessChanges(ICardDataAdapter model)
	{
		if (model != null)
		{
			return HasPerpetualToughnessChanges(model.Instance);
		}
		return false;
	}

	public static bool HasPerpetualToughnessChanges(MtgCardInstance card)
	{
		return card?.LayeredEffects.Exists((LayeredEffectData x) => x.IsPerpetualToughnessChange()) ?? false;
	}

	public static string GetText_FlavorText(ICardDataAdapter model, ICardDatabaseAdapter cdb)
	{
		uint flavorTextId = model.FlavorTextId;
		if (flavorTextId == 0 && model.ObjectType == GameObjectType.Ability && model.Instance != null && model.Instance.Parent != null)
		{
			string skinCode = model.Instance.Parent.SkinCode;
			flavorTextId = cdb.CardDataProvider.GetCardPrintingById(model.Instance.Parent.GrpId, skinCode).FlavorTextId;
		}
		if (flavorTextId == 0 && model.ObjectType == GameObjectType.Ability && model.ObjectSourceGrpId != 0)
		{
			CardPrintingData cardPrintingById = cdb.CardDataProvider.GetCardPrintingById(model.ObjectSourceGrpId, model.SkinCode);
			if (cardPrintingById != null)
			{
				flavorTextId = cardPrintingById.FlavorTextId;
			}
		}
		if (flavorTextId != 0)
		{
			return cdb.GreLocProvider.GetLocalizedText(flavorTextId);
		}
		return string.Empty;
	}

	public static string GetText_ArtistCredit(ICardDataAdapter model, ICardDatabaseAdapter cdb, AssetLookupSystem assetLookupSystem)
	{
		FillBlackboard(assetLookupSystem.Blackboard, cdb, model, CDCFieldFillerFieldType.ArtistCredit);
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ArtIdOverride> loadedTree))
		{
			ArtIdOverride payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null && !string.IsNullOrEmpty(payload.ArtistCredit))
			{
				return payload.ArtistCredit;
			}
		}
		if (cdb.AltArtistCreditProvider.TryGetAltArtistCredit(model, out var altArtistCredit))
		{
			return altArtistCredit;
		}
		return model.Printing.ArtistCredit;
	}

	public static string GetText_WildcardRarity(ICardDataAdapter model)
	{
		return model.Rarity.ToString();
	}

	public static string GetText_LinkedMDFCType(ICardDataAdapter model, ICardDatabaseAdapter cdb)
	{
		return MDFCHintText(model.Printing, cdb.GreLocProvider);
	}

	public static string GetText_LinkedMDFCManaAbility(ICardDataAdapter model, ICardDatabaseAdapter cdb)
	{
		string result = string.Empty;
		if (CardUtilities.IsLand(model))
		{
			AbilityPrintingData abilityPrintingData = model.Printing.Abilities.FirstOrDefault((AbilityPrintingData x) => x.SubCategory == AbilitySubCategory.Mana);
			if (abilityPrintingData != null)
			{
				result = cdb.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(model.Printing.GrpId, abilityPrintingData.Id, model.Abilities.Select((AbilityPrintingData o) => o.Id));
			}
		}
		return result;
	}

	public static string GetText_Ability(uint cardGrpId, uint abilityText, IAbilityTextProvider abilityTextProvider)
	{
		if (abilityText == 0)
		{
			return string.Empty;
		}
		return abilityTextProvider.GetAbilityTextByCardAbilityGrpId(cardGrpId, abilityText, Array.Empty<uint>());
	}

	public static string FindText(ICardDataAdapter model, MtgGameState gameState, ICardDatabaseAdapter cdb, CDCFieldFillerFieldType fieldType, CardHolderType cardHolderType, bool isMouseOver, CardTextColorSettings colorSettings, bool canShowFrameLanguage, AssetLookupSystem assetLookupSystem, bool tryParseHirigana, uint abilityId, out bool determineIfTruncated)
	{
		string text = null;
		FillBlackboard(assetLookupSystem.Blackboard, cdb, model, fieldType);
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FieldFillerTextOverride> loadedTree))
		{
			FieldFillerTextOverride payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
			if (payload != null)
			{
				text = cdb.ClientLocProvider.GetLocalizedText(payload.Key);
				goto IL_014f;
			}
		}
		switch (fieldType)
		{
		case CDCFieldFillerFieldType.Title:
			text = GetText_Title(model, cdb, canShowFrameLanguage);
			break;
		case CDCFieldFillerFieldType.AltTitle:
			text = GetText_AltTitle(model, cdb, canShowFrameLanguage);
			break;
		case CDCFieldFillerFieldType.TypeLine:
			text = GetText_TypeLine(model, cdb, canShowFrameLanguage, colorSettings, assetLookupSystem);
			break;
		case CDCFieldFillerFieldType.SuperAndCardType:
			text = GetText_CardType(model, cdb, colorSettings);
			break;
		case CDCFieldFillerFieldType.SubType:
			text = GetText_SubType(model, cdb, colorSettings);
			break;
		case CDCFieldFillerFieldType.PowerToughness:
			text = GetText_PowerToughness(model, gameState, colorSettings, cardHolderType, isMouseOver);
			break;
		case CDCFieldFillerFieldType.FlavorText:
			text = GetText_FlavorText(model, cdb);
			break;
		case CDCFieldFillerFieldType.ArtistCredit:
			text = GetText_ArtistCredit(model, cdb, assetLookupSystem);
			break;
		case CDCFieldFillerFieldType.WildcardRarity:
			text = GetText_WildcardRarity(model);
			break;
		case CDCFieldFillerFieldType.LinkedMDFCType:
			text = GetText_LinkedMDFCType(model, cdb);
			break;
		case CDCFieldFillerFieldType.LinkedMDFCManaAbility:
			text = GetText_LinkedMDFCManaAbility(model, cdb);
			break;
		case CDCFieldFillerFieldType.AbilityText:
			text = GetText_Ability(model.GrpId, abilityId, cdb.AbilityTextProvider);
			break;
		}
		goto IL_014f;
		IL_014f:
		if (tryParseHirigana)
		{
			text = CardUtilities.FormatComplexTitle(text);
		}
		FillBlackboard(assetLookupSystem.Blackboard, cdb, model, fieldType);
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<FieldTextMode> loadedTree2))
		{
			FieldTextMode payload2 = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
			if (payload2 != null && !string.IsNullOrEmpty(text))
			{
				text = ((!payload2.UsePerpetual) ? string.Format(colorSettings.DefaultFormat, text) : string.Format(colorSettings.PerpetualFormat, text));
			}
		}
		if (fieldType == CDCFieldFillerFieldType.Title || fieldType == CDCFieldFillerFieldType.TypeLine || fieldType == CDCFieldFillerFieldType.SuperAndCardType)
		{
			determineIfTruncated = true;
		}
		else
		{
			determineIfTruncated = false;
		}
		return text;
	}

	private static void FillBlackboard(IBlackboard bb, ICardDatabaseAdapter cdb, ICardDataAdapter model, CDCFieldFillerFieldType fieldType)
	{
		bb.Clear();
		bb.SetCardDataExtensive(model);
		bb.Ability = cdb.AbilityDataProvider.GetAbilityPrintingById(model.GrpId);
		bb.FieldFillerType = fieldType;
	}
}
