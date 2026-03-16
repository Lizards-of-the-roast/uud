using System.Collections.Generic;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace GreClient.Test;

public static class TestData
{
	public static class Abilities
	{
		public static AbilityPrintingRecord Enchant = new AbilityPrintingRecord(4u, 0u, null, 0u, 118u, null, AbilityCategory.Static);

		public static AbilityPrintingRecord Flying;

		public static AbilityPrintingRecord Haste;

		public static AbilityPrintingRecord Hexproof;

		public static AbilityPrintingRecord Lifelink;

		public static AbilityPrintingRecord Trample;

		public static AbilityPrintingRecord Vigilance;

		public static AbilityPrintingRecord Indestructible;

		public static AbilityPrintingRecord Ingest;

		public static AbilityPrintingRecord Devoid;

		public static AbilityPrintingRecord Station;

		public static AbilityPrintingRecord BasicLand_Add_W;

		public static AbilityPrintingRecord Ability1027;

		public static AbilityPrintingRecord Tap_Add_C;

		public static AbilityPrintingRecord Ability2433;

		public static AbilityPrintingRecord Ability60002;

		public static AbilityPrintingRecord Ability60024;

		public static AbilityPrintingRecord Ability121982;

		public static AbilityPrintingRecord Ability121983;

		public static AbilityPrintingRecord CubwardenMutateTrigger;

		public static AbilityPrintingRecord Ability137215;

		public static AbilityPrintingRecord Ward_o2;

		public static AbilityPrintingRecord Ability15269;

		public static AbilityPrintingRecord NowhereToRun_Ability3;

		public static AbilityPrintingRecord Ability181173;

		public static AbilityPrintingRecord Ability6154;

		public static AbilityPrintingRecord Ability181175;

		public static AbilityPrintingRecord Ability133104;

		public static AbilityPrintingRecord Ability19961;

		public static AbilityPrintingRecord Ability4833;

		public static AbilityPrintingRecord Fuse;

		public static AbilityPrintingRecord Foretell_2RG;

		public static AbilityPrintingRecord Foretell_R;

		public static AbilityPrintingRecord Ability132646;

		public static AbilityPrintingRecord Ability132642;

		public static AbilityPrintingRecord Ability61955;

		public static AbilityPrintingRecord Ability61956;

		public static AbilityPrintingRecord Ability61957;

		internal static IAbilityDataProvider provider;

		public static AbilityPrintingData GetAbility(AbilityPrintingRecord record)
		{
			return provider.GetAbilityPrintingById(record.Id);
		}

		public static AbilityPrintingData GetAbility(uint abilityId)
		{
			return provider.GetAbilityPrintingById(abilityId);
		}

		static Abilities()
		{
			IReadOnlyList<MetaDataTag> tags = new MetaDataTag[3]
			{
				MetaDataTag.Ability_Evergreen,
				MetaDataTag.Ability_Groupable,
				MetaDataTag.Ability_Keyword
			};
			Flying = new AbilityPrintingRecord(8u, 0u, null, 0u, 46u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, null, null, tags);
			tags = new MetaDataTag[3]
			{
				MetaDataTag.Ability_Evergreen,
				MetaDataTag.Ability_Groupable,
				MetaDataTag.Ability_Keyword
			};
			Haste = new AbilityPrintingRecord(9u, 0u, null, 0u, 47u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, null, null, tags);
			tags = new MetaDataTag[4]
			{
				MetaDataTag.Ability_Evergreen,
				MetaDataTag.Ability_Groupable,
				MetaDataTag.Ability_Keyword,
				MetaDataTag.Ability_OmitDuplicates
			};
			Hexproof = new AbilityPrintingRecord(10u, 0u, null, 0u, 29u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, null, null, tags);
			tags = new MetaDataTag[3]
			{
				MetaDataTag.Ability_Evergreen,
				MetaDataTag.Ability_Groupable,
				MetaDataTag.Ability_Keyword
			};
			Lifelink = new AbilityPrintingRecord(12u, 0u, null, 0u, 38u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, null, null, tags);
			tags = new MetaDataTag[4]
			{
				MetaDataTag.Ability_Evergreen,
				MetaDataTag.Ability_Groupable,
				MetaDataTag.Ability_Keyword,
				MetaDataTag.Ability_OmitDuplicates
			};
			Trample = new AbilityPrintingRecord(14u, 0u, null, 0u, 28u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, null, null, tags);
			tags = new MetaDataTag[4]
			{
				MetaDataTag.Ability_Evergreen,
				MetaDataTag.Ability_Groupable,
				MetaDataTag.Ability_Keyword,
				MetaDataTag.Ability_OmitDuplicates
			};
			Vigilance = new AbilityPrintingRecord(15u, 0u, null, 0u, 137u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, null, null, tags);
			tags = new MetaDataTag[4]
			{
				MetaDataTag.Ability_Evergreen,
				MetaDataTag.Ability_Groupable,
				MetaDataTag.Ability_Keyword,
				MetaDataTag.Ability_OmitDuplicates
			};
			Indestructible = new AbilityPrintingRecord(104u, 0u, null, 0u, 97u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, null, null, tags);
			Ingest = new AbilityPrintingRecord(144u, 0u, null, 0u, 130u, null, AbilityCategory.Triggered);
			tags = new MetaDataTag[2]
			{
				MetaDataTag.Ability_Keyword,
				MetaDataTag.Ability_OmitDuplicates
			};
			Devoid = new AbilityPrintingRecord(151u, 0u, null, 0u, 78u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, null, null, tags);
			IReadOnlyList<ZoneType> relevantZones = new ZoneType[1] { ZoneType.Battlefield };
			Station = new AbilityPrintingRecord(373u, 0u, null, 0u, 60000u, null, AbilityCategory.Activated, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, relevantZones);
			BasicLand_Add_W = new AbilityPrintingRecord(1001u, 0u, null, 0u, 227607u, null, AbilityCategory.None, AbilitySubCategory.Mana, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.TapSymbol);
			Ability1027 = new AbilityPrintingRecord(1027u, 4u, null, 0u, 605u, null, AbilityCategory.Static);
			Tap_Add_C = new AbilityPrintingRecord(1152u, 0u, null, 0u, 227595u, null, AbilityCategory.None, AbilitySubCategory.Mana, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.TapSymbol);
			Ability2433 = new AbilityPrintingRecord(2433u, 0u, null, 0u, 3315u, null, AbilityCategory.Static);
			IReadOnlyList<uint> hiddenAbilityIds = new uint[1] { Ability2433.Id };
			Ability60002 = new AbilityPrintingRecord(60002u, 0u, null, 0u, 60002u, null, AbilityCategory.None, AbilitySubCategory.StationIntrinsicLevel, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), "2+", default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: true, null, null, null, null, hiddenAbilityIds);
			StringBackedInt power = new StringBackedInt(3);
			StringBackedInt toughness = new StringBackedInt(5);
			hiddenAbilityIds = new uint[2] { Flying.Id, Lifelink.Id };
			Ability60024 = new AbilityPrintingRecord(60024u, 0u, null, 0u, 60024u, null, AbilityCategory.Static, AbilitySubCategory.StationIntrinsicLevel, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), "12+", power, toughness, fullyParsed: true, isIntrinsicAbility: true, null, null, null, null, hiddenAbilityIds);
			relevantZones = new ZoneType[2]
			{
				ZoneType.Hand,
				ZoneType.Stack
			};
			Ability121982 = new AbilityPrintingRecord(121982u, 0u, null, 0u, 835853u, null, AbilityCategory.Triggered, AbilitySubCategory.EnterTheBattlefield, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, relevantZones);
			Ability121983 = new AbilityPrintingRecord(121983u, 0u, null, 0u, 280300u, null, AbilityCategory.Activated, AbilitySubCategory.Mana, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.TapSymbol);
			IReadOnlyList<AbilityType> referencedAbilityTypes = new AbilityType[1] { AbilityType.Lifelink };
			hiddenAbilityIds = new uint[1] { Lifelink.Id };
			CubwardenMutateTrigger = new AbilityPrintingRecord(137157u, 0u, null, 0u, 985750u, null, AbilityCategory.Triggered, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, hiddenAbilityIds, referencedAbilityTypes);
			relevantZones = new ZoneType[2]
			{
				ZoneType.Hand,
				ZoneType.Stack
			};
			hiddenAbilityIds = new uint[2] { Lifelink.Id, Hexproof.Id };
			Ability137215 = new AbilityPrintingRecord(137215u, 0u, null, 0u, 840072u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, relevantZones, null, null, null, null, hiddenAbilityIds);
			hiddenAbilityIds = new uint[1] { 211u };
			Ward_o2 = new AbilityPrintingRecord(141939u, 211u, null, 0u, 488336u, null, AbilityCategory.Triggered, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, hiddenAbilityIds);
			referencedAbilityTypes = new AbilityType[1] { AbilityType.Trample };
			hiddenAbilityIds = new uint[1] { 14u };
			Ability15269 = new AbilityPrintingRecord(15269u, 0u, null, 0u, 23415u, null, AbilityCategory.None, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, hiddenAbilityIds, referencedAbilityTypes);
			referencedAbilityTypes = new AbilityType[2]
			{
				AbilityType.Hexproof,
				AbilityType.Ward
			};
			hiddenAbilityIds = new uint[2] { 10u, 211u };
			NowhereToRun_Ability3 = new AbilityPrintingRecord(174396u, 0u, null, 0u, 812650u, null, AbilityCategory.Static, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, hiddenAbilityIds, referencedAbilityTypes);
			relevantZones = new ZoneType[2]
			{
				ZoneType.Hand,
				ZoneType.Stack
			};
			hiddenAbilityIds = new uint[1] { 133104u };
			Ability181173 = new AbilityPrintingRecord(181173u, 0u, null, 0u, 896583u, null, AbilityCategory.Spell, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, relevantZones, null, null, null, null, null, null, null, hiddenAbilityIds);
			relevantZones = new ZoneType[2]
			{
				ZoneType.Hand,
				ZoneType.Stack
			};
			hiddenAbilityIds = new uint[1] { 133104u };
			Ability6154 = new AbilityPrintingRecord(6154u, 0u, null, 0u, 896584u, null, AbilityCategory.Spell, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, relevantZones, null, null, null, null, null, null, null, hiddenAbilityIds);
			relevantZones = new ZoneType[2]
			{
				ZoneType.Hand,
				ZoneType.Stack
			};
			hiddenAbilityIds = new uint[1] { 133104u };
			Ability181175 = new AbilityPrintingRecord(181175u, 0u, null, 0u, 896585u, null, AbilityCategory.Spell, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, relevantZones, null, null, null, null, null, null, null, hiddenAbilityIds);
			hiddenAbilityIds = new uint[3] { Ability181173.Id, Ability6154.Id, Ability181175.Id };
			IReadOnlyList<uint> hiddenAbilityIds2 = new uint[3] { Ability181173.Id, Ability6154.Id, Ability181175.Id };
			Ability133104 = new AbilityPrintingRecord(133104u, 0u, null, 0u, 896586u, null, AbilityCategory.Spell, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, hiddenAbilityIds2, null, null, null, hiddenAbilityIds);
			referencedAbilityTypes = new AbilityType[1] { AbilityType.DoubleStrike };
			relevantZones = new ZoneType[2]
			{
				ZoneType.Hand,
				ZoneType.Stack
			};
			Ability19961 = new AbilityPrintingRecord(19961u, 0u, null, 0u, 42624u, null, AbilityCategory.Spell, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, relevantZones, null, null, null, null, null, referencedAbilityTypes);
			relevantZones = new ZoneType[2]
			{
				ZoneType.Hand,
				ZoneType.Stack
			};
			Ability4833 = new AbilityPrintingRecord(4833u, 0u, null, 0u, 21871u, null, AbilityCategory.Spell, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, relevantZones);
			Fuse = new AbilityPrintingRecord(103u, 0u, null, 0u, 39u, null, AbilityCategory.Static);
			Foretell_2RG = new AbilityPrintingRecord(208u, 0u, null, 0u, 0u, "o2oRoG");
			Foretell_R = new AbilityPrintingRecord(208u, 0u, null, 0u, 0u, "oR");
			relevantZones = new ZoneType[1] { ZoneType.Battlefield };
			hiddenAbilityIds2 = new uint[4] { 132642u, 61955u, 61956u, 61957u };
			Ability132646 = new AbilityPrintingRecord(132646u, 0u, null, 0u, 333402u, null, AbilityCategory.Triggered, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, relevantZones, null, null, null, null, null, null, hiddenAbilityIds2);
			hiddenAbilityIds2 = new uint[1] { 132646u };
			Ability132642 = new AbilityPrintingRecord(132642u, 0u, null, 0u, 333396u, null, AbilityCategory.Chained, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, hiddenAbilityIds2);
			hiddenAbilityIds2 = new uint[1] { 132646u };
			Ability61955 = new AbilityPrintingRecord(61955u, 0u, null, 0u, 47808u, null, AbilityCategory.Chained, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, hiddenAbilityIds2);
			hiddenAbilityIds2 = new uint[1] { 132646u };
			Ability61956 = new AbilityPrintingRecord(61956u, 0u, null, 0u, 47809u, null, AbilityCategory.Chained, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, hiddenAbilityIds2);
			hiddenAbilityIds2 = new uint[1] { 132646u };
			Ability61957 = new AbilityPrintingRecord(61957u, 0u, null, 0u, 47810u, null, AbilityCategory.Chained, AbilitySubCategory.None, AbilityWord.None, NumericAid.NumericAid_None, RequiresConfirmation.None, AbilityPaymentType.None, default(StringBackedInt), null, default(StringBackedInt), default(StringBackedInt), fullyParsed: true, isIntrinsicAbility: false, null, null, null, null, null, null, null, null, null, hiddenAbilityIds2);
			provider = new MockAbilityDataProvider(Enchant, Flying, Haste, Hexproof, Lifelink, Trample, Vigilance, Indestructible, Devoid, Ingest, BasicLand_Add_W, Ability1027, Tap_Add_C, Station, Ability2433, Ability60002, Ability60024, Ability61955, Ability61956, Ability61957, CubwardenMutateTrigger, Ward_o2, Ability15269, NowhereToRun_Ability3, Ability121982, Ability121983, Ability137215, Ability181173, Ability6154, Ability181175, Ability132642, Ability132646, Ability133104);
		}
	}

	public static class Cards
	{
		public static CardPrintingRecord Plains;

		public static CardPrintingRecord Wastes;

		public static CardPrintingRecord RuneclawBear;

		public static CardPrintingRecord LumenClassFrigate;

		public static CardPrintingRecord YokedOx;

		public static CardPrintingRecord PlazaOfHarmony;

		public static CardPrintingRecord WingfoldPteron;

		public static CardPrintingRecord GreenwoodSentinel;

		public static CardPrintingRecord AngrathsRampage;

		public static CardPrintingRecord ArmedAndDangerous;

		public static CardPrintingRecord Armed;

		public static CardPrintingRecord CullingDrone;

		public static CardPrintingRecord ZombieToken;

		public static CardPrintingRecord MonsterToken;

		public static CardPrintingRecord DemonicPact;

		private static ICardDataProvider provider;

		public static CardPrintingData GetCard(uint grpId)
		{
			return provider.GetCardPrintingById(grpId);
		}

		static Cards()
		{
			IReadOnlyList<CardType> types = new CardType[1] { CardType.Land };
			IReadOnlyList<SuperType> supertypes = new SuperType[1] { SuperType.Basic };
			IReadOnlyList<SubType> subtypes = new SubType[1] { SubType.Plains };
			IReadOnlyList<CardColor> frameColors = new CardColor[1] { CardColor.White };
			Plains = new CardPrintingRecord(7193u, 0u, null, 648u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, frameColors, null, types, subtypes, supertypes);
			types = new CardType[1] { CardType.Land };
			supertypes = new SuperType[1] { SuperType.Basic };
			IReadOnlyList<(uint, uint)> abilityIds = new(uint, uint)[1] { (Abilities.Tap_Add_C.Id, 0u) };
			Wastes = new CardPrintingRecord(62531u, 0u, null, 3142u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, null, null, types, null, supertypes, abilityIds);
			types = new CardType[1] { CardType.Creature };
			subtypes = new SubType[1] { SubType.Bear };
			StringBackedInt power = new StringBackedInt(2);
			StringBackedInt toughness = new StringBackedInt(2);
			frameColors = new CardColor[1] { CardColor.Green };
			RuneclawBear = new CardPrintingRecord(79342u, 0u, null, 2288u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.Common, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, "o1oG", LinkedFace.None, null, null, default(TextChange), power, toughness, null, null, frameColors, null, types, subtypes);
			types = new CardType[1] { CardType.Artifact };
			subtypes = new SubType[1] { SubType.Spacecraft };
			frameColors = new CardColor[1] { CardColor.White };
			abilityIds = new(uint, uint)[6]
			{
				(Abilities.Station.Id, 0u),
				(Abilities.Ability60002.Id, 0u),
				(Abilities.Ability2433.Id, 0u),
				(Abilities.Ability60024.Id, 0u),
				(Abilities.Flying.Id, 0u),
				(Abilities.Lifelink.Id, 0u)
			};
			IReadOnlyList<(uint, uint)> hiddenAbilityIds = new(uint, uint)[3]
			{
				(Abilities.Ability2433.Id, 0u),
				(Abilities.Flying.Id, 0u),
				(Abilities.Lifelink.Id, 0u)
			};
			LumenClassFrigate = new CardPrintingRecord(96599u, 0u, null, 1055535u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, frameColors, null, types, subtypes, null, abilityIds, hiddenAbilityIds);
			types = new CardType[1] { CardType.Creature };
			subtypes = new SubType[1] { SubType.Ox };
			toughness = new StringBackedInt(0);
			power = new StringBackedInt(4);
			frameColors = new CardColor[1] { CardColor.White };
			IReadOnlyList<CardColor> colorIdentity = new CardColor[1] { CardColor.White };
			YokedOx = new CardPrintingRecord(69826u, 0u, null, 43200u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.Common, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, "oW", LinkedFace.None, null, null, default(TextChange), toughness, power, null, colorIdentity, frameColors, null, types, subtypes);
			types = new CardType[1] { CardType.Land };
			colorIdentity = new CardColor[5]
			{
				CardColor.White,
				CardColor.Blue,
				CardColor.Black,
				CardColor.Red,
				CardColor.Green
			};
			hiddenAbilityIds = new(uint, uint)[3]
			{
				(Abilities.Ability121982.Id, 0u),
				(Abilities.Tap_Add_C.Id, 0u),
				(Abilities.Ability121983.Id, 0u)
			};
			PlazaOfHarmony = new CardPrintingRecord(69402u, 0u, null, 280297u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, colorIdentity, null, types, null, null, hiddenAbilityIds);
			power = new StringBackedInt(3);
			toughness = new StringBackedInt(6);
			colorIdentity = new CardColor[1] { CardColor.Blue };
			hiddenAbilityIds = new(uint, uint)[1] { (Abilities.Ability137215.Id, 0u) };
			WingfoldPteron = new CardPrintingRecord(71138u, 0u, null, 428082u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.Common, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, "o5oU", LinkedFace.None, null, null, default(TextChange), power, toughness, null, null, colorIdentity, null, null, null, null, hiddenAbilityIds);
			toughness = new StringBackedInt(2);
			power = new StringBackedInt(2);
			colorIdentity = new CardColor[1] { CardColor.Green };
			frameColors = new CardColor[1] { CardColor.Green };
			hiddenAbilityIds = new(uint, uint)[1] { (Abilities.Vigilance.Id, 0u) };
			GreenwoodSentinel = new CardPrintingRecord(68054u, 0u, null, 229697u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.Common, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, "o1oG", LinkedFace.None, null, null, default(TextChange), toughness, power, colorIdentity, null, frameColors, null, null, null, null, hiddenAbilityIds);
			types = new CardType[1] { CardType.Sorcery };
			frameColors = new CardColor[2]
			{
				CardColor.Black,
				CardColor.Red
			};
			hiddenAbilityIds = new(uint, uint)[1] { (Abilities.Ability133104.Id, 0u) };
			abilityIds = new(uint, uint)[3]
			{
				(Abilities.Ability181173.Id, 0u),
				(Abilities.Ability6154.Id, 0u),
				(Abilities.Ability181175.Id, 0u)
			};
			AngrathsRampage = new CardPrintingRecord(69636u, 0u, null, 336768u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, "oBoR", LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, frameColors, null, types, null, null, hiddenAbilityIds, abilityIds);
			types = new CardType[1] { CardType.Sorcery };
			frameColors = new CardColor[2]
			{
				CardColor.Red,
				CardColor.Green
			};
			abilityIds = new(uint, uint)[3]
			{
				(Abilities.Ability19961.Id, 0u),
				(Abilities.Ability4833.Id, 0u),
				(Abilities.Fuse.Id, 0u)
			};
			ArmedAndDangerous = new CardPrintingRecord(94674u, 0u, null, 112710u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, "o1oRo3oG", LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, frameColors, null, types, null, null, abilityIds);
			types = new CardType[1] { CardType.Sorcery };
			frameColors = new CardColor[1] { CardColor.Red };
			abilityIds = new(uint, uint)[1] { (Abilities.Ability19961.Id, 42623u) };
			Armed = new CardPrintingRecord(94675u, 0u, null, 42623u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: false, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, "o1oR", LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, frameColors, null, types, null, null, abilityIds);
			types = new CardType[1] { CardType.Creature };
			subtypes = new SubType[2]
			{
				SubType.Eldrazi,
				SubType.Drone
			};
			frameColors = new CardColor[1] { CardColor.Black };
			abilityIds = new(uint, uint)[2]
			{
				(Abilities.Devoid.Id, 0u),
				(Abilities.Ingest.Id, 0u)
			};
			CullingDrone = new CardPrintingRecord(61797u, 0u, null, 635u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, "o1oB", LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, frameColors, null, types, subtypes, null, abilityIds);
			power = new StringBackedInt(2);
			toughness = new StringBackedInt(2);
			types = new CardType[1] { CardType.Creature };
			subtypes = new SubType[1] { SubType.Zombie };
			frameColors = new CardColor[1] { CardColor.Black };
			ZombieToken = new CardPrintingRecord(66583u, 0u, null, 572u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: true, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), power, toughness, null, null, frameColors, null, types, subtypes);
			types = new CardType[1] { CardType.Enchantment };
			subtypes = new SubType[2]
			{
				SubType.Aura,
				SubType.Role
			};
			abilityIds = new(uint, uint)[2]
			{
				(Abilities.Ability1027.Id, 0u),
				(Abilities.Ability15269.Id, 0u)
			};
			MonsterToken = new CardPrintingRecord(87500u, 0u, null, 738232u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: true, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, null, null, types, subtypes, null, abilityIds);
			types = new CardType[1] { CardType.Enchantment };
			frameColors = new CardColor[1] { CardColor.Black };
			abilityIds = new(uint, uint)[1] { (Abilities.Ability132646.Id, 0u) };
			hiddenAbilityIds = new(uint, uint)[4]
			{
				(Abilities.Ability132646.Id, 0u),
				(Abilities.Ability61955.Id, 0u),
				(Abilities.Ability61956.Id, 0u),
				(Abilities.Ability61957.Id, 0u)
			};
			DemonicPact = new CardPrintingRecord(95376u, 0u, null, 47806u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, null, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, "o2oBoB", LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, frameColors, null, types, null, null, abilityIds, hiddenAbilityIds);
			provider = new MockCardDataProvider(Abilities.provider, Plains, Wastes, RuneclawBear, LumenClassFrigate, YokedOx, WingfoldPteron, GreenwoodSentinel, AngrathsRampage, CullingDrone, ZombieToken, MonsterToken, PlazaOfHarmony, DemonicPact);
		}
	}

	public static class Actions
	{
		public static Action BanefireCast = new Action
		{
			ActionType = ActionType.Cast,
			GrpId = 67940u,
			ManaCost = 
			{
				new ManaRequirement
				{
					Color = { ManaColor.X },
					Count = 1
				},
				new ManaRequirement
				{
					Color = { ManaColor.Red },
					Count = 1
				}
			},
			ShouldStop = true,
			ManaSpentMatters = true
		};
	}

	public static CardPrintingData ToPrinting(this CardPrintingRecord record)
	{
		return Cards.GetCard(record.GrpId);
	}

	public static AbilityPrintingData ToPrinting(this AbilityPrintingRecord record)
	{
		return Abilities.GetAbility(record.Id);
	}
}
