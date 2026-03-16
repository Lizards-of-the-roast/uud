using System;
using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class CardsComponent : EventComponent
{
	[Header("UI")]
	[SerializeField]
	private CustomButton _inspectCardsButton;

	[SerializeField]
	private Localize _headerText;

	[SerializeField]
	private Localize _bodyText;

	[Header("Card")]
	[SerializeField]
	private List<Transform> _cardAnchors;

	public Action OnClick;

	private void Awake()
	{
		_inspectCardsButton.OnClick.AddListener(delegate
		{
			OnClick?.Invoke();
		});
	}

	public void CreateCards(CardData[] cardData, string headerKey, string bodyKey, CardViewBuilder cardViewBuilder)
	{
		foreach (Transform cardAnchor in _cardAnchors)
		{
			cardAnchor.DestroyChildren();
		}
		for (int i = 0; i < cardData.Length; i++)
		{
			cardViewBuilder.CreateMetaCdc(cardData[i], _cardAnchors[i]);
		}
		if (string.IsNullOrWhiteSpace(headerKey))
		{
			_headerText.gameObject.UpdateActive(active: false);
		}
		else
		{
			_headerText.gameObject.UpdateActive(active: true);
			_headerText.SetText(new MTGALocalizedString
			{
				Key = headerKey
			});
		}
		if (string.IsNullOrWhiteSpace(bodyKey))
		{
			_bodyText.gameObject.UpdateActive(active: false);
			return;
		}
		_bodyText.gameObject.UpdateActive(active: true);
		_bodyText.SetText(new MTGALocalizedString
		{
			Key = bodyKey
		});
	}

	public void InspectCards_OnClick()
	{
		OnClick?.Invoke();
	}
}
