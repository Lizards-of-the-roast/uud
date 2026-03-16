using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData.RulesTextOverrider;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.CardData;

public static class CardDataExtensions
{
	public static CardData CreateBlank(CardPrintingRecord? record = null)
	{
		return CardDataExtensionsCore.CreateBlank(record);
	}

	public static CardData CreateBlankExpansionCard(string expansionCode, string digitalReleaseSet)
	{
		MtgCardInstance instance = new MtgCardInstance
		{
			Zone = new MtgZone
			{
				Type = ZoneType.Library,
				Visibility = Visibility.Public
			},
			Visibility = Visibility.Public
		};
		CardPrintingData printing = new CardPrintingData(new CardPrintingRecord(0u, 0u, null, 0u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, expansionCode, digitalReleaseSet), NullCardDataProvider.Default, NullAbilityDataProvider.Default);
		return new CardData(instance, printing);
	}

	public static CardData CreateWithDatabase(MtgCardInstance instance, ICardDatabaseAdapter db)
	{
		if (instance.CatalogId == WellKnownCatalogId.WellKnownCatalogId_TransientEffect)
		{
			CardData cardData = CreateTransientEffect(instance, db);
			if (cardData != null)
			{
				return cardData;
			}
		}
		switch (instance.ObjectType)
		{
		case GameObjectType.Emblem:
			return CreateEmblem(instance, db.CardDataProvider.GetCardPrintingById(instance.ObjectSourceGrpId));
		case GameObjectType.Boon:
			return CreateBoon(instance, db.CardDataProvider.GetCardPrintingById(instance.ObjectSourceGrpId));
		case GameObjectType.Ability:
			return CreateAbility(instance, db);
		case GameObjectType.RevealedCard:
			return CreateRevealedCard(instance, db.CardDataProvider, db.AbilityDataProvider);
		default:
			if (instance.FaceDownState.ReasonFaceDown == ReasonFaceDown.Morph || instance.FaceDownState.CopiedCardsReasonFaceDown == ReasonFaceDown.Morph)
			{
				return CreateMorph(instance, db);
			}
			if ((instance.FaceDownState.IsFaceDown || instance.FaceDownState.IsCopiedFaceDown) && (instance.Zone.Type == ZoneType.Battlefield || instance.Zone.Type == ZoneType.Stack))
			{
				return CreateFaceDown(instance, db);
			}
			return new CardData(instance, db.CardDataProvider.GetCardPrintingById(instance.GrpId, instance.SkinCode));
		}
	}

	public static ICardDataAdapter ToCardData(this MtgCardInstance instance, ICardDatabaseAdapter db)
	{
		return CreateWithDatabase(instance, db);
	}

	public static CardData CreateFaceDown(MtgCardInstance instance, ICardDatabaseAdapter db)
	{
		return new CardData(instance, new CardPrintingData(new CardPrintingRecord(0u, 0u, null, power: instance.Power, toughness: instance.Toughness, supertypes: instance.Supertypes.ToArray(), types: instance.CardTypes.ToArray(), subtypes: instance.Subtypes.ToArray(), abilityIds: instance.Abilities.Select((AbilityPrintingData x) => (Id: x.Id, TextId: x.TextId)).ToArray(), titleId: instance.TitleId, altTitleId: 0u, interchangeableTitleId: 0u, flavorTextId: 0u, reminderTextId: 0u, typeTextId: 0u, subtypeTextId: 0u, artistCredit: null, artSize: CardArtSize.Full), db.CardDataProvider, db.AbilityDataProvider));
	}

	public static CardData CreateMorph(MtgCardInstance instance, ICardDatabaseAdapter db)
	{
		CardPrintingData printing = new CardPrintingData(new CardPrintingRecord(0u, 0u, null, power: new StringBackedInt(2), toughness: new StringBackedInt(2), types: new List<CardType> { CardType.Creature }, titleId: instance.TitleId, altTitleId: 0u, interchangeableTitleId: 0u, flavorTextId: 0u, reminderTextId: 0u, typeTextId: 0u, subtypeTextId: 0u, artistCredit: null, artSize: CardArtSize.Full), db.CardDataProvider, db.AbilityDataProvider);
		return new CardData(instance, printing);
	}

	public static CardData CreateBlankFaceDown(MtgCardInstance instance, ICardDatabaseAdapter db)
	{
		StringBackedInt uNDEFINED = StringBackedInt.UNDEFINED;
		StringBackedInt uNDEFINED2 = StringBackedInt.UNDEFINED;
		IReadOnlyList<CardType> types = new List<CardType>();
		CardPrintingData printing = new CardPrintingData(new CardPrintingRecord(3u, 0u, null, 0u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Full, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), uNDEFINED, uNDEFINED2, null, null, null, null, types), db.CardDataProvider, db.AbilityDataProvider);
		return new CardData(instance, printing);
	}

	public static CardData CreateEmblem(MtgCardInstance instance, CardPrintingData sourcePrinting)
	{
		instance.SkinCode = null;
		if (sourcePrinting != null)
		{
			CardPrintingRecord record = sourcePrinting.Record;
			CardRarity? rarity = CardRarity.Common;
			string empty = string.Empty;
			IReadOnlyList<(uint, uint)> abilityIds = instance.Abilities.Select((AbilityPrintingData x) => (Id: x.Id, TextId: x.TextId)).ToArray();
			IReadOnlyList<(uint, uint)> hiddenAbilityIds = Array.Empty<(uint, uint)>();
			LinkedFace? linkedFaceType = LinkedFace.None;
			IReadOnlyList<uint> linkedFaceGrpIds = Array.Empty<uint>();
			IReadOnlyList<CardColor> indicatorColors = Array.Empty<CardColor>();
			CardPrintingData cardPrintingData = new CardPrintingData(sourcePrinting, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, rarity, null, null, null, null, null, null, null, null, null, null, null, null, null, null, empty, linkedFaceType, null, null, null, null, null, null, null, null, indicatorColors, null, null, null, abilityIds, hiddenAbilityIds, linkedFaceGrpIds));
			instance.ObjectType = GameObjectType.Emblem;
			instance.TitleId = cardPrintingData.TitleId;
			return new CardData(instance, cardPrintingData);
		}
		return new CardData(instance, null);
	}

	public static CardData CreateBoon(MtgCardInstance instance, CardPrintingData sourcePrinting)
	{
		instance.SkinCode = null;
		if (sourcePrinting != null)
		{
			CardPrintingRecord record = sourcePrinting.Record;
			IReadOnlyList<(uint, uint)> abilityIds = Array.Empty<(uint, uint)>();
			IReadOnlyList<(uint, uint)> hiddenAbilityIds = Array.Empty<(uint, uint)>();
			IReadOnlyList<uint> linkedFaceGrpIds = Array.Empty<uint>();
			IReadOnlyList<CardType> types = Array.Empty<CardType>();
			IReadOnlyList<SubType> subtypes = Array.Empty<SubType>();
			IReadOnlyList<SuperType> supertypes = Array.Empty<SuperType>();
			StringBackedInt? power = StringBackedInt.UNDEFINED;
			StringBackedInt? toughness = StringBackedInt.UNDEFINED;
			CardPrintingData cardPrintingData = new CardPrintingData(sourcePrinting, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, power, toughness, null, null, null, null, types, subtypes, supertypes, abilityIds, hiddenAbilityIds, linkedFaceGrpIds));
			instance.ObjectType = GameObjectType.Boon;
			instance.CatalogId = WellKnownCatalogId.None;
			instance.TitleId = cardPrintingData.TitleId;
			return new CardData(instance, cardPrintingData);
		}
		return new CardData(instance, null);
	}

	public static CardData CreateAbilityCard(AbilityPrintingData ability, ICardDataAdapter parent, ICardDatabaseAdapter db)
	{
		MtgCardInstance instance = new MtgCardInstance
		{
			ObjectType = GameObjectType.Ability,
			Parent = parent.Instance.GetCopy(),
			ParentId = parent.InstanceId,
			Visibility = Visibility.Public,
			Viewers = new List<GREPlayerNum> { GREPlayerNum.LocalPlayer },
			Zone = new MtgZone
			{
				Type = ZoneType.Library,
				Visibility = Visibility.Public
			},
			Owner = new MtgPlayer(GREPlayerNum.LocalPlayer),
			TitleId = parent.TitleId,
			GrpId = ability.Id,
			Colors = new List<CardColor>(parent.GetFrameColors),
			ActiveAbilityWords = ((ability.PaymentType == AbilityPaymentType.Loyalty) ? new List<AbilityWordData>() : new List<AbilityWordData>(parent.ActiveAbilityWords)),
			SkinCode = parent.Instance.SkinCode,
			SleeveCode = parent.Instance.SleeveCode,
			FaceDownState = FaceDownState.CopyFaceDownState(parent.Instance.FaceDownState)
		};
		CardPrintingData printing = parent.Printing;
		string imageAssetPath = parent.ImageAssetPath;
		return new CardData(instance, new CardPrintingData(printing, new CardPrintingRecord(ability.Id, 0u, imageAssetPath, 0u, expansionCode: parent.ExpansionCode, rarity: parent.Rarity, colors: parent.Printing.Colors, frameColors: parent.GetFrameColors, additionalFrameDetails: parent.Printing.AdditionalFrameDetails, knownSupportedStyles: parent.Printing.KnownSupportedStyles, altTitleId: parent.Printing.AltTitleId)))
		{
			RulesTextOverride = ((db != null) ? new AbilityTextOverride(db, parent.TitleId).AddAbility(ability.Id).AddSource(parent.Instance.GetCopy()).AddSource(parent.Printing)
				.AddDieRollResult(parent.Instance?.DieRollResults?.Result) : null)
		};
	}

	public static CardData CreateTransientEffect(MtgCardInstance instance, ICardDatabaseAdapter db)
	{
		if (instance.Parent == null)
		{
			return new CardData(instance, db.CardDataProvider.GetCardPrintingById(instance.GrpId));
		}
		CardData cardData = CreateWithDatabase(instance.Parent, db);
		CardPrintingRecord record = cardData.Printing.Record;
		LinkedFace? linkedFaceType = LinkedFace.None;
		CardPrintingRecord record2 = new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, linkedFaceType);
		CardPrintingData printing = new CardPrintingData(cardData.Printing, record2);
		cardData = new CardData(instance.Parent, printing);
		instance.TitleId = cardData.TitleId;
		instance.CardTypes = new List<CardType>(cardData.CardTypes);
		instance.Colors = new List<CardColor>(cardData.Colors);
		AbilityPrintingData abilityPrintingData = instance.Abilities[0];
		ref DieRollResultData? dieRollResults = ref instance.DieRollResults;
		int? result = (dieRollResults.HasValue ? new int?(dieRollResults.GetValueOrDefault().Result) : cardData.Instance.DieRollResults?.Result);
		if (instance.GrpId == 144241)
		{
			result = 20;
		}
		return new CardData(instance, cardData.Printing.CreateMiniCDCPrintingData(abilityPrintingData.Id))
		{
			RulesTextOverride = new AbilityTextOverride(db, instance.TitleId).AddAbility(abilityPrintingData.Id).AddSource(cardData.Instance).AddSource(cardData.Printing)
				.AddDieRollResult(result)
		};
	}

	public static CardData CreateAbility(MtgCardInstance instance, ICardDatabaseAdapter db)
	{
		MtgCardInstance copy = instance.GetCopy();
		CardPrintingData cardPrintingData = null;
		MtgCardInstance parent = instance.Parent;
		if (SpecializeUtilities.TryCreateSpecializeAbility(copy, db.CardDataProvider, out var specializeAbility))
		{
			return specializeAbility;
		}
		if (copy.TryCreateRoomAbility(db.CardDataProvider, out var roomAbility))
		{
			return roomAbility;
		}
		if (parent != null)
		{
			if (cardPrintingData == null && parent.ObjectType == GameObjectType.Boon)
			{
				cardPrintingData = db.CardDataProvider.GetCardPrintingById(parent.ObjectSourceGrpId);
			}
			if (cardPrintingData == null && parent.GrpId != 0)
			{
				cardPrintingData = db.CardDataProvider.GetCardPrintingById(parent.GrpId, parent.SkinCode);
			}
			if (cardPrintingData == null && parent.BaseGrpId != 0)
			{
				cardPrintingData = db.CardDataProvider.GetCardPrintingById(parent.BaseGrpId, parent.SkinCode);
			}
			if (cardPrintingData == null && parent.ObjectSourceGrpId != 0)
			{
				cardPrintingData = db.CardDataProvider.GetCardPrintingById(parent.ObjectSourceGrpId);
			}
		}
		if (cardPrintingData == null && instance.GrpId != 0)
		{
			cardPrintingData = db.CardDataProvider.GetCardPrintingById(instance.GrpId, instance.SkinCode);
		}
		if (cardPrintingData == null && instance.BaseGrpId != 0)
		{
			cardPrintingData = db.CardDataProvider.GetCardPrintingById(instance.BaseGrpId, instance.SkinCode);
		}
		if (cardPrintingData == null && instance.ObjectSourceGrpId != 0)
		{
			cardPrintingData = db.CardDataProvider.GetCardPrintingById(instance.ObjectSourceGrpId, instance.SkinCode);
		}
		if (cardPrintingData == null)
		{
			cardPrintingData = CardPrintingData.Blank;
		}
		CardPrintingData other = cardPrintingData;
		CardPrintingRecord record = cardPrintingData.Record;
		uint? flavorTextId = 0u;
		string empty = string.Empty;
		IReadOnlyList<(uint, uint)> abilityIds = Array.Empty<(uint, uint)>();
		IReadOnlyList<(uint, uint)> hiddenAbilityIds = Array.Empty<(uint, uint)>();
		IReadOnlyList<SuperType> supertypes = Array.Empty<SuperType>();
		IReadOnlyList<CardType> types = Array.Empty<CardType>();
		IReadOnlyList<SubType> subtypes = Array.Empty<SubType>();
		CardPrintingData cardPrintingData2 = new CardPrintingData(other, new CardPrintingRecord(record, null, null, null, null, null, null, flavorTextId, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, empty, null, null, null, null, null, null, null, null, null, null, types, subtypes, supertypes, abilityIds, hiddenAbilityIds));
		copy.Colors = abilityInstanceColors(copy, cardPrintingData2);
		uint titleId = copy.TitleId;
		copy.TitleId = ((titleId == 0 || titleId == 1) ? cardPrintingData2.TitleId : copy.TitleId);
		AbilityTextOverride abilityTextOverride = new AbilityTextOverride(db, copy.TitleId).AddSource(copy.Parent).AddSource(cardPrintingData2).AddDieRollResult(instance.DieRollResults?.Result);
		if (instance.Abilities.Count > 0)
		{
			abilityTextOverride.AddAbility(copy.Abilities);
		}
		else
		{
			abilityTextOverride.AddAbility(copy.GrpId);
			AbilityPrintingData abilityPrintingById = db.AbilityDataProvider.GetAbilityPrintingById(copy.GrpId);
			if (abilityPrintingById != null && abilityPrintingById.SubCategory == AbilitySubCategory.ClassLevel)
			{
				abilityTextOverride.AddAbility(abilityPrintingById.GetClassGrantedAbilities(cardPrintingData));
			}
		}
		return new CardData(copy, cardPrintingData2)
		{
			RulesTextOverride = abilityTextOverride
		};
		static List<CardColor> abilityInstanceColors(MtgCardInstance mtgCardInstance, CardPrintingData printing)
		{
			if (printing == null)
			{
				return new List<CardColor>();
			}
			if (mtgCardInstance == null || mtgCardInstance.Parent == null)
			{
				return new List<CardColor>(printing.Colors);
			}
			return mtgCardInstance.Parent.ObjectType switch
			{
				GameObjectType.Emblem => new List<CardColor>(printing.Colors), 
				GameObjectType.Boon => new List<CardColor>(), 
				_ => new List<CardColor>(mtgCardInstance.Parent.Colors), 
			};
		}
	}

	public static CardData CreateRewardsCard(ICardDatabaseAdapter cardDatabase, int goldAwarded, int gemsAwarded, string setCode, string context = "")
	{
		int num = gemsAwarded;
		string item = "GemsReward";
		bool flag = num > 20;
		List<string> list = new List<string> { item };
		if (!string.IsNullOrEmpty(context))
		{
			list.Add(context);
		}
		int rarity = (flag ? 5 : 4);
		IReadOnlyCollection<string> additionalFrameDetails = list;
		return new CardData(null, new CardPrintingData(new CardPrintingRecord(0u, 0u, null, 0u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, (CardRarity)rarity, setCode, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, null, null, null, null, null, null, null, null, null, null, null, null, additionalFrameDetails), cardDatabase.CardDataProvider, cardDatabase.AbilityDataProvider))
		{
			RulesTextOverride = new RawTextOverride(gemsAwarded.ToString())
		};
	}

	public static CardData CreateSkinCard(uint grpId, ICardDatabaseAdapter cardDatabase, string skinCode = null, string sleeveCode = null, bool faceDown = false)
	{
		CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(grpId);
		return CreateSkinCard(grpId, cardPrintingById, cardDatabase, skinCode, sleeveCode, faceDown);
	}

	public static CardData CreateSkinCard(uint grpId, CardPrintingData finalPrinting, ICardDatabaseAdapter cardDatabase, string skinCode = null, string sleeveCode = null, bool faceDown = false)
	{
		MtgCardInstance mtgCardInstance = null;
		if (!string.IsNullOrEmpty(skinCode) || !string.IsNullOrEmpty(sleeveCode) || faceDown)
		{
			if (AltPrintingUtilities.FindAlternatePrinting(grpId, skinCode, cardDatabase.CardDataProvider, cardDatabase.AltPrintingProvider, out var altPrinting, out var _))
			{
				finalPrinting = altPrinting;
			}
			mtgCardInstance = finalPrinting?.CreateInstance() ?? MtgCardInstance.UnknownCardData(0u, new MtgZone
			{
				Type = ZoneType.Library
			});
			mtgCardInstance.SkinCode = skinCode;
			mtgCardInstance.SleeveCode = sleeveCode;
		}
		return new CardData(mtgCardInstance, finalPrinting);
	}

	public static CardData CreateRevealedCard(MtgCardInstance instance, ICardDataProvider cardProvider, IAbilityDataProvider abilityProvider)
	{
		CardData cardData = new CardData(instance, cardProvider.GetCardPrintingById(instance.GrpId, instance.SkinCode));
		for (int i = 0; i < cardData.LinkedFacePrintings.Count; i++)
		{
			ICardDataAdapter linkedFaceAtIndex = cardData.GetLinkedFaceAtIndex(i, ignoreInstance: true, cardProvider);
			if (cardData.HasPerpetualChanges())
			{
				PerpetualChangeUtilities.CopyPerpetualEffects(cardData, linkedFaceAtIndex, abilityProvider);
			}
			cardData.Instance.LinkedFaceInstances.Add(linkedFaceAtIndex.Instance);
		}
		return cardData;
	}
}
