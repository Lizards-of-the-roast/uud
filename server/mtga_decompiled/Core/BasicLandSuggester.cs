using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Code.Decks;
using Core.Shared.Code.Providers;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

public static class BasicLandSuggester
{
	public struct LandForDeckSize
	{
		public readonly int DeckSize;

		public readonly int LandToAdd;

		public LandForDeckSize(int deckSize, int landToAdd)
		{
			DeckSize = deckSize;
			LandToAdd = landToAdd;
		}
	}

	private class ColorItem
	{
		public int NumConsumers;

		public int NonbasicSources;

		public int DesiredTotalSources;

		public int BasicLandsToAdd;

		public int DesiredBasicLandsToAdd => Math.Max(0, DesiredTotalSources - NonbasicSources);
	}

	private static readonly List<LandForDeckSize> LandForDeckSizes = new List<LandForDeckSize>
	{
		new LandForDeckSize(40, 17),
		new LandForDeckSize(60, 24),
		new LandForDeckSize(100, 40)
	};

	public static void SuggestLand(bool keepExistingUnlimitedBasicLands = true)
	{
		CardDatabase cardDatabase = Pantry.Get<CardDatabase>();
		DeckBuilderModel model = Pantry.Get<DeckBuilderModelProvider>().Model;
		DeckBuilderContext context = Pantry.Get<DeckBuilderContextProvider>().Context;
		InventoryManager inventoryManager = Pantry.Get<InventoryManager>();
		IEmergencyCardBansProvider emergencyCardBansProvider = Pantry.Get<IEmergencyCardBansProvider>();
		CosmeticsProvider cosmeticsProvider = Pantry.Get<CosmeticsProvider>();
		CompanionUtil companionUtil = Pantry.Get<CompanionUtil>();
		IPreferredPrintingDataProvider preferredPrintingDataProvider = Pantry.Get<IPreferredPrintingDataProvider>();
		IReadOnlyList<CardPrintingQuantity> filteredMainDeck = model.GetFilteredMainDeck();
		IReadOnlyList<CardPrintingQuantity> filteredCommandZone = model.GetFilteredCommandZone();
		IEnumerable<CardPrintingQuantity> cards;
		if (filteredCommandZone == null)
		{
			IEnumerable<CardPrintingQuantity> enumerable = filteredMainDeck;
			cards = enumerable;
		}
		else
		{
			cards = filteredMainDeck.Concat(filteredCommandZone);
		}
		Dictionary<ManaColor, uint> dictionary = Calculate(cards, context.Format);
		bool wastesAreLegal = !context.Format.IsCardBanned(3142u) && context.Format.IsCardLegal(3142u) && !emergencyCardBansProvider.IsTitleIdEmergencyBanned(3142u);
		(Dictionary<ManaColor, List<uint>> BasicsInDeck, List<uint> LandsToRemove) tuple = DetermineBasicLandsToRemove(inventoryManager, filteredMainDeck, keepExistingUnlimitedBasicLands, wastesAreLegal);
		var (dictionary2, _) = tuple;
		foreach (uint item in tuple.LandsToRemove)
		{
			model.RemoveCardFromMainDeck(item);
		}
		foreach (KeyValuePair<ManaColor, uint> item2 in dictionary)
		{
			item2.Deconstruct(out var key, out var value);
			ManaColor suggestionColor = key;
			uint num = value;
			CardPrintingData cardPrintingData = null;
			List<uint> list = dictionary2[suggestionColor];
			for (int i = 0; i < num; i++)
			{
				if (i < list.Count)
				{
					model.AddCardToMainDeck(list[i]);
					continue;
				}
				if (list.Count > 0)
				{
					model.AddCardToMainDeck(list[list.Count - 1]);
					continue;
				}
				if (cardPrintingData == null)
				{
					cardPrintingData = cardDatabase.DatabaseUtilities.GetPrimaryPrintings().LastOrDefault((CardPrintingData kvp) => kvp.IsBasicLandUnlimited && kvp.ColorIdentity.FirstOrDefault().ToManaColor() == suggestionColor && inventoryManager.Cards.TryGetValue(kvp.GrpId, out var value2) && value2 > 0);
				}
				if (cardPrintingData == null && suggestionColor == ManaColor.None)
				{
					if (context.IsConstructed)
					{
						cardPrintingData = cardDatabase.DatabaseUtilities.GetPrimaryPrintings().LastOrDefault((CardPrintingData kvp) => kvp.TitleId == 3142 && wastesAreLegal && inventoryManager.Cards.TryGetValue(kvp.GrpId, out var value2) && value2 > 0);
					}
					if (cardPrintingData == null)
					{
						cardPrintingData = cardDatabase.DatabaseUtilities.GetPrimaryPrintings().LastOrDefault((CardPrintingData kvp) => kvp.IsBasicLandUnlimited && kvp.ColorIdentity.FirstOrDefault().ToManaColor() == ManaColor.White && inventoryManager.Cards.TryGetValue(kvp.GrpId, out var value2) && value2 > 0);
					}
				}
				if (cardPrintingData == null)
				{
					continue;
				}
				PreferredPrintingWithStyle preferredPrintingForTitleId = preferredPrintingDataProvider.GetPreferredPrintingForTitleId((int)cardPrintingData.TitleId);
				if (preferredPrintingForTitleId == null)
				{
					model.AddCardToMainDeck(cardPrintingData.GrpId);
					continue;
				}
				model.AddCardToMainDeck((uint)preferredPrintingForTitleId.printingGrpId);
				if (preferredPrintingForTitleId != null && preferredPrintingForTitleId.styleCode != null)
				{
					CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById((uint)preferredPrintingForTitleId.printingGrpId);
					if (DeckBuilderWidgetUtilities.OwnsOrHasSkinInPool(cosmeticsProvider, model, cardDatabase, cardPrintingById.ArtId, preferredPrintingForTitleId.styleCode))
					{
						model.SetCardSkin((uint)preferredPrintingForTitleId.printingGrpId, preferredPrintingForTitleId.styleCode);
					}
				}
			}
		}
		model.UpdateMainDeck();
		companionUtil.UpdateValidation(model, context.Format);
		WrapperDeckBuilder.CacheDeck(model, context);
	}

	public static int GetTotalLandCount(int minDeckSize)
	{
		return GetTotalLandCount(minDeckSize, LandForDeckSizes);
	}

	public static int GetTotalLandCount(int minDeckSize, List<LandForDeckSize> deckSizes)
	{
		LandForDeckSize landForDeckSize = deckSizes[0];
		foreach (LandForDeckSize deckSize in deckSizes)
		{
			if (deckSize.DeckSize <= minDeckSize)
			{
				landForDeckSize = deckSize;
				continue;
			}
			return landForDeckSize.LandToAdd;
		}
		return landForDeckSize.LandToAdd;
	}

	private static int GetDesiredSources(DeckFormat format, int numConsumers)
	{
		if (numConsumers <= 0)
		{
			return 0;
		}
		if (format.FormatType == MDNEFormatType.Constructed)
		{
			switch (numConsumers)
			{
			case 1:
				return 3;
			case 2:
				return 4;
			case 3:
			case 4:
				return 5;
			case 5:
				return 6;
			case 6:
			case 7:
				return 7;
			case 8:
			case 9:
				return 8;
			case 10:
			case 11:
				return 9;
			case 12:
			case 13:
				return 10;
			case 14:
			case 15:
			case 16:
				return 12;
			case 17:
			case 18:
			case 19:
				return 13;
			case 20:
			case 21:
			case 22:
			case 23:
				return 14;
			default:
				return 15;
			}
		}
		switch (numConsumers)
		{
		case 1:
			return 3;
		case 2:
			return 4;
		case 3:
		case 4:
			return 5;
		case 5:
			return 6;
		case 6:
		case 7:
			return 7;
		case 8:
		case 9:
			return 8;
		default:
			return 9;
		}
	}

	public static ManaColor? SuggestibleLandManaColor(CardPrintingData printing, bool wastesAreLegal)
	{
		if (printing.IsBasicLandUnlimited && printing.ColorIdentity.Count == 1)
		{
			return printing.ColorIdentity[0].ToManaColor();
		}
		if (wastesAreLegal && printing.TitleId == 3142)
		{
			return ManaColor.None;
		}
		return null;
	}

	public static ManaColor ToManaColor(this CardColor cardColor)
	{
		return cardColor switch
		{
			CardColor.White => ManaColor.White, 
			CardColor.Blue => ManaColor.Blue, 
			CardColor.Black => ManaColor.Black, 
			CardColor.Red => ManaColor.Red, 
			CardColor.Green => ManaColor.Green, 
			_ => ManaColor.None, 
		};
	}

	private static bool IsSuggestibleLand(CardPrintingData cardData)
	{
		if (cardData.IsBasicLand && cardData.Supertypes.Count == 1 && cardData.Types.Count == 1 && cardData.Subtypes.Count <= 1)
		{
			return true;
		}
		return false;
	}

	public static Dictionary<ManaColor, uint> Calculate(IEnumerable<CardPrintingQuantity> cards, DeckFormat format)
	{
		int num = 0;
		List<ManaQuantity> list = new List<ManaQuantity>();
		Dictionary<ManaColor, ColorItem> dictionary = new Dictionary<ManaColor, ColorItem>
		{
			{
				ManaColor.White,
				new ColorItem()
			},
			{
				ManaColor.Blue,
				new ColorItem()
			},
			{
				ManaColor.Black,
				new ColorItem()
			},
			{
				ManaColor.Red,
				new ColorItem()
			},
			{
				ManaColor.Green,
				new ColorItem()
			}
		};
		ColorItem colorItem = new ColorItem();
		foreach (CardPrintingQuantity card in cards)
		{
			if (IsSuggestibleLand(card.Printing))
			{
				continue;
			}
			if (card.Printing.Types.Contains(CardType.Land))
			{
				num += (int)card.Quantity;
				foreach (ManaColor item in ParseManaSourceAbilities(card))
				{
					if ((uint)(item - 1) <= 4u)
					{
						dictionary[item].NonbasicSources += (int)card.Quantity;
					}
				}
				continue;
			}
			foreach (ManaQuantity item2 in card.Printing.CastingCost)
			{
				if (IsBasicLandColor(item2.Color))
				{
					if (item2.Hybrid)
					{
						if (IsBasicLandColor(item2.AltColor))
						{
							for (int i = 0; i < card.Quantity; i++)
							{
								list.Add(item2);
							}
						}
					}
					else
					{
						dictionary[item2.Color].NumConsumers += (int)card.Quantity;
					}
				}
				else
				{
					colorItem.NumConsumers += (int)card.Quantity;
				}
			}
		}
		foreach (ManaQuantity item3 in list)
		{
			ManaColor key = ((dictionary[item3.AltColor].NumConsumers > dictionary[item3.Color].NumConsumers) ? item3.AltColor : item3.Color);
			dictionary[key].NumConsumers++;
		}
		if (!dictionary.Values.Any((ColorItem colorItem2) => colorItem2.NumConsumers > 0))
		{
			if (colorItem.NumConsumers <= 0)
			{
				return new Dictionary<ManaColor, uint>();
			}
			colorItem.NonbasicSources = dictionary.Values.Select((ColorItem _) => _.NonbasicSources).Sum();
			dictionary = new Dictionary<ManaColor, ColorItem> { 
			{
				ManaColor.None,
				colorItem
			} };
		}
		foreach (ColorItem value in dictionary.Values)
		{
			value.DesiredTotalSources = GetDesiredSources(format, value.NumConsumers);
			value.BasicLandsToAdd = value.DesiredBasicLandsToAdd;
		}
		int totalLandCount = GetTotalLandCount(format.MinMainDeckCards);
		int num2 = Math.Max(0, totalLandCount - num);
		int num3 = dictionary.Values.Sum((ColorItem colorItem2) => colorItem2.DesiredBasicLandsToAdd);
		if (num3 > num2)
		{
			List<ColorItem> list2 = (from colorItem2 in dictionary.Values
				orderby colorItem2.DesiredTotalSources descending, colorItem2.NumConsumers
				select colorItem2).ToList();
			int num4 = 0;
			while (num3 > num2)
			{
				int num5 = (list2.Any((ColorItem colorItem2) => colorItem2.BasicLandsToAdd > 1) ? 1 : 0);
				if (list2[num4].BasicLandsToAdd > num5)
				{
					list2[num4].BasicLandsToAdd--;
					num3--;
				}
				if (++num4 == list2.Count)
				{
					num4 = 0;
				}
			}
		}
		else if (num3 < num2)
		{
			List<ColorItem> list3 = (from colorItem2 in dictionary.Values
				orderby colorItem2.DesiredTotalSources, colorItem2.NumConsumers descending
				select colorItem2).ToList();
			int num6 = 0;
			while (num3 < num2)
			{
				if (list3[num6].DesiredTotalSources > 0)
				{
					list3[num6].BasicLandsToAdd++;
					num3++;
				}
				if (++num6 == list3.Count)
				{
					num6 = 0;
				}
			}
		}
		return dictionary.ToDictionary((KeyValuePair<ManaColor, ColorItem> kvp) => kvp.Key, (KeyValuePair<ManaColor, ColorItem> kvp) => (uint)kvp.Value.BasicLandsToAdd);
	}

	private static bool IsBasicLandColor(ManaColor color)
	{
		if (color != ManaColor.White && color != ManaColor.Blue && color != ManaColor.Black && color != ManaColor.Red)
		{
			return color == ManaColor.Green;
		}
		return true;
	}

	public static Dictionary<ManaColor, uint> Calculate_OLD(List<CardPrintingQuantity> cards, DeckFormat format)
	{
		Dictionary<ManaColor, uint> dictionary = new Dictionary<ManaColor, uint>();
		Dictionary<ManaColor, uint> dictionary2 = new Dictionary<ManaColor, uint>();
		uint num = 0u;
		uint num2 = 0u;
		foreach (CardPrintingQuantity card in cards)
		{
			if (card.Printing.Types.Contains(CardType.Land))
			{
				if (!card.Printing.Supertypes.Contains(SuperType.Basic))
				{
					num += card.Quantity;
				}
				continue;
			}
			foreach (ManaQuantity item in card.Printing.CastingCost)
			{
				if (!item.Hybrid && IsBasicLandColor(item.Color))
				{
					dictionary.TryGetValue(item.Color, out var value);
					uint num3 = item.Count * card.Quantity;
					dictionary[item.Color] = value + num3;
					num2 += num3;
				}
			}
		}
		if ((float)num2 <= 0f)
		{
			return new Dictionary<ManaColor, uint>();
		}
		uint num4 = Math.Max(0u, (uint)GetTotalLandCount(format.MinMainDeckCards) - num);
		uint num5 = num4;
		foreach (KeyValuePair<ManaColor, uint> item2 in dictionary)
		{
			uint num6 = (uint)((float)item2.Value / (float)num2 * (float)num4);
			dictionary2.Add(item2.Key, num6);
			num5 -= num6;
		}
		List<KeyValuePair<ManaColor, uint>> list = dictionary.OrderBy((KeyValuePair<ManaColor, uint> x) => x.Value).ToList();
		for (int num7 = 0; num7 < num5; num7++)
		{
			dictionary2[list[num7 % list.Count].Key]++;
		}
		return dictionary2;
	}

	public static (Dictionary<ManaColor, List<uint>> BasicsInDeck, List<uint> LandsToRemove) DetermineBasicLandsToRemove(IInventoryManager inventoryManager, IReadOnlyList<CardPrintingQuantity> deck, bool keepExistingUnlimitedBasicLands, bool wastesAreLegal)
	{
		Dictionary<ManaColor, List<uint>> dictionary = new Dictionary<ManaColor, List<uint>>
		{
			{
				ManaColor.White,
				new List<uint>()
			},
			{
				ManaColor.Blue,
				new List<uint>()
			},
			{
				ManaColor.Black,
				new List<uint>()
			},
			{
				ManaColor.Red,
				new List<uint>()
			},
			{
				ManaColor.Green,
				new List<uint>()
			},
			{
				ManaColor.None,
				new List<uint>()
			}
		};
		List<uint> list = new List<uint>();
		foreach (CardPrintingQuantity item in deck)
		{
			ManaColor? manaColor = SuggestibleLandManaColor(item.Printing, wastesAreLegal);
			if (!manaColor.HasValue)
			{
				continue;
			}
			if (keepExistingUnlimitedBasicLands)
			{
				inventoryManager.Cards.TryGetValue(item.Printing.GrpId, out var value);
				if (value > 0)
				{
					for (int i = 0; i < item.Quantity; i++)
					{
						dictionary[manaColor.Value].Add(item.Printing.GrpId);
					}
				}
			}
			for (int j = 0; j < item.Quantity; j++)
			{
				list.Add(item.Printing.GrpId);
			}
		}
		return (BasicsInDeck: dictionary, LandsToRemove: list);
	}

	public static bool PrintingIsBasicLandOrWaste(CardData cardData)
	{
		if (!cardData.Printing.IsBasicLandUnlimited)
		{
			if (cardData.Printing.IsBasicLand)
			{
				return cardData.Printing.ColorIdentityFlags == CardColorFlags.None;
			}
			return false;
		}
		return true;
	}

	private static List<ManaColor> ParseManaSourceAbilities(CardPrintingQuantity card)
	{
		List<ManaColor> list = new List<ManaColor>();
		if (card.Printing.Supertypes.Contains(SuperType.Basic) && card.Printing.Supertypes.Contains(SuperType.Snow) && card.Printing.ColorIdentity.Count > 0)
		{
			list.Add(card.Printing.ColorIdentity[0].ToManaColor());
			return list;
		}
		IAbilityTextProvider abilityTextProvider = Pantry.Get<CardDatabase>().AbilityTextProvider;
		List<uint> abilityIds = card.Printing.AbilityIds.Select(((uint Id, uint TextId) a) => a.Id).ToList();
		foreach (AbilityPrintingData ability in card.Printing.Abilities)
		{
			string abilityTextByCardAbilityGrpId = abilityTextProvider.GetAbilityTextByCardAbilityGrpId(card.Printing.GrpId, ability.Id, abilityIds, 0u, null, formatted: false);
			if (!abilityTextByCardAbilityGrpId.Contains(":"))
			{
				continue;
			}
			string input = abilityTextByCardAbilityGrpId.Split(":")[1];
			if (ability.Category != AbilityCategory.Activated || ability.SubCategory != AbilitySubCategory.Mana)
			{
				continue;
			}
			Match[] array = new Regex("\\{([^}]*)\\}").Matches(input).ToArray();
			foreach (Match obj in array)
			{
				if (obj.ToString().Contains("oW"))
				{
					list.Add(ManaColor.White);
				}
				if (obj.ToString().Contains("oU"))
				{
					list.Add(ManaColor.Blue);
				}
				if (obj.ToString().Contains("oB"))
				{
					list.Add(ManaColor.Black);
				}
				if (obj.ToString().Contains("oR"))
				{
					list.Add(ManaColor.Red);
				}
				if (obj.ToString().Contains("oG"))
				{
					list.Add(ManaColor.Green);
				}
			}
		}
		return list;
	}
}
