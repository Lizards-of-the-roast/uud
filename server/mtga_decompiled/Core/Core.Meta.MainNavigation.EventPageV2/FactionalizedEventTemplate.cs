using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using EventPage;
using EventPage.Components;
using UnityEngine;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Unification.Models.Events;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

namespace Core.Meta.MainNavigation.EventPageV2;

public class FactionalizedEventTemplate : NavContentController
{
	private AssetLookupSystem _assetLookupSystem;

	private SceneLoader _sceneLoader;

	private SharedEventPageClasses _sharedClasses;

	private EventPageScaffolding _factionSelectEventScaffolding;

	private TextComponent _currentFactionDescription;

	private MainButtonComponent _purchaseButtonsComponent;

	private MainButtonComponentController _purchaseButtonsController;

	private BoosterPacksComponent _currentFactionPacks;

	private TitleRankComponent _titleRankComponent;

	private LossDetailsComponent _lossDetailsComponent;

	private FactionalizedEventBlade _factionBlade;

	private ObjectiveTrackComponent _objectiveTrackComponent;

	private Dictionary<string, FactionEventContext> _eventContexts = new Dictionary<string, FactionEventContext>();

	private string _currentEventContextKey = "";

	private bool _isInitialized;

	private const uint GenericPack = 0u;

	private EventPageComponentFactory _factory;

	public override NavContentType NavContentType { get; } = NavContentType.FactionalizedEvent;

	public void Init(AssetLookupSystem assetLookupSystem, SharedEventPageClasses sharedClasses)
	{
		_assetLookupSystem = assetLookupSystem;
		_sceneLoader = SceneLoader.GetSceneLoader();
		_sharedClasses = sharedClasses;
		_factory = new EventPageComponentFactory(sharedClasses, this);
	}

	public void SetEvent(EventContext eventContext)
	{
		string eventId = eventContext.PlayerEvent.EventInfo.EventId;
		if (!(_currentEventContextKey == eventId))
		{
			if (!_eventContexts.ContainsKey(eventId))
			{
				_eventContexts.Add(eventId, InitializeFactionEventContext(eventId, eventContext));
			}
			_currentEventContextKey = eventId;
		}
	}

	private FactionEventContext InitializeFactionEventContext(string key, EventContext eventContext)
	{
		OneTimeInitialization();
		FactionEventContext factionEventContext = new FactionEventContext
		{
			EventId = key,
			EventContext = eventContext
		};
		foreach (FactionSealedUXInfo item in eventContext.PlayerEvent.EventUXInfo.FactionSealedUXInfo)
		{
			factionEventContext.FactionInfo.Add(item.FactionInternalName, item);
		}
		_factionSelectEventScaffolding.gameObject.name = "FactionSelect - " + eventContext.PlayerEvent?.EventUXInfo?.PublicEventName;
		factionEventContext.PostPurchaseScaffolding = AssetLoader.Instantiate<EventPageScaffolding>(_sharedClasses.AssetLookupSystem.GetPrefabPath<EventScaffoldingPrefab, EventPageScaffolding>(), base.transform);
		factionEventContext.PostPurchaseComponentManager = new EventComponentManager(_sharedClasses, eventContext);
		_factory.CreateComponents(factionEventContext.PostPurchaseComponentManager, factionEventContext.PostPurchaseScaffolding.LayoutGroups, factionEventContext.PostPurchaseScaffolding.safeArea);
		_titleRankComponent.SetText(eventContext.PlayerEvent?.EventUXInfo?.EventComponentData?.TitleRankText?.LocKey ?? "");
		EventComponentManager postPurchaseComponentManager = factionEventContext.PostPurchaseComponentManager;
		postPurchaseComponentManager.OnJoinPayEvent = (Action<bool>)Delegate.Combine(postPurchaseComponentManager.OnJoinPayEvent, new Action<bool>(OnJoinPaySuccess));
		EventComponentManager postPurchaseComponentManager2 = factionEventContext.PostPurchaseComponentManager;
		postPurchaseComponentManager2.OnPrizeClaim = (Action<bool>)Delegate.Combine(postPurchaseComponentManager2.OnPrizeClaim, new Action<bool>(ActivateFactionScene));
		_lossDetailsComponent.CreateLossSlots((eventContext.PlayerEvent?.EventUXInfo?.EventComponentData?.LossDetailsDisplay.Games).GetValueOrDefault());
		factionEventContext.LossController = new LossDetailsComponentController(_lossDetailsComponent, eventContext.PlayerEvent?.EventUXInfo?.EventComponentData?.LossDetailsDisplay);
		factionEventContext.ObjectiveTrackController = new ObjectiveTrackComponentController(_objectiveTrackComponent, eventContext.PlayerEvent?.EventUXInfo?.EventComponentData?.ByCourseObjectiveTrack, _factionSelectEventScaffolding.safeArea, _sharedClasses.CardDatabase, _sharedClasses.CardViewBuilder, _sharedClasses.CardMaterialBuilder);
		_currentFactionDescription.SetText("Events/FactionSealed/EventDescription");
		Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
		foreach (FactionCollation item2 in (eventContext?.PlayerEvent?.EventUXInfo?.FactionSealedUXInfo[0])?.FactionCollations ?? new List<FactionCollation>())
		{
			if (!dictionary.TryGetValue(item2.CollationId, out var value))
			{
				dictionary.Add(item2.CollationId, 0u);
			}
			Dictionary<uint, uint> dictionary2 = dictionary;
			value = item2.CollationId;
			dictionary2[value]++;
		}
		uint num = 0u;
		foreach (KeyValuePair<uint, uint> item3 in dictionary)
		{
			if (num < item3.Value)
			{
				num = item3.Value;
				factionEventContext.NormalPack = item3.Key;
			}
		}
		factionEventContext.DefaultBackground = ClientEventDefinitionList.GetBackgroundImagePath(_sharedClasses.AssetLookupSystem, eventContext);
		return factionEventContext;
	}

	private void OneTimeInitialization()
	{
		if (!_isInitialized)
		{
			_factionSelectEventScaffolding = GetComponentInChildren<EventPageScaffolding>();
			_titleRankComponent = GetComponentInChildren<TitleRankComponent>();
			TitleRankComponent titleRankComponent = _titleRankComponent;
			titleRankComponent.BackButtonAction = (Action<bool>)Delegate.Combine(titleRankComponent.BackButtonAction, new Action<bool>(OnBackButtonClicked));
			_factionBlade = _factionSelectEventScaffolding.GetComponentInChildren<FactionalizedEventBlade>();
			FactionalizedEventBlade factionBlade = _factionBlade;
			factionBlade.OnFactionSelect = (Action<string>)Delegate.Combine(factionBlade.OnFactionSelect, new Action<string>(FactionSelect));
			_purchaseButtonsComponent = _factionSelectEventScaffolding.GetComponentInChildren<MainButtonComponent>();
			_purchaseButtonsController = new MainButtonComponentController(_purchaseButtonsComponent, _sharedClasses.InventoryManager.Inventory, _assetLookupSystem, _sharedClasses.CustomTokenProvider, OnPayJoinClick, null);
			_lossDetailsComponent = _factionSelectEventScaffolding.GetComponentInChildren<LossDetailsComponent>();
			_objectiveTrackComponent = _factionSelectEventScaffolding.GetComponentInChildren<ObjectiveTrackComponent>();
			Transform transform = FindChildByName(_factionSelectEventScaffolding.transform, "EventComponent_Description");
			_currentFactionDescription = transform.GetComponentInChildren<TextComponent>();
			_currentFactionPacks = _factionSelectEventScaffolding.GetComponentInChildren<BoosterPacksComponent>();
			_isInitialized = true;
		}
	}

	public override void OnBeginOpen()
	{
		StartCoroutine(Coroutine_LoadData());
		if (!string.IsNullOrWhiteSpace(_currentEventContextKey) && _eventContexts.TryGetValue(_currentEventContextKey, out var value))
		{
			string text = value.EventContext.PlayerEvent.CourseData.MadeChoice ?? "";
			if (!string.IsNullOrWhiteSpace(text))
			{
				value.SelectedFaction = text;
				ActivatePostPurchaseScene();
				return;
			}
		}
		ActivateFactionScene();
	}

	public void OnBackButtonClicked(bool returnToPlayBlade)
	{
		if (!string.IsNullOrWhiteSpace(_currentEventContextKey) && _eventContexts.TryGetValue(_currentEventContextKey, out var value))
		{
			value.PostPurchaseComponentManager.OnBackButtonClicked(returnToPlayBlade);
		}
	}

	private void OnDestroy()
	{
		FactionalizedEventBlade factionBlade = _factionBlade;
		factionBlade.OnFactionSelect = (Action<string>)Delegate.Remove(factionBlade.OnFactionSelect, new Action<string>(FactionSelect));
		TitleRankComponent titleRankComponent = _titleRankComponent;
		titleRankComponent.BackButtonAction = (Action<bool>)Delegate.Remove(titleRankComponent.BackButtonAction, new Action<bool>(OnBackButtonClicked));
		foreach (KeyValuePair<string, FactionEventContext> item in _eventContexts ?? new Dictionary<string, FactionEventContext>())
		{
			EventComponentManager postPurchaseComponentManager = item.Value.PostPurchaseComponentManager;
			postPurchaseComponentManager.OnPrizeClaim = (Action<bool>)Delegate.Remove(postPurchaseComponentManager.OnPrizeClaim, new Action<bool>(ActivateFactionScene));
		}
	}

	public void DisablePurchaseButtons()
	{
		if (!_purchaseButtonsComponent.IsViewStateHidden("GemsState"))
		{
			_purchaseButtonsComponent.SetViewStateInteractable("GemsState", interactable: false);
		}
		if (!_purchaseButtonsComponent.IsViewStateHidden("GoldState"))
		{
			_purchaseButtonsComponent.SetViewStateInteractable("GoldState", interactable: false);
		}
		if (!_purchaseButtonsComponent.IsViewStateHidden("EventState"))
		{
			_purchaseButtonsComponent.SetViewStateInteractable("EventState", interactable: false);
		}
	}

	public void UpdatePurchaseButtons()
	{
		if (!string.IsNullOrWhiteSpace(_currentEventContextKey) && _eventContexts.TryGetValue(_currentEventContextKey, out var value))
		{
			_purchaseButtonsController.Update(value.EventContext.PlayerEvent);
		}
	}

	private void OnPayJoinClick(EventEntryFeeInfo fee)
	{
		if (!string.IsNullOrWhiteSpace(_currentEventContextKey) && _eventContexts.TryGetValue(_currentEventContextKey, out var value))
		{
			value.PostPurchaseComponentManager.JoinEventChoice = value.SelectedFaction;
			value.PostPurchaseComponentManager.MainButton_OnPayJoinButtonClicked(fee);
		}
	}

	private void OnJoinPaySuccess(bool success)
	{
		if (success)
		{
			ActivatePostPurchaseScene();
		}
	}

	private void DisablePostPurchaseComponents()
	{
		foreach (KeyValuePair<string, FactionEventContext> eventContext in _eventContexts)
		{
			eventContext.Value.PostPurchaseScaffolding.SetActive(active: false);
		}
	}

	private void ActivateFactionScene(bool success = true)
	{
		if (!success)
		{
			return;
		}
		DisablePostPurchaseComponents();
		if (!string.IsNullOrWhiteSpace(_currentEventContextKey))
		{
			if (_eventContexts.TryGetValue(_currentEventContextKey, out var value))
			{
				_factionBlade.Init(value.EventContext, value.FactionInfo, _sharedClasses);
				value.SelectedFaction = FactionEventContext.NO_FACTION_SELECTED;
				List<uint> materials = new List<uint> { value.NormalPack, value.NormalPack, value.NormalPack, value.NormalPack, 0u, 0u };
				_currentFactionPacks.SetMaterials(materials);
				_titleRankComponent.SetText(value.EventContext.PlayerEvent?.EventUXInfo?.EventComponentData?.TitleRankText?.LocKey ?? "");
				_factionSelectEventScaffolding.SetBackgroundImage(value.DefaultBackground);
				_objectiveTrackComponent.SetRewardData(value.EventContext.PlayerEvent?.EventUXInfo?.EventComponentData?.ByCourseObjectiveTrack.ChestDescriptions, _sharedClasses.CardDatabase, _sharedClasses.CardViewBuilder, _sharedClasses.CardMaterialBuilder);
				value.ObjectiveTrackController.OnEventPageStateChanged(value.EventContext.PlayerEvent, EventPageStates.DisplayEvent);
				_lossDetailsComponent.CreateLossSlots((value.EventContext.PlayerEvent?.EventUXInfo?.EventComponentData?.LossDetailsDisplay.Games).GetValueOrDefault());
				value.LossController.Update(value.EventContext.PlayerEvent);
			}
			UpdatePurchaseButtons();
			DisablePurchaseButtons();
			_factionSelectEventScaffolding.SetActive(active: true);
			_factionBlade.SelectRandomFaction();
		}
	}

	private void ActivatePostPurchaseScene()
	{
		if (!string.IsNullOrWhiteSpace(_currentEventContextKey) && _eventContexts.TryGetValue(_currentEventContextKey, out var value))
		{
			_factionSelectEventScaffolding.SetActive(active: false);
			DisablePostPurchaseComponents();
			value.PostPurchaseScaffolding.SetBackgroundImage(GetFactionBackgroundPath());
			value.PostPurchaseScaffolding.SetActive(active: true);
		}
	}

	private string GetFactionBackgroundPath()
	{
		if (string.IsNullOrWhiteSpace(_currentEventContextKey))
		{
			return "";
		}
		if (!_eventContexts.TryGetValue(_currentEventContextKey, out var value))
		{
			return "";
		}
		if (value.SelectedFaction == FactionEventContext.NO_FACTION_SELECTED)
		{
			return value.DefaultBackground;
		}
		if (!FactionalizedEventUtils.TryFetchFactionalizedEvent_BackgroundPayload(value.EventContext, value.SelectedFaction, _sharedClasses.AssetLookupSystem, out var payload))
		{
			_sharedClasses.Logger.Error("Event Faction [ " + value.SelectedFaction + " ] is missing an event background!");
		}
		return payload.Reference.RelativePath;
	}

	private void FactionSelect(string factionName)
	{
		if (string.IsNullOrWhiteSpace(_currentEventContextKey) || !_eventContexts.TryGetValue(_currentEventContextKey, out var value) || factionName == value.SelectedFaction || string.IsNullOrWhiteSpace(factionName))
		{
			return;
		}
		value.SelectedFaction = factionName;
		List<uint> list = new List<uint>();
		foreach (FactionCollation factionCollation in value.FactionInfo[value.SelectedFaction].FactionCollations)
		{
			list.Add(factionCollation.CollationId);
			if (list.Count >= 6)
			{
				break;
			}
		}
		_currentFactionPacks.SetMaterials(list);
		_currentFactionDescription.SetText(value.FactionInfo[value.SelectedFaction].EventPageDescriptionLoc);
		UpdatePurchaseButtons();
		_factionSelectEventScaffolding.SetBackgroundImage(GetFactionBackgroundPath());
		_titleRankComponent.SetText(value.FactionInfo[value.SelectedFaction].FactionEventNameLoc);
	}

	private static Transform FindChildByName(Transform parent, string childName)
	{
		foreach (Transform item in parent)
		{
			if (item.name.Contains(childName))
			{
				return item;
			}
			Transform transform2 = FindChildByName(item, childName);
			if (transform2 != null)
			{
				return transform2;
			}
		}
		return null;
	}

	private IEnumerator Coroutine_LoadData()
	{
		_sceneLoader.EnableLoadingIndicator(shouldEnable: true);
		if (string.IsNullOrWhiteSpace(_currentEventContextKey) || !_eventContexts.TryGetValue(_currentEventContextKey, out var context))
		{
			yield break;
		}
		Promise<ICourseInfoWrapper> getCourse = context.EventContext.PlayerEvent.GetEventCourse();
		yield return getCourse.AsCoroutine();
		_sceneLoader.EnableLoadingIndicator(shouldEnable: false);
		if (!getCourse.Successful)
		{
			if (getCourse.ErrorSource != ErrorSource.Debounce)
			{
				SceneLoader.GetSceneLoader().ShowConnectionFailedMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Progress_Get_Error_Text"), allowRetry: true, exitInsteadOfLogout: true);
			}
		}
		else
		{
			context.PostPurchaseComponentManager?.OnEventPageOpen(context.EventContext);
		}
	}
}
