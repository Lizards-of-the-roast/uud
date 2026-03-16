using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code.CardFilters;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga;
using Wotc.Mtga.Cards.Database;

public class StringFilter : CardPropertyFilter
{
	public PropertyType Property;

	public MTGALocalizedString Value;

	private static readonly ConcurrentDictionary<string, HashSet<uint>> LocIdsCache = new ConcurrentDictionary<string, HashSet<uint>>();

	private ICardDatabaseAdapter _cardDatabase;

	private ICardNicknamesProvider _cardNicknamesProvider;

	private ISetMetadataProvider _setMetadataProvider;

	private static readonly Dictionary<string, UnlocalizedMTGAString> TermSymbolMap = new Dictionary<string, UnlocalizedMTGAString>
	{
		{
			"ENERGY",
			new UnlocalizedMTGAString("{E")
		},
		{
			"TAP",
			new UnlocalizedMTGAString("{OT")
		},
		{
			"SNOW",
			new UnlocalizedMTGAString("{OS")
		},
		{
			"UNTAP",
			new UnlocalizedMTGAString("{OQ")
		}
	};

	public ICardDatabaseAdapter CardDatabase
	{
		get
		{
			return _cardDatabase ?? (_cardDatabase = NullCardDatabaseAdapter.Default);
		}
		set
		{
			_cardDatabase = value;
		}
	}

	public ICardNicknamesProvider CardNicknamesProvider
	{
		get
		{
			return _cardNicknamesProvider ?? (_cardNicknamesProvider = Pantry.Get<ICardNicknamesProvider>());
		}
		set
		{
			_cardNicknamesProvider = value;
		}
	}

	public ISetMetadataProvider SetMetaDataProvider
	{
		get
		{
			return _setMetadataProvider ?? (_setMetadataProvider = Pantry.Get<ISetMetadataProvider>());
		}
		set
		{
			_setMetadataProvider = value;
		}
	}

	private HashSet<uint> LocIdsForSearchTerm()
	{
		if (!LocIdsCache.TryGetValue(Value.Key, out var _))
		{
			List<string> list = new List<string>();
			if (TermSymbolMap.TryGetValue(Value, out var value2))
			{
				list.Add(value2.ToString());
			}
			LocIdsCache[Value.Key] = CardDatabase.GreLocProvider.GetLocIdsForSearchTerms(Value, list);
		}
		return LocIdsCache[Value.Key];
	}

	private bool CompareExpansionCodeWithAliases(string filterValue, string cardExpansionCode)
	{
		if (SetMetaDataProvider.TryGetSetCodeAliasTargetList(filterValue, out var setCodeAliases) && setCodeAliases.Contains(cardExpansionCode))
		{
			return true;
		}
		return filterValue.Equals(cardExpansionCode);
	}

	public override CardFilterGroup Evaluate(CardFilterGroup cards, CardMatcher.CardMatcherMetadata metadata)
	{
		switch (Property)
		{
		case PropertyType.Type:
			cards = ValueMatch_CardType(cards);
			break;
		case PropertyType.ExpansionCode:
			cards = ValueMatch_ExpansionCode(cards);
			break;
		case PropertyType.Title:
			cards = ValueMatch_Title(cards);
			break;
		case PropertyType.Text:
			cards = ValueMatch_CardRules(cards);
			break;
		case PropertyType.Power:
			cards = ValueMatch_Power(cards);
			break;
		case PropertyType.Toughness:
			cards = ValueMatch_Toughness(cards);
			break;
		case PropertyType.ManaCost:
			cards = ValueMatch_CardManaCost(cards);
			break;
		case PropertyType.Artist:
			cards = ValueMatch_Artist(cards);
			break;
		case PropertyType.Flavor:
			cards = ValueMatch_Flavor(cards);
			break;
		case PropertyType.AnyText:
			cards = ValueMatch_AnyText(cards);
			break;
		case PropertyType.Rebalanced:
			cards = ValueMatch_Rebalanced(cards);
			break;
		case PropertyType.Nickname:
			cards = ValueMatch_CardNickname(cards);
			break;
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_ExpansionCode(CardFilterGroup cards)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = CompareExpansionCodeWithAliases(Value, cards.Cards[num].Card.ExpansionCode) || Value == cards.Cards[num].Card.DigitalReleaseSet;
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.StringFilter_ExpansionCode);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_Title(CardFilterGroup cards)
	{
		bool[] results = new bool[cards.Cards.Count];
		HashSet<uint> locIdsForSearchTerm = LocIdsForSearchTerm();
		results = CheckCardTitles(cards, locIdsForSearchTerm, results);
		results = CheckCardNicknames(cards, results);
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = results[num];
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.StringFilter_Title);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_Power(CardFilterGroup cards)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = cards.Cards[num].Card.Power.RawText.Contains(Value);
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.StringFilter_Power);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_Toughness(CardFilterGroup cards)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = cards.Cards[num].Card.Toughness.RawText.Contains(Value);
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.StringFilter_Toughness);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_Artist(CardFilterGroup cards)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = cards.Cards[num].Card.ArtistCredit.IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0;
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.StringFilter_Artist);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_Flavor(CardFilterGroup cards)
	{
		HashSet<uint> hashSet = LocIdsForSearchTerm();
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = hashSet.Contains(cards.Cards[num].Card.FlavorTextId);
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.StringFilter_Flavor);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_AnyText(CardFilterGroup cards)
	{
		bool[] results = new bool[cards.Cards.Count];
		HashSet<uint> locIdsForSearchTerm = LocIdsForSearchTerm();
		results = CheckCardNicknames(cards, results);
		results = CheckCardTypes(cards, locIdsForSearchTerm, results);
		results = CheckCardRules(cards, locIdsForSearchTerm, results);
		results = CheckCardTitles(cards, locIdsForSearchTerm, results);
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = results[num];
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.StringFilter_AnyText);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_Rebalanced(CardFilterGroup cards)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			if (!(Negate ? (!cards.Cards[num].Card.IsRebalanced) : cards.Cards[num].Card.IsRebalanced))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.StringFilter_Rebalanced);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_CardRules(CardFilterGroup cards)
	{
		bool[] results = new bool[cards.Cards.Count];
		HashSet<uint> locIdsForSearchTerm = LocIdsForSearchTerm();
		results = CheckCardRules(cards, locIdsForSearchTerm, results);
		for (int i = 0; i < results.Length; i++)
		{
			if (!(Negate ? (!results[i]) : results[i]))
			{
				cards.Cards[i].FailFilter(CardFilterGroup.FilteredReason.StringFilter_Rules);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_CardType(CardFilterGroup cards)
	{
		bool[] results = new bool[cards.Cards.Count];
		HashSet<uint> locIdsForSearchTerm = LocIdsForSearchTerm();
		results = CheckCardTypes(cards, locIdsForSearchTerm, results);
		for (int i = 0; i < results.Length; i++)
		{
			if (!(Negate ? (!results[i]) : results[i]))
			{
				cards.Cards[i].FailFilter(CardFilterGroup.FilteredReason.StringFilter_Type);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_CardManaCost(CardFilterGroup cards)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			bool flag = false;
			if (cards.Cards[num].Card.CastingCostText.IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				flag = true;
			}
			else
			{
				foreach (CardPrintingData linkedFacePrinting in cards.Cards[num].Card.LinkedFacePrintings)
				{
					if (linkedFacePrinting.CastingCostText.IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						flag = true;
						break;
					}
				}
			}
			if (!(Negate ? (!flag) : flag))
			{
				cards.Cards[num].FailFilter(CardFilterGroup.FilteredReason.StringFilter_ManaCost);
			}
		}
		return cards;
	}

	private CardFilterGroup ValueMatch_CardNickname(CardFilterGroup cards)
	{
		bool[] results = new bool[cards.Cards.Count];
		results = CheckCardNicknames(cards, results);
		for (int i = 0; i < results.Length; i++)
		{
			if (!(Negate ? (!results[i]) : results[i]))
			{
				cards.Cards[i].FailFilter(CardFilterGroup.FilteredReason.StringFilter_Nickname);
			}
		}
		return cards;
	}

	private bool TypesContainTextFragment(CardPrintingData card, HashSet<uint> locIdsForSearchTerm)
	{
		if (card.TypeTextId != 0 && locIdsForSearchTerm.Contains(card.TypeTextId))
		{
			return true;
		}
		if (card.SubtypeTextId != 0 && locIdsForSearchTerm.Contains(card.SubtypeTextId))
		{
			return true;
		}
		return false;
	}

	private bool[] CheckCardTitles(CardFilterGroup cards, HashSet<uint> locIdsForSearchTerm, bool[] results)
	{
		List<int> list = new List<int>(results.Length);
		List<uint> list2 = new List<uint>(results.Length);
		for (int i = 0; i < results.Length; i++)
		{
			if (!results[i])
			{
				list.Add(i);
				list2.Add(cards.Cards[i].Card.GrpId);
			}
		}
		List<HashSet<uint>> altTitleIds;
		bool[] array = AltPrintingUtilities.TryGetBatchedAltTitleIds(list2, _cardDatabase.AltPrintingProvider, _cardDatabase.CardDataProvider, out altTitleIds);
		for (int j = 0; j < list.Count; j++)
		{
			int num = list[j];
			CardPrintingData card = cards.Cards[num].Card;
			bool flag = locIdsForSearchTerm.Contains(card.TitleId) || locIdsForSearchTerm.Contains(card.AltTitleId) || locIdsForSearchTerm.Contains(card.InterchangeableTitleId);
			if (!flag)
			{
				foreach (CardPrintingData linkedFacePrinting in card.LinkedFacePrintings)
				{
					if (locIdsForSearchTerm.Contains(linkedFacePrinting.TitleId))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag && array[j])
			{
				foreach (uint item in altTitleIds[j])
				{
					if (locIdsForSearchTerm.Contains(item))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				results[num] = true;
			}
		}
		return results;
	}

	private bool[] CheckCardNicknames(CardFilterGroup cards, bool[] results)
	{
		for (int i = 0; i < results.Length; i++)
		{
			if (!results[i] && CardNicknamesProvider.GetTitleIdsForNickname(Value).Contains(cards.Cards[i].Card.TitleId))
			{
				results[i] = true;
			}
		}
		return results;
	}

	private bool[] CheckCardTypes(CardFilterGroup cards, HashSet<uint> locIdsForSearchTerm, bool[] results)
	{
		for (int i = 0; i < cards.Cards.Count; i++)
		{
			if (results[i])
			{
				continue;
			}
			bool flag = TypesContainTextFragment(cards.Cards[i].Card, locIdsForSearchTerm);
			if (!flag)
			{
				foreach (CardPrintingData linkedFacePrinting in cards.Cards[i].Card.LinkedFacePrintings)
				{
					if (TypesContainTextFragment(linkedFacePrinting, locIdsForSearchTerm))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				results[i] = true;
			}
		}
		return results;
	}

	private bool[] CheckCardRules(CardFilterGroup cards, HashSet<uint> locIdsForSearchTerm, bool[] results)
	{
		for (int num = cards.Cards.Count - 1; num >= 0; num--)
		{
			if (!results[num])
			{
				bool flag = CheckPrintingAbilitiesForText(cards.Cards[num].Card.AbilityIds);
				if (!flag)
				{
					IReadOnlyList<CardPrintingData> linkedFacePrintings = cards.Cards[num].Card.LinkedFacePrintings;
					for (int i = 0; i < linkedFacePrintings.Count; i++)
					{
						if (CheckPrintingAbilitiesForText(linkedFacePrintings[i].AbilityIds))
						{
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					results[num] = true;
				}
			}
		}
		return results;
		bool CheckPrintingAbilitiesForText(IReadOnlyList<(uint Id, uint TextId)> abilities)
		{
			for (int j = 0; j < abilities.Count; j++)
			{
				if (locIdsForSearchTerm.Contains(abilities[j].TextId))
				{
					return true;
				}
			}
			return false;
		}
	}
}
