using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Decks;
using Core.Code.Input;
using MTGA.KeyboardManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

public class ConstructedDeckSelectController : NavContentController, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	[SerializeField]
	private TMP_Text FormatLabel;

	[SerializeField]
	private Transform _deckSelectorParent;

	[SerializeField]
	private CustomButton _okButton;

	[SerializeField]
	private CustomButton _editButton;

	[SerializeField]
	private Localize _editButtonLocalize;

	[SerializeField]
	private CustomButton _backButton;

	private Promise<List<Client_Deck>> _getDecksRequest;

	private DeckViewSelector _deckSelectorInstance;

	private DeckSelectContext _deckSelectContext;

	private AssetLookupSystem _assetLookupSystem;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private DeckDataProvider _deckDataProvider;

	private List<Client_Deck> _decks;

	private Client_Deck _selectedDeck;

	private bool _selectedDeckInvalidForEvent;

	public override NavContentType NavContentType => NavContentType.ConstructedDeckSelect;

	private DecksManager DM => WrapperController.Instance.DecksManager;

	private EventContext _event => _deckSelectContext.EventContext;

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper;

	public override bool IsReadyToShow
	{
		get
		{
			if (_getDecksRequest != null)
			{
				return _getDecksRequest.IsDone;
			}
			return false;
		}
	}

	private void Awake()
	{
		_okButton.OnMouseover.AddListener(Big_OnMouseover);
		_okButton.OnClick.AddListener(OnOk);
		_editButton.OnMouseover.AddListener(Big_OnMouseover);
		_editButton.OnClick.AddListener(OnEdit);
		_backButton.OnClick.AddListener(OnBack);
	}

	private void OnDestroy()
	{
		_okButton.OnMouseover.RemoveListener(Big_OnMouseover);
		_okButton.OnClick.RemoveListener(OnOk);
		_editButton.OnMouseover.RemoveListener(Big_OnMouseover);
		_editButton.OnClick.RemoveListener(OnEdit);
		_backButton.OnClick.RemoveListener(OnBack);
	}

	public void Init(AssetLookupSystem assetLookupSystem, KeyboardManager keyboardManager, IActionSystem actionSystem)
	{
		_assetLookupSystem = assetLookupSystem;
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_deckDataProvider = Pantry.Get<DeckDataProvider>();
	}

	public override void Activate(bool active)
	{
		if (active)
		{
			_keyboardManager?.Subscribe(this);
			_actionSystem.PushFocus(this);
			CreateInstantiatedControls();
			StartCoroutine(Coroutine_LoadDecks());
			OnLocalize();
			Languages.LanguageChangedSignal.Listeners += OnLocalize;
		}
		else
		{
			_keyboardManager?.Unsubscribe(this);
			_actionSystem.PopFocus(this);
			FormatLabel.enabled = false;
			_deckSelectorInstance.ClearDecks();
			_getDecksRequest = null;
			Languages.LanguageChangedSignal.Listeners -= OnLocalize;
		}
	}

	private void OnLocalize()
	{
		if (_event != null)
		{
			FormatLabel.enabled = true;
			if (_event.PlayerEvent.EventInfo.IsPreconEvent)
			{
				string titleLocKey = _event.PlayerEvent.EventUXInfo.TitleLocKey;
				FormatLabel.text = Languages.ActiveLocProvider.GetLocalizedText(titleLocKey);
				_okButton.gameObject.UpdateActive(_event.DeckSelectContext != EventContext.DeckSelectSceneContext.InspectDeck);
				_editButton.gameObject.UpdateActive(active: true);
				_editButton.Interactable = true;
				_editButtonLocalize.SetText("MainNav/DeckManager/Button_View");
			}
			else
			{
				FormatLabel.text = WrapperController.Instance.FormatManager.GetSafeFormat(_event.PlayerEvent.EventUXInfo.DeckSelectFormat).GetLocalizedName();
				_editButton.gameObject.UpdateActive(active: true);
				_editButtonLocalize.SetText("MainNav/ConstructedDeckSelect/Button_EditDecks");
			}
		}
		else
		{
			FormatLabel.enabled = false;
		}
	}

	public void SetDeckSelectContext(DeckSelectContext context)
	{
		_deckSelectContext = context;
	}

	public void Big_OnMouseover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	private void CreateInstantiatedControls()
	{
		if (_deckSelectorInstance == null)
		{
			_assetLookupSystem.Blackboard.Clear();
			DeckViewSelectorPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<DeckViewSelectorPrefab>().GetPayload(_assetLookupSystem.Blackboard);
			_deckSelectorInstance = AssetLoader.Instantiate(payload.Prefab, _deckSelectorParent);
			_deckSelectorInstance.Initialize(DeckView_OnClick, DeckView_OnDoubleClick, null, _event.PlayerEvent.EventInfo.IsPreconEvent);
			_deckSelectorInstance.SetSort(DeckViewSortType.Default);
			_deckSelectorInstance.GetComponent<RectTransform>().StretchToParent();
		}
		ScrollRect componentInChildren = _deckSelectorInstance.GetComponentInChildren<ScrollRect>();
		ScrollFade component = FormatLabel.GetComponent<ScrollFade>();
		if (componentInChildren != null && component != null)
		{
			component.ScrollView = componentInChildren;
		}
	}

	private void UpdateSelectedDeckView(DeckViewInfo deckViewInfo)
	{
		bool flag = deckViewInfo != null;
		bool flag2 = false;
		DeckDisplayInfo deckDisplayInfo = null;
		DeckFormat safeFormat = WrapperController.Instance.FormatManager.GetSafeFormat(_event.PlayerEvent.EventUXInfo.DeckSelectFormat);
		if (flag)
		{
			deckDisplayInfo = deckViewInfo.GetValidationForFormat(safeFormat.FormatName);
		}
		if (flag && deckDisplayInfo.ValidationResult != null)
		{
			bool num = deckDisplayInfo.ValidationResult.NumberBannedCards == 0 && deckDisplayInfo.ValidationResult.NumberEmergencyBannedCards == 0 && deckDisplayInfo.ValidationResult.NumberOfInvalidCards == 0 && deckDisplayInfo.ValidationResult.NumberNonFormatCard == 0 && !deckDisplayInfo.IsMalformed;
			bool flag3 = !deckDisplayInfo.ValidationResult.HasUnOwnedCards || _event.PlayerEvent.EventInfo.AllowUncollectedCards;
			flag2 = num && flag3;
			_selectedDeckInvalidForEvent = !flag2 && deckViewInfo.DeckFormat != safeFormat.FormatName;
		}
		else
		{
			flag2 = flag && deckDisplayInfo.IsValid;
		}
		_okButton.Interactable = flag2;
		_editButton.Interactable = flag;
		if (_event.PlayerEvent.EventInfo.IsPreconEvent)
		{
			_okButton.gameObject.UpdateActive(_event.DeckSelectContext != EventContext.DeckSelectSceneContext.InspectDeck);
		}
		else
		{
			_okButton.gameObject.UpdateActive(active: true);
		}
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			OnBack();
			return true;
		}
		return false;
	}

	private IEnumerator Coroutine_SubmitDeck(Client_Deck deck)
	{
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
		if (_event.PlayerEvent.EventInfo.IsPreconEvent)
		{
			Promise<ICourseInfoWrapper> submitDeck = _event.PlayerEvent.SubmitEventDeckFromChoices(deck);
			yield return submitDeck.AsCoroutine();
			if (submitDeck.Successful)
			{
				SceneLoader.GetSceneLoader().GoToEventScreen(_event);
			}
			else
			{
				Utils.GetDeckSubmissionErrorMessages(submitDeck.Error, out var errTitle, out var errText);
				SystemMessageManager.Instance.ShowOk(errTitle, errText);
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			}
		}
		else
		{
			WrapperDeckUtilities.setLastPlayed(deck);
			Promise<Client_Deck> submitDeck2 = _event.PlayerEvent.SubmitEventDeck(WrapperDeckUtilities.GetSubmitDeck(deck, DM));
			yield return submitDeck2.AsCoroutine();
			if (!submitDeck2.Successful)
			{
				Debug.LogError(submitDeck2.Error.Message);
				Utils.GetDeckSubmissionErrorMessages(submitDeck2.Error, out var errTitle2, out var errText2);
				SystemMessageManager.Instance.ShowOk(errTitle2, errText2);
				SceneLoader.GetSceneLoader().GoToLanding(new HomePageContext());
			}
			else
			{
				SceneLoader.GetSceneLoader().GoToEventScreen(_event);
			}
		}
		SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
	}

	private IEnumerator Coroutine_LoadDecks()
	{
		_getDecksRequest = _deckDataProvider.GetAllDecks();
		if (!_getDecksRequest.IsDone)
		{
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: true);
			yield return _getDecksRequest.AsCoroutine();
			SceneLoader.GetSceneLoader().EnableLoadingIndicator(shouldEnable: false);
		}
		if (!_getDecksRequest.Successful)
		{
			SceneLoader.GetSceneLoader().ShowConnectionFailedMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Decks_Get_Failure_Text"), allowRetry: true, exitInsteadOfLogout: true);
			yield break;
		}
		DeckFormat eventFormat = WrapperController.Instance.FormatManager.GetSafeFormat(_event.PlayerEvent.EventUXInfo.DeckSelectFormat);
		if (_event?.PlayerEvent?.EventInfo?.IsPreconEvent == true)
		{
			_decks = new List<Client_Deck>();
			if (_deckSelectContext.PreconDeckIds != null)
			{
				Promise<Dictionary<Guid, Client_Deck>> promise = WrapperController.Instance.PreconDeckManager.EnsurePreconDecks();
				yield return promise.AsCoroutine();
				foreach (Guid preconDeckId in _deckSelectContext.PreconDeckIds)
				{
					if (promise.Result.TryGetValue(preconDeckId, out var value))
					{
						_decks.Add(value);
					}
				}
			}
			_deckSelectorInstance.HideCreateDeckButton();
		}
		else
		{
			_decks = new List<Client_Deck>(_getDecksRequest.Result);
			_deckSelectorInstance.ShowCreateDeckButton(OnCreateDeck);
		}
		List<DeckViewInfo> decks = Pantry.Get<DeckViewBuilder>().CreateDeckViewInfos(_decks);
		bool valueOrDefault = _event?.PlayerEvent?.EventInfo?.AllowUncollectedCards == true;
		_deckSelectorInstance.SetDecks(decks, valueOrDefault);
		_deckSelectorInstance.SetFormat(eventFormat.FormatName, valueOrDefault, _event?.PlayerEvent?.EventInfo?.IsPreconEvent == true);
		_selectedDeck = null;
		UpdateSelectedDeckView(null);
	}

	private void OnOk()
	{
		if (_event.DeckIsFixed)
		{
			SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("AbilityHanger/PlayWarning/Header_Undefined"), Languages.ActiveLocProvider.GetLocalizedText("Events/DeckLockDisclaimer"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Shared/Cancel"), null, Languages.ActiveLocProvider.GetLocalizedText("MainNav/ConstructedDeckSelect/Button_Submit"), SubmitSelectedDeck);
		}
		else
		{
			SubmitSelectedDeck();
		}
		void SubmitSelectedDeck()
		{
			StartCoroutine(Coroutine_SubmitDeck(_selectedDeck));
		}
	}

	private void OnCreateDeck()
	{
		if (!DM.ShowDeckLimitError())
		{
			DeckBuilderContext context = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(WrapperController.Instance.FormatManager.GetSafeFormat(_event.PlayerEvent.EventUXInfo.DeckSelectFormat).NewDeck(WrapperController.Instance.DecksManager)), _event, sideboarding: false, firstEdit: true);
			SceneLoader.GetSceneLoader().GoToDeckBuilder(context);
		}
	}

	private void OnEdit()
	{
		EditOrViewDeck(_selectedDeck);
	}

	private void EditOrViewDeck(Client_Deck deck)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deck_open, base.gameObject);
		DeckInfo deck2 = DeckServiceWrapperHelpers.ToAzureModel(deck);
		EventContext evt = _event;
		int startingMode = ((_event.PlayerEvent.EventInfo.IsPreconEvent || deck.Summary.IsNetDeck) ? 2 : 0);
		DeckSelectContext deckSelectContext = (_event.PlayerEvent.EventInfo.IsPreconEvent ? _deckSelectContext : null);
		string deckSelectFormat = _event.PlayerEvent.EventUXInfo.DeckSelectFormat;
		bool selectedDeckInvalidForEvent = _selectedDeckInvalidForEvent;
		DeckBuilderContext deckBuilderContext = new DeckBuilderContext(deck2, evt, sideboarding: false, firstEdit: false, (DeckBuilderMode)startingMode, ambiguousFormat: false, default(Guid), null, null, deckSelectContext, cachingEnabled: false, isPlayblade: false, deckSelectFormat, selectedDeckInvalidForEvent);
		deckBuilderContext.Format = WrapperController.Instance.FormatManager.GetSafeFormat(_event.PlayerEvent.EventUXInfo.DeckSelectFormat ?? deck.Summary.Format);
		SceneLoader.GetSceneLoader().GoToDeckBuilder(deckBuilderContext);
	}

	public void OnBack(ActionContext context)
	{
		OnBack();
	}

	public void OnBack()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_back, base.gameObject);
		SceneLoader.GetSceneLoader().GoToEventScreen(_event);
	}

	private void DeckView_OnClick(DeckViewInfo deckViewInfo)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deck_select, base.gameObject);
		if (deckViewInfo != null)
		{
			_selectedDeck = _decks.Find(deckViewInfo.deckId, (Client_Deck d, Guid deckViewDeckId) => d.Id == deckViewDeckId);
		}
		UpdateSelectedDeckView(deckViewInfo);
	}

	private void DeckView_OnDoubleClick(DeckViewInfo deckViewInfo)
	{
		EditOrViewDeck(_selectedDeck);
	}
}
