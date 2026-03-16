using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Cards.Database;

public class CardCollection : IEnumerable<ICardCollectionItem>, IEnumerable
{
	private readonly ICardDatabaseAdapter _cardDb;

	private List<CardCollectionItem> _itemList;

	private Dictionary<uint, CardCollectionItem> _itemTable;

	public int Count => _itemList.Count;

	public ICardCollectionItem this[uint i]
	{
		get
		{
			if (!_itemTable.TryGetValue(i, out var value))
			{
				return null;
			}
			return value;
		}
	}

	public List<ListMetaCardViewDisplayInformation> ToTJDisplay(bool sort)
	{
		IEnumerable<ListMetaCardViewDisplayInformation> enumerable = _itemList.Select((CardCollectionItem x) => new ListMetaCardViewDisplayInformation
		{
			Banned = false,
			Invalid = false,
			Quantity = (uint)x.Quantity,
			Card = x.Card.Printing,
			Unowned = false,
			SkinCode = x.Card.Instance?.SkinCode
		});
		if (sort)
		{
			enumerable = CardSorter.Sort(enumerable, _cardDb, SortType.LandLast, SortType.CMCWithXLast, SortType.ColorOrder, SortType.Title);
		}
		return enumerable.ToList();
	}

	public List<CardPrintingQuantity> ToCardPrintingQuantityList()
	{
		List<CardPrintingQuantity> list = new List<CardPrintingQuantity>();
		foreach (CardCollectionItem item in _itemList)
		{
			CardPrintingQuantity cardPrintingQuantity = new CardPrintingQuantity();
			cardPrintingQuantity.Printing = item.Card.Printing;
			cardPrintingQuantity.Quantity = (uint)item.Quantity;
			list.Add(cardPrintingQuantity);
		}
		return list;
	}

	public CardCollection(ICardDatabaseAdapter cardDb)
	{
		_cardDb = cardDb;
		_itemList = new List<CardCollectionItem>();
		_itemTable = new Dictionary<uint, CardCollectionItem>();
	}

	public CardCollection(ICardDatabaseAdapter cardDb, int capacity)
	{
		_cardDb = cardDb;
		_itemList = new List<CardCollectionItem>(capacity);
		_itemTable = new Dictionary<uint, CardCollectionItem>(capacity);
	}

	public CardCollection(ICardDatabaseAdapter cardDb, IEnumerable<ICardCollectionItem> original)
	{
		_cardDb = cardDb;
		_itemList = new List<CardCollectionItem>();
		_itemTable = new Dictionary<uint, CardCollectionItem>();
		Add(original);
	}

	public int Quantity(CardData card)
	{
		if (_itemTable.TryGetValue(card.GrpId, out var value))
		{
			return value.Quantity;
		}
		return 0;
	}

	public int Add(CardData card, int quantityToAdd, bool forceAddIfZero = false)
	{
		if (_itemTable.TryGetValue(card.GrpId, out var value))
		{
			if (quantityToAdd > 0)
			{
				int num = ((value.Quantity > 0) ? (int.MaxValue - value.Quantity) : int.MaxValue);
				if (quantityToAdd > num)
				{
					quantityToAdd = num;
				}
			}
			else if (quantityToAdd < 0)
			{
				int num2 = ((value.Quantity < 0) ? (int.MinValue - value.Quantity) : int.MinValue);
				if (quantityToAdd < num2)
				{
					quantityToAdd = num2;
				}
			}
			int num3 = value.Quantity + quantityToAdd;
			value.Quantity = (forceAddIfZero ? num3 : Math.Max(0, num3));
			return value.Quantity;
		}
		if (forceAddIfZero || quantityToAdd > 0)
		{
			value = new CardCollectionItem(card, quantityToAdd);
			_itemTable.Add(card.GrpId, value);
			_itemList.Add(value);
			return quantityToAdd;
		}
		return 0;
	}

	public void Add(IEnumerable<ICardCollectionItem> original)
	{
		using IEnumerator<ICardCollectionItem> enumerator = original.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Add(enumerator.Current.Card, enumerator.Current.Quantity, forceAddIfZero: true);
		}
	}

	public void Remove(IEnumerable<ICardCollectionItem> other, bool forceAddIfZero = false)
	{
		foreach (ICardCollectionItem item in other)
		{
			Add(item.Card, -item.Quantity, forceAddIfZero);
		}
	}

	public IEnumerator<ICardCollectionItem> GetEnumerator()
	{
		return _itemList.Cast<ICardCollectionItem>().GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _itemList.Cast<ICardCollectionItem>().GetEnumerator();
	}
}
