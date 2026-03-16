using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Core.Meta.Utilities;
using Core.Code.Promises;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.Enums.Card;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Models;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Client.Models.Mercantile;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;

public class InventoryManager : IInventoryManager
{
	private class InventoryUpdateCallback
	{
		public Wizards.Mtga.FrontDoorModels.InventoryUpdateContext Context;

		public Action<ClientInventoryUpdateReportItem> Callback;
	}

	private int _playerCardsVersion = -1;

	private bool _cacheNeedsRefreshed;

	private Action<ClientInventoryUpdateReportItem> globalSubscribers;

	private List<InventoryUpdateCallback> subscribers = new List<InventoryUpdateCallback>();

	private IInventoryServiceWrapper _inventoryServiceWrapper;

	private IMercantileServiceWrapper _mercantileServiceWrapper;

	private List<RotationWarningContext> _rotationWarningsSeen = new List<RotationWarningContext>();

	public ClientPlayerInventory Inventory => _inventoryServiceWrapper.Inventory;

	public Dictionary<uint, int> Cards => _inventoryServiceWrapper.Cards;

	public Dictionary<uint, int> newCards => _inventoryServiceWrapper.newCards;

	public Dictionary<uint, int> CardsToTagNew => _inventoryServiceWrapper.CardsToTagNew;

	public bool cacheNeedsRefreshed
	{
		get
		{
			return _cacheNeedsRefreshed;
		}
		private set
		{
			_cacheNeedsRefreshed = value;
		}
	}

	public InventoryPurchaseMode CurrentPurchaseMode { get; private set; }

	public event Action NewCardUpdate;

	public event Action CardsUpdated;

	public event Action InventoryUpdated;

	public event Action<bool> OnRedeemWildcardResponse;

	public event Action<uint, string, bool> OnPurchaseSkinResponse;

	public InventoryManager(IInventoryServiceWrapper inventoryServiceWrapper, IMercantileServiceWrapper mercantileServiceWrapper)
	{
		_inventoryServiceWrapper = inventoryServiceWrapper;
		IInventoryServiceWrapper inventoryServiceWrapper2 = _inventoryServiceWrapper;
		inventoryServiceWrapper2.PublishEvents = (Action)Delegate.Combine(inventoryServiceWrapper2.PublishEvents, new Action(PublishEvents));
		IInventoryServiceWrapper inventoryServiceWrapper3 = _inventoryServiceWrapper;
		inventoryServiceWrapper3.OnInventoryUpdated = (Action)Delegate.Combine(inventoryServiceWrapper3.OnInventoryUpdated, new Action(InventoryHasBeenUpdated));
		IInventoryServiceWrapper inventoryServiceWrapper4 = _inventoryServiceWrapper;
		inventoryServiceWrapper4.OnCardsUpdated = (Action)Delegate.Combine(inventoryServiceWrapper4.OnCardsUpdated, new Action(OnNewCardsUpdated));
		IInventoryServiceWrapper inventoryServiceWrapper5 = _inventoryServiceWrapper;
		inventoryServiceWrapper5.OnCardsUpdated = (Action)Delegate.Combine(inventoryServiceWrapper5.OnCardsUpdated, new Action(OnCardsUpdated));
		_mercantileServiceWrapper = mercantileServiceWrapper;
	}

	public void AcknowledgeNew(uint grpID)
	{
		CardsToTagNew.Remove(grpID);
		newCards.Remove(grpID);
		SetCacheNeedsRefreshed();
		OnNewCardsUpdated();
	}

	public void AcknowledgeAllCards()
	{
		CardsToTagNew.Clear();
		newCards.Clear();
		SetCacheNeedsRefreshed();
	}

	private void OnCardsUpdated()
	{
		this.CardsUpdated?.Invoke();
	}

	private void OnNewCardsUpdated()
	{
		DeckViewUtilities.ClearNewCardSortedCacheIfRequired();
		this.NewCardUpdate?.Invoke();
	}

	public void SetCacheNeedsRefreshed(bool refresh = true)
	{
		_cacheNeedsRefreshed = refresh;
	}

	public void PublishEvents()
	{
		MainThreadDispatcher.Instance.Add(publishEvents);
	}

	private void publishEvents()
	{
		List<ClientInventoryUpdateReportItem> updates = _inventoryServiceWrapper.Updates;
		if (updates == null || updates.Count <= 0)
		{
			return;
		}
		foreach (ClientInventoryUpdateReportItem update2 in _inventoryServiceWrapper.Updates)
		{
			globalSubscribers?.Invoke(update2);
		}
		List<InventoryUpdateCallback> list = subscribers;
		if (list == null || list.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < subscribers.Count; i++)
		{
			InventoryUpdateCallback update = subscribers[i];
			if (update.Context.sourceId != null)
			{
				foreach (ClientInventoryUpdateReportItem item in _inventoryServiceWrapper.Updates.Where((ClientInventoryUpdateReportItem x) => x.context.source == update.Context.source && x.context.sourceId == update.Context.sourceId).ToList())
				{
					update.Callback?.Invoke(item);
					_inventoryServiceWrapper.Updates.Remove(item);
				}
				continue;
			}
			foreach (ClientInventoryUpdateReportItem item2 in _inventoryServiceWrapper.Updates.Where((ClientInventoryUpdateReportItem x) => x.context.source == update.Context.source).ToList())
			{
				update.Callback?.Invoke(item2);
				_inventoryServiceWrapper.Updates.Remove(item2);
			}
		}
	}

	public List<ClientInventoryUpdateReportItem> FetchExistingUpdates(InventoryUpdateSource source)
	{
		List<ClientInventoryUpdateReportItem> list = _inventoryServiceWrapper.Updates.Where((ClientInventoryUpdateReportItem x) => x.context.source == source).ToList();
		foreach (ClientInventoryUpdateReportItem item in list)
		{
			_inventoryServiceWrapper.Updates.Remove(item);
		}
		return list;
	}

	public void SubscribeToAll(Action<ClientInventoryUpdateReportItem> handler, bool publish = false)
	{
		globalSubscribers = (Action<ClientInventoryUpdateReportItem>)Delegate.Combine(globalSubscribers, handler);
		if (publish)
		{
			PublishEvents();
		}
	}

	public void UnsubscribeFromAll(Action<ClientInventoryUpdateReportItem> handler)
	{
		globalSubscribers = (Action<ClientInventoryUpdateReportItem>)Delegate.Remove(globalSubscribers, handler);
	}

	public void Subscribe(InventoryUpdateSource source, Action<ClientInventoryUpdateReportItem> handler, string sourceId = null, bool publish = true)
	{
		Wizards.Mtga.FrontDoorModels.InventoryUpdateContext context = new Wizards.Mtga.FrontDoorModels.InventoryUpdateContext
		{
			source = source,
			sourceId = sourceId
		};
		InventoryUpdateCallback inventoryUpdateCallback = subscribers.FirstOrDefault((InventoryUpdateCallback x) => x.Context.source == context.source && x.Context.sourceId == context.sourceId);
		if (inventoryUpdateCallback != null)
		{
			inventoryUpdateCallback.Callback = (Action<ClientInventoryUpdateReportItem>)Delegate.Remove(inventoryUpdateCallback.Callback, handler);
			inventoryUpdateCallback.Callback = (Action<ClientInventoryUpdateReportItem>)Delegate.Combine(inventoryUpdateCallback.Callback, handler);
		}
		else
		{
			subscribers.Add(new InventoryUpdateCallback
			{
				Context = context,
				Callback = handler
			});
		}
		if (publish)
		{
			PublishEvents();
		}
	}

	public void UnSubscribe(InventoryUpdateSource source, Action<ClientInventoryUpdateReportItem> handler, string sourceId = null)
	{
		Wizards.Mtga.FrontDoorModels.InventoryUpdateContext context = new Wizards.Mtga.FrontDoorModels.InventoryUpdateContext
		{
			source = source,
			sourceId = sourceId
		};
		InventoryUpdateCallback inventoryUpdateCallback = subscribers.FirstOrDefault((InventoryUpdateCallback x) => x.Context.source == context.source && x.Context.sourceId == context.sourceId);
		if (inventoryUpdateCallback != null)
		{
			inventoryUpdateCallback.Callback = (Action<ClientInventoryUpdateReportItem>)Delegate.Remove(inventoryUpdateCallback.Callback, handler);
			if (inventoryUpdateCallback.Callback == null)
			{
				subscribers.Remove(inventoryUpdateCallback);
			}
		}
	}

	public void ClearSubscribers()
	{
		foreach (InventoryUpdateCallback subscriber in subscribers)
		{
			subscriber.Callback = null;
		}
		subscribers.Clear();
		globalSubscribers = null;
	}

	public Promise<CardsAndCacheVersion> RefreshCards()
	{
		return _inventoryServiceWrapper.GetPlayerCards(_playerCardsVersion).ThenOnMainThread(delegate(Promise<CardsAndCacheVersion> promise)
		{
			if (!promise.Successful)
			{
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Collection_Get_Error"));
			}
			else if (promise.Result.cacheVersion > _playerCardsVersion)
			{
				_inventoryServiceWrapper.Cards = promise.Result.cards;
				this.CardsUpdated?.Invoke();
				_playerCardsVersion = promise.Result.cacheVersion;
			}
		});
	}

	public void InventoryHasBeenUpdated()
	{
		if (this.InventoryUpdated != null)
		{
			MainThreadDispatcher.Instance.Add(this.InventoryUpdated);
		}
	}

	public void SetWcTrackPosition(int wcTrackInfo)
	{
		_inventoryServiceWrapper.Inventory.wcTrackPosition = wcTrackInfo;
	}

	public IEnumerator Coroutine_RedeemWildcards(WildcardBulkRequest request)
	{
		if (CurrentPurchaseMode == InventoryPurchaseMode.None)
		{
			CurrentPurchaseMode = InventoryPurchaseMode.RedeemWildcard;
			SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
			sceneLoader.EnableLoadingIndicator(shouldEnable: true);
			Promise<InventoryInfo> handle = _inventoryServiceWrapper?.RedeemWildcards(request) ?? new SimplePromise<InventoryInfo>(default(Error), ErrorSource.NotConnected);
			while (!handle.IsDone)
			{
				yield return null;
			}
			if (!handle.Successful)
			{
				sceneLoader.SystemMessages.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Wildcard_Redeem_Failure_Text"));
			}
			CurrentPurchaseMode = InventoryPurchaseMode.None;
			sceneLoader.EnableLoadingIndicator(shouldEnable: false);
			this.OnRedeemWildcardResponse?.Invoke(handle.Successful);
		}
	}

	public IEnumerator Coroutine_PurchaseSkin(ArtStyleEntry cardSkin, uint grpId, Client_PurchaseCurrencyType currencyType)
	{
		if (CurrentPurchaseMode == InventoryPurchaseMode.None)
		{
			CurrentPurchaseMode = InventoryPurchaseMode.PurchaseSkin;
			SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
			sceneLoader.EnableLoadingIndicator(shouldEnable: true);
			Promise<InventoryInfoShared> promise = _mercantileServiceWrapper?.PurchaseProduct(cardSkin.StoreItem, 1, currencyType);
			yield return promise.AsCoroutine();
			if (!promise.Successful)
			{
				MTGALocalizedString mTGALocalizedString = "SystemMessage/System_Store_Error_Title";
				MTGALocalizedString mTGALocalizedString2 = (false ? "MainNav/Store/PurchaseResult_ProductNotActive" : "MainNav/Store/Purchase_Error_Unknown_Title");
				sceneLoader.SystemMessages.ShowOk(mTGALocalizedString, mTGALocalizedString2);
			}
			CurrentPurchaseMode = InventoryPurchaseMode.None;
			sceneLoader.EnableLoadingIndicator(shouldEnable: false);
			this.OnPurchaseSkinResponse?.Invoke(grpId, cardSkin.Variant, promise.Successful);
		}
	}

	public void HandlePurchaseAvailabilityWarnings(RotationWarningContext context, SetAvailability availability, bool hasCardsBannedInDeckFormat, string localizedPurchaseButton, Action purchase, Action cancel, bool willBeOverRestrictedLimit = false)
	{
		bool flag = (availability == SetAvailability.RotatingOutSoonAlchemy || availability == SetAvailability.RotatingOutSoonStandard) && !_rotationWarningsSeen.Contains(context);
		if (hasCardsBannedInDeckFormat && flag)
		{
			confirmPurchaseBannedCardDialog(localizedPurchaseButton, delegate
			{
				confirmPurchaseRotatingOutSoonDialog(localizedPurchaseButton, purchase, cancel);
			}, cancel);
		}
		else if (hasCardsBannedInDeckFormat)
		{
			confirmPurchaseBannedCardDialog(localizedPurchaseButton, purchase, cancel);
		}
		else if (flag)
		{
			_rotationWarningsSeen.Add(context);
			confirmPurchaseRotatingOutSoonDialog(localizedPurchaseButton, purchase, cancel);
		}
		else if (willBeOverRestrictedLimit)
		{
			confirmPurchaseRestrictedCardDialog(localizedPurchaseButton, purchase, cancel);
		}
		else
		{
			purchase();
		}
	}

	private void confirmPurchaseRotatingOutSoonDialog(string localizedPurchaseButton, Action purchase, Action cancel)
	{
		SceneLoader.GetSceneLoader().SystemMessages.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/Warning_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/Warning_Message"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_Cancel"), cancel, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/Warning_LearnMoreButton"), delegate
		{
			cancel();
			UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Learn/SetRotation_Url"));
		}, localizedPurchaseButton, purchase);
	}

	private void confirmPurchaseBannedCardDialog(string localizedPurchaseButton, Action purchase, Action cancel)
	{
		SceneLoader.GetSceneLoader().SystemMessages.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("BanAnnouncement/Banned_Card_Redemption_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/WarningUnavailable_Message"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_Cancel"), cancel, localizedPurchaseButton, purchase);
	}

	private void confirmPurchaseRestrictedCardDialog(string localizedPurchaseButton, Action purchase, Action cancel)
	{
		SceneLoader.GetSceneLoader().SystemMessages.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("RestrictedAnnouncement/Restricted_Craft_Warning_Title"), Languages.ActiveLocProvider.GetLocalizedText("RestrictedAnnouncement/Restricted_Craft_Warning_Body"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Popups/Modal/ModalOptions_Cancel"), cancel, localizedPurchaseButton, purchase);
	}
}
