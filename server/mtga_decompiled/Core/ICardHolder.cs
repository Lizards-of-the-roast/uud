using System.Collections.Generic;
using MovementSystem;
using UnityEngine;

public interface ICardHolder
{
	int Layer { get; }

	ICardLayout Layout { get; set; }

	GREPlayerNum PlayerNum { get; }

	CardHolderType CardHolderType { get; }

	List<DuelScene_CDC> CardViews { get; }

	float CardScale { get; }

	bool EnableShadows { get; }

	Transform CardRoot { get; }

	bool IgnoreDummyCards { get; set; }

	void AddCard(DuelScene_CDC cardView);

	void RemoveCard(DuelScene_CDC cardView);

	void OnCardUpdated(DuelScene_CDC cardView);

	void SwapCards(int cardIndexOne, int cardIndexTwo);

	void ShiftCards(int cardIndex, int targetIndex);

	int GetIndexForCard(DuelScene_CDC cardView);

	int GetClosestCardIndexToPosition(float cardLocalX);

	void LayoutNow();

	IdealPoint GetLayoutEndpoint(DuelScene_CDC cardView);

	void SetCardAdded(DuelScene_CDC cardView);
}
