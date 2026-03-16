using System.Collections.Generic;
using Wotc.Mtga.Cards.Database;

public class Deck
{
	private CardCollection _main;

	private CardCollection _sideboard;

	public CardCollection Main
	{
		get
		{
			return _main;
		}
		set
		{
			if (_main != value)
			{
				_main = value;
			}
		}
	}

	public List<uint> MainDeckIds
	{
		get
		{
			List<uint> list = new List<uint>();
			foreach (ICardCollectionItem item in Main)
			{
				int quantity = item.Quantity;
				for (int i = 0; i < quantity; i++)
				{
					list.Add(item.Card.GrpId);
				}
			}
			return list;
		}
	}

	public CardCollection Sideboard
	{
		get
		{
			return _sideboard;
		}
		set
		{
			if (_sideboard != value)
			{
				_sideboard = value;
			}
		}
	}

	public List<uint> SideboardIds
	{
		get
		{
			List<uint> list = new List<uint>();
			foreach (ICardCollectionItem item in Sideboard)
			{
				int quantity = item.Quantity;
				for (int i = 0; i < quantity; i++)
				{
					list.Add(item.Card.GrpId);
				}
			}
			return list;
		}
	}

	public uint DeckTileId { get; set; }

	public uint DeckArtId { get; set; }

	public string Name { get; set; }

	public Deck(ICardDatabaseAdapter cardDb)
	{
		Main = new CardCollection(cardDb);
		Sideboard = new CardCollection(cardDb);
		DeckTileId = 0u;
	}
}
