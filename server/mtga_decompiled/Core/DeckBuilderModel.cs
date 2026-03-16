using System;
using System.Collections.Generic;
using System.Linq;
using Core.Code.Decks;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

public class DeckBuilderModel
{
	private bool _isConstructed;

	private bool _isSideboarding;

	private uint _maxCardsByTitle = 4u;

	private bool _allowUnownedInConstructed = true;

	private ICardDatabaseAdapter _db;

	private CardList _cardPool;

	private CardList _mainDeck;

	private CardList _sideboard;

	private CardList _commandZone;

	private Dictionary<uint, string> _cardSkins;

	private Dictionary<uint, string> _cardSkinsOverride;

	private CardPrintingData _companion;

	private bool _companionValid;

	public Guid _deckId;

	public string _deckName;

	public uint _deckTileId;

	public uint _deckArtId;

	public string _deckFormat;

	public string _description;

	public string _cardBack;

	public string _avatar;

	public string _pet;

	public List<string> _emotes;

	public DateTime _lastPlayed;

	public bool _isFavorite;

	public readonly SortType[] SideboardSortCriteria = new SortType[4]
	{
		SortType.LandLast,
		SortType.CMCWithXLast,
		SortType.ColorOrder,
		SortType.Title
	};

	private List<CardPrintingQuantity> _appliedFilteredMainDeck;

	private List<CardPrintingQuantity> _appliedFilteredSideboard;

	public IReadOnlyList<CardPrintingQuantity> AppliedFilteredMainDeck
	{
		get
		{
			IReadOnlyList<CardPrintingQuantity> appliedFilteredMainDeck = _appliedFilteredMainDeck;
			return appliedFilteredMainDeck ?? GetFilteredMainDeck();
		}
	}

	public IReadOnlyList<CardPrintingQuantity> AppliedFilteredSideboard
	{
		get
		{
			IReadOnlyList<CardPrintingQuantity> appliedFilteredSideboard = _appliedFilteredSideboard;
			return appliedFilteredSideboard ?? GetFilteredSideboard();
		}
	}

	public bool HasLoadedDeck => _mainDeck != null;

	public DeckBuilderModel(ICardDatabaseAdapter db, DeckInfo deck, Dictionary<uint, uint> cardPool, bool isConstructed, bool isSideboarding, uint maxCardsByTitle, bool ignoreIsCollectible = false, Dictionary<uint, string> cardSkinOverride = null, string formatName = null)
	{
		_db = db;
		_cardPool = new CardList(db, cardPool, ignoreIsCollectible);
		_isConstructed = isConstructed;
		_isSideboarding = isSideboarding;
		_maxCardsByTitle = maxCardsByTitle;
		_cardSkinsOverride = cardSkinOverride;
		if (deck != null)
		{
			_deckId = deck.id;
			_deckName = deck.name;
			_deckTileId = deck.deckTileId;
			_deckArtId = deck.deckArtId;
			_deckFormat = ((formatName != null) ? formatName : deck.format);
			_description = deck.description;
			_cardBack = deck.cardBack;
			_avatar = deck.avatar;
			_pet = deck.pet;
			_emotes = new List<string>(deck.emotes);
			_lastPlayed = deck.LastPlayed;
			_isFavorite = deck.IsFavorite;
			_mainDeck = new CardList(db, GetCardQuantityByIdDictionary(deck.mainDeck), ignoreIsCollectible);
			_sideboard = new CardList(db, GetCardQuantityByIdDictionary(deck.sideboard), ignoreIsCollectible);
			if (!_isConstructed || _isSideboarding)
			{
				_cardPool.SetIncludeUnownedOnly(includeUnowned: false);
			}
			_mainDeck.SetIncludeUnownedOnly(includeUnowned: false);
			_sideboard.SetIncludeUnownedOnly(includeUnowned: false);
			_mainDeck.SetSortAndFilter(SortType.LandLast, SortType.CMCWithXLast, SortType.ColorOrder, SortType.Title);
			_sideboard.SetSortAndFilter(SideboardSortCriteria);
			if (cardSkinOverride != null && cardSkinOverride.Count > 0)
			{
				_cardSkins = cardSkinOverride;
			}
			else
			{
				Dictionary<uint, string> dictionary = new Dictionary<uint, string>();
				foreach (CardSkin item in deck.cardSkins.Distinct())
				{
					dictionary[(uint)item.GrpId] = item.CCV;
				}
				_cardSkins = dictionary;
			}
			_commandZone = new CardList(db, GetCardQuantityByIdDictionary(deck.commandZone), ignoreIsCollectible: false, allowSpecializeFacets: true);
			_commandZone.SetIncludeUnownedOnly(includeUnowned: false);
			_commandZone.SetSortAndFilter(SortType.Title);
			CardInDeck companion = deck.companion;
			if (companion != null)
			{
				_ = companion.Id;
				if (true)
				{
					_companion = db.CardDataProvider.GetCardPrintingById(deck.companion.Id);
					_companionValid = deck.isCompanionValid;
				}
			}
		}
		else
		{
			_cardSkins = cardSkinOverride ?? new Dictionary<uint, string>();
		}
	}

	private Dictionary<uint, uint> GetCardQuantityByIdDictionary(List<CardInDeck> cards)
	{
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
		if (cards == null)
		{
			return dictionary;
		}
		foreach (CardInDeck card in cards)
		{
			if (dictionary.ContainsKey(card.Id))
			{
				dictionary[card.Id] += card.Quantity;
			}
			else
			{
				dictionary[card.Id] = card.Quantity;
			}
		}
		return dictionary;
	}

	public bool CanAddCardToPile(DeckBuilderPile pile, uint grpId, bool checkAllowUnownedInConstructed = true)
	{
		return pile switch
		{
			DeckBuilderPile.MainDeck => CanAddCardToMainDeck(grpId, checkAllowUnownedInConstructed), 
			DeckBuilderPile.Sideboard => CanAddCardToSideboard(grpId), 
			DeckBuilderPile.Companion => CanAddCardToMainDeck(grpId), 
			DeckBuilderPile.Commander => CanAddCardToCommand(grpId), 
			DeckBuilderPile.Partner => CanAddCardToPartner(grpId), 
			DeckBuilderPile.Pool => true, 
			_ => throw new ArgumentOutOfRangeException("pile", pile, null), 
		};
	}

	public bool CanAddCardToMainDeck(uint grpId, bool checkAllowUnownedInConstructed = true)
	{
		CardPrintingData cardPrintingById = _db.CardDataProvider.GetCardPrintingById(grpId);
		uint quantityInWholeDeckByTitle = GetQuantityInWholeDeckByTitle(cardPrintingById);
		uint num = cardPrintingById.AlternateDeckLimit ?? _maxCardsByTitle;
		if (quantityInWholeDeckByTitle >= num)
		{
			return false;
		}
		uint quantityInCardPoolByTitle = GetQuantityInCardPoolByTitle(cardPrintingById);
		bool num2 = quantityInCardPoolByTitle > quantityInWholeDeckByTitle;
		bool flag = cardPrintingById.IsBasicLandUnlimited && (!_isConstructed || !_isSideboarding) && quantityInCardPoolByTitle >= cardPrintingById.MaxCollected;
		bool flag2 = cardPrintingById.AlternateDeckLimit.HasValue && _isConstructed && !_isSideboarding && quantityInCardPoolByTitle >= cardPrintingById.MaxCollected;
		bool flag3 = checkAllowUnownedInConstructed && _allowUnownedInConstructed && _isConstructed && !_isSideboarding;
		if (!num2 && !flag && !flag2 && !flag3)
		{
			return false;
		}
		return true;
	}

	public bool CanAddCardToSideboard(uint grpId)
	{
		if (_isSideboarding)
		{
			return false;
		}
		if (!_isConstructed)
		{
			return false;
		}
		return CanAddCardToMainDeck(grpId);
	}

	private bool CanAddCardToCommand(uint grpId)
	{
		if (!DeckFormat.CardCanBeCommander(_db.CardDataProvider.GetCardPrintingById(grpId)))
		{
			return false;
		}
		return CanAddCardToMainDeck(grpId);
	}

	private bool CanAddCardToPartner(uint grpId)
	{
		if (!_db.CardDataProvider.GetCardPrintingById(grpId).HasPartnerAbilityCompatibleWithCommanders(GetFilteredCommandZone()))
		{
			return false;
		}
		return CanAddCardToCommand(grpId);
	}

	public bool HasMultipleCommanders()
	{
		return _commandZone.GetCardCollection().Count() >= 2;
	}

	public Dictionary<uint, CardPrintingQuantity> GetCardsNeededToFinishDeck()
	{
		Dictionary<uint, CardPrintingQuantity> dictionary = new Dictionary<uint, CardPrintingQuantity>();
		if (!HasLoadedDeck)
		{
			return dictionary;
		}
		foreach (IGrouping<uint, (CardPrintingData, CardPrintingData)> item3 in from grpId in _mainDeck.GetRawTable().Keys.Concat(_sideboard.GetRawTable().Keys).Concat(_commandZone.GetRawTable().Keys).Distinct()
			select SpecializeUtilities.GetBasePrinting(_db.CardDataProvider, grpId) into card
			group card by card.BasePrinting.TitleId)
		{
			item3.Deconstruct(out var key, out var grouped);
			uint titleId = key;
			IEnumerable<(CardPrintingData, CardPrintingData)> enumerable = grouped;
			uint num = 0u;
			bool flag = false;
			foreach (var item4 in enumerable)
			{
				CardPrintingData item = item4.Item1;
				uint quantityInCardPool = GetQuantityInCardPool(item.GrpId);
				bool flag2 = !string.IsNullOrWhiteSpace(GetCardSkin(item.GrpId));
				if (quantityInCardPool == 0 && !flag2)
				{
					AddCount(dictionary, item, 1u);
					num++;
				}
				if (item.MaxCollected == 1)
				{
					flag = true;
				}
			}
			if (flag)
			{
				continue;
			}
			uint num2 = Math.Min(GetQuantityInCardPoolByTitle(titleId), 4u);
			uint num3 = Math.Min(GetQuantityInWholeDeckByTitle(titleId), 4u);
			int num4 = Math.Clamp((int)(num3 - num2 - num), 0, (int)num3);
			if (num4 <= 0)
			{
				continue;
			}
			int num5 = num4;
			foreach (var item5 in enumerable.OrderByDescending<(CardPrintingData, CardPrintingData), uint>(((CardPrintingData BasePrinting, CardPrintingData DirectPrinting) p) => p.BasePrinting.GrpId))
			{
				CardPrintingData item2 = item5.Item1;
				uint quantityInCardPool2 = GetQuantityInCardPool(item2.GrpId);
				uint num6 = GetQuantityInWholeDeck(item2.GrpId);
				if (num6 > item2.MaxCollected)
				{
					num6 = item2.MaxCollected;
				}
				uint count = GetCount(dictionary, item2);
				int num7 = (int)(num6 - quantityInCardPool2 - count);
				if (num7 > 0)
				{
					int num8 = Math.Min(num7, num5);
					num5 -= num8;
					AddCount(dictionary, item2, (uint)num8);
					if (num5 <= 0)
					{
						break;
					}
				}
			}
		}
		return dictionary;
	}

	private static void AddCount(Dictionary<uint, CardPrintingQuantity> dict, CardPrintingData card, uint quantity)
	{
		if (dict.TryGetValue(card.GrpId, out var value))
		{
			value.Quantity += quantity;
			return;
		}
		dict[card.GrpId] = new CardPrintingQuantity
		{
			Printing = card,
			Quantity = quantity
		};
	}

	private static uint GetCount(Dictionary<uint, CardPrintingQuantity> dict, CardPrintingData card)
	{
		if (!dict.TryGetValue(card.GrpId, out var value))
		{
			return 0u;
		}
		return value.Quantity;
	}

	public DeckInfo GetServerModel()
	{
		List<CardInDeck> list;
		if (!_isConstructed)
		{
			Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>(_cardPool.GetRawTable());
			foreach (var (num3, num4) in _mainDeck.GetRawTable())
			{
				if (!_db.CardDataProvider.GetCardPrintingById(num3).IsBasicLandUnlimited && dictionary.TryGetValue(num3, out var value))
				{
					if (value <= num4)
					{
						dictionary.Remove(num3);
					}
					else
					{
						dictionary[num3] = value - num4;
					}
				}
			}
			list = DeckListForDeckTable(dictionary.Where((KeyValuePair<uint, uint> x) => !_db.CardDataProvider.GetCardPrintingById(x.Key).IsBasicLandUnlimited).ToDictionary((KeyValuePair<uint, uint> x) => x.Key, (KeyValuePair<uint, uint> y) => y.Value));
		}
		else if (_isSideboarding)
		{
			Dictionary<uint, uint> dictionary2 = new Dictionary<uint, uint>(_cardPool.GetRawTable());
			foreach (KeyValuePair<uint, uint> item in _mainDeck.GetRawTable())
			{
				if (dictionary2.TryGetValue(item.Key, out var value2))
				{
					if (value2 <= item.Value)
					{
						dictionary2.Remove(item.Key);
					}
					else
					{
						dictionary2[item.Key] = value2 - item.Value;
					}
				}
			}
			list = DeckListForDeckTable(dictionary2);
		}
		else
		{
			list = DeckListForDeckTable(_sideboard.GetRawTable());
		}
		IReadOnlyDictionary<uint, uint> rawTable = _mainDeck.GetRawTable();
		IReadOnlyDictionary<uint, uint> rawTable2 = _commandZone.GetRawTable();
		List<CardSkin> list2 = new List<CardSkin>();
		foreach (KeyValuePair<uint, string> kvp in _cardSkins)
		{
			if (kvp.Value != null && (rawTable.ContainsKey(kvp.Key) || list.Exists((CardInDeck c) => c.Id == kvp.Key) || rawTable2.ContainsKey(kvp.Key)))
			{
				list2.Add(new CardSkin(kvp.Key, kvp.Value, 0L));
			}
		}
		return new DeckInfo
		{
			id = _deckId,
			deckTileId = _deckTileId,
			deckArtId = _deckArtId,
			name = _deckName,
			format = _deckFormat,
			description = _description,
			lastUpdated = DateTime.Now,
			mainDeck = DeckListForDeckTable(rawTable),
			sideboard = list,
			cardSkins = list2,
			cardBack = _cardBack,
			avatar = _avatar,
			pet = _pet,
			emotes = new List<string>(_emotes),
			commandZone = DeckListForDeckTable(rawTable2),
			companion = ((_companion != null) ? new CardInDeck(_companion.GrpId, 1u) : null),
			isCompanionValid = _companionValid,
			LastPlayed = _lastPlayed,
			IsFavorite = _isFavorite,
			isLoaded = true
		};
	}

	public static List<CardInDeck> DeckListForDeckTable(IReadOnlyDictionary<uint, uint> deckTable)
	{
		return (from x in deckTable
			where x.Value != 0
			select new CardInDeck(x.Key, x.Value)).ToList();
	}

	public void UpdateFormatInfo(string deckFormat, uint maxCardsByTitle)
	{
		_deckFormat = deckFormat;
		_maxCardsByTitle = maxCardsByTitle;
	}

	public uint GetQuantityInMainDeck(uint grpId)
	{
		return GetCollectableQuantityByGrpId(_mainDeck, grpId);
	}

	public uint GetQuantityInSideboard(uint grpId)
	{
		return GetCollectableQuantityByGrpId(_sideboard, grpId);
	}

	public uint GetQuantityInCommand(uint grpId)
	{
		return GetCollectableQuantityByGrpId(_commandZone, grpId);
	}

	public uint GetQuantityCompanion(uint grpId)
	{
		CardPrintingData companion = _companion;
		if (companion == null || companion.GrpId != grpId)
		{
			return 0u;
		}
		return 1u;
	}

	public uint GetQuantityInCardPool(uint grpId)
	{
		return GetCollectableQuantityByGrpId(_cardPool, grpId);
	}

	public uint GetQuantityInWholeDeck(uint grpId)
	{
		return GetQuantityInMainDeck(grpId) + Math.Max(GetQuantityInSideboard(grpId), GetQuantityCompanion(grpId)) + GetQuantityInCommand(grpId);
	}

	public uint GetQuantityInPileByTitle(DeckBuilderPile pile, CardPrintingData card)
	{
		return pile switch
		{
			DeckBuilderPile.MainDeck => GetQuantityInMainDeckByTitle(card), 
			DeckBuilderPile.Sideboard => GetQuantityInSideboardByTitle(card), 
			DeckBuilderPile.Companion => GetQuantityCompanionByTitle(card), 
			DeckBuilderPile.Commander => GetQuantityInCommandByTitle(card), 
			DeckBuilderPile.Partner => GetQuantityInCommandByTitle(card), 
			DeckBuilderPile.Pool => GetQuantityInCardPoolByTitle(card), 
			_ => throw new ArgumentOutOfRangeException("pile", pile, null), 
		};
	}

	public uint GetQuantityInMainDeckByTitle(CardPrintingData card)
	{
		return GetCollectableQuantityByTitle(_mainDeck, card);
	}

	public uint GetQuantityInMainDeckByTitle(uint titleId)
	{
		return GetCollectableQuantityByTitle(_mainDeck, titleId);
	}

	public uint GetQuantityInSideboardByTitle(CardPrintingData card)
	{
		return GetCollectableQuantityByTitle(_sideboard, card);
	}

	public uint GetQuantityInSideboardByTitle(uint titleId)
	{
		return GetCollectableQuantityByTitle(_sideboard, titleId);
	}

	public uint GetQuantityInCommandByTitle(CardPrintingData card)
	{
		return GetCollectableQuantityByTitle(_commandZone, card);
	}

	public uint GetQuantityInCommandByTitle(uint titleId)
	{
		return GetCollectableQuantityByTitle(_commandZone, titleId);
	}

	public uint GetQuantityCompanionByTitle(CardPrintingData card)
	{
		return GetQuantityCompanionByTitle(card.TitleId);
	}

	public uint GetQuantityCompanionByTitle(uint titleId)
	{
		if (_companion == null)
		{
			return 0u;
		}
		if (titleId != _companion.TitleId)
		{
			return 0u;
		}
		return 1u;
	}

	public uint GetQuantityInCardPoolByTitle(CardPrintingData card)
	{
		return GetCollectableQuantityByTitle(_cardPool, card);
	}

	public uint GetQuantityInCardPoolByTitle(uint titleId)
	{
		return GetCollectableQuantityByTitle(_cardPool, titleId);
	}

	public uint GetQuantityInWholeDeckByTitle(CardPrintingData card)
	{
		return GetQuantityInMainDeckByTitle(card) + Math.Max(GetQuantityInSideboardByTitle(card), GetQuantityCompanionByTitle(card)) + GetQuantityInCommandByTitle(card);
	}

	public uint GetQuantityInWholeDeckByTitle(uint titleId)
	{
		return GetQuantityInMainDeckByTitle(titleId) + Math.Max(GetQuantityInSideboardByTitle(titleId), GetQuantityCompanionByTitle(titleId)) + GetQuantityInCommandByTitle(titleId);
	}

	public uint GetQuantityInMainDeckIncludingStyle(uint grpId, string styleCode)
	{
		return GetCollectableQuantityIncludingStyle(_mainDeck, grpId, styleCode);
	}

	public uint GetQuantityInSideboardIncludingStyle(uint grpId, string styleCode)
	{
		return GetCollectableQuantityIncludingStyle(_sideboard, grpId, styleCode);
	}

	public uint GetQuantityInCommandIncludingStyle(uint grpId, string styleCode)
	{
		return GetCollectableQuantityIncludingStyle(_commandZone, grpId, styleCode);
	}

	public uint GetQuantityCompanionIncludingStyle(uint grpId, string styleCode)
	{
		if (styleCode == null && _cardSkins.TryGetValue(grpId, out var value) && styleCode == value)
		{
			return 0u;
		}
		if (styleCode == null)
		{
			return GetQuantityCompanion(grpId);
		}
		CardPrintingData companion = _companion;
		if (companion == null || companion.GrpId != grpId || !_cardSkins.TryGetValue(grpId, out value) || !(styleCode == value))
		{
			return 0u;
		}
		return 1u;
	}

	public uint GetQuantityInCardPoolIncludingStyle(uint grpId, string styleCode)
	{
		return GetCollectableQuantityIncludingStyle(_cardPool, grpId, styleCode);
	}

	public uint GetQuantityInWholeDeckIncludingStyle(uint grpId, string styleCode)
	{
		return GetQuantityInMainDeckIncludingStyle(grpId, styleCode) + Math.Max(GetQuantityInSideboardIncludingStyle(grpId, styleCode), GetQuantityCompanionIncludingStyle(grpId, styleCode)) + GetQuantityInCommandIncludingStyle(grpId, styleCode);
	}

	public uint GetTotalSideboardSize()
	{
		return _sideboard?.GetTotalSize() ?? 0;
	}

	public uint GetTotalMainDeckSize()
	{
		return _mainDeck?.GetTotalSize() ?? 0;
	}

	public uint GetTotalCommandSize()
	{
		return _commandZone?.GetTotalSize() ?? 0;
	}

	public IReadOnlyList<CardPrintingQuantity> GetFilteredPool()
	{
		return _cardPool?.GetFilteredSortedList() ?? Array.Empty<CardPrintingQuantity>();
	}

	public IReadOnlyList<CardPrintingQuantity> GetCardCollection()
	{
		return _cardPool?.GetCardCollection() ?? Array.Empty<CardPrintingQuantity>();
	}

	public IReadOnlyList<CardPrintingQuantity> GetFilteredMainDeck()
	{
		return _mainDeck?.GetFilteredSortedList() ?? Array.Empty<CardPrintingQuantity>();
	}

	public IReadOnlyList<CardPrintingQuantity> GetFilteredSideboard()
	{
		return _sideboard?.GetFilteredSortedList() ?? Array.Empty<CardPrintingQuantity>();
	}

	public IReadOnlyList<CardPrintingQuantity> GetFilteredCommandZone()
	{
		return _commandZone?.GetFilteredSortedList() ?? Array.Empty<CardPrintingQuantity>();
	}

	public List<CardPrintingQuantity> GetAllFilteredCards()
	{
		if (!HasLoadedDeck)
		{
			return new List<CardPrintingQuantity>();
		}
		return GetFilteredMainDeck().Concat(GetFilteredSideboard()).Concat(GetFilteredCommandZone()).ToList();
	}

	private uint GetCollectableQuantityByGrpId(CardList cardList, uint grpId)
	{
		if (cardList == null)
		{
			return 0u;
		}
		CardPrintingData item = SpecializeUtilities.GetBasePrinting(_db.CardDataProvider, grpId).BasePrinting;
		uint num = cardList.GetQuantityByGrpId(item.GrpId);
		foreach (CardPrintingData specializeFacet in SpecializeUtilities.GetSpecializeFacets(item))
		{
			num += cardList.GetQuantityByGrpId(specializeFacet.GrpId);
		}
		return num;
	}

	private uint GetCollectableQuantityByTitle(CardList cardList, CardPrintingData card)
	{
		if (cardList == null)
		{
			return 0u;
		}
		CardPrintingData item = SpecializeUtilities.GetBasePrinting(_db.CardDataProvider, card.GrpId).BasePrinting;
		uint num = cardList.GetQuantityByTitle(item.TitleId);
		foreach (CardPrintingData specializeFacet in SpecializeUtilities.GetSpecializeFacets(item))
		{
			num += cardList.GetQuantityByTitle(specializeFacet.TitleId);
		}
		return num;
	}

	private uint GetCollectableQuantityByTitle(CardList cardList, uint titleId)
	{
		if (cardList == null)
		{
			return 0u;
		}
		CardPrintingData item = SpecializeUtilities.GetBasePrintingByTitleId(_db, titleId).BasePrinting;
		uint num = cardList.GetQuantityByTitle(item.TitleId);
		foreach (CardPrintingData specializeFacet in SpecializeUtilities.GetSpecializeFacets(item))
		{
			num += cardList.GetQuantityByTitle(specializeFacet.TitleId);
		}
		return num;
	}

	private uint GetCollectableQuantityIncludingStyle(CardList cardList, uint grpId, string styleCode)
	{
		if (cardList == null)
		{
			return 0u;
		}
		if (styleCode == null && _cardSkins.TryGetValue(grpId, out var value) && value != null)
		{
			return 0u;
		}
		if (styleCode == null)
		{
			return GetCollectableQuantityByGrpId(cardList, grpId);
		}
		CardPrintingData item = SpecializeUtilities.GetBasePrinting(_db.CardDataProvider, grpId).BasePrinting;
		uint num = ((_cardSkins.TryGetValue(item.GrpId, out value) && value == styleCode) ? cardList.GetQuantityByGrpId(item.GrpId) : 0u);
		foreach (CardPrintingData specializeFacet in SpecializeUtilities.GetSpecializeFacets(item))
		{
			num += ((_cardSkins.TryGetValue(specializeFacet.GrpId, out value) && value == styleCode) ? cardList.GetQuantityByGrpId(item.GrpId) : 0);
		}
		return num;
	}

	public void AddCardToPile(DeckBuilderPile pile, CardData card, uint count = 1u)
	{
		if (string.IsNullOrWhiteSpace(card.SkinCode))
		{
			RemoveCardSkinData(card.GrpId);
		}
		switch (pile)
		{
		case DeckBuilderPile.MainDeck:
			AddCardToMainDeck(card.GrpId, count);
			break;
		case DeckBuilderPile.Sideboard:
			AddCardToSideboard(card.GrpId, count);
			break;
		case DeckBuilderPile.Companion:
			SetCompanion(card.GrpId);
			break;
		case DeckBuilderPile.Commander:
			RemoveCardFromCommandZone(GetFilteredCommandZone().FirstOrDefault()?.Printing.GrpId);
			RemoveCardSkinData(card.GrpId);
			AddCardToCommandZone(card.GrpId);
			break;
		case DeckBuilderPile.Partner:
			RemoveCardFromCommandZone(GetFilteredCommandZone().Skip(1).LastOrDefault()?.Printing.GrpId);
			RemoveCardSkinData(card.GrpId);
			AddCardToCommandZone(card.GrpId);
			break;
		default:
			throw new ArgumentOutOfRangeException("pile", pile, null);
		case DeckBuilderPile.Pool:
			break;
		}
	}

	public void AddCardToMainDeck(uint grpId, uint count = 1u)
	{
		_mainDeck.AddByGrpId(grpId, count);
	}

	public void AddCardToSideboard(uint grpId, uint count = 1u)
	{
		_sideboard.AddByGrpId(grpId, count);
	}

	public void AddCardToCommandZone(uint grpId)
	{
		_commandZone.AddByGrpId(grpId);
	}

	public void AddCardToPool(uint grpId, uint count = 1u)
	{
		_cardPool.AddByGrpId(grpId, count);
	}

	public void SetCompanion(uint grpId)
	{
		RemoveCompanion();
		_companion = _db.CardDataProvider.GetCardPrintingById(grpId);
		_sideboard.AddByGrpId(grpId);
	}

	public void SetCompanionValid(bool valid)
	{
		_companionValid = valid;
	}

	public CardPrintingData GetCompanion()
	{
		return _companion;
	}

	public void RemoveCompanion()
	{
		if (_companion != null)
		{
			if (_sideboard.GetQuantityByGrpId(_companion.GrpId) != 0)
			{
				_sideboard.RemoveByGrpId(_companion.GrpId);
			}
			_companion = null;
		}
	}

	public void RemoveCardFromPile(DeckBuilderContext context, DeckBuilderPile pile, uint grpId, uint count = 1u)
	{
		switch (pile)
		{
		case DeckBuilderPile.MainDeck:
			RemoveCardFromMainDeck(grpId, count);
			break;
		case DeckBuilderPile.Sideboard:
			RemoveCardFromSideboard(grpId, count);
			break;
		case DeckBuilderPile.Companion:
			RemoveCompanion();
			break;
		case DeckBuilderPile.Commander:
		case DeckBuilderPile.Partner:
			RemoveCardFromCommandZone(grpId);
			break;
		default:
			throw new ArgumentOutOfRangeException("pile", pile, null);
		case DeckBuilderPile.Pool:
			break;
		}
		if (GetQuantityInWholeDeck(grpId) == 0 && !context.IsLimited && !context.IsSideboarding)
		{
			RemoveCardSkinData(grpId);
		}
	}

	private void RemoveCardSkinData(uint grpId)
	{
		if (_cardSkins != null && _cardSkins.ContainsKey(grpId))
		{
			_cardSkins.Remove(grpId);
		}
	}

	public void RemoveCardFromMainDeck(uint grpId, uint count = 1u)
	{
		_mainDeck.RemoveByGrpId(grpId, count);
	}

	public void RemoveCardFromSideboard(uint grpId, uint count = 1u)
	{
		_sideboard.RemoveByGrpId(grpId, count);
	}

	public void ClearCardsFromSideboard()
	{
		_sideboard.Clear();
	}

	public void RemoveCardFromPool(uint grpId, uint count = 1u)
	{
		_cardPool.RemoveByGrpId(grpId, count);
	}

	public void RemoveCardFromCommandZone(uint? grpId)
	{
		if (grpId.HasValue && _commandZone.GetQuantityByGrpId(grpId.Value) != 0)
		{
			_commandZone.RemoveByGrpId(grpId.Value);
		}
	}

	public void ClearCommandZone()
	{
		_commandZone = new CardList(_db, new Dictionary<uint, uint>(), ignoreIsCollectible: false, allowSpecializeFacets: true);
		_commandZone.SetIncludeUnownedOnly(includeUnowned: false);
		_commandZone.SetSortAndFilter(SortType.Title);
	}

	public void ReSortSkipFilters()
	{
		_cardPool.ReSortSkipFilter();
		_mainDeck?.ReSortSkipFilter();
		_sideboard?.ReSortSkipFilter();
		_commandZone?.ReSortAndFilter();
	}

	public void ReSort()
	{
		_cardPool.ReSortAndFilter();
		_mainDeck?.ReSortAndFilter();
		_sideboard?.ReSortAndFilter();
		_commandZone?.ReSortAndFilter();
	}

	public void SortPool(params SortType[] sortType)
	{
		_cardPool.SetSortAndFilter(sortType);
	}

	public void FilterPool(IReadOnlyList<Func<CardFilterGroup, CardFilterGroup>> filters)
	{
		_cardPool.SetFilters(filters);
	}

	public void ApplyFilterMainDeck(IReadOnlyList<Func<CardFilterGroup, CardFilterGroup>> filters)
	{
		_mainDeck.SetFilters(filters);
		if (_appliedFilteredMainDeck == null)
		{
			_appliedFilteredMainDeck = new List<CardPrintingQuantity>();
		}
		_appliedFilteredMainDeck.Clear();
		_appliedFilteredMainDeck.AddRange(_mainDeck.GetFilteredSortedList());
		_mainDeck.SetFilters(Array.Empty<Func<CardFilterGroup, CardFilterGroup>>());
	}

	public void ApplyFilterSideboard(IReadOnlyList<Func<CardFilterGroup, CardFilterGroup>> filters)
	{
		_sideboard.SetFilters(filters);
		if (_appliedFilteredSideboard == null)
		{
			_appliedFilteredSideboard = new List<CardPrintingQuantity>();
		}
		_appliedFilteredSideboard.Clear();
		_appliedFilteredSideboard.AddRange(_sideboard.GetFilteredSortedList());
		_sideboard.SetFilters(Array.Empty<Func<CardFilterGroup, CardFilterGroup>>());
	}

	public void ApplyDeckBaseFilters()
	{
		_mainDeck?.ApplyFilters();
		_sideboard?.ApplyFilters();
	}

	public void UpdatePile(DeckBuilderPile pile)
	{
		switch (pile)
		{
		case DeckBuilderPile.MainDeck:
			UpdateMainDeck();
			break;
		case DeckBuilderPile.Sideboard:
			UpdateSideboard();
			break;
		case DeckBuilderPile.Commander:
		case DeckBuilderPile.Partner:
			UpdateCommandZone();
			if (Pantry.Get<DeckBuilderCardFilterProvider>().IsAutoSuggestLandsToggleOn)
			{
				BasicLandSuggester.SuggestLand();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("pile", pile, null);
		case DeckBuilderPile.Companion:
		case DeckBuilderPile.Pool:
			break;
		}
		if (pile == DeckBuilderPile.Companion || _companion != null)
		{
			CompanionUtil companionUtil = Pantry.Get<CompanionUtil>();
			DeckBuilderContextProvider deckBuilderContextProvider = Pantry.Get<DeckBuilderContextProvider>();
			companionUtil.UpdateValidation(this, deckBuilderContextProvider.Context?.Format);
		}
	}

	public void UpdateMainDeck()
	{
		_mainDeck.ApplyFilters();
	}

	public void UpdateSideboard()
	{
		_sideboard.ApplyFilters();
	}

	public void UpdateCommandZone()
	{
		_commandZone.ApplyFilters();
	}

	public void UpdatePool()
	{
		_cardPool.ApplyFilters();
	}

	public void SetPoolCounts(Dictionary<uint, uint> grpQuantityTable)
	{
		_cardPool.SetQuantities(grpQuantityTable);
	}

	public string GetCardSkin(uint grpId)
	{
		_cardSkins.TryGetValue(grpId, out var value);
		return value;
	}

	public bool TryGetCardSkin(uint grpId, out string ccv)
	{
		return _cardSkins.TryGetValue(grpId, out ccv);
	}

	public void SetCardSkin(uint grpId, string ccv)
	{
		if (ccv != null)
		{
			_cardSkins[grpId] = ccv;
		}
	}

	public bool CardSkinIsOverridden(ICardDatabaseAdapter cardDatabase, uint artId, string skinCode)
	{
		if (_cardSkinsOverride == null || _cardSkinsOverride.Count == 0)
		{
			return false;
		}
		foreach (var (data, text2) in _cardSkinsOverride)
		{
			if (!(skinCode != text2) && cardDatabase.DatabaseUtilities.GetPrintingsByArtId(artId).Exists(data, (CardPrintingData p, uint grpId) => p.GrpId == grpId))
			{
				return true;
			}
		}
		return false;
	}

	public void SwapRebalancedCards(DeckBuilderContext context)
	{
		bool valueOrDefault = context?.Format?.UseRebalancedCards == true;
		bool num = SwapRebalancedCards(_mainDeck, context?.Deck?.mainDeck, valueOrDefault) | SwapRebalancedCards(_sideboard, context?.Deck?.sideboard, valueOrDefault) | SwapRebalancedCards(_commandZone, context?.Deck?.commandZone, valueOrDefault) | SwapRebalancedCards(_cardPool, null, valueOrDefault);
		if (_companion != null && DeckUtilities.SwapRebalancedCard(_db as CardDatabase, _companion, valueOrDefault, out var replacement))
		{
			_companion = replacement;
		}
		if (num)
		{
			UpdateMainDeck();
			UpdateSideboard();
			UpdateCommandZone();
		}
	}

	private bool SwapRebalancedCards(CardList modelCardList, List<CardInDeck> contextCardList, bool useRebalancedCards)
	{
		if (modelCardList == null)
		{
			return false;
		}
		bool result = false;
		Dictionary<uint, uint> dictionary = modelCardList.GetRawTable().ToDictionary((KeyValuePair<uint, uint> kv) => kv.Key, (KeyValuePair<uint, uint> kv) => kv.Value);
		foreach (KeyValuePair<uint, uint> item in dictionary)
		{
			item.Deconstruct(out var key, out var value);
			uint id = key;
			uint num = value;
			CardPrintingData cardPrinting = _db.CardDataProvider.GetCardPrintingById(id);
			if (cardPrinting == null)
			{
				continue;
			}
			if (cardPrinting.DefunctRebalancedCardLink != 0 && cardPrinting.IsRebalanced && num != 0)
			{
				contextCardList?.RemoveAll((CardInDeck c) => c.Id == cardPrinting.GrpId);
				modelCardList.RemoveByGrpId(cardPrinting.GrpId, num);
				dictionary.TryGetValue(cardPrinting.DefunctRebalancedCardLink, out var value2);
				if (value2 == 0)
				{
					modelCardList.AddByGrpId(cardPrinting.DefunctRebalancedCardLink, num);
				}
				result = true;
			}
			else if (cardPrinting.RebalancedCardLink != 0 && useRebalancedCards != cardPrinting.IsRebalanced && num != 0)
			{
				dictionary.TryGetValue(cardPrinting.RebalancedCardLink, out var value3);
				uint num2 = Math.Max(0u, num - value3);
				modelCardList.RemoveByGrpId(cardPrinting.GrpId, num);
				if (num2 != 0)
				{
					modelCardList.AddByGrpId(cardPrinting.RebalancedCardLink, num2);
				}
				result = true;
			}
		}
		return result;
	}

	private CardPrintingData SwapRebalancedCard(CardPrintingData cardPrinting, bool useRebalancedCards)
	{
		if (cardPrinting.RebalancedCardLink != 0 && useRebalancedCards != cardPrinting.IsRebalanced)
		{
			return _db.CardDataProvider.GetCardPrintingById(cardPrinting.RebalancedCardLink);
		}
		return cardPrinting;
	}

	public CardColorFlags GetCommanderColors()
	{
		CardColorFlags cardColorFlags = CardColorFlags.None;
		foreach (CardPrintingQuantity item in GetFilteredCommandZone())
		{
			cardColorFlags |= item.Printing.ColorIdentityFlags;
		}
		return cardColorFlags;
	}
}
