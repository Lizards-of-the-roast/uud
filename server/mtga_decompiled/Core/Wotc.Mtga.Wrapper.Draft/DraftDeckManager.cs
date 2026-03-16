using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using SharedClientCore.SharedClientCore.Code.Utilities;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftDeckManager
{
	private readonly ICardDatabaseAdapter _adapter;

	private readonly Deck _deck;

	private readonly DictionaryList<DraftPackCardView, DeckBuilderPile?> _reservedCards = new DictionaryList<DraftPackCardView, DeckBuilderPile?>();

	private readonly DictionaryList<DraftPackCardView, bool> _lockedReservedCards = new DictionaryList<DraftPackCardView, bool>();

	private int _numCardsToPick = 1;

	private readonly bool _showReservedCardsInDeckAndSideboard = true;

	public DraftDeckManager(ICardDatabaseAdapter adapter)
	{
		_adapter = adapter;
		_deck = new Deck(adapter)
		{
			Name = Languages.ActiveLocProvider.GetLocalizedText("Draft/Draft_Deck_Title"),
			Main = new CardCollection(_adapter),
			Sideboard = new CardCollection(_adapter),
			DeckArtId = 0u
		};
	}

	public void SetNumCardsToPick(int numCardsToPick)
	{
		_numCardsToPick = numCardsToPick;
	}

	public void Clear()
	{
		_deck.Main = new CardCollection(_adapter);
		_deck.Sideboard = new CardCollection(_adapter);
		_deck.DeckArtId = 0u;
		_reservedCards.Clear();
		_lockedReservedCards.Clear();
	}

	public Deck GetDeck()
	{
		return _deck;
	}

	public Deck GetDeckForVisuals()
	{
		Deck deck = new Deck(_adapter)
		{
			Main = new CardCollection(_adapter, _deck.Main),
			Sideboard = new CardCollection(_adapter, _deck.Sideboard),
			Name = _deck.Name,
			DeckArtId = _deck.DeckArtId
		};
		if (!_showReservedCardsInDeckAndSideboard)
		{
			return deck;
		}
		foreach (var (draftPackCardView2, flag2) in _lockedReservedCards)
		{
			if (flag2)
			{
				if (_reservedCards[draftPackCardView2] == DeckBuilderPile.MainDeck)
				{
					deck.Main.Add(draftPackCardView2.Card, 1);
				}
				else
				{
					deck.Sideboard.Add(draftPackCardView2.Card, 1);
				}
			}
		}
		return deck;
	}

	public DictionaryList<DraftPackCardView, DeckBuilderPile?> GetReservedCards(bool filterLocked = false)
	{
		DictionaryList<DraftPackCardView, DeckBuilderPile?> dictionaryList = new DictionaryList<DraftPackCardView, DeckBuilderPile?>();
		if (filterLocked)
		{
			foreach (KeyValuePair<DraftPackCardView, DeckBuilderPile?> reservedCard in _reservedCards)
			{
				if (!_lockedReservedCards[reservedCard.Key])
				{
					dictionaryList.Add(reservedCard.Key, reservedCard.Value);
				}
			}
		}
		else
		{
			dictionaryList.AddRange(_reservedCards);
		}
		return dictionaryList;
	}

	public bool IsCardAlreadyReserved(DraftPackCardView cardView)
	{
		return _reservedCards.ContainsKey(cardView);
	}

	public int ReservedCardCount()
	{
		return _reservedCards.Count;
	}

	public void UpdateReservedDestination(DraftPackCardView cardView, DeckBuilderPile destination)
	{
		if (_reservedCards.ContainsKey(cardView))
		{
			_reservedCards[cardView] = destination;
		}
	}

	public void UpdateLockReservation(DraftPackCardView cardView, bool lockedIn)
	{
		if (_lockedReservedCards.ContainsKey(cardView))
		{
			_lockedReservedCards[cardView] = lockedIn;
		}
	}

	public bool AtMaxReservedCards()
	{
		return _reservedCards.Count >= _numCardsToPick;
	}

	public bool AllReservedCardsLocked()
	{
		return _lockedReservedCards.All((KeyValuePair<DraftPackCardView, bool> kvp) => kvp.Value);
	}

	public bool AnyReservedForSideboard()
	{
		return _reservedCards.Any((KeyValuePair<DraftPackCardView, DeckBuilderPile?> r) => r.Value == DeckBuilderPile.Sideboard);
	}

	public bool MoveCardInDeck(CardData card, DeckBuilderPile destinationPile)
	{
		foreach (KeyValuePair<DraftPackCardView, DeckBuilderPile?> reservedCard in _reservedCards)
		{
			reservedCard.Deconstruct(out var key, out var value);
			DraftPackCardView draftPackCardView = key;
			DeckBuilderPile? deckBuilderPile = value;
			if (draftPackCardView.Card.GrpId == card.GrpId)
			{
				value = deckBuilderPile;
				if (value != destinationPile)
				{
					_reservedCards[draftPackCardView] = destinationPile;
					return true;
				}
			}
		}
		int num = _deck.Main.Quantity(card);
		int num2 = _deck.Sideboard.Quantity(card);
		if (destinationPile == DeckBuilderPile.MainDeck && num2 > 0)
		{
			_deck.Sideboard.Add(card, -1);
			_deck.Main.Add(card, 1);
		}
		else
		{
			if (DeckBuilderPile.Sideboard != destinationPile || num <= 0)
			{
				return false;
			}
			_deck.Main.Add(card, -1);
			_deck.Sideboard.Add(card, 1);
		}
		return true;
	}

	public bool TryAddReservedCard(DraftPackCardView cardView, DeckBuilderPile? pile, bool lockedIn = false)
	{
		if (_reservedCards.ContainsKey(cardView))
		{
			return false;
		}
		if (_reservedCards.Count >= _numCardsToPick)
		{
			DraftPackCardView draftPackCardView = null;
			foreach (var (draftPackCardView3, flag2) in _lockedReservedCards)
			{
				if (!flag2)
				{
					draftPackCardView = draftPackCardView3;
					break;
				}
			}
			if (draftPackCardView == null)
			{
				return false;
			}
			_reservedCards.Remove(draftPackCardView);
			_lockedReservedCards.Remove(draftPackCardView);
		}
		_reservedCards.Add(cardView, pile);
		_lockedReservedCards.Add(cardView, lockedIn);
		return true;
	}

	public bool TryRemoveReservedCard(DraftPackCardView cardView)
	{
		if (_reservedCards.Count == 0 || !_reservedCards.ContainsKey(cardView))
		{
			return false;
		}
		_reservedCards.Remove(cardView);
		_lockedReservedCards.Remove(cardView);
		return true;
	}

	public void CommitPicks(DictionaryList<DraftPackCardView, DeckBuilderPile?> pickedCards)
	{
		foreach (KeyValuePair<DraftPackCardView, DeckBuilderPile?> pickedCard in pickedCards)
		{
			if (_deck.DeckArtId == 0)
			{
				_deck.DeckArtId = pickedCard.Key.Card.Printing.ArtId;
			}
			if (pickedCard.Value == DeckBuilderPile.MainDeck)
			{
				_deck.Main.Add(pickedCard.Key.Card, 1);
			}
			else
			{
				_deck.Sideboard.Add(pickedCard.Key.Card, 1);
			}
		}
		_reservedCards.Clear();
		_lockedReservedCards.Clear();
	}

	public void UpdateDeckFromServer(CardCollection main, CardCollection sideboard)
	{
		_deck.Main = new CardCollection(_adapter, main);
		_deck.Sideboard = new CardCollection(_adapter, sideboard);
		ICardCollectionItem cardCollectionItem = _deck.Main.FirstOrDefault();
		if (cardCollectionItem != null)
		{
			_deck.DeckArtId = cardCollectionItem.Card.Printing.ArtId;
		}
		_reservedCards.Clear();
		_lockedReservedCards.Clear();
	}
}
