using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

public class MetaCardViewPool<TCardView> : MonoBehaviour where TCardView : MetaCardView
{
	public TCardView CardViewPrefab;

	private List<TCardView> _pool = new List<TCardView>();

	protected virtual void OnCardViewInstantiated(TCardView cardView, ICardCollectionItem item, MetaCardHolder holder, Transform parent)
	{
	}

	protected virtual void OnCardViewAcquired(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, TCardView cardView, ICardCollectionItem item, MetaCardHolder holder, Transform parent, CardData previousCard)
	{
	}

	private void Start()
	{
		base.gameObject.SetActive(value: false);
	}

	public TCardView Acquire(ICardCollectionItem item, MetaCardHolder holder, Transform parent)
	{
		TCardView val = _pool.FirstOrDefault((TCardView v) => v.Card.GrpId == item.Card.GrpId);
		if (val == null)
		{
			val = _pool.FirstOrDefault();
		}
		if (val == null)
		{
			val = Object.Instantiate(CardViewPrefab);
			OnCardViewInstantiated(val, item, holder, parent);
		}
		else
		{
			_pool.Remove(val);
		}
		CardData card = val.Card;
		val.Card = item.Card;
		val.Holder = holder;
		val.transform.SetParent(parent);
		val.transform.localScale = Vector3.one;
		val.transform.localRotation = Quaternion.Euler(Vector3.zero);
		OnCardViewAcquired(holder.CardDatabase, holder.CardViewBuilder, val, item, holder, parent, card);
		return val;
	}

	public void Release(TCardView cardView)
	{
		cardView.Holder = null;
		cardView.transform.SetParent(base.transform);
		_pool.Insert(0, cardView);
	}
}
