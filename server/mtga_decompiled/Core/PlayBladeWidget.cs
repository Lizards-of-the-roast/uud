using System;
using Core.Meta.MainNavigation.Challenge;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public abstract class PlayBladeWidget : MonoBehaviour
{
	[Header("General References")]
	[SerializeField]
	protected PlayBladeController _playBlade;

	[SerializeField]
	protected CustomButton _mainButton;

	[SerializeField]
	protected Localize _mainButtonText;

	[SerializeField]
	protected CustomButton _secondaryButton;

	[SerializeField]
	protected Localize _secondaryButtonText;

	[Header("Deck References")]
	[SerializeField]
	protected CustomButton _selectDeckSlot;

	[SerializeField]
	protected Localize _selectDeckHandheldText;

	[SerializeField]
	protected Transform _deckBoxParent;

	private DeckViewBuilder _deckViewBuilder;

	protected (Client_Deck, DeckView)? _selectedDeckAndView;

	protected ContentControllerObjectives _objectivesController;

	public Guid CurrentChallengeId;

	protected PVPChallengeController _challengeController;

	public bool IsDeckSelected => _selectedDeckAndView.HasValue;

	public Client_Deck SelectedDeck => _selectedDeckAndView?.Item1;

	public DeckView SelectedDeckView => _selectedDeckAndView?.Item2;

	public void SetMainButton(bool enable, string locKey = "")
	{
		_mainButton.gameObject.UpdateActive(enable);
		if (string.IsNullOrEmpty(locKey))
		{
			locKey = "MainNav/Landing/Play";
		}
		_mainButtonText.SetText(locKey);
	}

	protected bool TryGetCurrentChallengeData(out PVPChallengeData challengeData)
	{
		challengeData = _challengeController.GetChallengeData(CurrentChallengeId);
		return challengeData != null;
	}

	public abstract void Show();

	public virtual bool Hide()
	{
		return true;
	}

	public abstract DeckFormat GetDeckFormat();

	protected void InitChallengeController()
	{
		_challengeController = Pantry.Get<PVPChallengeController>();
	}

	public void Initialize(ContentControllerObjectives objectivesController)
	{
		base.gameObject.SetActive(value: true);
		_objectivesController = objectivesController;
		base.gameObject.SetActive(value: false);
	}

	private void SetDeckBox(Client_Deck deck)
	{
		if (_deckViewBuilder == null)
		{
			_deckViewBuilder = Pantry.Get<DeckViewBuilder>();
		}
		if (SelectedDeckView != null)
		{
			_deckViewBuilder.ReleaseDeckView(SelectedDeckView);
			_selectedDeckAndView = null;
		}
		if (deck != null)
		{
			DeckView deckView = _deckViewBuilder.CreateDeckView(deck, _deckBoxParent);
			deckView.SetDeckOnClick(delegate
			{
				OnSelectDeckClicked();
			});
			deckView.SetToolTipLocString("MainNav/HomePage/EventBlade/Tooltip_ChooseDeck");
			_selectedDeckAndView = (deck, deckView);
		}
	}

	public abstract void SetChallengeDeck(Client_Deck deck, DeckFormat deckFormat);

	public virtual bool ProceedWithHide()
	{
		return true;
	}

	protected virtual void SetDeck(Client_Deck deck, DeckFormat deckFormat)
	{
		SetDeckBox(deck);
		if (IsDeckSelected)
		{
			SelectedDeckView.UpdateDisplayForFormat(deckFormat.FormatName, allowUnowned: false);
		}
	}

	public void SetDeckBoxSelected(bool isSelected)
	{
		if (IsDeckSelected)
		{
			SelectedDeckView.SetIsSelected(isSelected);
		}
	}

	protected abstract void OnSelectDeckClicked();

	protected abstract void OnMainOrSecondaryButtonClicked();
}
