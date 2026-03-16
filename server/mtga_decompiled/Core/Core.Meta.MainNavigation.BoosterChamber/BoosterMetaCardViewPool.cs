using System.Collections;
using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace Core.Meta.MainNavigation.BoosterChamber;

public class BoosterMetaCardViewPool
{
	private BoosterMetaCardView _cardPrefab;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private readonly Queue<BoosterMetaCardView> _cardViewPool = new Queue<BoosterMetaCardView>();

	public BoosterMetaCardViewPool(BoosterMetaCardView cardPrefab, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_cardPrefab = cardPrefab;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
	}

	public IEnumerator PreloadCards(Transform parent)
	{
		BoosterMetaCardView[] cachedCards = new BoosterMetaCardView[10];
		for (int i = 0; i < cachedCards.Length; i++)
		{
			cachedCards[i] = GetCardView();
			if (i == 0)
			{
				cachedCards[i].SetData(CardDataExtensions.CreateSkinCard(72050u, _cardDatabase, ""));
				cachedCards[i].transform.SetParent(parent);
			}
			else
			{
				cachedCards[i].SetData(CardDataExtensions.CreateBlank());
			}
			cachedCards[i].gameObject.SetActive(value: true);
		}
		yield return null;
		yield return null;
		BoosterMetaCardView[] array = cachedCards;
		foreach (BoosterMetaCardView boosterMetaCardView in array)
		{
			boosterMetaCardView.gameObject.SetActive(value: false);
			boosterMetaCardView.transform.SetParent(null);
			ReturnCardView(boosterMetaCardView);
		}
	}

	public BoosterMetaCardView GetCardView()
	{
		BoosterMetaCardView boosterMetaCardView;
		if (_cardViewPool.Count > 0)
		{
			boosterMetaCardView = _cardViewPool.Dequeue();
			boosterMetaCardView.gameObject.SetActive(value: true);
		}
		else
		{
			boosterMetaCardView = Object.Instantiate(_cardPrefab);
			boosterMetaCardView.Init(_cardDatabase, _cardViewBuilder);
		}
		return boosterMetaCardView;
	}

	public void ReturnCardView(BoosterMetaCardView cardView)
	{
		cardView.gameObject.SetActive(value: false);
		_cardViewPool.Enqueue(cardView);
	}

	public void Clear()
	{
		while (_cardViewPool.Count > 0)
		{
			BoosterMetaCardView boosterMetaCardView = _cardViewPool.Dequeue();
			if ((bool)boosterMetaCardView && (bool)boosterMetaCardView.gameObject)
			{
				Object.Destroy(boosterMetaCardView.gameObject);
			}
		}
	}
}
