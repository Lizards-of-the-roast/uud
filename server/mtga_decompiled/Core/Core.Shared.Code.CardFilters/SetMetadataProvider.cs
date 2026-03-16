using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Collations;
using Core.Code.Collections;
using Core.Meta.MainNavigation.Store.Data;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.Enums.Card;
using Wizards.Arena.Enums.Set;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Wrapper;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Core.Shared.Code.CardFilters;

public class SetMetadataProvider : ISetMetadataProvider
{
	public List<List<FilterElement>> Groups;

	public List<FilterElement> ColorGroup;

	private List<FilterElement> _collationFilters;

	private Dictionary<CollationMapping, ClientSetCollation> _collationsByMapping;

	private HashSet<string> _bonusSheets;

	private readonly HashSet<string> _unpublishedSets = new HashSet<string>();

	private Dictionary<CardFilterType, string> _setcodesByFilter;

	private Dictionary<CollationMapping, string> _flavorForCollation;

	private Dictionary<string, List<CardFilterType>> _setGroupsAsFilters;

	private Dictionary<string, SetAvailability> _availabilitiesBySetCode;

	private List<StoreSetFilterModel> _storeSets;

	private Dictionary<string, List<string>> SetCodeAliasesReverseMap;

	public Dictionary<string, List<uint>> SetCollectionGroup { get; private set; } = new Dictionary<string, List<uint>>();

	public List<CardFilterType> AllExpansionsAsFilterTypes { get; private set; }

	public List<StoreSetFilterModel> StoreSets => _storeSets;

	public string LastPublishedMajorSet { get; private set; }

	public string SetCodeForFilter(CardFilterType filterType)
	{
		return _setcodesByFilter.GetValueOrDefault(in filterType);
	}

	public string FlavorForCollation(CollationMapping itemSubType)
	{
		return _flavorForCollation.GetValueOrDefault(in itemSubType);
	}

	public bool IsAlchemy(CollationMapping itemSubType)
	{
		return _flavorForCollation.GetValueOrDefault(in itemSubType) == "Alchemy";
	}

	private bool IsAlchemy(ClientSetMetadata setMetadata)
	{
		foreach (ClientSetCollation collation in setMetadata.Collations)
		{
			if (IsAlchemy(collation.CollationCode))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsUniversesBeyond(CollationMapping itemSubType)
	{
		return _storeSets.Exists((StoreSetFilterModel ss) => ss.Sets.Contains(itemSubType) && ss.Tags.Contains(GroupTag.UniversesBeyond));
	}

	public CollationMapping GetMainExpansion(string expansionCode)
	{
		return CollationMappingUtils.FromString(expansionCode.Substring(4, 3));
	}

	public void LoadData(SetMetadataCollection collection)
	{
		List<ClientSetMetadata> setDatas = collection.SetDatas;
		_collationsByMapping = (from col in setDatas.SelectMany((ClientSetMetadata col) => col.Collations)
			where col.CollationCode != CollationMapping.None
			select col).ToDictionary((ClientSetCollation x) => x.CollationCode);
		_bonusSheets = (from sd in setDatas
			where sd.IsBonusSheet
			select sd.SetCode).ToHashSet();
		SetCollectionGroup = collection.SetCollectionGroup;
		_setcodesByFilter = SetMetadataCore.CreateCardFiltersMap(setDatas);
		_setGroupsAsFilters = SetMetadataCore.MapFromSetGroups(collection.SetGroups);
		_availabilitiesBySetCode = SetMetadataCore.MapAvailabilitiesFromSetData(setDatas);
		AllExpansionsAsFilterTypes = new List<CardFilterType>();
		AllExpansionsAsFilterTypes.AddRange(_setGroupsAsFilters["AllFilters"]);
		_storeSets = SetMetadataCore.ToStoreSetFilterModelsForStorePackGroups(collection.StoreSetGroups, _collationsByMapping);
		UpdateCollationFilters(_collationFilters, setDatas);
		_flavorForCollation = (from col in setDatas.SelectMany((ClientSetMetadata col) => col.Collations)
			where col.CollationCode != CollationMapping.None
			select col).ToDictionary((ClientSetCollation x) => x.CollationCode, (ClientSetCollation y) => y.FlavorId);
		DateTime dateTime = DateTime.MinValue;
		string text = null;
		foreach (ClientSetMetadata item in setDatas)
		{
			if (item.IsPublished && item.ReleaseDate > dateTime && item.IsMajorCardset)
			{
				dateTime = item.ReleaseDate;
				text = item.SetCode;
			}
			if (!item.IsPublished)
			{
				_unpublishedSets.Add(item.SetCode);
			}
		}
		LastPublishedMajorSet = text;
		if (string.IsNullOrEmpty(text))
		{
			SimpleLog.LogError("LastPublishedMajorSet was not determined correctly! There is likely an error in SetMetadata content");
		}
		LoadSetCodeAliases(setDatas);
	}

	public ClientSetCollation CollationForMapping(CollationMapping mapping)
	{
		return _collationsByMapping.GetValueOrDefault(in mapping);
	}

	private static void UpdateCollationFilters(List<FilterElement> collationFilters, List<ClientSetMetadata> setDatas)
	{
		collationFilters.Clear();
		collationFilters.AddRange(SetMetadataCore.ToFilterElements(setDatas));
		collationFilters.Add(new FilterElement(CardFilterType.RebalancedCards, SetMetadataCore.CreateRebalancedNode()));
	}

	public List<CardFilterType> FiltersForFormat(string key)
	{
		return _setGroupsAsFilters.GetValueOrDefault(in key);
	}

	public DateTime ReleaseDateForSet(CollationMapping expansionCode)
	{
		if (_collationsByMapping.TryGetValue(expansionCode, out var value))
		{
			return value.Set.ReleaseDate;
		}
		return DateTime.MinValue;
	}

	public bool SetIsBonusSheet(string expansionCode)
	{
		return _bonusSheets?.Contains(expansionCode) ?? false;
	}

	public bool IsSetPublished(string expansionCode)
	{
		HashSet<string> unpublishedSets = _unpublishedSets;
		if (unpublishedSets == null)
		{
			return true;
		}
		return !unpublishedSets.Contains(expansionCode);
	}

	public Dictionary<string, SetAvailability> GetSetAvailabilities()
	{
		return _availabilitiesBySetCode;
	}

	public IEnumerable<string> GetRotatingSetsStandard()
	{
		return from x in _collationsByMapping
			where x.Value.Set.Availability == SetAvailability.RotatingOutSoonStandard
			select x.Key.ToString();
	}

	public IEnumerable<string> GetRotatingSetsAlchemy()
	{
		return from x in _collationsByMapping
			where x.Value.Set.Availability == SetAvailability.RotatingOutSoonAlchemy
			select x.Key.ToString();
	}

	public IEnumerable<string> GetSetsStandardNotAlchemy()
	{
		return from x in _collationsByMapping
			where x.Value.Set.Availability == SetAvailability.StandardNotAlchemy
			select x.Key.ToString();
	}

	public IEnumerable<string> GetSetsAlchemyNotStandard()
	{
		return from x in _collationsByMapping
			where x.Value.Set.Availability == SetAvailability.AlchemyNotStandard
			select x.Key.ToString();
	}

	public static SetMetadataProvider Create()
	{
		return new SetMetadataProvider();
	}

	public SetMetadataProvider()
	{
		ColorGroup = new List<FilterElement>
		{
			new FilterElement(CardFilterType.White, SetMetadataCore.CreateSingleColorFilterNode(CardColorFlags.White)),
			new FilterElement(CardFilterType.Blue, SetMetadataCore.CreateSingleColorFilterNode(CardColorFlags.Blue)),
			new FilterElement(CardFilterType.Black, SetMetadataCore.CreateSingleColorFilterNode(CardColorFlags.Black)),
			new FilterElement(CardFilterType.Red, SetMetadataCore.CreateSingleColorFilterNode(CardColorFlags.Red)),
			new FilterElement(CardFilterType.Green, SetMetadataCore.CreateSingleColorFilterNode(CardColorFlags.Green)),
			new FilterElement(CardFilterType.Color_Colorless, SetMetadataCore.CreateColorlessFilterNode())
		};
		_collationFilters = new List<FilterElement>();
		if (Groups == null)
		{
			Groups = new List<List<FilterElement>>
			{
				ColorGroup,
				new List<FilterElement>
				{
					new FilterElement(CardFilterType.Multicolor, SetMetadataCore.CreateMulticolorFilterNode())
				},
				new List<FilterElement>
				{
					new FilterElement(CardFilterType.Cost_0, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.CMC, TokenType.Equals, 0)),
					new FilterElement(CardFilterType.Cost_1, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.CMC, TokenType.Equals, 1)),
					new FilterElement(CardFilterType.Cost_2, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.CMC, TokenType.Equals, 2)),
					new FilterElement(CardFilterType.Cost_3, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.CMC, TokenType.Equals, 3)),
					new FilterElement(CardFilterType.Cost_4, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.CMC, TokenType.Equals, 4)),
					new FilterElement(CardFilterType.Cost_5, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.CMC, TokenType.Equals, 5)),
					new FilterElement(CardFilterType.Cost_6, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.CMC, TokenType.Equals, 6)),
					new FilterElement(CardFilterType.Cost_7OrGreater, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.CMC, TokenType.GreaterThanOrEqual, 7))
				},
				new List<FilterElement>
				{
					new FilterElement(CardFilterType.Rarity_BasicLand, SetMetadataCore.CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType.Land), SetMetadataCore.CreateCardSuperTypeFilterNode(Wotc.Mtgo.Gre.External.Messaging.SuperType.Basic)),
					new FilterElement(CardFilterType.Rarity_Common, SetMetadataCore.CreateNormalizedCommonRarityCheck()),
					new FilterElement(CardFilterType.Rarity_Uncommon, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.Rarity, TokenType.Equals, 3)),
					new FilterElement(CardFilterType.Rarity_Rare, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.Rarity, TokenType.Equals, 4)),
					new FilterElement(CardFilterType.Rarity_MythicRare, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.Rarity, TokenType.Equals, 5))
				},
				new List<FilterElement>
				{
					new FilterElement(CardFilterType.Type_Creature, SetMetadataCore.CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType.Creature)),
					new FilterElement(CardFilterType.Type_Planeswalker, SetMetadataCore.CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType.Planeswalker)),
					new FilterElement(CardFilterType.Type_Instant, SetMetadataCore.CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType.Instant)),
					new FilterElement(CardFilterType.Type_Sorcery, SetMetadataCore.CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType.Sorcery)),
					new FilterElement(CardFilterType.Type_Enchantment, SetMetadataCore.CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType.Enchantment)),
					new FilterElement(CardFilterType.Type_Artifact, SetMetadataCore.CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType.Artifact)),
					new FilterElement(CardFilterType.Type_Battle, SetMetadataCore.CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType.Battle)),
					new FilterElement(CardFilterType.Type_Land, SetMetadataCore.CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType.Land))
				},
				_collationFilters,
				new List<FilterElement>
				{
					new FilterElement(CardFilterType.Collection_Collected, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.Owned, TokenType.GreaterThan, 0)),
					new FilterElement(CardFilterType.Collection_Uncollected, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.Owned, TokenType.Equals, 0))
				},
				new List<FilterElement>
				{
					new FilterElement(CardFilterType.Collection_1_Of, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.Owned, TokenType.Equals, 1)),
					new FilterElement(CardFilterType.Collection_2_Of, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.Owned, TokenType.Equals, 2)),
					new FilterElement(CardFilterType.Collection_3_Of, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.Owned, TokenType.Equals, 3)),
					new FilterElement(CardFilterType.Collection_4_Of, SetMetadataCore.CreateNumericFilterNode(CardPropertyFilter.PropertyType.Owned, TokenType.Equals, 4))
				},
				new List<FilterElement>
				{
					new FilterElement(CardFilterType.Commanders, SetMetadataCore.CreateCommandersNode())
				},
				new List<FilterElement>
				{
					new FilterElement(CardFilterType.Companions, SetMetadataCore.CreateCompanionNode())
				}
			};
		}
		SetCodeAliasesReverseMap = new Dictionary<string, List<string>>();
	}

	private void LoadSetCodeAliases(List<ClientSetMetadata> SetDatas)
	{
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (ClientSetMetadata SetData in SetDatas)
		{
			if (SetData.SetCodeAliases == null)
			{
				continue;
			}
			foreach (string setCodeAlias in SetData.SetCodeAliases)
			{
				if (!dictionary.TryGetValue(setCodeAlias, out var value))
				{
					value = (dictionary[setCodeAlias] = new List<string>());
				}
				value.Add(SetData.SetCode);
			}
		}
		SetCodeAliasesReverseMap = dictionary;
	}

	public bool TryGetSetCodeAliasTargetList(string setCode, out List<string> setCodeAliases)
	{
		return SetCodeAliasesReverseMap.TryGetValue(setCode, out setCodeAliases);
	}
}
