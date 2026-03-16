using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.EventComponents;
using Core.Shared.Code;
using EventPage.Components.NetworkModels;
using UnityEngine;
using Wizards.Mtga;

namespace EventPage.Components;

public class EventPageComponentFactory
{
	private SharedEventPageClasses _sharedClasses;

	private NavContentController _eventPageContentController;

	public EventPageComponentFactory(SharedEventPageClasses sharedClasses, NavContentController eventPageContentController)
	{
		_sharedClasses = sharedClasses;
		_eventPageContentController = eventPageContentController;
	}

	public void CreateComponents(EventComponentManager componentManager, Dictionary<ComponentLocation, Transform> transforms, RectTransform safeArea)
	{
		AssetLookupSystem assetLookupSystem = _sharedClasses.AssetLookupSystem;
		EventComponentData componentData = componentManager.ComponentData;
		if (componentData.AvgQueueTimeDisplay != null && instantiateComponent<AverageQueueTimeComponentPayload, AverageQueueTimeComponent>(out var componentObject))
		{
			componentManager.AddComponent(new AverageQueueTimeComponentController(componentObject));
		}
		if (componentData.BoosterPacksDisplay != null && componentData.StickerDisplay == null && instantiateComponent<BoosterPackComponentPayload, BoosterPacksComponent>(out var componentObject2))
		{
			componentManager.AddComponent(new BoosterPacksComponentController(componentObject2, componentData.BoosterPacksDisplay));
		}
		if (componentData.StickerDisplay != null && instantiateComponent<StickerComponentPayload, StickerComponent>(out var componentObject3))
		{
			componentManager.AddComponent(new StickerComponentController(componentObject3, componentData.StickerDisplay, WrapperController.Instance.EmoteDataProvider, assetLookupSystem));
		}
		if (componentData.CardsDisplay != null && instantiateComponent<CardsComponentPayload, CardsComponent>(out var componentObject4))
		{
			componentManager.AddComponent(new CardsComponentController(componentObject4, componentData.CardsDisplay, _sharedClasses.CardDatabase, _sharedClasses.CardViewBuilder, componentManager.CardsComponent_OnClicked));
		}
		if (componentData.PreviewEventWidget != null && instantiateComponent<CashTournamentComponentPayload, CashTournamentComponent>(out var componentObject5))
		{
			componentManager.AddComponent(new PreviewEventComponentController(componentObject5, componentData.PreviewEventWidget, _sharedClasses.EventManager, componentManager.CashTournamentComponent_OnClicked));
		}
		if (componentData.ChestWidget != null && instantiateComponent<ChestWidgetComponentPayload, ChestWidgetComponent>(out var componentObject6))
		{
			componentManager.AddComponent(new ChestWidgetComponentController(componentObject6, componentData.ChestWidget, new CardSkinDatabase(_sharedClasses.CardSkinCatalogWrapper, _sharedClasses.CardDatabase), _sharedClasses.CosmeticsProvider, componentManager.ChestWidgetComponent_OnClicked));
		}
		if (componentData.DescriptionText != null && instantiateComponent<DescriptionComponentPayload, TextComponent>(out var componentObject7))
		{
			componentManager.AddComponent(new DescriptionComponentController(componentObject7));
		}
		if (componentData.EmblemDisplay != null && componentData.StickerDisplay == null && instantiateComponent<EmblemComponentPayload, EmblemComponent>(out var componentObject8))
		{
			componentManager.AddComponent(new EmblemComponentController(componentObject8, componentData.EmblemDisplay, _sharedClasses.CardDatabase, _sharedClasses.CardViewBuilder));
		}
		if (componentData.InspectPreconDecksWidget != null && instantiateComponent<InspectPreconDecksComponentPayload, InspectPreconDecksComponent>(out var componentObject9))
		{
			componentManager.AddComponent(new InspectPreconDecksComponentController(_sharedClasses.CardDatabase, _sharedClasses.CardViewBuilder, componentObject9, componentData.InspectPreconDecksWidget, _sharedClasses.PreconDeckManager, componentManager.InspectPreconDecks_OnClicked));
		}
		if (componentData.InspectSingleDeckWidget != null && instantiateComponent<InspectSingleDeckComponentPayload, InspectSingleDeckComponent>(out var componentObject10))
		{
			componentManager.AddComponent(new InspectSingleDeckComponentController(componentObject10, componentManager.InspectSingleDeck_OnClicked));
		}
		if (componentData.LossDetailsDisplay != null && instantiateComponent<LossDetailsComponentPayload, LossDetailsComponent>(out var componentObject11))
		{
			componentManager.AddComponent(new LossDetailsComponentController(componentObject11, componentData.LossDetailsDisplay));
		}
		if (componentData.SubtitleText != null && instantiateComponent<SubtitleComponentPayload, TextComponent>(out var componentObject12))
		{
			componentManager.AddComponent(new SubtitleComponentController(componentObject12));
		}
		if (componentData.TitleRankText != null && instantiateComponent<TitleRankComponentPayload, TitleRankComponent>(out var componentObject13))
		{
			componentManager.AddComponent(new TitleRankComponentController(componentObject13, assetLookupSystem, delegate(bool buttonPressed)
			{
				Pantry.Get<GlobalCoroutineExecutor>().StartGlobalCoroutine(DelayedDisable(0.5f));
				componentManager.OnBackButtonClicked(buttonPressed);
			}));
		}
		if (componentData.ResignWidget != null && instantiateComponent<ResignComponentPayload, ResignComponent>(out var componentObject14))
		{
			componentManager.AddComponent(new ResignComponentController(componentObject14, componentManager.ResignComponent_OnClicked));
		}
		if (componentData.SelectedDeckWidget != null && instantiateComponent<SelectedDeckComponentPayload, SelectedDeckComponent>(out var componentObject15))
		{
			componentManager.AddComponent(new SelectedDeckComponentController(componentObject15, componentManager.SelectedDeck_SubmitDeck, componentManager.SelectedDeck_OnDeckBoxClicked, componentManager.SelectedDeck_CopyToDecksClicked, componentManager.SelectDeckButtonClicked));
		}
		if (componentData.PrizeWallData != null && !string.IsNullOrEmpty(componentData.PrizeWallData.PrizeWallId) && instantiateComponent<PrizeWallComponentPayload, PrizeWallComponent>(out var componentObject16))
		{
			componentManager.AddComponent(new PrizeWallComponentController(componentObject16, componentManager.EventContext, _sharedClasses.PrizeWallDataProvider, componentData.PrizeWallData, assetLookupSystem));
		}
		if (componentData.TimerDisplay != null && instantiateComponent<TimerComponentPayload, TimerComponent>(out var componentObject17))
		{
			componentManager.AddComponent(new TimerComponentController(componentObject17, componentManager.Timer_OnEnded));
		}
		if (componentData.ViewCardPoolWidget != null && instantiateComponent<ViewCardPoolComponentPayload, ViewCardPoolComponent>(out var componentObject18))
		{
			componentManager.AddComponent(new ViewCardPoolComponentController(componentObject18, componentData.ViewCardPoolWidget, componentManager.CardPool_OnClicked));
		}
		if (componentData.MainButtonWidget != null && instantiateComponent<MainButtonComponentPayload, MainButtonComponent>(out var componentObject19))
		{
			componentManager.AddComponent(new MainButtonComponentController(componentObject19, _sharedClasses.InventoryManager.Inventory, assetLookupSystem, _sharedClasses.CustomTokenProvider, componentManager.MainButton_OnPayJoinButtonClicked, componentManager.MainButton_OnPlayButtonClicked));
		}
		if (componentData.ByCourseObjectiveTrack != null && instantiateComponent<ObjectiveTrackByCoursePayload, ObjectiveTrackComponent>(out var componentObject20))
		{
			componentManager.AddComponent(new ObjectiveTrackComponentController(componentObject20, componentData.ByCourseObjectiveTrack, safeArea, _sharedClasses.CardDatabase, _sharedClasses.CardViewBuilder, _sharedClasses.CardMaterialBuilder));
		}
		if (componentData.CumulativeObjectiveTrack != null && instantiateComponent<ObjectiveTrackCumulativePayload, ObjectiveTrackComponent>(out var componentObject21))
		{
			componentManager.AddComponent(new ObjectiveTrackComponentController(componentObject21, componentData.CumulativeObjectiveTrack, safeArea, _sharedClasses.CardDatabase, _sharedClasses.CardViewBuilder, _sharedClasses.CardMaterialBuilder));
		}
		if (componentData.HiddenBubblesObjectiveTrack != null && instantiateComponent<ObjectiveTrackHiddenBubblesPayload, ObjectiveTrackComponent>(out var componentObject22))
		{
			componentManager.AddComponent(new ObjectiveTrackComponentController(componentObject22, componentData.HiddenBubblesObjectiveTrack, safeArea, _sharedClasses.CardDatabase, _sharedClasses.CardViewBuilder, _sharedClasses.CardMaterialBuilder));
		}
		List<EventPageLayoutObject> list = new List<EventPageLayoutObject>(8);
		foreach (Transform value in transforms.Values)
		{
			list.Clear();
			foreach (Transform item in value)
			{
				EventPageLayoutObject component = item.GetComponent<EventPageLayoutObject>();
				if (component != null)
				{
					int num = list.BinarySearch(component);
					if (num < 0)
					{
						num = ~num;
					}
					list.Insert(num, component);
				}
			}
			foreach (EventPageLayoutObject item2 in list)
			{
				item2.transform.SetAsLastSibling();
			}
		}
		bool instantiateComponent<TPayload, TComponent>(out TComponent reference) where TPayload : EventComponentPayload<TComponent> where TComponent : EventComponent
		{
			try
			{
				string eventComponentPath = assetLookupSystem.GetEventComponentPath<TPayload, TComponent>();
				reference = AssetLoader.Instantiate<TComponent>(eventComponentPath);
				reference.transform.SetParent(transforms[reference.Location], worldPositionStays: false);
			}
			catch
			{
				Debug.LogWarning("Error creating event page component: " + typeof(TComponent).Name);
				reference = null;
			}
			return reference != null;
		}
	}

	private IEnumerator DelayedDisable(float delayInSeconds)
	{
		yield return new WaitForSeconds(delayInSeconds);
		_eventPageContentController.gameObject.SetActive(value: false);
	}
}
