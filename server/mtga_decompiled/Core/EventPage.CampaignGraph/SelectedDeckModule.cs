using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using Core.Code.Input;
using Core.Code.Promises;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.Events;
using Wizards.Arena.Enums.Deck;
using Wizards.Arena.Enums.UILayout;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Format;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace EventPage.CampaignGraph;

public class SelectedDeckModule : EventModule
{
	[SerializeField]
	private CustomButton _selectDeckButton;

	[SerializeField]
	private RectTransform _deckBoxContainer;

	[SerializeField]
	private CustomButton _copyToDecksButton;

	private DeckView _deckBox;

	private DeckDataProvider _deckDataProvider;

	private IAccountClient _accountClient;

	private DeckViewBuilder _deckViewBuilder;

	private DecksManager _decksManager => WrapperController.Instance.DecksManager;

	private FormatManager _formatManager => WrapperController.Instance.FormatManager;

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
		_selectDeckButton.OnClick.AddListener(DeckSelectButtonClicked);
		_copyToDecksButton.OnClick.AddListener(AddToDecksButtonClicked);
	}

	public override void Init(EventTemplate parentTemplate, KeyboardManager keyboardManager, IActionSystem actionSystem, AssetLookupSystem assetLookupSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		base.Init(parentTemplate, keyboardManager, actionSystem, assetLookupSystem, cardDatabase, cardViewBuilder);
		_deckDataProvider = Pantry.Get<DeckDataProvider>();
		_accountClient = Pantry.Get<IAccountClient>();
		_deckViewBuilder = Pantry.Get<DeckViewBuilder>();
	}

	public override void Show()
	{
		_copyToDecksButton.gameObject.SetActive(value: false);
		_selectDeckButton.gameObject.SetActive(value: false);
		_deckBoxContainer.gameObject.SetActive(value: false);
		if (base.EventContext.PlayerEvent.UILayoutOptions != null)
		{
			base.gameObject.SetActive(value: true);
		}
		else if (string.IsNullOrEmpty(base.EventContext.PlayerEvent.EventUXInfo.DeckSelectFormat) && !base.EventContext.PlayerEvent.EventInfo.IsPreconEvent)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public override void UpdateModule()
	{
		if (_deckBox != null)
		{
			_deckViewBuilder.ReleaseDeckView(_deckBox);
			_deckBox = null;
		}
		PlayerEventModule currentModule = base.EventContext.PlayerEvent.CourseData.CurrentModule;
		bool flag = base.EventContext.PlayerEvent.GetTimerState() == EventTimerState.ClosedAndCompleted;
		bool flag2 = FormatUtilities.IsLimited(base.EventContext.PlayerEvent.EventInfo.FormatType);
		bool flag3 = currentModule == PlayerEventModule.ClaimPrize;
		LayoutDeckButtonBehavior deckButtonBehavior = base.EventContext.PlayerEvent.DeckButtonBehavior;
		Client_Deck client_Deck = base.EventContext.PlayerEvent.CourseData.CourseDeck;
		bool num = currentModule == PlayerEventModule.DeckSelect || currentModule == PlayerEventModule.Choice;
		bool flag4 = client_Deck != null && !string.IsNullOrWhiteSpace(client_Deck.Summary.Name) && ((!flag && base.EventContext.PlayerEvent.InPlayingMatchesModule) || (flag3 && flag2));
		if (num)
		{
			if (deckButtonBehavior == LayoutDeckButtonBehavior.Fixed || deckButtonBehavior == LayoutDeckButtonBehavior.Selectable)
			{
				_selectDeckButton.gameObject.SetActive(value: true);
			}
		}
		else
		{
			if (!flag4)
			{
				return;
			}
			_deckBoxContainer.gameObject.SetActive(value: true);
			if (base.EventContext.PlayerEvent.ShowCopyDecksButton)
			{
				_copyToDecksButton.gameObject.SetActive(value: true);
			}
			switch (deckButtonBehavior)
			{
			case LayoutDeckButtonBehavior.Selectable:
			{
				if (base.EventContext.PlayerEvent.EventInfo.IsPreconEvent)
				{
					client_Deck.Summary.Name = Utils.GetLocalizedDeckName(client_Deck.Summary.Name);
					showDeckBoxButton(client_Deck, "MainNav/EventPage/Button_SelectDeck", enabled: true, DeckSelectButtonClicked);
					break;
				}
				bool flag5 = false;
				foreach (Client_Deck cachedDeck in _deckDataProvider.GetCachedDecks())
				{
					if (client_Deck.Id == cachedDeck.Id)
					{
						client_Deck = cachedDeck;
						flag5 = true;
						break;
					}
				}
				if (flag5)
				{
					client_Deck.Summary.Name = Utils.GetLocalizedDeckName(client_Deck.Summary.Name);
					showDeckBoxButton(client_Deck, "MainNav/EventPage/Button_SelectDeck", enabled: true, DeckSelectButtonClicked);
					SubmitDeck(client_Deck);
				}
				else
				{
					base.EventContext.PlayerEvent.CourseData.CurrentModule = PlayerEventModule.DeckSelect;
					_parentTemplate.UpdateTemplate();
					_deckBox.gameObject.UpdateActive(active: false);
				}
				break;
			}
			case LayoutDeckButtonBehavior.Fixed:
				client_Deck.Summary.Name = Utils.GetLocalizedDeckName(client_Deck.Summary.Name);
				showDeckBoxButton(client_Deck, "MainNav/ConstructedDeckSelect/Tooltip_InspectDeck", enabled: true, DeckInspectButtonClicked);
				break;
			case LayoutDeckButtonBehavior.Editable:
				showDeckBoxButton(client_Deck, "MainNav/EventPage/Tooltip_ClickToEditDeck", enabled: true, DeckBuildButtonClicked);
				break;
			default:
				showDeckBoxButton(client_Deck, "MainNav/General/Empty_String", enabled: false, null);
				break;
			}
		}
		void showDeckBoxButton(Client_Deck deck, LocalizedString tooltip, bool enabled, UnityAction onClick)
		{
			_deckBox = _deckViewBuilder.CreateDeckView(deck, _deckBoxContainer);
			if (enabled)
			{
				_deckBox.SetDeckOnClick(delegate
				{
					onClick();
				});
			}
			_deckBox.ClearValidationIcons();
			_deckBox.SetIsSelected(isSelected: false);
			_deckBox.SetToolTipLocString(tooltip);
		}
	}

	public override void Hide()
	{
		if (_deckBox != null)
		{
			_deckViewBuilder.ReleaseDeckView(_deckBox);
			_deckBox = null;
		}
		base.gameObject.SetActive(value: false);
	}

	private void SubmitDeck(Client_Deck eventDeck)
	{
		WrapperDeckUtilities.setLastPlayed(eventDeck);
		base.EventContext.PlayerEvent.SubmitEventDeck(WrapperDeckUtilities.GetSubmitDeck(eventDeck, _decksManager)).ThenOnMainThreadIfError(delegate(Error error)
		{
			SimpleLog.LogError(error.Message);
			Utils.GetDeckSubmissionErrorMessages(error, out var errTitle, out var errText);
			SystemMessageManager.Instance.ShowOk(errTitle, errText, delegate
			{
				base.EventContext.PlayerEvent.CourseData.CurrentModule = PlayerEventModule.DeckSelect;
				_parentTemplate.UpdateTemplate();
				_deckBox.gameObject.UpdateActive(active: false);
			});
		});
	}

	private IEnumerator Coroutine_CreateDeck(Client_Deck deck)
	{
		Promise<Client_DeckSummary> createRequest = _decksManager.CreateDeck(deck, DeckActionType.CopyLimited.ToString());
		yield return createRequest.AsCoroutine();
		if (createRequest.Successful)
		{
			WrapperDeckBuilder.ClearCachedDeck();
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Add_Deck_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/Add_Deck_Text"));
		}
		else
		{
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Add_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Deck_Add_Error_Text"));
		}
	}

	private void AddToDecksButtonClicked()
	{
		if (base.EventContext.PlayerEvent.CourseData.CourseDeck != null)
		{
			Client_Deck client_Deck = new Client_Deck(base.EventContext.PlayerEvent.CourseData.CourseDeck);
			DeckFormat deckFormat = base.EventContext.PlayerEvent.Format;
			if (deckFormat == null)
			{
				deckFormat = _formatManager.GetSafeFormat(client_Deck.Summary.Format, null);
			}
			DeckUtilities.UpdateDeckBasedOnEventFormat(deckFormat, client_Deck);
			StartCoroutine(Coroutine_CreateDeck(client_Deck));
		}
		_copyToDecksButton.gameObject.SetActive(value: false);
	}

	private void DeckSelectButtonClicked()
	{
		SceneLoader.GetSceneLoader().GoToConstructedDeckSelect(new DeckSelectContext
		{
			EventContext = base.EventContext
		});
	}

	private void DeckInspectButtonClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_deckbuilding_box_open, AudioManager.Default);
		DeckBuilderContext deckBuilderContext = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(base.EventContext.PlayerEvent.CourseData.CourseDeck), base.EventContext, sideboarding: false, firstEdit: false, DeckBuilderMode.ReadOnly);
		deckBuilderContext.Format = _formatManager.GetSafeFormat(base.EventContext.PlayerEvent.EventUXInfo.DeckSelectFormat);
		SceneLoader.GetSceneLoader().GoToDeckBuilder(deckBuilderContext);
	}

	private void DeckBuildButtonClicked()
	{
		Client_Deck courseDeck = base.EventContext.PlayerEvent.CourseData.CourseDeck;
		if (courseDeck != null)
		{
			DeckBuilderContext context = new DeckBuilderContext(DeckServiceWrapperHelpers.ToAzureModel(courseDeck), base.EventContext);
			SceneLoader.GetSceneLoader().GoToDeckBuilder(context);
			return;
		}
		DeckInfo deckInfo = new DeckInfo
		{
			id = Guid.NewGuid(),
			format = base.EventContext.PlayerEvent.EventUXInfo.DeckSelectFormat,
			deckTileId = base.EventContext.PlayerEvent.CourseData.CardPool.First()
		};
		DeckBuilderContext context2 = new DeckBuilderContext(deckInfo, base.EventContext, sideboarding: false, firstEdit: true);
		List<CardInDeck> list = (from x in base.EventContext.PlayerEvent.CourseData.CardPool
			group x by x into x
			select new CardInDeck(x.Key, (uint)x.Count()) into x
			where x.Quantity != 0
			select x).ToList();
		switch (base.EventContext.PlayerEvent.EventInfo.FormatType)
		{
		case MDNEFormatType.Draft:
			deckInfo.name = Languages.ActiveLocProvider.GetLocalizedText("MainNav/EventPage/DefaultDraftDeckName");
			deckInfo.mainDeck = list;
			deckInfo.sideboard = new List<CardInDeck>();
			SceneLoader.GetSceneLoader().GoToDeckBuilder(context2);
			break;
		case MDNEFormatType.Sealed:
			deckInfo.name = Languages.ActiveLocProvider.GetLocalizedText("MainNav/EventPage/DefaultSealedDeckName");
			deckInfo.mainDeck = new List<CardInDeck>();
			deckInfo.sideboard = list;
			if (MDNPlayerPrefs.GetHasOpenedSealedPool(_accountClient.AccountInformation?.PersonaID, base.EventContext.PlayerEvent.CourseData.Id))
			{
				SceneLoader.GetSceneLoader().GoToDeckBuilder(context2);
			}
			break;
		default:
			Debug.LogWarningFormat("Unknown deck select format: {0}", base.EventContext.PlayerEvent.EventUXInfo.DeckSelectFormat);
			deckInfo.name = "Deck";
			deckInfo.mainDeck = list;
			deckInfo.sideboard = new List<CardInDeck>();
			SceneLoader.GetSceneLoader().GoToDeckBuilder(context2);
			break;
		}
	}
}
