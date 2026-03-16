using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using AssetLookupTree.Payloads.Card;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using UnityEngine.UI;
using Wizards.GeneralUtilities.Extensions;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Wrapper;
using Wotc.Mtgo.Gre.External.Messaging;

public class SetCollectionController
{
	public enum Metrics
	{
		Common,
		Uncommon,
		Rare,
		MythicRare,
		White,
		Blue,
		Black,
		Red,
		Green,
		Colorless,
		None
	}

	public struct MetricTotals
	{
		public int numOwned;

		public int numAvailable;
	}

	private readonly CardDatabase _cardDatabase;

	private readonly Dictionary<uint, int> _cardInventory;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly ISetMetadataProvider _setMetadataProvider;

	private readonly ITitleCountManager _titleCountManager;

	private readonly Dictionary<string, List<CardPrintingData>> _ownedCardsBySetCode;

	private readonly Dictionary<string, List<CardPrintingData>> _validGrpIdsBySetCode;

	private readonly Dictionary<(string, Metrics), MetricTotals> _metricTotalsBySetCode;

	public List<string> ValidSetCollectionSets => (from x in _validGrpIdsBySetCode
		where x.Value.Count >= 20 && _setMetadataProvider.IsSetPublished(x.Key)
		select x.Key).ToList();

	public SetCollectionController(CardDatabase db, AssetLookupSystem assetLookupSystem, ISetMetadataProvider setMetadataProvider, ITitleCountManager titleCountManager, Dictionary<uint, int> cardInventory)
	{
		_cardDatabase = db;
		_assetLookupSystem = assetLookupSystem;
		_cardInventory = cardInventory;
		_setMetadataProvider = setMetadataProvider;
		_titleCountManager = titleCountManager;
		_ownedCardsBySetCode = new Dictionary<string, List<CardPrintingData>>();
		_validGrpIdsBySetCode = new Dictionary<string, List<CardPrintingData>>();
		_metricTotalsBySetCode = new Dictionary<(string, Metrics), MetricTotals>();
		UpdateOwnedCards();
		UpdateValidGrpIdsForSets();
		UpdateMetricTotalsPerSet();
	}

	public void UpdateOwnedCards()
	{
		string key;
		foreach (KeyValuePair<string, List<CardPrintingData>> item in _ownedCardsBySetCode)
		{
			item.Deconstruct(out key, out var value);
			value.Clear();
		}
		foreach (KeyValuePair<string, List<uint>> item2 in _setMetadataProvider.SetCollectionGroup)
		{
			item2.Deconstruct(out key, out var value2);
			string key2 = key;
			List<uint> list = value2;
			List<CardPrintingData> orCreate = _ownedCardsBySetCode.GetOrCreate(key2);
			foreach (uint item3 in list)
			{
				CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(item3);
				if (_cardInventory.TryGetValue(item3, out var value3) && value3 > 0)
				{
					orCreate.Add(cardPrintingById);
				}
			}
		}
	}

	public void UpdateValidGrpIdsForSets()
	{
		foreach (var (text2, list2) in _setMetadataProvider.SetCollectionGroup)
		{
			if (!_setMetadataProvider.IsSetPublished(text2))
			{
				continue;
			}
			List<CardPrintingData> orCreate = _validGrpIdsBySetCode.GetOrCreate(text2);
			CollationMapping itemSubType = CollationMappingUtils.FromString(text2);
			Dictionary<uint, CardPrintingData> dictionary;
			if (_setMetadataProvider.IsAlchemy(itemSubType))
			{
				string alchemyExpansionString = text2.AsDigitalReleaseCode();
				string expansionCode = alchemyExpansionString.Substring(0, 3);
				dictionary = (from x in _cardDatabase.DatabaseUtilities.GetPrintingsByExpansion(expansionCode)
					where x.DigitalReleaseSet == alchemyExpansionString
					select x).ToDictionary((CardPrintingData x) => x.GrpId);
			}
			else
			{
				dictionary = _cardDatabase.DatabaseUtilities.GetPrintingsByExpansion(text2).ToDictionary((CardPrintingData x) => x.GrpId);
			}
			foreach (uint item in list2)
			{
				if (dictionary.TryGetValue(item, out var value))
				{
					orCreate.Add(value);
				}
			}
		}
	}

	public void UpdateMetricTotalsPerSet()
	{
		foreach (string validSetCollectionSet in ValidSetCollectionSets)
		{
			CollationMapping itemSubType = CollationMappingUtils.FromString(validSetCollectionSet);
			bool isAlchemySet = _setMetadataProvider.IsAlchemy(itemSubType);
			foreach (Metrics value in Enum.GetValues(typeof(Metrics)))
			{
				(string, Metrics) key = (validSetCollectionSet, value);
				if (!IsCollectionComplete(validSetCollectionSet, value, CountMode.UsePlayerInvOneOf, isAlchemySet))
				{
					_metricTotalsBySetCode[key] = new MetricTotals
					{
						numOwned = MetricCount(validSetCollectionSet, CountMode.UsePlayerInvOneOf, value, isAlchemySet),
						numAvailable = MetricCount(validSetCollectionSet, CountMode.UseCardDatabase, value, isAlchemySet)
					};
				}
				else
				{
					_metricTotalsBySetCode[key] = new MetricTotals
					{
						numOwned = MetricCount(validSetCollectionSet, CountMode.UsePlayerInvFourOf, value, isAlchemySet),
						numAvailable = MetricCount(validSetCollectionSet, CountMode.UseCardDatabase, value, isAlchemySet) * 4
					};
				}
			}
		}
	}

	private IEnumerable<CardPrintingData> OwnedCardsInSet(string expansionCode)
	{
		return _ownedCardsBySetCode.GetOrCreate(expansionCode);
	}

	private IEnumerable<CardPrintingData> GetSetCards(string expansionCode, CountMode countMode, bool isAlchemySet)
	{
		if (countMode == CountMode.UseCardDatabase)
		{
			return _validGrpIdsBySetCode.GetValueOrDefault(in expansionCode);
		}
		return OwnedCardsInSet(expansionCode);
	}

	private int GetOwnedCount(CardPrintingData card)
	{
		_cardInventory.TryGetValue(card.GrpId, out var value);
		if (value > 0 && _titleCountManager.OwnedTitleCounts.TryGetValue(card.TitleId, out var value2))
		{
			return Math.Clamp(value2, 0, 4);
		}
		return 0;
	}

	private int RarityCount(string expansionCode, CardRarity rarity, CountMode countMode = CountMode.UseCardDatabase, bool isAlchemySet = false)
	{
		IEnumerable<CardPrintingData> obj = GetSetCards(expansionCode, countMode, isAlchemySet) ?? Enumerable.Empty<CardPrintingData>();
		int num = 0;
		foreach (CardPrintingData item in obj)
		{
			if (item.Rarity == rarity && item.IsPrimaryCard && CardUtilities.IsCardCollectible(item) && CardUtilities.IsCardCraftable(item))
			{
				num = ((countMode != CountMode.UsePlayerInvFourOf) ? (num + 1) : (num + GetOwnedCount(item)));
			}
		}
		return num;
	}

	private int ColorCount(string expansionCode, CardColor color, CountMode countMode = CountMode.UseCardDatabase, bool isAlchemySet = false)
	{
		IEnumerable<CardPrintingData> obj = GetSetCards(expansionCode, countMode, isAlchemySet) ?? Enumerable.Empty<CardPrintingData>();
		int num = 0;
		foreach (CardPrintingData item in obj)
		{
			if ((color == CardColor.Colorless && item.Colors.Count == 0 && !item.IsBasicLand && item.IsPrimaryCard) || (item.Colors.Contains(color) && !item.IsBasicLand && item.IsPrimaryCard && CardUtilities.IsCardCollectible(item) && CardUtilities.IsCardCraftable(item)))
			{
				num = ((countMode != CountMode.UsePlayerInvFourOf) ? (num + 1) : (num + GetOwnedCount(item)));
			}
		}
		return num;
	}

	public int MetricCount(string expansionCode, CountMode countMode, Metrics metric = Metrics.None, bool isAlchemySet = false)
	{
		switch (metric)
		{
		case Metrics.Common:
		case Metrics.Uncommon:
		case Metrics.Rare:
		case Metrics.MythicRare:
			return RarityCount(expansionCode, metric.AsRarity(), countMode, isAlchemySet);
		case Metrics.White:
		case Metrics.Blue:
		case Metrics.Black:
		case Metrics.Red:
		case Metrics.Green:
		case Metrics.Colorless:
			return ColorCount(expansionCode, metric.AsCardColor(), countMode, isAlchemySet);
		default:
			return TotalCount(expansionCode, countMode, isAlchemySet);
		}
	}

	public MetricTotals GetMetricTotals(string expansionCode, Metrics metric = Metrics.None)
	{
		return _metricTotalsBySetCode.GetOrCreate((expansionCode, metric));
	}

	private int TotalCount(string expansionCode, CountMode countMode, bool isAlchemy = false)
	{
		IEnumerable<CardPrintingData> enumerable = GetSetCards(expansionCode, countMode, isAlchemy) ?? Enumerable.Empty<CardPrintingData>();
		int num = 0;
		if (countMode == CountMode.UsePlayerInvFourOf)
		{
			foreach (CardPrintingData item in enumerable)
			{
				num += GetOwnedCount(item);
			}
		}
		else
		{
			num = enumerable.Count();
		}
		return num;
	}

	public Sprite GetSetIcon(CollationMapping expansionCode, CardRarity cardRarity = CardRarity.Common)
	{
		if (_setMetadataProvider.IsAlchemy(expansionCode))
		{
			expansionCode = _setMetadataProvider.GetMainExpansion(expansionCode.ToString());
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(CardDataExtensions.CreateBlankExpansionCard(expansionCode.ToString(), expansionCode.ToString()));
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ExpansionSymbol> loadedTree))
		{
			return AssetLoader.GetObjectData<Sprite>((loadedTree?.GetPayload(_assetLookupSystem.Blackboard)).GetIconRef(cardRarity).RelativePath);
		}
		return null;
	}

	public Sprite GetAlchemyIcon(CollationMapping expansionCode)
	{
		_assetLookupSystem.Blackboard.Clear();
		string text = expansionCode.ToString().Substring(0, 3);
		_assetLookupSystem.Blackboard.SetCardDataExtensive(CardDataExtensions.CreateBlankExpansionCard(text, text));
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<ExpansionSymbol> loadedTree))
		{
			return AssetLoader.GetObjectData<Sprite>((loadedTree?.GetPayload(_assetLookupSystem.Blackboard)).GetIconRef(CardRarity.Common).RelativePath);
		}
		return null;
	}

	public void GetSetLogo(CollationMapping expansionCode, ref RawImageReferenceLoader loader, ref RawImage image)
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.BoosterCollationMapping = expansionCode;
		if (!_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree))
		{
			return;
		}
		Logo payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
		if (payload != null)
		{
			if (loader == null)
			{
				loader = new RawImageReferenceLoader(image);
			}
			loader.SetTexture(payload.GetHeaderFilePath());
		}
	}

	public DateTime GetReleaseDate(CollationMapping expansionCode)
	{
		return _setMetadataProvider.ReleaseDateForSet(expansionCode);
	}

	public CollationMapping GetMostRecentExpansion()
	{
		return (from mapping in ValidSetCollectionSets.Select(CollationMappingUtils.FromString)
			orderby _setMetadataProvider.ReleaseDateForSet(mapping) descending
			select mapping).DefaultIfEmpty(CollationMapping.None).First();
	}

	public bool IsCollectionComplete(string expansionCode, Metrics metric = Metrics.None, CountMode countMode = CountMode.UsePlayerInvOneOf, bool isAlchemySet = false)
	{
		return MetricCount(expansionCode, countMode, metric, isAlchemySet) >= MetricCount(expansionCode, CountMode.UseCardDatabase, metric, isAlchemySet);
	}
}
