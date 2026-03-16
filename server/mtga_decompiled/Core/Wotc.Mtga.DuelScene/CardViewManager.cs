using System;
using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class CardViewManager : ICardViewManager, ICardViewProvider, ICardViewController
{
	private readonly MutableCardViewProvider _provider;

	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	private readonly Dictionary<uint, uint> _previousIdMap = new Dictionary<uint, uint>();

	public CardViewManager(MutableCardViewProvider provider, ICardBuilder<DuelScene_CDC> cardBuilder)
	{
		_provider = provider ?? new MutableCardViewProvider();
		_cardBuilder = cardBuilder ?? NullCardBuilder<DuelScene_CDC>.Default;
	}

	public DuelScene_CDC GetCardView(uint cardId)
	{
		return _provider.GetCardView(cardId);
	}

	public IEnumerable<DuelScene_CDC> GetAllCards()
	{
		return _provider.GetAllCards();
	}

	public bool TryGetCardView(uint cardId, out DuelScene_CDC cardView)
	{
		return _provider.TryGetCardView(cardId, out cardView);
	}

	public DuelScene_CDC CreateCardView(ICardDataAdapter cardData)
	{
		if (cardData.InstanceId != 0 && _provider.CardViews.ContainsKey(cardData.InstanceId) && _provider.CardViews[cardData.InstanceId] != null)
		{
			Debug.LogWarning("Trying to create duplicate CardView w/ id of " + cardData.InstanceId);
			return _provider.CardViews[cardData.InstanceId];
		}
		DuelScene_CDC duelScene_CDC = _cardBuilder.CreateCDC(cardData);
		if (duelScene_CDC == null)
		{
			throw new Exception("No cardview for id: " + cardData.InstanceId);
		}
		if (cardData.InstanceId != 0)
		{
			_provider.CardViews[cardData.InstanceId] = duelScene_CDC;
			_provider.AllCards.Add(duelScene_CDC);
			_provider.AllCards.RemoveAll((DuelScene_CDC x) => x == null || x.gameObject == null);
		}
		return duelScene_CDC;
	}

	public DuelScene_CDC UpdateIdForCardView(uint oldId, uint newId)
	{
		if (_provider.CardViews.ContainsKey(oldId) && !_provider.CardViews.ContainsKey(newId))
		{
			_provider.CardViews.Add(newId, _provider.CardViews[oldId]);
			_provider.CardViews.Remove(oldId);
			if (newId > oldId)
			{
				_previousIdMap[newId] = oldId;
			}
			else
			{
				_previousIdMap.Remove(oldId);
			}
		}
		if (!_provider.CardViews.TryGetValue(newId, out var value))
		{
			if (!_provider.CardViews.TryGetValue(oldId, out var value2))
			{
				return null;
			}
			return value2;
		}
		return value;
	}

	public uint GetCardUpdatedId(uint id)
	{
		if (_provider.CardViews.ContainsKey(id))
		{
			return id;
		}
		foreach (KeyValuePair<uint, uint> item in _previousIdMap)
		{
			if (item.Value == id)
			{
				return item.Key;
			}
		}
		return id;
	}

	public uint GetCardPreviousId(uint id)
	{
		if (_previousIdMap.TryGetValue(id, out var value))
		{
			return value;
		}
		return id;
	}

	public void DeleteCard(params uint[] cardIds)
	{
		List<DuelScene_CDC> list = new List<DuelScene_CDC>();
		foreach (uint key in cardIds)
		{
			_previousIdMap.Remove(key);
			if (_provider.CardViews.ContainsKey(key))
			{
				DuelScene_CDC duelScene_CDC = _provider.CardViews[key];
				if (duelScene_CDC != null)
				{
					list.Add(duelScene_CDC);
				}
				_provider.AllCards.Remove(duelScene_CDC);
				_provider.CardViews.Remove(key);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		foreach (DuelScene_CDC item in list)
		{
			if (item.CurrentCardHolder != null)
			{
				item.CurrentCardHolder.RemoveCard(item);
			}
			_cardBuilder.DestroyCDC(item);
		}
		_provider.AllCards.RemoveAll((DuelScene_CDC x) => x == null || x.gameObject == null);
	}
}
