using System.Collections.Generic;
using System.IO;
using GreClient.CardData;
using Newtonsoft.Json;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public class CardSetData
{
	private class AbilitiesByExpansionCode
	{
		public Dictionary<string, List<uint>> ExpansionCodeToAbilityIds = new Dictionary<string, List<uint>>();

		public Dictionary<string, List<FakeAbilityData>> ExpansionCodeToFakeAbilityData = new Dictionary<string, List<FakeAbilityData>>();
	}

	private class FakeAbilityData
	{
		public AbilitySubCategory SubCategory;

		public uint AbilityId;

		public AbilityPrintingData ToAbilityData()
		{
			uint abilityId = AbilityId;
			AbilitySubCategory subCategory = SubCategory;
			return new AbilityPrintingData(new AbilityPrintingRecord(abilityId, 0u, null, 0u, 0u, null, AbilityCategory.None, subCategory), NullAbilityDataProvider.Default);
		}
	}

	public static readonly string AgnosticSetCode = "SetAgnostic";

	private static readonly IReadOnlyCollection<CardData> SpecialHardCodedCardDatas;

	public readonly string ExpansionCode;

	public readonly Dictionary<uint, AbilityPrintingData> Abilities = new Dictionary<uint, AbilityPrintingData>();

	public readonly List<CardPrintingData> CardPrintingData = new List<CardPrintingData>();

	public readonly List<CardData> CardData = new List<CardData>();

	private CardSetData(string expansionCode)
	{
		ExpansionCode = expansionCode;
	}

	public static List<CardSetData> GetCardSetDataFromPrintingData(CardDatabase cardDatabase)
	{
		IReadOnlyDictionary<uint, CardPrintingData> allPrintings = cardDatabase.DatabaseUtilities.GetAllPrintings();
		Dictionary<string, CardSetData> dictionary = new Dictionary<string, CardSetData>();
		foreach (CardPrintingData value5 in allPrintings.Values)
		{
			if (string.IsNullOrEmpty(value5.ExpansionCode))
			{
				continue;
			}
			if (!dictionary.TryGetValue(value5.ExpansionCode, out var value))
			{
				value = new CardSetData(value5.ExpansionCode);
				dictionary.Add(value5.ExpansionCode, value);
			}
			foreach (AbilityPrintingData ability2 in value5.Abilities)
			{
				AddAbilityToCardSetData(value, ability2);
			}
			foreach (AbilityPrintingData hiddenAbility in value5.HiddenAbilities)
			{
				AddAbilityToCardSetData(value, hiddenAbility);
			}
			value.CardPrintingData.Add(value5);
			value.CardData.Add(new CardData(value5.CreateInstance(), value5));
		}
		foreach (CardData specialHardCodedCardData in SpecialHardCodedCardDatas)
		{
			if (string.IsNullOrEmpty(specialHardCodedCardData.ExpansionCode))
			{
				continue;
			}
			if (!dictionary.TryGetValue(specialHardCodedCardData.ExpansionCode, out var value2))
			{
				value2 = new CardSetData(specialHardCodedCardData.ExpansionCode);
				dictionary.Add(specialHardCodedCardData.ExpansionCode, value2);
			}
			foreach (AbilityPrintingData ability3 in specialHardCodedCardData.Printing.Abilities)
			{
				AddAbilityToCardSetData(value2, ability3);
			}
			foreach (AbilityPrintingData hiddenAbility2 in specialHardCodedCardData.Printing.HiddenAbilities)
			{
				AddAbilityToCardSetData(value2, hiddenAbility2);
			}
			value2.CardPrintingData.Add(specialHardCodedCardData.Printing);
			value2.CardData.Add(specialHardCodedCardData);
		}
		string path = Path.Combine(Directory.GetCurrentDirectory(), "BuildDataSources", "ForceInclude", "AbilitiesByExpansion.json");
		AbilitiesByExpansionCode abilitiesByExpansionCode = null;
		if (File.Exists(path))
		{
			using StreamReader streamReader = File.OpenText(path);
			abilitiesByExpansionCode = JsonConvert.DeserializeObject<AbilitiesByExpansionCode>(streamReader.ReadToEnd());
		}
		if (abilitiesByExpansionCode != null)
		{
			foreach (KeyValuePair<string, List<uint>> expansionCodeToAbilityId in abilitiesByExpansionCode.ExpansionCodeToAbilityIds)
			{
				if (!dictionary.TryGetValue(expansionCodeToAbilityId.Key, out var value3))
				{
					value3 = new CardSetData(expansionCodeToAbilityId.Key);
					dictionary.Add(expansionCodeToAbilityId.Key, value3);
				}
				foreach (uint item2 in expansionCodeToAbilityId.Value)
				{
					if (cardDatabase.AbilityDataProvider.TryGetAbilityPrintingById(item2, out var ability))
					{
						AddAbilityToCardSetData(value3, ability);
					}
				}
			}
			foreach (KeyValuePair<string, List<FakeAbilityData>> expansionCodeToFakeAbilityDatum in abilitiesByExpansionCode.ExpansionCodeToFakeAbilityData)
			{
				if (!dictionary.TryGetValue(expansionCodeToFakeAbilityDatum.Key, out var value4))
				{
					value4 = new CardSetData(expansionCodeToFakeAbilityDatum.Key);
					dictionary.Add(expansionCodeToFakeAbilityDatum.Key, value4);
				}
				foreach (FakeAbilityData item3 in expansionCodeToFakeAbilityDatum.Value)
				{
					AddAbilityToCardSetData(value4, item3.ToAbilityData());
				}
			}
		}
		List<CardSetData> list = new List<CardSetData>(dictionary.Values);
		int num = list.FindIndex((CardSetData x) => x.ExpansionCode.ToLower() == "ana");
		if (num > 0)
		{
			CardSetData item = list[num];
			list.RemoveAt(num);
			list.Insert(0, item);
		}
		return list;
	}

	private static void AddAbilityToCardSetData(CardSetData cardSetData, AbilityPrintingData ability)
	{
		if (cardSetData.Abilities.ContainsKey(ability.Id))
		{
			return;
		}
		cardSetData.Abilities.Add(ability.Id, ability);
		foreach (AbilityPrintingData referencedAbility in ability.ReferencedAbilities)
		{
			AddAbilityToCardSetData(cardSetData, referencedAbility);
		}
		foreach (AbilityPrintingData hiddenAbility in ability.HiddenAbilities)
		{
			AddAbilityToCardSetData(cardSetData, hiddenAbility);
		}
		foreach (AbilityPrintingData modalAbilityChild in ability.ModalAbilityChildren)
		{
			AddAbilityToCardSetData(cardSetData, modalAbilityChild);
		}
	}

	static CardSetData()
	{
		List<CardData> list = new List<CardData>();
		string agnosticSetCode = AgnosticSetCode;
		IReadOnlyCollection<string> additionalFrameDetails = (IReadOnlyCollection<string>)(object)new string[1] { "GemsReward" };
		list.Add(new CardData(null, new CardPrintingData(new CardPrintingRecord(0u, 0u, null, 0u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.MythicRare, agnosticSetCode, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, null, null, null, null, null, null, null, null, null, null, null, null, additionalFrameDetails), NullCardDataProvider.Default, NullAbilityDataProvider.Default)));
		string agnosticSetCode2 = AgnosticSetCode;
		additionalFrameDetails = (IReadOnlyCollection<string>)(object)new string[2] { "GemsReward", "Booster" };
		list.Add(new CardData(null, new CardPrintingData(new CardPrintingRecord(0u, 0u, null, 0u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.MythicRare, agnosticSetCode2, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, null, null, null, null, null, null, null, null, null, null, null, null, additionalFrameDetails), NullCardDataProvider.Default, NullAbilityDataProvider.Default)));
		string agnosticSetCode3 = AgnosticSetCode;
		additionalFrameDetails = (IReadOnlyCollection<string>)(object)new string[1] { "Library" };
		list.Add(new CardData(null, new CardPrintingData(new CardPrintingRecord(0u, 0u, null, 0u, 0u, 0u, 0u, 0u, 0u, 0u, null, CardArtSize.Normal, CardRarity.None, agnosticSetCode3, null, isToken: false, isPrimaryCard: true, isDigitalOnly: false, isRebalanced: false, 0u, 0u, 0u, null, null, null, null, usesSideboard: false, null, LinkedFace.None, null, null, default(TextChange), default(StringBackedInt), default(StringBackedInt), null, null, null, null, null, null, null, null, null, null, null, null, null, null, additionalFrameDetails), NullCardDataProvider.Default, NullAbilityDataProvider.Default)));
		SpecialHardCodedCardDatas = list;
	}
}
