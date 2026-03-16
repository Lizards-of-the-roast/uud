using System;
using System.Collections.Generic;
using MovementSystem;
using UnityEngine;

public class NoCardHolder : ICardHolder
{
	public static readonly ICardHolder Default = new NoCardHolder();

	public int Layer => 0;

	public ICardLayout Layout
	{
		get
		{
			throw new InvalidOperationException("Trying to get the layout a non-existent card holder!!");
		}
		set
		{
			throw new InvalidOperationException("Trying to set the layout a non-existent card holder!!");
		}
	}

	public GREPlayerNum PlayerNum => GREPlayerNum.Invalid;

	public CardHolderType CardHolderType => CardHolderType.None;

	public List<DuelScene_CDC> CardViews => new List<DuelScene_CDC>();

	public float CardScale => 0f;

	public bool EnableShadows => false;

	public Transform CardRoot
	{
		get
		{
			throw new NotImplementedException("Should never be grabbing the root of NoCardHolder");
		}
	}

	public bool EnableAutoLayout
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public bool IgnoreDummyCards { get; set; }

	public bool LockCardDetails { get; set; }

	public void AddCard(DuelScene_CDC cardView)
	{
	}

	public void SetCardAdded(DuelScene_CDC cardView)
	{
	}

	public void RemoveCard(DuelScene_CDC cardView)
	{
	}

	public virtual void OnCardUpdated(DuelScene_CDC cardView)
	{
	}

	public void SwapCards(int cardIndexOne, int cardIndexTwo)
	{
	}

	public void ShiftCards(int cardIndex, int targetIndex)
	{
	}

	public int GetIndexForCard(DuelScene_CDC cardView)
	{
		return 0;
	}

	public int GetClosestCardIndexToPosition(float cardLocalX)
	{
		return 0;
	}

	public void LayoutNow()
	{
	}

	public IdealPoint GetLayoutEndpoint(DuelScene_CDC cardView)
	{
		return default(IdealPoint);
	}
}
