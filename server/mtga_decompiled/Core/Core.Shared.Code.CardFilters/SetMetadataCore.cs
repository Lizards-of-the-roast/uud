using System.Collections.Generic;
using System.Linq;
using Core.Code.Collations;
using Core.Meta.MainNavigation.Store.Data;
using Wizards.Arena.Enums.Card;
using Wotc.Mtga.Wrapper;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Core.Shared.Code.CardFilters;

public class SetMetadataCore
{
	public static List<FilterElement> ToFilterElements(List<ClientSetMetadata> setMetadatas)
	{
		IEnumerable<(string SetCode, CardFilterType CardFilterType)> first = from set in setMetadatas
			where set.CardFilterType != CardFilterType.None
			select (SetCode: set.SetCode, CardFilterType: set.CardFilterType);
		IEnumerable<(string, CardFilterType)> second = from col in setMetadatas.SelectMany((ClientSetMetadata col) => col.Collations)
			where col.CardFilterType != CardFilterType.None
			select (SetCode: col.SetCode, CardFilterType: col.CardFilterType);
		IEnumerable<(string, CardFilterType)> source = first.Concat<(string, CardFilterType)>(second);
		HashSet<string> foundFilters = new HashSet<string>();
		return (from tuple in source
			where foundFilters.Add(tuple.Item1)
			select ToFilterElement(tuple.Item1, tuple.Item2)).ToList();
	}

	private static FilterElement ToFilterElement(string setCode, CardFilterType cardFilterType)
	{
		return new FilterElement(cardFilterType, CreateStringFilterNode(CardPropertyFilter.PropertyType.ExpansionCode, new UnlocalizedMTGAString
		{
			Key = setCode
		}));
	}

	public static Dictionary<string, List<CardFilterType>> MapFromSetGroups(List<ClientSetGroup> groups)
	{
		Dictionary<string, List<CardFilterType>> dictionary = new Dictionary<string, List<CardFilterType>>();
		foreach (ClientSetGroup group in groups)
		{
			UpdateFilterForGroup(group, dictionary);
		}
		return dictionary;
	}

	public static Dictionary<string, SetAvailability> MapAvailabilitiesFromSetData(List<ClientSetMetadata> setDatas)
	{
		Dictionary<string, SetAvailability> dictionary = new Dictionary<string, SetAvailability>();
		foreach (ClientSetMetadata setData in setDatas)
		{
			dictionary[setData.SetCode] = setData.Availability;
		}
		return dictionary;
	}

	private static void UpdateFilterForGroup(ClientSetGroup group, Dictionary<string, List<CardFilterType>> retVal)
	{
		List<CardFilterType> list = new List<CardFilterType>();
		ExpansionsToFilters(group?.Sets, list);
		retVal[group.GroupName] = list;
	}

	private static ClientSetGroup GroupForName(string groupName, List<ClientSetGroup> groups)
	{
		return groups.Find((ClientSetGroup x) => x.GroupName == groupName);
	}

	public static void ExpansionsToFilters(List<string> sets, List<CardFilterType> filterTypes)
	{
		if (sets == null)
		{
			return;
		}
		foreach (string set in sets)
		{
			CardFilterType cardFilterType = CardFilterTypeUtils.FromCollationCode(set);
			if (cardFilterType != CardFilterType.None)
			{
				filterTypes.Add(cardFilterType);
			}
		}
	}

	public static List<StoreSetFilterModel> ToStoreSetFilterModelsForStorePackGroups(List<ClientStorePackGroup> packGroups, Dictionary<CollationMapping, ClientSetCollation> collations)
	{
		return packGroups.Select((ClientStorePackGroup packGroup) => ToStoreSetFilterModelForStorePackGroup(packGroup, collations)).ToList();
	}

	private static StoreSetFilterModel ToStoreSetFilterModelForStorePackGroup(ClientStorePackGroup packGroup, Dictionary<CollationMapping, ClientSetCollation> collations)
	{
		return new StoreSetFilterModel
		{
			SetSymbol = packGroup.SetSymbol,
			Sets = packGroup.Sets,
			Tags = packGroup.Tags,
			Availability = AvailabilityForPackGroup(packGroup, collations)
		};
	}

	private static SetAvailability AvailabilityForPackGroup(ClientStorePackGroup packGroup, Dictionary<CollationMapping, ClientSetCollation> collations)
	{
		SetAvailability result = SetAvailability.EternalOnly;
		if (packGroup.Sets.Count > 0)
		{
			CollationMapping key = packGroup.Sets.OrderBy((CollationMapping x) => (int)x).FirstOrDefault();
			if (collations.TryGetValue(key, out var value))
			{
				return value.Set.Availability;
			}
		}
		return result;
	}

	public static Dictionary<CardFilterType, string> CreateCardFiltersMap(List<ClientSetMetadata> setDatas)
	{
		Dictionary<CardFilterType, string> dictionary = new Dictionary<CardFilterType, string>();
		foreach (ClientSetMetadata setData in setDatas)
		{
			AddCardFilterToMap(dictionary, setData.CardFilterType, setData.SetCode);
			foreach (ClientSetCollation collation in setData.Collations)
			{
				AddCardFilterToMap(dictionary, collation.CardFilterType, collation.SetCode);
			}
		}
		return dictionary;
	}

	private static void AddCardFilterToMap(Dictionary<CardFilterType, string> cardFilterMap, CardFilterType filterType, string setCode)
	{
		if (filterType != CardFilterType.None)
		{
			cardFilterMap[filterType] = setCode;
		}
	}

	public static ReqTerm CreateNumericFilterNode(CardPropertyFilter.PropertyType property, TokenType op, int val)
	{
		return new ReqTerm
		{
			Req = new NumericFilter
			{
				Operator = op,
				Property = property,
				Value = val
			}
		};
	}

	public static ReqTerm CreateNormalizedCommonRarityCheck()
	{
		return new ReqTerm
		{
			Req = new NormalizedCommon()
		};
	}

	public static ReqTerm CreateStringFilterNode(CardPropertyFilter.PropertyType property, MTGALocalizedString val)
	{
		return new ReqTerm
		{
			Req = new StringFilter
			{
				Property = property,
				Value = val
			}
		};
	}

	public static ReqTerm CreateSingleColorFilterNode(CardColorFlags color)
	{
		return new ReqTerm
		{
			Req = new ColorFilter
			{
				ColorFlags = color,
				Operator = TokenType.GreaterThanOrEqual,
				Property = CardPropertyFilter.PropertyType.ColorIdentity,
				Type = ColorFilter.ColorFilterType.Flags
			}
		};
	}

	public static ReqTerm CreateColorlessFilterNode()
	{
		return new ReqTerm
		{
			Req = new ColorFilter
			{
				Operator = TokenType.GreaterThanOrEqual,
				Property = CardPropertyFilter.PropertyType.ColorIdentity,
				Type = ColorFilter.ColorFilterType.Colorless
			}
		};
	}

	public static ReqTerm CreateMulticolorFilterNode()
	{
		return new ReqTerm
		{
			Req = new ColorFilter
			{
				Operator = TokenType.GreaterThanOrEqual,
				Property = CardPropertyFilter.PropertyType.ColorIdentity,
				Type = ColorFilter.ColorFilterType.Gold
			}
		};
	}

	public static ReqTerm CreateCardTypeFilterNode(Wizards.Arena.Enums.Card.CardType cardType)
	{
		return new ReqTerm
		{
			Req = new CardTypeFilter(cardType)
		};
	}

	public static ReqTerm CreateCardSuperTypeFilterNode(Wotc.Mtgo.Gre.External.Messaging.SuperType superType)
	{
		return new ReqTerm
		{
			Req = new SuperTypeFilter(superType)
		};
	}

	private static ReqTerm CreateTraditionalBasicLandNode()
	{
		return new ReqTerm
		{
			Req = new TraitFilter
			{
				Trait = TraitFilter.TraitFilterType.TraditionalBasicLand
			}
		};
	}

	public static ReqTerm CreateCommandersNode()
	{
		return new ReqTerm
		{
			Req = new TraitFilter
			{
				Trait = TraitFilter.TraitFilterType.Commanders
			}
		};
	}

	public static ReqTerm CreateCompanionNode()
	{
		return new ReqTerm
		{
			Req = new TraitFilter
			{
				Trait = TraitFilter.TraitFilterType.Companions
			}
		};
	}

	public static ReqTerm CreateRebalancedNode()
	{
		return new ReqTerm
		{
			Req = new TraitFilter
			{
				Trait = TraitFilter.TraitFilterType.Rebalanced
			}
		};
	}
}
