using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

public class DeckCollection
{
	private Dictionary<string, DeckCollectionDeck> _decks = new Dictionary<string, DeckCollectionDeck>();

	public const string FallbackDeck = "FallbackDeck";

	public const string FallbackDeckWithSideboard = "FallbackDeck (w/ SB)";

	public const int NumFallbackDecks = 2;

	public const string TARGETFIELD_MAINDECK = "MainDeck";

	public const string TARGETFIELD_DECK = "Deck";

	public const string TARGETFIELD_SIDEBOARD = "Sideboard";

	public const string TARGETFIELD_COMMANDER = "Commander";

	public const string TARGETFIELD_MUSICIANS = "Musicians";

	public const string TARGETFIELD_COMPANION = "Companion";

	public DeckCollection(ICardDatabaseAdapter db)
	{
		if (!AssetBundleManager.AssetBundlesActive)
		{
			return;
		}
		foreach (string item in AssetLoader.GetFilePathsForAssetType("Deck"))
		{
			using StreamReader streamReader = FileSystemUtils.OpenText(item);
			string text = streamReader.ReadToEnd();
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
			if (CreateDeckFromText(db, text, out var deck))
			{
				_decks.Add(fileNameWithoutExtension, deck);
				continue;
			}
			Debug.LogWarningFormat("Deck \"{0}\" had one or more invalid cards, it will not be loaded.", item);
		}
	}

	public DeckCollection(ICardDatabaseAdapter db, string collectionsPath)
	{
		Initialize(db, collectionsPath);
	}

	private void Initialize(ICardDatabaseAdapter db, string collectionsPath, bool toLowerDeckNames = false)
	{
		if (CreateDeckFromText(db, "60 66505", out var deck))
		{
			_decks.Add("FallbackDeck", deck);
		}
		if (CreateDeckFromText(db, "60 66505", out var deck2))
		{
			for (int i = 0; i < 15; i++)
			{
				deck2.sideboardCards.Add(66505u);
			}
			_decks.Add("FallbackDeck (w/ SB)", deck2);
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(collectionsPath);
		if (directoryInfo.Exists)
		{
			FileInfo[] files = directoryInfo.GetFiles("*.txt");
			foreach (FileInfo fileInfo in files)
			{
				string text = Path.GetFileNameWithoutExtension(fileInfo.FullName);
				if (CreateDeckFromText(db, File.ReadAllText(fileInfo.FullName), out var deck3))
				{
					if (toLowerDeckNames)
					{
						text = text.ToLower();
					}
					_decks.Add(text, deck3);
				}
				else
				{
					Debug.LogWarningFormat("Deck \"{0}\" had one or more invalid cards, it will not be loaded.", fileInfo.Name);
				}
			}
		}
		else
		{
			Debug.LogWarningFormat("{0} does not exist, no additional decks will be loaded", collectionsPath);
		}
	}

	public List<string> GetDeckNames()
	{
		return new List<string>(_decks.Keys);
	}

	public DeckCollectionDeck TryGetDeckByName(string name)
	{
		if (_decks.TryGetValue(name, out var value))
		{
			return value;
		}
		if (_decks.Count > 0)
		{
			return _decks.Values.First();
		}
		Debug.LogErrorFormat("NO DECK FOUND W/ NAME {0}", string.IsNullOrEmpty(name) ? "[NULL]" : name);
		return default(DeckCollectionDeck);
	}

	public static bool CreateDeckFromText(ICardDatabaseAdapter db, string text, out DeckCollectionDeck deck, ISetMetadataProvider setMetadataProvider = null)
	{
		if (setMetadataProvider == null)
		{
			setMetadataProvider = new NullSetMetadataProvider();
		}
		if (CreateDeckFromDeckBuilderFormatText(db, setMetadataProvider, text, out deck))
		{
			return true;
		}
		if (CreateDeckFromLegacyDeckFormatText(db, text, out deck))
		{
			return true;
		}
		return false;
	}

	public static bool CreateDeckFromDeckBuilderFormatText(ICardDatabaseAdapter db, ISetMetadataProvider setMetadataProvider, string text, out DeckCollectionDeck deck)
	{
		deck = default(DeckCollectionDeck);
		if (!text.Contains('('))
		{
			return false;
		}
		if (WrapperDeckUtilities.TryImportDeck(text, db, setMetadataProvider, db.ClientLocProvider, new Dictionary<uint, int>(), Languages.CurrentLanguage, out var deck2, out var _))
		{
			deck = new DeckCollectionDeck(deck2);
			return true;
		}
		return false;
	}

	public static string ConvertGrpIdsToCardList(ICardDataProvider cardDataProvider, ICardTitleProvider cardTitleProvider, IReadOnlyList<uint> cardIds)
	{
		Dictionary<uint, int> dictionary = new Dictionary<uint, int>();
		foreach (uint cardId in cardIds)
		{
			dictionary.TryAdd(cardId, 0);
			dictionary[cardId]++;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (uint key in dictionary.Keys)
		{
			string cardTitle = cardTitleProvider.GetCardTitle(key);
			CardPrintingData cardPrintingById = cardDataProvider.GetCardPrintingById(key);
			stringBuilder.AppendLine($"{dictionary[key]} {cardTitle} {cardPrintingById.ArtId}");
		}
		return stringBuilder.ToString();
	}

	private static string TrimTrailingComments(string entry)
	{
		int num = entry.IndexOf('#');
		if (num > 0)
		{
			entry = entry.Substring(0, num).Trim();
		}
		return entry;
	}

	private static bool TryParseTargetField(string entry, out string targetField)
	{
		targetField = string.Empty;
		if (string.Compare(entry, "Deck", StringComparison.OrdinalIgnoreCase) == 0)
		{
			targetField = "MainDeck";
			return true;
		}
		if (string.Compare(entry, "Sideboard", StringComparison.OrdinalIgnoreCase) == 0)
		{
			targetField = "Sideboard";
			return true;
		}
		if (string.Compare(entry, "Musicians", StringComparison.OrdinalIgnoreCase) == 0)
		{
			targetField = "Musicians";
			return true;
		}
		if (string.Compare(entry, "Companion", StringComparison.OrdinalIgnoreCase) == 0)
		{
			targetField = "Companion";
			return true;
		}
		if (string.Compare(entry, "Commander", StringComparison.OrdinalIgnoreCase) == 0)
		{
			targetField = "Commander";
			return true;
		}
		return false;
	}

	public static bool IsValidLegacyDecklistLine(ICardDatabaseAdapter db, string entry)
	{
		entry = TrimTrailingComments(entry);
		uint grpId;
		int count;
		if (!(entry == "") && !TryParseTargetField(entry, out var _))
		{
			return TryGetGrpId(db, entry, out grpId, out count);
		}
		return true;
	}

	private static bool CreateDeckFromLegacyDeckFormatText(ICardDatabaseAdapter db, string text, out DeckCollectionDeck deck)
	{
		deck = new DeckCollectionDeck
		{
			mainDeckCards = new List<uint>(),
			sideboardCards = new List<uint>(),
			commanders = new List<uint>(),
			companion = 0u
		};
		string text2 = "MainDeck";
		bool result = true;
		string[] array = Regex.Split(text, "\r\n|\r|\n");
		for (int i = 0; i < array.Length; i++)
		{
			string text3 = array[i].Trim();
			if (text3 == "")
			{
				continue;
			}
			text3 = TrimTrailingComments(text3);
			if (TryParseTargetField(text3, out var targetField))
			{
				text2 = targetField;
				continue;
			}
			if (!TryGetGrpId(db, text3, out var grpId, out var count))
			{
				result = false;
				continue;
			}
			switch (text2)
			{
			case "MainDeck":
			{
				for (int k = 0; k < count; k++)
				{
					deck.mainDeckCards.Add(grpId);
				}
				break;
			}
			case "Sideboard":
			{
				for (int l = 0; l < count; l++)
				{
					deck.sideboardCards.Add(grpId);
				}
				break;
			}
			case "Commander":
			case "Musicians":
			{
				for (int j = 0; j < count; j++)
				{
					deck.commanders.Add(grpId);
				}
				break;
			}
			case "Companion":
				deck.companion = grpId;
				break;
			}
		}
		return result;
	}

	private static bool SetCodeMatches(in string expected, in CardPrintingData cardPrinting)
	{
		return string.CompareOrdinal(expected, cardPrinting.ExpansionCode) == 0;
	}

	private static bool TryGetGrpId(ICardDatabaseAdapter db, string decklistEntry, out uint grpId, out int count)
	{
		grpId = 0u;
		count = 1;
		string setCode = null;
		Match match = Regex.Match(decklistEntry, "\\((.*)\\)");
		if (match.Success && match.Groups.Count > 1)
		{
			setCode = match.Groups[1].Value;
		}
		decklistEntry = decklistEntry.Split('(')[0];
		if (uint.TryParse(decklistEntry, out grpId))
		{
			return grpId != 0;
		}
		int num = decklistEntry.IndexOf(' ');
		if (num > 0 && !int.TryParse(decklistEntry.Substring(0, num), out count))
		{
			count = 1;
			num = -1;
		}
		string text = decklistEntry.Substring(num + 1).Trim();
		text = text.Replace('’', '\'');
		if (uint.TryParse(text, out grpId))
		{
			return grpId != 0;
		}
		List<string> list = (from w in text.Split(text.Contains('\t') ? '\t' : ' ')
			select w.Trim()).ToList();
		string text2 = list[list.Count - 1];
		bool flag = false;
		if (uint.TryParse(text2, out var result) && result > 9999)
		{
			flag = true;
			text = text.Substring(0, text.Length - text2.Length - 1);
		}
		IReadOnlyList<CardPrintingData> printingsByEnglishTitle = db.DatabaseUtilities.GetPrintingsByEnglishTitle(text);
		if (printingsByEnglishTitle.Count <= 0)
		{
			return grpId != 0;
		}
		if (flag && TryGetGrpId(printingsByEnglishTitle, in setCode, result, out grpId))
		{
			return true;
		}
		if (!string.IsNullOrEmpty(setCode))
		{
			foreach (CardPrintingData item in printingsByEnglishTitle)
			{
				CardPrintingData cardPrinting = item;
				if (cardPrinting != null && SetCodeMatches(in setCode, in cardPrinting))
				{
					grpId = cardPrinting.GrpId;
					break;
				}
			}
		}
		if (grpId == 0)
		{
			grpId = printingsByEnglishTitle[0].GrpId;
		}
		return grpId != 0;
	}

	private static bool TryGetGrpId(IReadOnlyList<CardPrintingData> possiblePrintings, in string setCode, uint artId, out uint grpId)
	{
		bool flag = !string.IsNullOrEmpty(setCode);
		bool flag2 = false;
		grpId = 0u;
		foreach (CardPrintingData possiblePrinting in possiblePrintings)
		{
			CardPrintingData cardPrinting = possiblePrinting;
			if (cardPrinting != null && cardPrinting.ArtId == artId)
			{
				grpId = cardPrinting.GrpId;
				if (flag && SetCodeMatches(in setCode, in cardPrinting))
				{
					flag2 = true;
					break;
				}
			}
		}
		bool num = grpId != 0;
		if (num && flag)
		{
		}
		return num;
	}
}
