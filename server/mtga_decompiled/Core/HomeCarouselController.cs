using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Carousel;
using Assets.Core.Meta.Utilities;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Enums.Carousel;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Unification.Models.Mercantile;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

public class HomeCarouselController : MonoBehaviour
{
	private enum CarouselState
	{
		Active,
		Paused3D,
		Paused2D
	}

	[Header("Banner Visuals")]
	[Space(20f)]
	[SerializeField]
	private Transform _prefabAnchor;

	[SerializeField]
	private GameObject _imagesContainer;

	[SerializeField]
	private CustomButton MainButton;

	[SerializeField]
	private Animator MainAnimator;

	[SerializeField]
	private Image MainImage;

	[SerializeField]
	private Image FrameBreakImage;

	[SerializeField]
	private Image FrameBreakBackgroundImage;

	[SerializeField]
	private Localize TitleLoc;

	[SerializeField]
	private Localize DescriptionLoc;

	[SerializeField]
	private GameObject ExternalLinkImage;

	[SerializeField]
	private VerticalLayoutGroup TextLayoutGroup;

	[Header("Navigation")]
	[Space(20f)]
	[SerializeField]
	private Transform NavDotsParent;

	[SerializeField]
	private GameObject NavDotPrefab;

	[SerializeField]
	private Sprite NavDotOnSprite;

	[SerializeField]
	private Sprite NavDotOffSprite;

	[SerializeField]
	private CustomButton NavLeftButton;

	[SerializeField]
	private CustomButton NavRightButton;

	[Header("Auto-Rotate")]
	[SerializeField]
	private float AutoRotateSeconds = 15f;

	[SerializeField]
	private RectTransform AutoRotateProgress;

	[SerializeField]
	private RectTransform ProgressParent;

	private CarouselState _carouselState;

	private int _currentIndex;

	private List<Client_CarouselItem> _visibleItems;

	private List<GameObject> _navDots = new List<GameObject>();

	private float _autoRotateElapsedSeconds;

	private Coroutine _refreshItemsCoroutine;

	private CarouselDataProvider _carouselDataProvider;

	private int _initialIndex;

	private float _totalElapsedSeconds;

	private ConnectionManager _connectionManager;

	private IAccountClient _accountClient;

	private IBILogger _biLogger;

	private void Awake()
	{
		MainButton.OnMouseover.AddListener(delegate
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
		});
		MainButton.OnClick.AddListener(MainButton_OnClick);
		NavLeftButton.OnClick.AddListener(NavigateLeft);
		NavRightButton.OnClick.AddListener(NavigateRight);
		_currentIndex = -1;
		_connectionManager = Pantry.Get<ConnectionManager>();
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Combine(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
		_carouselDataProvider = Pantry.Get<CarouselDataProvider>();
		_accountClient = Pantry.Get<IAccountClient>();
		_biLogger = Pantry.Get<IBILogger>();
	}

	private void OnDestroy()
	{
		ConnectionManager connectionManager = _connectionManager;
		connectionManager.OnFdReconnected = (Action)Delegate.Remove(connectionManager.OnFdReconnected, new Action(OnFdReconnected));
	}

	private void OnFdReconnected()
	{
		Refresh();
	}

	public void Refresh()
	{
		if (base.gameObject.activeInHierarchy)
		{
			_currentIndex++;
			_initialIndex = _currentIndex;
			_totalElapsedSeconds = 0f;
			if (_refreshItemsCoroutine != null)
			{
				StopCoroutine(_refreshItemsCoroutine);
			}
			StartCoroutine(Coroutine_RefreshItems());
		}
	}

	public void Pause()
	{
		if (_carouselState == CarouselState.Active)
		{
			if (_prefabAnchor.gameObject.activeSelf)
			{
				_carouselState = CarouselState.Paused3D;
				_prefabAnchor.gameObject.SetActive(value: false);
			}
			else
			{
				_carouselState = CarouselState.Paused2D;
			}
		}
	}

	public void Resume()
	{
		if (_carouselState == CarouselState.Paused3D)
		{
			_prefabAnchor.gameObject.SetActive(value: true);
		}
		_carouselState = CarouselState.Active;
	}

	private IEnumerator Coroutine_RefreshItems()
	{
		WrapperController.EnableUniqueLoadingIndicator("HomeCarouselController.RefreshItems");
		NavDotPrefab.SetActive(value: false);
		foreach (GameObject navDot in _navDots)
		{
			UnityEngine.Object.Destroy(navDot);
		}
		_navDots.Clear();
		EventManager eventManager = WrapperController.Instance.EventManager;
		Promise<List<Client_CarouselItem>> carouselRequest = _carouselDataProvider.RefreshCarouselItems(_accountClient.AccountInformation.CountryCode, MDNPlayerPrefs.PLAYERPREFS_ClientLanguage);
		while (eventManager.RefreshingEventContexts)
		{
			yield return null;
		}
		while (!carouselRequest.IsDone)
		{
			yield return null;
		}
		if (carouselRequest.Successful)
		{
			_visibleItems = FilterByAvailableAssets(carouselRequest.Result);
			_visibleItems = FilterByAvailableEvents(_visibleItems, eventManager);
		}
		else
		{
			Debug.LogError($"Carousel Refresh Error: {carouselRequest.Error}");
			if (_visibleItems == null)
			{
				_visibleItems = new List<Client_CarouselItem>();
			}
		}
		if (_currentIndex >= _visibleItems.Count)
		{
			_currentIndex = 0;
		}
		bool flag = _visibleItems.Count >= 2;
		NavDotsParent.gameObject.SetActive(flag);
		NavLeftButton.gameObject.SetActive(flag);
		NavRightButton.gameObject.SetActive(flag);
		AutoRotateProgress.gameObject.SetActive(flag);
		if (flag)
		{
			for (int i = 0; i < _visibleItems.Count; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(NavDotPrefab, NavDotsParent);
				gameObject.SetActive(value: true);
				_navDots.Add(gameObject);
				int toIndex = i;
				gameObject.GetComponentInChildren<Button>().onClick.AddListener(delegate
				{
					NavigateToIndex(toIndex);
				});
			}
		}
		UpdateCurrentItem();
		WrapperController.DisableUniqueLoadingIndicator("HomeCarouselController.RefreshItems");
	}

	private List<Client_CarouselItem> FilterByAvailableAssets(List<Client_CarouselItem> carouselItemList)
	{
		AssetLookupSystem assetSystem = WrapperController.Instance.AssetLookupSystem;
		AssetLookupTree<CarouselPayload> assetTree = assetSystem.TreeLoader.LoadTree<CarouselPayload>();
		carouselItemList = carouselItemList.FindAll(delegate(Client_CarouselItem i)
		{
			assetSystem.Blackboard.Clear();
			assetSystem.Blackboard.CarouselName = i.AssetTreeItem;
			CarouselPayload payload = assetTree.GetPayload(assetSystem.Blackboard);
			bool flag = true;
			if (payload != null && payload.item != null)
			{
				if (AssetLoader.HaveAsset(payload.item.PrefabRef))
				{
					flag = false;
				}
				else
				{
					bool num = !AssetLoader.HaveAsset(payload.item.MainSpriteRef);
					bool flag2 = !AssetLoader.HaveAsset(payload.item.FrameBreakSpriteRef);
					bool flag3 = !AssetLoader.HaveAsset(payload.item.FrameBreakBackgroundSpriteRef);
					flag = num || flag2 || flag3;
				}
			}
			return !flag;
		});
		return carouselItemList;
	}

	private List<Client_CarouselItem> FilterByAvailableEvents(List<Client_CarouselItem> carouselItemList, EventManager eventManager)
	{
		List<Client_CarouselItem> list = new List<Client_CarouselItem>();
		foreach (Client_CarouselItem carouselItem in carouselItemList)
		{
			if (!carouselItem.Actions.Exists((Client_CarouselAction action) => action.Type == CarouselActionType.GoToEvent && !eventManager.EventsByInternalName.ContainsKey(action.Arguments)))
			{
				list.Add(carouselItem);
			}
		}
		return list;
	}

	private void NavigateLeft()
	{
		MainAnimator.SetTrigger("TransitionL");
		MainAnimator.SetTrigger("Up");
		if (--_currentIndex < 0)
		{
			_currentIndex = _visibleItems.Count - 1;
		}
		UpdateCurrentItem();
	}

	private void NavigateRight()
	{
		MainAnimator.SetTrigger("TransitionR");
		MainAnimator.SetTrigger("Up");
		if (++_currentIndex >= _visibleItems.Count)
		{
			_currentIndex = 0;
		}
		UpdateCurrentItem();
	}

	private void NavigateToIndex(int toIndex)
	{
		MainAnimator.SetTrigger((toIndex < _currentIndex) ? "TransitionL" : "TransitionR");
		MainAnimator.SetTrigger("Up");
		_currentIndex = toIndex;
		UpdateCurrentItem();
	}

	private void UpdateCurrentItem()
	{
		if (_visibleItems.Count <= 0)
		{
			Debug.LogError("No visible carousel items to display.  This should not happen");
			return;
		}
		_currentIndex = Mathf.Clamp(_currentIndex, 0, _visibleItems.Count - 1);
		Client_CarouselItem client_CarouselItem = _visibleItems[_currentIndex];
		AssetLookupSystem assetLookupSystem = WrapperController.Instance.AssetLookupSystem;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.CarouselName = client_CarouselItem.AssetTreeItem;
		CarouselPayload payload = assetLookupSystem.TreeLoader.LoadTree<CarouselPayload>().GetPayload(assetLookupSystem.Blackboard);
		if (payload.IsPrefabBased)
		{
			_prefabAnchor.DestroyChildren();
			AssetLoader.Instantiate(payload.item.PrefabRef, _prefabAnchor);
			_prefabAnchor.gameObject.SetActive(value: true);
			_imagesContainer.SetActive(value: false);
		}
		else
		{
			_imagesContainer.SetActive(value: true);
			_prefabAnchor.gameObject.SetActive(value: false);
			MainImage.sprite = AssetLoader.AcquireAndTrackAsset(base.gameObject, "MainImage", payload.item.MainSpriteRef);
			FrameBreakImage.sprite = AssetLoader.AcquireAndTrackAsset(base.gameObject, "FrameBreakImage", payload.item.FrameBreakSpriteRef);
			FrameBreakBackgroundImage.sprite = AssetLoader.AcquireAndTrackAsset(base.gameObject, "FrameBreakBackgroundImage", payload.item.FrameBreakBackgroundSpriteRef);
		}
		TitleLoc.SetText(client_CarouselItem.TitleKey);
		DescriptionLoc.SetText(client_CarouselItem.DescriptionKey);
		if (client_CarouselItem.Actions.FirstOrDefault((Client_CarouselAction i) => i.Type == CarouselActionType.GoToExternalUrl) != null)
		{
			ExternalLinkImage.SetActive(value: true);
			TextLayoutGroup.padding.right = 95;
		}
		else
		{
			ExternalLinkImage.SetActive(value: false);
			TextLayoutGroup.padding.right = 20;
		}
		for (int num = 0; num < _navDots.Count; num++)
		{
			_navDots[num].GetComponentInChildren<Image>().sprite = ((_currentIndex == num) ? NavDotOnSprite : NavDotOffSprite);
		}
		_autoRotateElapsedSeconds = 0f;
		Vector2 offsetMax = AutoRotateProgress.offsetMax;
		offsetMax.x = 0f - ProgressParent.rect.width;
		AutoRotateProgress.offsetMax = offsetMax;
	}

	private void MainButton_OnClick()
	{
		if (_currentIndex < 0 || _visibleItems.Count <= _currentIndex)
		{
			HomePageContentController component = GetComponent<HomePageContentController>();
			if (component != null)
			{
				component.ShowBladeAndSelect("Play");
			}
			Debug.LogWarningFormat("Clicked on carousel item at index {0} but there are only {1} items. Using default.", _currentIndex, _visibleItems.Count);
			return;
		}
		Client_CarouselItem client_CarouselItem = _visibleItems[_currentIndex];
		OnCarouselAction(client_CarouselItem);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_click_pane, base.gameObject);
		HomeCarouselClicked payload = new HomeCarouselClicked
		{
			ClickedItemName = client_CarouselItem.Name,
			ClickedItemIndex = _currentIndex,
			InitialItemIndex = _initialIndex,
			VisibleItemsCount = _visibleItems.Count,
			SecondsViewingItem = _autoRotateElapsedSeconds,
			SecondsViewingPage = _totalElapsedSeconds,
			UrlLink = client_CarouselItem.Actions.FirstOrDefault((Client_CarouselAction i) => i.Type == CarouselActionType.GoToExternalUrl)?.Arguments
		};
		_biLogger.Send(ClientBusinessEventType.HomeCarouselClicked, payload);
	}

	private static void OnCarouselAction(Client_CarouselItem currentItem)
	{
		for (int i = 0; i < currentItem.Actions.Count; i++)
		{
			Client_CarouselAction action = currentItem.Actions[i];
			switch (action.Type)
			{
			case CarouselActionType.GoToEvent:
			{
				EventContext evt2 = WrapperController.Instance.EventManager.EventContexts.Find((EventContext c) => c.PlayerEvent.EventInfo.InternalEventName == action.Arguments);
				WrapperController.Instance.SceneLoader.GoToEventScreen(evt2, reloadIfAlreadyLoaded: false, SceneLoader.NavMethod.Carousel);
				break;
			}
			case CarouselActionType.GoToColorChallenge:
			{
				EventContext evt = WrapperController.Instance.EventManager.EventContexts.FirstOrDefault((EventContext c) => c.PlayerEvent is IColorChallengePlayerEvent);
				WrapperController.Instance.SceneLoader.GoToEventScreen(evt, reloadIfAlreadyLoaded: false, SceneLoader.NavMethod.Carousel);
				break;
			}
			case CarouselActionType.GoToStoreItem:
			{
				if (WrapperController.Instance.Store.StoreListings.TryGetValue(action.Arguments, out var value))
				{
					PAPA.StartGlobalCoroutine(WrapperController.Instance.Store.PurchaseItemYield(value, Client_PurchaseCurrencyType.RMT));
				}
				break;
			}
			case CarouselActionType.GoToExternalUrl:
				UrlOpener.OpenURL(action.Arguments);
				break;
			case CarouselActionType.OpenPlayBlade:
				SceneLoader.GetSceneLoader().ShowPlayBladeAndSelect(action.Arguments);
				break;
			case CarouselActionType.OpenStoreTab:
			{
				EStoreSection result2;
				if (Enum.TryParse<StoreTabType>(action.Arguments, out var result))
				{
					SceneLoader.GetSceneLoader().GoToStore(result, "Carousel Item");
				}
				else if (Enum.TryParse<EStoreSection>(action.Arguments, out result2))
				{
					StoreTabType tabTypeForStoreSection = ContentController_StoreCarousel.GetTabTypeForStoreSection(result2);
					if (tabTypeForStoreSection != StoreTabType.None)
					{
						SceneLoader.GetSceneLoader().GoToStore(tabTypeForStoreSection, "Carousel Item");
					}
				}
				break;
			}
			case CarouselActionType.GoToScreen:
			{
				string arguments = action.Arguments;
				if (!(arguments == "MasteryTab"))
				{
					if (arguments == "LearnPage")
					{
						SceneLoader.GetSceneLoader().GoToLearnToPlay("Carousel");
					}
				}
				else if (SceneLoader.GetSceneLoader().CurrentContentType == NavContentType.Home)
				{
					ProgressionTrackPageContext trackPageContext = new ProgressionTrackPageContext(Pantry.Get<SetMasteryDataProvider>().CurrentBpName, NavContentType.Home, NavContentType.Home);
					SceneLoader.GetSceneLoader().GoToProgressionTrackScene(trackPageContext, "From Carousel");
				}
				break;
			}
			case CarouselActionType.GoToDynamicFilter:
				SceneLoader.GetSceneLoader().ShowPlayBladeEventsAndFilter(action.Arguments);
				break;
			}
		}
	}

	private void Update()
	{
		if (_visibleItems != null && _visibleItems.Count > 1 && _carouselState == CarouselState.Active)
		{
			float deltaTime = Time.deltaTime;
			_autoRotateElapsedSeconds += deltaTime;
			_totalElapsedSeconds += deltaTime;
			if (_autoRotateElapsedSeconds >= AutoRotateSeconds)
			{
				NavigateRight();
				return;
			}
			Vector2 offsetMax = AutoRotateProgress.offsetMax;
			offsetMax.x = (0f - ProgressParent.rect.width) * (AutoRotateSeconds - _autoRotateElapsedSeconds) / AutoRotateSeconds;
			AutoRotateProgress.offsetMax = offsetMax;
		}
	}
}
