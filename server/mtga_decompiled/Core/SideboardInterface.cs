using System;
using AssetLookupTree;
using Core.Code.Decks;
using Core.Code.Input;
using GreClient.Rules;
using MTGA.KeyboardManager;
using Pooling;
using SharedClientCore.SharedClientCore.Code.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

public class SideboardInterface : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	private UniversalAdditionalCameraData duelSceneCameraData;

	[Header("Deck Builder Elements")]
	[SerializeField]
	private DeckBuilderWidget _deckBuilder;

	[SerializeField]
	private CardRolloverZoomBase _zoomHandler;

	[SerializeField]
	private GameObject _deckBuilderLights;

	[SerializeField]
	private GameObject _deckBuilderBackground;

	[Header("Sideboard Elements")]
	[SerializeField]
	private SideboardNavBar _navBar;

	[SerializeField]
	private TMP_Text _sideboardIntroText;

	[SerializeField]
	private Button _showHideToggle;

	[SerializeField]
	private GameObject _viewSideboardObj;

	[SerializeField]
	private GameObject _viewBattlefieldObj;

	[SerializeField]
	private GameObject _viewBattlefieldSwitch;

	private bool _deckBuilderVisible = true;

	private Camera _gameCamera;

	public CardDatabase CardDatabase { get; private set; }

	public event Action<DeckInfo> DoneClicked;

	private void Start()
	{
		_showHideToggle.onClick.AddListener(onClicked);
		if (_viewBattlefieldSwitch != null)
		{
			SubscribeToClickViewBattlefieldEvent();
		}
		_gameCamera.enabled = true;
		duelSceneCameraData = _gameCamera.GetUniversalAdditionalCameraData();
		duelSceneCameraData.cameraStack.Add(_camera);
	}

	public void InitializeDeckBuilder(Camera gameCamera, DeckFormat currentEventFormat, DeckBuilderContext dbContext, InventoryManager inventoryManager, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, IBILogger logger, CosmeticsProvider cosmetics, KeyboardManager keyboardManager, IActionSystem actionSystem, EventManager eventManager, FormatManager formatManager, IClientLocProvider localizationManager, IUnityObjectPool unityObjectPool, IObjectPool objectPool, AssetLookupSystem assetLookupSystem, IEmoteDataProvider emoteDataProvider, ISetMetadataProvider setMetadataProvider)
	{
		_gameCamera = gameCamera;
		_zoomHandler.Initialize(cardViewBuilder, cardDatabase, localizationManager, unityObjectPool, objectPool, keyboardManager, currentEventFormat);
		Pantry.Get<DeckBuilderContextProvider>().Context = dbContext;
		_deckBuilder.Initialize(_zoomHandler, inventoryManager, cardDatabase, cardViewBuilder, null, eventManager, formatManager, assetLookupSystem, logger, cosmetics, null, null, null, null, actionSystem, emoteDataProvider, localizationManager, unityObjectPool, setMetadataProvider);
		_deckBuilder.ShowOrHide(active: true);
		_deckBuilder.DoneClicked += OnDoneClicked;
		CardDatabase = cardDatabase;
	}

	public void SetIntroText(string text)
	{
		_sideboardIntroText.text = text;
	}

	public void SetPlayerName(GREPlayerNum playerType, string name)
	{
		_navBar.SetPlayerName(playerType, name);
	}

	public void SetPlayerWins(GREPlayerNum playerType, int wins)
	{
		_navBar.SetPlayerWins(playerType, wins);
	}

	public void SetTimer(MtgTimer timer)
	{
		if (timer != null)
		{
			_navBar.SetTimerActive(isActive: true);
			_navBar.StartTimer(timer);
		}
		else
		{
			_navBar.SetTimerActive(isActive: false);
		}
	}

	private void OnDestroy()
	{
		_deckBuilder.DoneClicked -= OnDoneClicked;
		DisableShowHideButton();
		DisableViewBattlefieldToggle();
		Pantry.ResetScope(Pantry.Scope.Deckbuilder);
		Pantry.ResetScope(Pantry.Scope.Wrapper);
		duelSceneCameraData.cameraStack.Remove(_camera);
		this.DoneClicked = null;
	}

	private void onClicked()
	{
		_deckBuilderVisible = !_deckBuilderVisible;
		_deckBuilder.gameObject.UpdateActive(_deckBuilderVisible);
		_deckBuilderLights.UpdateActive(_deckBuilderVisible);
		_deckBuilderBackground.UpdateActive(_deckBuilderVisible);
		_viewSideboardObj.UpdateActive(!_deckBuilderVisible);
		_viewBattlefieldObj.UpdateActive(_deckBuilderVisible);
	}

	private void OnDoneClicked()
	{
		this.DoneClicked?.Invoke(_deckBuilder.GetDeck());
		DisableShowHideButton();
		DisableViewBattlefieldToggle();
	}

	private void DisableShowHideButton()
	{
		_showHideToggle.onClick.RemoveAllListeners();
	}

	private void SubscribeToClickViewBattlefieldEvent()
	{
		EventTrigger[] componentsInChildren = _viewBattlefieldSwitch.GetComponentsInChildren<EventTrigger>(includeInactive: true);
		foreach (EventTrigger eventTrigger in componentsInChildren)
		{
			EventTrigger.Entry entry = null;
			foreach (EventTrigger.Entry trigger in eventTrigger.triggers)
			{
				if (trigger.eventID == EventTriggerType.PointerUp)
				{
					entry = trigger;
					break;
				}
			}
			if (entry == null)
			{
				entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerUp;
				entry.callback = new EventTrigger.TriggerEvent();
				eventTrigger.triggers.Add(entry);
			}
			entry.callback.AddListener(onClicked);
		}
	}

	private void onClicked(BaseEventData arg0)
	{
		onClicked();
	}

	private void DisableViewBattlefieldToggle()
	{
		if (_viewBattlefieldSwitch == null)
		{
			return;
		}
		EventTrigger[] componentsInChildren = _viewBattlefieldSwitch.GetComponentsInChildren<EventTrigger>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (EventTrigger.Entry trigger in componentsInChildren[i].triggers)
			{
				if (trigger.eventID == EventTriggerType.PointerUp)
				{
					trigger.callback.RemoveListener(onClicked);
				}
			}
		}
	}
}
