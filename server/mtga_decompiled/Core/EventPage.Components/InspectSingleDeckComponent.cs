using System;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Decks;

namespace EventPage.Components;

public class InspectSingleDeckComponent : EventComponent
{
	[SerializeField]
	private Transform deckBoxParent;

	private DeckViewBuilder _deckViewBuilder;

	private DeckView _deckView;

	public Action OnClick;

	public Client_Deck Deck { get; private set; }

	protected override Animator Animator
	{
		get
		{
			if (_transitionAnimator == null && _deckView != null)
			{
				_transitionAnimator = _deckView.GetComponent<Animator>();
			}
			return _transitionAnimator;
		}
	}

	public void Init(Client_Deck deck)
	{
		Deck = deck;
		_deckViewBuilder = Pantry.Get<DeckViewBuilder>();
		if (_deckView != null)
		{
			_deckViewBuilder.ReleaseDeckView(_deckView);
			_transitionAnimator = null;
		}
		_deckView = _deckViewBuilder.CreateDeckView(deck, deckBoxParent);
		_deckView.SetOnMouseOver(delegate
		{
			InspectDeckBoxesHovered();
		});
		_deckView.SetDeckOnClick(delegate
		{
			InspectDecksButtonClicked();
		});
		_deckView.ClearValidationIcons();
		_deckView.SetIsSelected(isSelected: false);
	}

	private void InspectDeckBoxesHovered()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rewards_rollover_deck, AudioManager.Default);
	}

	private void InspectDecksButtonClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deckbuilding_box_open, AudioManager.Default);
		OnClick?.Invoke();
	}
}
