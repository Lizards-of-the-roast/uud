using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Decks;
using Core.Code.Promises;
using Core.Shared.Code.Providers;
using UnityEngine;
using UnityEngine.Events;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Extensions;

public class DeckSelectBlade : MonoBehaviour
{
	[Serializable]
	public class MetaDeckViewUEvent : UnityEvent<MetaDeckView>
	{
	}

	[SerializeField]
	private Transform _deckSelectorParent;

	[SerializeField]
	private CustomButton _editDeckButton;

	private DeckViewBuilder _deckViewBuilder;

	private DeckViewSelector _deckViewSelector;

	private Client_Deck _selectedInDeckSelectorBlade;

	private CustomButton _newDeckButtonInstance;

	private PlayBladeController _playBladeController;

	private ContentControllerObjectives _objectivesReferenceForAnimation;

	private bool _shouldAnimateObjectiveTrack = true;

	private Action _onHideCallback;

	private EventContext _eventContext;

	private DeckFormat _deckFormat;

	public UnityEvent UEvOnBackgroundClicked = new UnityEvent();

	public MetaDeckViewUEvent UEvOnDeckClicked = new MetaDeckViewUEvent();

	private IAccountClient Account => WrapperController.Instance.AccountClient;

	public bool IsShowing { get; private set; }

	private DeckViewBuilder DeckViewBuilder => _deckViewBuilder ?? (_deckViewBuilder = Pantry.Get<DeckViewBuilder>());

	private IEmergencyCardBansProvider EmergencyCardBansProvider => Pantry.Get<IEmergencyCardBansProvider>();

	private void Big_OnMouseover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public void Init(ContentControllerObjectives objectivesPanel, AssetLookupSystem assetLookupSystem, PlayBladeController playBladeController)
	{
		_objectivesReferenceForAnimation = objectivesPanel;
		assetLookupSystem.Blackboard.Clear();
		DeckViewSelectorPrefab payload = assetLookupSystem.TreeLoader.LoadTree<DeckViewSelectorPrefab>().GetPayload(assetLookupSystem.Blackboard);
		_deckViewSelector = AssetLoader.Instantiate(payload.Prefab, _deckSelectorParent);
		_deckViewSelector.Initialize(DeckView_OnClick, DeckView_OnDoubleClick);
		_deckViewSelector.SetSort(DeckViewSortType.LastModified);
		_deckViewSelector.GetComponent<RectTransform>().StretchToParent();
		_deckViewSelector.ShowCreateDeckButton(NewDeckButton_OnClick);
		_playBladeController = playBladeController;
	}

	private void Awake()
	{
		_editDeckButton.OnMouseover.AddListener(Big_OnMouseover);
	}

	private void OnDestroy()
	{
		_editDeckButton.OnMouseover.RemoveListener(Big_OnMouseover);
		_playBladeController = null;
		_deckViewSelector = null;
		_selectedInDeckSelectorBlade = null;
	}

	private void NewDeckButton_OnClick()
	{
		DeckBuilderContext deckBuilderContext = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(_deckFormat.NewDeck(WrapperController.Instance.DecksManager)), _eventContext, sideboarding: false, firstEdit: true, DeckBuilderMode.DeckBuilding, ambiguousFormat: false, _playBladeController.CurrentChallengeId);
		deckBuilderContext.Format = _deckFormat;
		SceneLoader.GetSceneLoader().GoToDeckBuilder(deckBuilderContext);
	}

	public void Edit_OnClick()
	{
		if (_selectedInDeckSelectorBlade != null)
		{
			EditDeck(_selectedInDeckSelectorBlade);
		}
		else
		{
			EditDeck(_playBladeController.SelectedDeckInfo);
		}
	}

	private void DeckView_OnClick(DeckViewInfo deckView)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deck_select, base.gameObject);
		UpdateSelectedDeckView();
		string userAccountID = Account.AccountInformation?.PersonaID;
		string internalEventName = _eventContext.PlayerEvent.EventInfo.InternalEventName;
		if (deckView != null)
		{
			Client_Deck client_Deck = new Client_Deck(deckView.deck.Summary, deckView.deck.Contents);
			MDNPlayerPrefs.SetSelectedDeckId(userAccountID, internalEventName, client_Deck.Id.ToString());
			_playBladeController.SetDeck(client_Deck, _deckFormat);
			_selectedInDeckSelectorBlade = client_Deck;
		}
		else
		{
			MDNPlayerPrefs.SetSelectedDeckId(userAccountID, internalEventName, "");
			_playBladeController.SetDeck(null, _deckFormat);
		}
	}

	private void DeckView_OnDoubleClick(DeckViewInfo deckView)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deck_select, base.gameObject);
		_playBladeController.SetDeck(new Client_Deck(deckView.deck.Summary, deckView.deck.Contents), _deckFormat);
		EditDeck(_playBladeController.SelectedDeckInfo);
	}

	public void Show(EventContext eventContext, DeckFormat deckFormat, Action onHide, bool allowRefresh = false)
	{
		_eventContext = eventContext;
		_deckFormat = deckFormat;
		base.gameObject.SetActive(value: true);
		_onHideCallback = onHide;
		bool flag = WrapperController.Instance == null || (WrapperController.Instance.DecksManager?.DeckLimitReached ?? true);
		if (_newDeckButtonInstance != null)
		{
			_newDeckButtonInstance.gameObject.SetActive(!flag);
		}
		string selectedDeckId = MDNPlayerPrefs.GetSelectedDeckId(Account.AccountInformation?.PersonaID, _eventContext.PlayerEvent.EventInfo.InternalEventName);
		if (allowRefresh && IsShowing)
		{
			RefreshDecks(selectedDeckId);
		}
		else
		{
			LoadDecks(selectedDeckId);
		}
		_playBladeController.SetDeckBoxSelected(isSelected: true);
		if (_shouldAnimateObjectiveTrack)
		{
			_objectivesReferenceForAnimation.AnimateOutro();
		}
		IsShowing = true;
	}

	public void Hide()
	{
		if (IsShowingAndActive())
		{
			IsShowing = false;
			_deckViewSelector.ClearDecks();
			base.gameObject.SetActive(value: false);
			_playBladeController.SetDeckBoxSelected(isSelected: false);
			if (_shouldAnimateObjectiveTrack)
			{
				_objectivesReferenceForAnimation.AnimateIntro();
			}
			_onHideCallback?.Invoke();
		}
	}

	private bool IsShowingAndActive()
	{
		if (!IsShowing)
		{
			return base.gameObject.activeSelf;
		}
		return true;
	}

	private void LoadDecks(string selectedDeckId)
	{
		WrapperController.Instance.DecksManager.GetAllDecks().ThenOnMainThread(delegate(Promise<List<Client_Deck>> decks)
		{
			List<DeckViewInfo> decks2 = decks.Result.Select((Client_Deck _) => DeckViewBuilder.CreateDeckViewInfoFromDeckSummary(_)).ToList();
			_deckViewSelector.SetDecks(decks2, allowUnownedCards: false);
			RefreshDecks(selectedDeckId);
		});
	}

	private void RefreshDecks(string selectedDeckId)
	{
		_deckViewSelector.SelectDeck(selectedDeckId);
		_deckViewSelector.SetFormat(_deckFormat.FormatName, allowUnownedCards: false);
		UpdateSelectedDeckView();
	}

	private void UpdateSelectedDeckView()
	{
		_editDeckButton.Interactable = _playBladeController != null && _playBladeController.SelectedDeckInfo != null;
	}

	private void EditDeck(Client_Deck deck)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_done, base.gameObject);
		DeckBuilderContext deckBuilderContext = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(deck), _eventContext, sideboarding: false, firstEdit: false, challengeId: _playBladeController.CurrentChallengeId, startingMode: (_eventContext.PlayerEvent.EventInfo.IsPreconEvent || deck.Summary.IsNetDeck) ? DeckBuilderMode.ReadOnly : DeckBuilderMode.DeckBuilding);
		deckBuilderContext.Format = _deckFormat ?? WrapperController.Instance.FormatManager.GetSafeFormat(_eventContext.PlayerEvent.EventUXInfo.DeckSelectFormat ?? deck.Summary.Format);
		SceneLoader.GetSceneLoader().GoToDeckBuilder(deckBuilderContext);
	}

	public void SetShouldAnimateObjectiveTrack(bool isVisible)
	{
		_shouldAnimateObjectiveTrack = isVisible;
	}
}
