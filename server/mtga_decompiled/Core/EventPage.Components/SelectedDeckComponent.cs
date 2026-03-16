using System;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class SelectedDeckComponent : EventComponent
{
	[SerializeField]
	private CustomButton _selectDeckButton;

	[SerializeField]
	private RectTransform _deckBoxContainer;

	[SerializeField]
	private CustomButton _copyToDecksButton;

	private DeckView _deckBox;

	public Action OnCopyToDecksClicked;

	public Action OnSelectDeckClicked;

	protected override Animator Animator
	{
		get
		{
			if (_transitionAnimator == null && _deckBox != null)
			{
				_transitionAnimator = _deckBox.GetComponent<Animator>();
			}
			return _transitionAnimator;
		}
	}

	private void Awake()
	{
		_selectDeckButton.OnClick.AddListener(delegate
		{
			OnSelectDeckClicked?.Invoke();
		});
		_copyToDecksButton.OnClick.AddListener(delegate
		{
			OnCopyToDecksClicked?.Invoke();
			_copyToDecksButton.gameObject.SetActive(value: false);
		});
	}

	public void UpdateDeckBoxUI(Client_Deck deck, LocalizedString tooltip, bool enabled, bool showCopyButton, Action onClicked)
	{
		if (deck.Id == Guid.Empty)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		DeckViewBuilder deckViewBuilder = Pantry.Get<DeckViewBuilder>();
		if (_deckBox != null)
		{
			deckViewBuilder.ReleaseDeckView(_deckBox);
		}
		_deckBox = deckViewBuilder.CreateDeckView(deck, _deckBoxContainer);
		if (enabled)
		{
			_deckBox.SetDeckOnClick(delegate
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_deckbuilding_box_open, AudioManager.Default);
				onClicked?.Invoke();
			});
		}
		_deckBox.ClearValidationIcons();
		base.gameObject.SetActive(value: true);
		_deckBoxContainer.gameObject.UpdateActive(active: true);
		_deckBox.gameObject.UpdateActive(active: true);
		_deckBox.SetToolTipLocString(tooltip);
		_deckBox.SetIsSelected(isSelected: false);
		_copyToDecksButton.gameObject.UpdateActive(showCopyButton);
		_selectDeckButton.gameObject.UpdateActive(active: false);
	}

	public void ShowSelectDeckButton()
	{
		base.gameObject.SetActive(value: true);
		_selectDeckButton.gameObject.UpdateActive(active: true);
		_copyToDecksButton.gameObject.UpdateActive(active: false);
		_deckBoxContainer.gameObject.UpdateActive(active: false);
	}
}
