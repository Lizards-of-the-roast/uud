using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Enums.Deck;
using Wizards.Arena.Promises;
using Wizards.MDN.DeckManager;
using Wizards.Models;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class DecksManager : IDeckNameProvider, IDeckSleeveProvider
{
	private readonly CardDatabase _cardDatabase;

	private readonly InventoryManager _inventoryManager;

	private readonly CosmeticsProvider _cosmeticsProvider;

	private DeckDataProvider _deckDataProvider;

	private const string DefaultCardBackName = "CardBack_Default";

	private int _deckLimit = 75;

	public bool DeckLimitReached => _deckDataProvider.GetCachedDecks().Count((Client_Deck deck) => !deck.Summary.IsNetDeck) >= GetDeckLimit();

	public static DecksManager Create()
	{
		return new DecksManager(Pantry.Get<DeckDataProvider>(), Pantry.Get<CardDatabase>(), Pantry.Get<InventoryManager>(), Pantry.Get<CosmeticsProvider>());
	}

	public DecksManager(DeckDataProvider dataProvider, CardDatabase cardDatabase, InventoryManager inventoryManager, CosmeticsProvider cosmetics)
	{
		_deckDataProvider = dataProvider;
		_cosmeticsProvider = cosmetics;
		_cardDatabase = cardDatabase;
		_inventoryManager = inventoryManager;
		_inventoryManager.Subscribe(InventoryUpdateSource.MercantileBoosterPurchase, OnInventoryUpdate_General);
		_inventoryManager.Subscribe(InventoryUpdateSource.MercantileChestPurchase, OnInventoryUpdate_General);
		_inventoryManager.Subscribe(InventoryUpdateSource.CosmeticPurchase, OnInventoryUpdate_General);
		_inventoryManager.Subscribe(InventoryUpdateSource.CatalogPurchase, OnInventoryUpdate_General);
		_inventoryManager.Subscribe(InventoryUpdateSource.ModifyPlayerInventory, OnInventoryUpdate_General);
		_inventoryManager.Subscribe(InventoryUpdateSource.OpenChest, OnInventoryUpdate_General);
		_inventoryManager.Subscribe(InventoryUpdateSource.QuestReward, OnInventoryUpdate_General);
		_inventoryManager.SubscribeToAll(_deckDataProvider.OnInventoryUpdate);
	}

	public void OnDestroy()
	{
		_inventoryManager.UnSubscribe(InventoryUpdateSource.MercantileBoosterPurchase, OnInventoryUpdate_General);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.MercantileChestPurchase, OnInventoryUpdate_General);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.CosmeticPurchase, OnInventoryUpdate_General);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.CatalogPurchase, OnInventoryUpdate_General);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.ModifyPlayerInventory, OnInventoryUpdate_General);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.OpenChest, OnInventoryUpdate_General);
		_inventoryManager.UnSubscribe(InventoryUpdateSource.QuestReward, OnInventoryUpdate_General);
		_inventoryManager.UnsubscribeFromAll(_deckDataProvider.OnInventoryUpdate);
	}

	public void OnInventoryUpdate_General(ClientInventoryUpdateReportItem update)
	{
		if (update?.delta?.artSkinsAdded != null && update.delta.artSkinsAdded.Length != 0 && MDNPlayerPrefs.AutoApplyCardStyles)
		{
			PAPA.StartGlobalCoroutine(Coroutine_UpdateAllDecksWithSkins(update.delta.artSkinsAdded));
		}
	}

	public string GetDefaultSleeve()
	{
		string playerCardbackSelection = _cosmeticsProvider.PlayerCardbackSelection;
		if (!string.IsNullOrEmpty(playerCardbackSelection))
		{
			return playerCardbackSelection;
		}
		return "CardBack_Default";
	}

	private IEnumerator Coroutine_UpdateAllDecksWithSkins(ArtSkin[] skins)
	{
		yield break;
	}

	public IEnumerator Coroutine_UpdateAllDecksWithDefaultSleeve()
	{
		string defaultSleeve = GetDefaultSleeve();
		foreach (Client_Deck cachedDeck in _deckDataProvider.GetCachedDecks())
		{
			if (cachedDeck.Summary.CardBack == defaultSleeve)
			{
				cachedDeck.Summary.CardBack = null;
				yield return _deckDataProvider.UpdateDeck(cachedDeck, DeckActionType.Updated).AsCoroutine();
			}
		}
	}

	public void SetDeckLimit(int limit)
	{
		_deckLimit = limit;
	}

	public int GetDeckLimit()
	{
		return _deckLimit;
	}

	public bool ShowDeckLimitError()
	{
		if (!DeckLimitReached)
		{
			return false;
		}
		SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Limit_Reached_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Limit_Reached", ("deckLimit", GetDeckLimit().ToString())));
		return true;
	}

	public Client_Deck GetDeck(Guid deckId)
	{
		return _deckDataProvider.GetDeckForId(deckId);
	}

	public Client_Deck GetDeckFromDescription(string description)
	{
		if (!string.IsNullOrWhiteSpace(description))
		{
			return _deckDataProvider.GetCachedDecks().FirstOrDefault((Client_Deck d) => d.Summary.Description == description);
		}
		return null;
	}

	public IEnumerable<string> GetAllDeckNames()
	{
		return _deckDataProvider.GetDeckNames();
	}

	public bool DeckNameAlreadyExists(Guid deckId, string newName)
	{
		foreach (Client_Deck cachedDeck in _deckDataProvider.GetCachedDecks())
		{
			if (cachedDeck.Id != deckId && !cachedDeck.Summary.IsNetDeck && string.Compare(cachedDeck.Summary.Name, newName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
		}
		return false;
	}

	public Promise<Client_Deck> GetFullDeck(Guid id)
	{
		return _deckDataProvider.GetFullDeck(id);
	}

	public Promise<Client_DeckSummary> CreateDeck(Client_Deck deck, string action, bool forceCopy = false)
	{
		if (forceCopy || _deckDataProvider.HasDeck(deck.Id))
		{
			Client_Deck client_Deck = new Client_Deck(deck);
			client_Deck.Summary.DeckId = Guid.NewGuid();
			client_Deck.Summary.Name = WrapperDeckUtilities.GetUniqueName(client_Deck.Summary.Name, GetAllDeckNames());
			client_Deck.Summary.Description = "";
			client_Deck.Summary.LastUpdated = DateTime.Now;
			deck = client_Deck;
		}
		return _deckDataProvider.CreateDeck(deck, action);
	}

	public Promise<Client_DeckSummary> UpdateDeck(Client_Deck deck, DeckActionType action)
	{
		return _deckDataProvider.UpdateDeck(deck, action);
	}

	public Promise<bool> DeleteDeck(Guid id)
	{
		return _deckDataProvider.DeleteDeck(id);
	}

	public Promise<List<Client_Deck>> GetAllDecks()
	{
		return _deckDataProvider.GetAllDecks();
	}

	public List<Client_Deck> GetAllCachedDecks()
	{
		return _deckDataProvider.GetCachedDecks();
	}

	public Promise<List<Client_DeckSummary>> ForceRefreshCachedDecks()
	{
		return _deckDataProvider.ForceRefreshCachedDecks();
	}
}
