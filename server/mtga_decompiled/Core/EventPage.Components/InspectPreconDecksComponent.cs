using System;
using System.Collections.Generic;
using UnityEngine;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class InspectPreconDecksComponent : EventComponent
{
	[SerializeField]
	private MetaDeckView[] _metaDeckViews;

	[SerializeField]
	private CustomButton _deckContainerButton;

	[SerializeField]
	private CustomButton _inspectDecksButton;

	[SerializeField]
	private int _middleDeckViewIndex = 1;

	public Action OnClick;

	private void Awake()
	{
		_deckContainerButton.OnMouseover.AddListener(InspectDeckBoxesHovered);
		_deckContainerButton.OnClick.AddListener(InspectDeckBoxesClicked);
		_inspectDecksButton.OnClick.AddListener(InspectDecksButtonClicked);
	}

	public void SetDecks(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, List<Client_Deck> decks)
	{
		if (decks.Count == 1)
		{
			_metaDeckViews[_middleDeckViewIndex].gameObject.SetActive(value: true);
			_metaDeckViews[_middleDeckViewIndex].Init(cardDatabase, cardViewBuilder, decks[0]);
			if (_middleDeckViewIndex > 0)
			{
				_metaDeckViews[_middleDeckViewIndex - 1].gameObject.SetActive(value: false);
			}
			if (_middleDeckViewIndex < _metaDeckViews.Length - 1)
			{
				_metaDeckViews[_middleDeckViewIndex + 1].gameObject.SetActive(value: false);
			}
		}
		else if (decks.Count == 2)
		{
			_metaDeckViews[_middleDeckViewIndex].gameObject.UpdateActive(active: false);
			if (_middleDeckViewIndex > 0)
			{
				_metaDeckViews[_middleDeckViewIndex - 1].Init(cardDatabase, cardViewBuilder, decks[0]);
				_metaDeckViews[_middleDeckViewIndex - 1].gameObject.SetActive(value: true);
			}
			if (_middleDeckViewIndex < _metaDeckViews.Length - 1)
			{
				_metaDeckViews[_middleDeckViewIndex + 1].Init(cardDatabase, cardViewBuilder, decks[1]);
				_metaDeckViews[_middleDeckViewIndex + 1].gameObject.SetActive(value: true);
			}
		}
		else
		{
			for (int i = 0; i < _metaDeckViews.Length && i < decks.Count; i++)
			{
				_metaDeckViews[i].gameObject.SetActive(value: true);
				_metaDeckViews[i].Init(cardDatabase, cardViewBuilder, decks[i]);
			}
		}
	}

	private void InspectDeckBoxesHovered()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_rollover_deck, AudioManager.Default);
	}

	private void InspectDeckBoxesClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deckbuilding_box_open, AudioManager.Default);
		OnClick?.Invoke();
	}

	private void InspectDecksButtonClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		OnClick?.Invoke();
	}
}
