using System;
using System.Collections.Generic;
using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public class FakeCardViewManager : IFakeCardViewManager, IFakeCardViewProvider, IFakeCardViewController
{
	private readonly MutableFakeCardViewProvider _provider;

	private readonly ICardBuilder<DuelScene_CDC> _cardBuilder;

	public FakeCardViewManager(MutableFakeCardViewProvider provider, ICardBuilder<DuelScene_CDC> cardBuilder)
	{
		_provider = provider ?? new MutableFakeCardViewProvider();
		_cardBuilder = cardBuilder ?? NullCardBuilder<DuelScene_CDC>.Default;
	}

	public IEnumerable<DuelScene_CDC> GetAllFakeCards()
	{
		return _provider.GetAllFakeCards();
	}

	public DuelScene_CDC GetFakeCard(string key)
	{
		return _provider.GetFakeCard(key);
	}

	public DuelScene_CDC CreateFakeCard(string key, ICardDataAdapter cardData, bool isVisible = false)
	{
		DuelScene_CDC duelScene_CDC = _cardBuilder.CreateCDC(cardData, isVisible);
		if (duelScene_CDC == null)
		{
			throw new Exception("Failed to create fake card for " + key);
		}
		return _provider.FakeCards[key] = duelScene_CDC;
	}

	public bool DeleteFakeCard(string key)
	{
		if (!_provider.FakeCards.TryGetValue(key, out var value))
		{
			return false;
		}
		if (value.CurrentCardHolder != null)
		{
			value.CurrentCardHolder.RemoveCard(value);
		}
		_cardBuilder.DestroyCDC(value);
		return _provider.FakeCards.Remove(key);
	}
}
