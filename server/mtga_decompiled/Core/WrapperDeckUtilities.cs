using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Core.Meta;
using Core.Meta.Cards;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.Enums.Deck;
using Wizards.MDN.DeckManager;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

public static class WrapperDeckUtilities
{
	private class NameItem
	{
		public string BaseName;

		public int Number;

		private static readonly Regex NUMBER_REGEX = new Regex(" \\((\\d+)\\)$");

		public NameItem(string name)
		{
			Match match = NUMBER_REGEX.Match(name);
			if (match.Success && match.Groups.Count >= 1)
			{
				BaseName = name.Substring(0, match.Index);
				if (!int.TryParse(match.Groups[1].Value, out Number))
				{
					Number = 1;
				}
			}
			else
			{
				BaseName = name;
				Number = 1;
			}
		}
	}

	private static readonly Regex IMPORT_LINE_REGEX = new Regex("^(\\d+) ([\\w\\.’'\"\\&\\,\\-\\:\\! /<>、・，…。〜「」Ⅱ＝+]*)(?:\\s*$| (\\([\\w]*\\)) ([\\w]*)$)", RegexOptions.Multiline);

	private static readonly Regex IMPORT_JPN_LINE_REGEX = new Regex("^(\\d+) ([\\w\\.：＆|（）||、]*)(?:\\s*$| (\\([\\w]*\\)) ([\\w]*)$)", RegexOptions.Multiline);

	public static void setLastPlayed(Client_Deck deck)
	{
		DecksManager decksManager = WrapperController.Instance?.DecksManager;
		if (decksManager == null)
		{
			SimpleLog.LogError("attempting to update deck while not loaded - don't do this!");
		}
		else
		{
			setLastPlayedInternal(decksManager, deck);
		}
	}

	private static void setLastPlayedInternal(DecksManager manager, Client_Deck deck)
	{
		if (!deck.Summary.IsNetDeck)
		{
			deck.Summary.LastPlayed = DateTime.Now;
			manager.UpdateDeck(deck, DeckActionType.Updated);
		}
	}

	public static void setFavorite(Client_Deck deck, bool isFavorite)
	{
		DecksManager decksManager = WrapperController.Instance?.DecksManager;
		if (decksManager == null)
		{
			SimpleLog.LogError("attempting to update deck while not loaded - don't do this!");
		}
		else
		{
			setFavoriteInternal(decksManager, deck, isFavorite);
		}
	}

	private static void setFavoriteInternal(DecksManager manager, Client_Deck deck, bool favorite)
	{
		deck.Summary.IsFavorite = favorite;
		manager.UpdateDeck(deck, DeckActionType.Updated);
	}

	public static HashSet<ManaColor> GetDeckColors(this Client_Deck deck, ICardDataProvider cardDataProvider, bool isLimited)
	{
		if (deck == null)
		{
			return new HashSet<ManaColor>();
		}
		if (deck.Contents.Piles[EDeckPile.CommandZone].Count > 0)
		{
			List<Client_DeckCard> list = new List<Client_DeckCard>(deck.Contents.Piles[EDeckPile.CommandZone]);
			list.AddRange(deck.Contents.Piles[EDeckPile.Main]);
			CollectColorIdentities(list, out var cardColors, out var landColors);
			return new HashSet<ManaColor>(cardColors.Union(landColors));
		}
		List<Client_DeckCard> list2 = new List<Client_DeckCard>(deck.Contents.Piles[EDeckPile.Main]);
		if (!isLimited)
		{
			list2.AddRange(deck.Contents.Piles[EDeckPile.Sideboard]);
		}
		CollectColorIdentities(list2, out var cardColors2, out var landColors2);
		return new HashSet<ManaColor>(cardColors2.Intersect(landColors2));
		void CollectColorIdentities(IEnumerable<Client_DeckCard> cards, out HashSet<ManaColor> reference, out HashSet<ManaColor> reference2)
		{
			reference = new HashSet<ManaColor>();
			reference2 = new HashSet<ManaColor>();
			foreach (CardPrintingData item in CollectPrintingsToInspect(cards))
			{
				HashSet<ManaColor> hashSet = (item.IsLand ? reference2 : reference);
				foreach (CardColor item2 in item.ColorIdentity)
				{
					hashSet.Add(item2.ToManaColor());
				}
			}
		}
		IEnumerable<CardPrintingData> CollectPrintingsToInspect(IEnumerable<Client_DeckCard> cards)
		{
			foreach (Client_DeckCard card in cards)
			{
				if (card.Quantity != 0)
				{
					CardPrintingData cardPrinting = cardDataProvider.GetCardPrintingById(card.Id);
					if (cardPrinting == null)
					{
						SimpleLog.LogError($"[WrapperDeckUtilities] Card {card.Id} not part of the given card printings.");
					}
					else
					{
						yield return cardPrinting;
						LinkedFace linkedFaceType = cardPrinting.LinkedFaceType;
						if (CardUtilities.IsMultifacetParent(linkedFaceType) || CardUtilities.IsModalChildFacet(linkedFaceType) || linkedFaceType == LinkedFace.SpecializeParent)
						{
							foreach (CardPrintingData linkedFacePrinting in cardPrinting.LinkedFacePrintings)
							{
								yield return linkedFacePrinting;
							}
						}
					}
				}
			}
		}
	}

	public static List<CardInDeck> ToCardInDeckList(CardCollection cardCollection)
	{
		return cardCollection.Select((ICardCollectionItem item) => new CardInDeck(item.Card.GrpId, (item.Quantity >= 0) ? ((uint)item.Quantity) : 0u)).ToList();
	}

	public static List<CardSkin> ExtractSkinsFromDeck(Deck deck)
	{
		return (from item in deck.Main.Concat(deck.Sideboard)
			where !string.IsNullOrEmpty(item.Card.SkinCode)
			select new CardSkin(item.Card.GrpId, item.Card.SkinCode, item.Card.Printing.ArtId)).Distinct().ToList();
	}

	public static Dictionary<uint, string> SkinsToOverrideLookup(List<CardSkin> skins)
	{
		return (from skin in skins
			group skin by skin.GrpId into @group
			select @group.First()).ToDictionary((CardSkin skin) => (uint)skin.GrpId, (CardSkin skin) => skin.CCV);
	}

	public static uint GetDeckBoxImage(DeckInfo deck)
	{
		uint num = deck.deckTileId;
		if (num == 0 && deck.mainDeck.Count > 0)
		{
			num = deck.mainDeck[0].Id;
		}
		return num;
	}

	public static List<DeckDisplayInfo> SortDecksBy(IEnumerable<DeckDisplayInfo> decks, string deckOrdering)
	{
		return deckOrdering switch
		{
			"MainNav/DeckManager/DeckManager_SortBy_LastPlayed" => (from di in decks
				orderby di.Deck.Summary.IsFavorite descending, di.ColorChallengeEventLock, di.IsMalformed, di.IsUncraftable, di.IsValid descending, di.Deck.Summary.LastPlayed descending, di.Deck.Summary.Name
				select di).ToList(), 
			"MainNav/DeckManager/DeckManager_SortBy_LastModified" => (from di in decks
				orderby di.Deck.Summary.IsFavorite descending, di.ColorChallengeEventLock, di.IsMalformed, di.IsUncraftable, di.IsValid descending, di.Deck.Summary.LastUpdated descending, di.Deck.Summary.Name
				select di).ToList(), 
			"MainNav/DeckManager/DeckManager_SortBy_Alphabetical" => (from di in decks
				orderby di.Deck.Summary.IsFavorite descending, di.ColorChallengeEventLock, di.IsMalformed, di.IsUncraftable, di.IsValid descending, di.Deck.Summary.Name, di.Deck.Summary.LastUpdated descending
				select di).ToList(), 
			_ => (from di in decks
				orderby di.IsMalformed, di.IsUnowned, di.IsValid descending, di.Deck.Summary.IsFavorite descending, (!(di.Deck.Summary.LastUpdated > di.Deck.Summary.LastPlayed)) ? di.Deck.Summary.LastPlayed : di.Deck.Summary.LastUpdated descending, di.Deck.Summary.Name
				select di).ToList(), 
		};
	}

	public static string GetSleeveOrDefault(string deckSleeve, IDeckSleeveProvider decksManager)
	{
		if (string.IsNullOrEmpty(deckSleeve) && decksManager != null)
		{
			return decksManager.GetDefaultSleeve();
		}
		return deckSleeve;
	}

	public static Client_Deck GetSubmitDeck(Client_Deck original, DecksManager decksManager)
	{
		if (string.IsNullOrEmpty(original.Summary.CardBack) && decksManager != null)
		{
			Client_Deck client_Deck = new Client_Deck(original);
			client_Deck.Summary.CardBack = decksManager.GetDefaultSleeve();
			return client_Deck;
		}
		return original;
	}

	public static Client_Deck UpdateDeckWithPreferredPrintings(Client_Deck inputDeck, IPreferredPrintingDataProvider preferredPrintingDataProvider, CardDatabase cardDatabase, CosmeticsProvider cosmeticsProvider)
	{
		InventoryManager inventoryManager = WrapperController.Instance.InventoryManager;
		Client_Deck client_Deck = new Client_Deck(inputDeck);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (KeyValuePair<EDeckPile, List<Client_DeckCard>> pile in client_Deck.Contents.Piles)
		{
			List<Client_DeckCard> value = pile.Value;
			List<Client_DeckCard> list = new List<Client_DeckCard>();
			int count = value.Count;
			for (int i = 0; i < count; i++)
			{
				Client_DeckCard item = value[i];
				CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(item.Id);
				PreferredPrintingWithStyle preferredPrintingForTitleId = preferredPrintingDataProvider.GetPreferredPrintingForTitleId((int)cardPrintingById.TitleId);
				List<uint> list2 = (from x in cardDatabase.DatabaseUtilities.GetPrintingsByTitleId(cardPrintingById.TitleId)
					select x.GrpId).ToList();
				list2.Sort();
				if (preferredPrintingForTitleId != null)
				{
					int value2 = 0;
					if (!dictionary.TryGetValue(preferredPrintingForTitleId.printingGrpId, out value2))
					{
						inventoryManager.Cards.TryGetValue(Convert.ToUInt32(preferredPrintingForTitleId.printingGrpId), out value2);
						value2 = (cardDatabase.CardDataProvider.GetCardPrintingById((uint)preferredPrintingForTitleId.printingGrpId).IsBasicLand ? 999 : value2);
						dictionary.Add(preferredPrintingForTitleId.printingGrpId, value2);
					}
					uint num = item.Quantity;
					if (value2 >= num)
					{
						list.Add(new Client_DeckCard((uint)preferredPrintingForTitleId.printingGrpId, item.Quantity));
						AddPreferredSkinIfPossible(preferredPrintingForTitleId, client_Deck, cosmeticsProvider, cardDatabase.CardDataProvider);
						dictionary[preferredPrintingForTitleId.printingGrpId] = value2 - (int)num;
						continue;
					}
					if (value2 > 0)
					{
						list.Add(new Client_DeckCard((uint)preferredPrintingForTitleId.printingGrpId, (uint)value2));
						AddPreferredSkinIfPossible(preferredPrintingForTitleId, client_Deck, cosmeticsProvider, cardDatabase.CardDataProvider);
						num -= (uint)value2;
						dictionary[preferredPrintingForTitleId.printingGrpId] = 0;
					}
					foreach (uint item2 in list2)
					{
						if (item2 != preferredPrintingForTitleId.printingGrpId)
						{
							inventoryManager.Cards.TryGetValue(item2, out var _);
							int value4 = 0;
							if (!dictionary.TryGetValue((int)item2, out value4))
							{
								inventoryManager.Cards.TryGetValue(item2, out value4);
								dictionary.Add((int)item2, value4);
							}
							if (value4 >= num)
							{
								list.Add(new Client_DeckCard(item2, num));
								dictionary[(int)item2] = value4 - (int)num;
								num = 0u;
								break;
							}
							if (value4 > 0)
							{
								list.Add(new Client_DeckCard(item2, (uint)value4));
								dictionary[(int)item2] = 0;
								num -= (uint)value4;
							}
						}
					}
					if (num != 0)
					{
						list.Add(new Client_DeckCard((uint)preferredPrintingForTitleId.printingGrpId, num));
					}
					continue;
				}
				if (cardPrintingById.IsBasicLandUnlimited && (!inventoryManager.Cards.TryGetValue(cardPrintingById.GrpId, out var value5) || value5 == 0))
				{
					uint num2 = list2.FirstOrDefault((uint p) => inventoryManager.Cards.ContainsKey(p));
					if (num2 != 0)
					{
						item = new Client_DeckCard(num2, item.Quantity);
					}
				}
				list.Add(item);
			}
			value.Clear();
			value.AddRange(list);
		}
		return client_Deck;
	}

	private static void AddPreferredSkinIfPossible(PreferredPrintingWithStyle preferredPrinting, Client_Deck deck, CosmeticsProvider cosmeticsProvider, ICardDataProvider cardDataProvider)
	{
		if (!string.IsNullOrEmpty(preferredPrinting.styleCode))
		{
			CardPrintingData cardPrintingById = cardDataProvider.GetCardPrintingById((uint)preferredPrinting.printingGrpId);
			if (cosmeticsProvider.TryGetOwnedArtStyles(cardPrintingById.ArtId, out var ownedStyles) && ownedStyles.Contains(preferredPrinting.styleCode))
			{
				deck.Contents.Skins.Add(new Client_CardSkin
				{
					GrpId = preferredPrinting.printingGrpId,
					CCV = preferredPrinting.styleCode
				});
			}
		}
	}

	public static string ToExportString(DeckInfo deck, IClientLocProvider localizationManager, ICardDatabaseAdapter db)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (deck.commandZone.Count > 0)
		{
			stringBuilder.AppendLine(GetCommanderLabel(localizationManager));
			ToExportString_BySection(stringBuilder, deck.commandZone, db);
			stringBuilder.AppendLine();
		}
		if (deck.companion != null)
		{
			stringBuilder.AppendLine(GetCompanionLabel(localizationManager));
			ToExportString_BySection(stringBuilder, new List<CardInDeck> { deck.companion }, db);
			stringBuilder.AppendLine();
		}
		if (deck.mainDeck.Count > 0)
		{
			stringBuilder.AppendLine(GetMainLabel(localizationManager));
			ToExportString_BySection(stringBuilder, deck.mainDeck, db);
			stringBuilder.AppendLine();
		}
		if (deck.sideboard.Count > 0)
		{
			stringBuilder.AppendLine(GetSideboardLabel(localizationManager));
			ToExportString_BySection(stringBuilder, deck.sideboard, db);
			stringBuilder.AppendLine();
		}
		return stringBuilder.ToString().TrimNewlinesFromEnd() + Environment.NewLine;
	}

	private static string GetMainLabel(IClientLocProvider localizationManager)
	{
		return localizationManager.GetLocalizedText("MainNav/DeckBuilder/Deck_Label");
	}

	private static string GetSideboardLabel(IClientLocProvider localizationManager)
	{
		return localizationManager.GetLocalizedText("MainNav/DeckBuilder/Sideboard_Label");
	}

	private static string GetCommanderLabel(IClientLocProvider localizationManager)
	{
		return localizationManager.GetLocalizedText("MainNav/DeckBuilder/Commander");
	}

	private static string GetCompanionLabel(IClientLocProvider localizationManager)
	{
		return localizationManager.GetLocalizedText("MainNav/DeckBuilder/Companion");
	}

	private static void ToExportString_BySection(StringBuilder builder, List<CardInDeck> cardCollection, ICardDatabaseAdapter db)
	{
		foreach (CardInDeck item in cardCollection)
		{
			CardPrintingData cardPrintingById = db.CardDataProvider.GetCardPrintingById(item.Id);
			builder.AppendLine($"{item.Quantity} {db.GreLocProvider.GetLocalizedText(cardPrintingById.TitleId, null, formatted: false)} ({cardPrintingById.ExpansionCode}) {cardPrintingById.CollectorNumber}");
		}
	}

	private static string ReadUntilWeHaveContents(StringReader stringReader)
	{
		string text;
		do
		{
			text = stringReader.ReadLine();
			if (text == null)
			{
				return null;
			}
		}
		while (!(text != string.Empty));
		return text;
	}

	private static bool IsEmpty(string strVal)
	{
		return ReadUntilWeHaveContents(new StringReader(strVal)) == null;
	}

	public static bool TryImportDeck(string importString, ICardDatabaseAdapter cardDatabase, ISetMetadataProvider setMetadataProvider, IClientLocProvider localizationManager, Dictionary<uint, int> cardInventory, string currentLanguage, out Client_Deck deck, out MTGALocalizedString errorMessage)
	{
		deck = null;
		errorMessage = string.Empty;
		Client_Deck client_Deck = new Client_Deck();
		if (string.IsNullOrEmpty(importString))
		{
			errorMessage = new MTGALocalizedString
			{
				Key = "SystemMessage/System_Deck_Empty_String_Error"
			};
			return false;
		}
		if (DeckImportParser.IsEmpty(importString))
		{
			errorMessage = new MTGALocalizedString
			{
				Key = "SystemMessage/System_Deck_Invalid_Section_Error"
			};
			return false;
		}
		Dictionary<string, List<string>> localizedNames = DeckImportParser.GetLocalizedNames(localizationManager, Languages.ClientLanguages, DeckImportParser.LocalizedPileMappings);
		foreach (var (text2, lines) in DeckImportParser.ParseDeckFromString(importString, localizedNames, DeckImportParser.PilesInSequence))
		{
			if (text2 == "Metadata")
			{
				Dictionary<string, List<string>> localizedNames2 = DeckImportParser.GetLocalizedNames(localizationManager, Languages.ClientLanguages, DeckImportMetadata.LocalizedKeyMappings);
				DeckImportMetadata.TryImportMetadata(lines, localizedNames2, client_Deck);
			}
			else if (!TryImportPile(text2, lines, cardDatabase, setMetadataProvider, client_Deck, cardInventory, currentLanguage, ref errorMessage))
			{
				return false;
			}
		}
		Dictionary<EDeckPile, List<Client_DeckCard>> piles = client_Deck.Contents.Piles;
		if (piles[EDeckPile.Main].Count == 0 && piles[EDeckPile.Sideboard].Count == 0 && piles[EDeckPile.CommandZone].Count == 0 && piles[EDeckPile.Companions].Count == 0)
		{
			errorMessage = new MTGALocalizedString
			{
				Key = "SystemMessage/System_Deck_Invalid_Input_Line_Error",
				Parameters = new Dictionary<string, string> { { "line", "123" } }
			};
			return false;
		}
		client_Deck.Summary.LastUpdated = DateTime.Now;
		client_Deck.UpdateWith(client_Deck.Contents);
		deck = client_Deck;
		return true;
	}

	public static string GetUniqueName(string currentName, string preferredBaseName, IEnumerable<string> existingNames)
	{
		if (string.IsNullOrWhiteSpace(currentName))
		{
			return GetUniqueName(preferredBaseName, existingNames);
		}
		return GetUniqueName(currentName.Substring(0, (currentName.Length > 50) ? 50 : currentName.Length), existingNames);
	}

	public static string GetUniqueName(string preferredBaseName, IEnumerable<string> existingNames)
	{
		NameItem nameItem = new NameItem(preferredBaseName);
		int num = 0;
		foreach (string existingName in existingNames)
		{
			NameItem nameItem2 = new NameItem(existingName);
			if (string.Compare(nameItem2.BaseName, nameItem.BaseName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				num = Math.Max(num, nameItem2.Number);
			}
		}
		if (num != 0)
		{
			return $"{nameItem.BaseName} ({num + 1})";
		}
		return nameItem.BaseName;
	}

	private static bool TryImportPile(string pileName, List<string> lines, ICardDatabaseAdapter cardDatabase, ISetMetadataProvider setMetadataProvider, Client_Deck deck, Dictionary<uint, int> cardInventory, string currentLanguage, ref MTGALocalizedString errorMessage)
	{
		List<Client_DeckCard> list = new List<Client_DeckCard>();
		Dictionary<EDeckPile, List<Client_DeckCard>> piles = deck.Contents.Piles;
		if (Enum.TryParse<EDeckPile>(pileName, out var result))
		{
			piles[result] = list;
			foreach (string line in lines)
			{
				if (!ParseLine(line, cardDatabase, setMetadataProvider, cardInventory, out errorMessage, list, result, currentLanguage))
				{
					return false;
				}
			}
			return true;
		}
		errorMessage = new MTGALocalizedString
		{
			Key = "SystemMessage/System_Deck_Invalid_Input_Line_Error",
			Parameters = new Dictionary<string, string> { { "line", pileName } }
		};
		return false;
	}

	private static bool ParseLine(string line, ICardDatabaseAdapter cardDatabase, ISetMetadataProvider setMetadataProvider, Dictionary<uint, int> cardInventory, out MTGALocalizedString errorMessage, List<Client_DeckCard> importedCards, EDeckPile activePile, string currentLanguage)
	{
		Match match = IMPORT_LINE_REGEX.Match(line);
		if (!match.Success && currentLanguage == "ja-JP")
		{
			match = IMPORT_JPN_LINE_REGEX.Match(line);
		}
		if (!match.Success)
		{
			errorMessage = new MTGALocalizedString
			{
				Key = "SystemMessage/System_Deck_Invalid_Input_Line_Error",
				Parameters = new Dictionary<string, string> { { "line", line } }
			};
			return false;
		}
		int num = int.Parse(match.Groups[1].ToString());
		string text = match.Groups[2].ToString().Replace("’", "'").TrimEnd(' ');
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Substring(0, text.Length);
		}
		string text2 = match.Groups[3].ToString();
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = text2.Substring(1, text2.Length - 2);
			if (setMetadataProvider.TryGetSetCodeAliasTargetList(text2, out var setCodeAliases) && setCodeAliases.Any())
			{
				text2 = setCodeAliases.First();
			}
			if (text2.Equals("DOM"))
			{
				text2 = "DAR";
			}
			if (text2.Equals("CON"))
			{
				text2 = "CONF";
			}
		}
		else
		{
			text2 = string.Empty;
		}
		string value = match.Groups[4].Value;
		List<CardPrintingData> cardsWithParams = GetCardsWithParams(cardDatabase, text, text2, value);
		if (cardsWithParams.Count == 0)
		{
			errorMessage = new MTGALocalizedString
			{
				Key = "SystemMessage/System_Deck_Unknown_Card_Error",
				Parameters = new Dictionary<string, string> { { "cardName", text } }
			};
			return false;
		}
		var (mTGALocalizedString, list) = MatchingCardsRefinedForPile(cardsWithParams, activePile, text);
		if (mTGALocalizedString != null)
		{
			errorMessage = mTGALocalizedString;
			return false;
		}
		CardPrintingData cardPrintingData = list[0];
		List<CardPrintingData> list2 = (from p in list.Skip(1)
			orderby p.GrpId
			select p).ToList();
		list2.Insert(0, cardPrintingData);
		List<Client_DeckCard> list3 = new List<Client_DeckCard>();
		foreach (CardPrintingData item2 in list2)
		{
			cardInventory.TryGetValue(item2.GrpId, out var value2);
			if (value2 >= item2.MaxCollected && item2.AlternateDeckLimit.HasValue)
			{
				value2 = (int)item2.AlternateDeckLimit.Value;
			}
			if (value2 > 0)
			{
				Client_DeckCard item = new Client_DeckCard(item2.GrpId, (uint)Math.Min(value2, num));
				list3.Add(item);
				num -= (int)item.Quantity;
			}
			if (num == 0)
			{
				break;
			}
		}
		if (num > 0)
		{
			if (list3.Count == 0 || list3[0].Id != cardPrintingData.GrpId)
			{
				list3.Add(new Client_DeckCard(cardPrintingData.GrpId, (uint)num));
			}
			else
			{
				list3[0] = list3[0].Add(num);
			}
		}
		if (activePile == EDeckPile.Companions && importedCards.Count == 0 && !CompanionUtil.CardCanBeCompanion(cardPrintingData))
		{
			errorMessage = new MTGALocalizedString
			{
				Key = "SystemMessage/System_Deck_Invalid_Companion_Error",
				Parameters = new Dictionary<string, string> { { "cardName", text } }
			};
			return false;
		}
		importedCards.AddRange(list3);
		errorMessage = string.Empty;
		return true;
	}

	private static IReadOnlyList<CardPrintingData> GetPrintingsByLocalizedTitle(ICardDatabaseAdapter cardDatabase, string title)
	{
		IReadOnlyList<CardPrintingData> printingsByLocalizedTitle = cardDatabase.DatabaseUtilities.GetPrintingsByLocalizedTitle(title);
		if (printingsByLocalizedTitle != null)
		{
			CardPrintingData cardPrintingData = printingsByLocalizedTitle.FirstOrDefault((CardPrintingData c) => !c.IsPrimaryCard && c.DefunctRebalancedCardLink != 0 && cardDatabase.CardDataProvider.GetCardPrintingById(c.DefunctRebalancedCardLink).IsPrimaryCard);
			if (cardPrintingData != null)
			{
				CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(cardPrintingData.DefunctRebalancedCardLink);
				return cardDatabase.DatabaseUtilities.GetPrintingsByTitleId(cardPrintingById.TitleId);
			}
		}
		return printingsByLocalizedTitle;
	}

	private static List<CardPrintingData> GetCardsWithParams(ICardDatabaseAdapter cardDatabase, string title, string expansionCode, string collectorNum)
	{
		IReadOnlyList<CardPrintingData> printingsByLocalizedTitle = GetPrintingsByLocalizedTitle(cardDatabase, title);
		IEnumerable<CardPrintingData> source;
		if (printingsByLocalizedTitle == null || printingsByLocalizedTitle.Count == 0)
		{
			source = from p in cardDatabase.DatabaseUtilities.GetPrintingsByExpansion(expansionCode)
				where p.CollectorNumber == collectorNum
				select p;
		}
		else
		{
			source = printingsByLocalizedTitle;
			if (!string.IsNullOrWhiteSpace(expansionCode) && !string.IsNullOrWhiteSpace(collectorNum))
			{
				source = from e in source
					orderby e.ExpansionCode.Equals(expansionCode) descending, e.CollectorNumber.Equals(collectorNum)
					select e;
			}
			else if (!string.IsNullOrWhiteSpace(expansionCode))
			{
				source = source.OrderByDescending((CardPrintingData e) => e.ExpansionCode.Equals(expansionCode));
			}
			else if (!string.IsNullOrWhiteSpace(collectorNum))
			{
				source = source.OrderByDescending((CardPrintingData e) => e.CollectorNumber.Equals(collectorNum));
			}
		}
		return source.Where((CardPrintingData p) => !p.IsToken && CardUtilities.CanCardExistInDeck(p)).ToList();
	}

	private static (MTGALocalizedString ErrorMessage, List<CardPrintingData> RefinedMatchingCards) MatchingCardsRefinedForPile(List<CardPrintingData> matchingCards, EDeckPile activePile, string cardName)
	{
		MTGALocalizedString item = null;
		List<CardPrintingData> list;
		if (activePile == EDeckPile.CommandZone)
		{
			list = matchingCards.Where((CardPrintingData _) => (!_.IsPrimaryCard) ? (_.LinkedFaceType == LinkedFace.SpecializeParent) : (_.LinkedFaceType != LinkedFace.SpecializeChild)).ToList();
			if (list.Count == 0)
			{
				item = new MTGALocalizedString
				{
					Key = "SystemMessage/System_Deck_Invalid_Commander_Error",
					Parameters = new Dictionary<string, string> { { "cardName", cardName } }
				};
			}
		}
		else
		{
			list = matchingCards.Where((CardPrintingData _) => _.IsPrimaryCard).ToList();
			if (list.Count == 0)
			{
				item = new MTGALocalizedString
				{
					Key = "SystemMessage/System_Deck_Invalid_Maindeck_Error",
					Parameters = new Dictionary<string, string> { { "cardName", cardName } }
				};
			}
		}
		return (ErrorMessage: item, RefinedMatchingCards: list);
	}
}
