using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Loc;

public class OpponentHandCardHolder_Handheld : OpponentHandCardHolder, ICardLock
{
	[SerializeField]
	private float _fanRadius = 23.5f;

	public bool LockCardDetails { get; set; }

	protected override void Awake()
	{
		base.Awake();
		SetHandLayout();
	}

	private void SetHandLayout()
	{
		_handLayout.Radius = _fanRadius;
		LayoutNow();
	}

	public override void HandleClick(PointerEventData eventData, CardInput cardInput)
	{
		if (OwnerIsControlledByLocalPlayer() || !LockCardDetails)
		{
			DisplayCardsInBrowser();
		}
		else
		{
			base.HandleClick(eventData, cardInput);
		}
	}

	private void DisplayCardsInBrowser()
	{
		LockCardDetails = true;
		Action<DuelScene_CDC> onCardSelected = _gameManager.InteractionSystem.HandleViewDismissCardClick;
		List<DuelScene_CDC> list = new List<DuelScene_CDC>();
		foreach (DuelScene_CDC sortedCard in _sorter.GetSortedCards())
		{
			sortedCard.HolderTypeOverride = CardHolderType.Hand;
			list.Add(sortedCard);
		}
		ViewDismissBrowserProvider viewDismissBrowserProvider = new ViewDismissBrowserProvider(list, null, Languages.ActiveLocProvider.GetLocalizedText("ZoneType_Opponent_Hand"), onCardSelected, null, base.PlayerNum)
		{
			LockCardDetails = true
		};
		IBrowser openedBrowser = _gameManager.BrowserManager.OpenBrowser(viewDismissBrowserProvider);
		viewDismissBrowserProvider.SetOpenedBrowser(openedBrowser);
	}

	protected override void HandleAddedCardReveal(DuelScene_CDC cardView)
	{
		if (!LockCardDetails)
		{
			base.HandleAddedCardReveal(cardView);
		}
	}

	protected override void RemoveCardReveal(DuelScene_CDC cardView)
	{
		if (!LockCardDetails)
		{
			base.RemoveCardReveal(cardView);
		}
	}

	public IEnumerator ClearLockCardDetails()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LockCardDetails = false;
	}
}
