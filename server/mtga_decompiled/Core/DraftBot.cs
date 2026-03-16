using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AWS;
using Core.Code.Promises;
using Core.Shared.Code.DebugTools;
using Core.Shared.Code.Network.Utils;
using Newtonsoft.Json;
using UnityEngine;
using WAS;
using Wizards.Arena.Client.Logging;
using Wizards.Arena.Enums.Store;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.Arena.TcpConnection;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Currency;
using Wizards.Unification.Models.Draft;
using Wizards.Unification.Models.Event;
using Wizards.Unification.Models.Events;
using Wizards.Unification.Models.FrontDoor;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

public class DraftBot
{
	public enum StatusType
	{
		NotConnected,
		Connecting,
		NotInEvent,
		WaitingForPod,
		NotReady,
		WaitingForReady,
		WaitingForPack,
		Picking,
		CompleteDraft,
		ConfirmCardPool,
		Done
	}

	private enum BotState
	{
		NotInPod,
		WaitingForPod,
		ReceivedReadyReq,
		WaitingForOthersToReady,
		WaitingForPack,
		Picking,
		PickingCompleted,
		CardPoolGranted,
		Completed
	}

	private string _email;

	private string _password;

	private EnvironmentDescription _environment;

	private FrontDoorConnectionAWS _frontDoor;

	private IEventsServiceWrapper _eventsServiceWrapper;

	private CosmeticsProvider _cosmeticsProvider;

	private DeckDataProvider _deckDataProvider;

	private IInventoryServiceWrapper _inventoryServiceWrapper;

	private IAccountClient _accountClient;

	private AccountInformation _accountInformation;

	private Wizards.Arena.Client.Logging.ILogger _logger;

	private EventInfoV3 _event;

	private ICourseInfoWrapper _course;

	private int _seatIndex;

	private int _numAsyncOps;

	private int _numPlayersInPod;

	private int _podCapacity;

	private int _numPlayersReady;

	private BotState _botState;

	private TableInfo _tableInfo;

	private string _draftId = "";

	private int _packNumber;

	private int _pickNumber;

	private int _numCardsToPick;

	private int _secondsLeft;

	private List<int> _cardsInCurrentPack;

	public string GetEmail()
	{
		return _email;
	}

	public string GetPassword()
	{
		return _password;
	}

	public int GetNumPlayersInPod()
	{
		return _numPlayersInPod;
	}

	public int GetPodCapacity()
	{
		return _numPlayersInPod;
	}

	public int GetSeatIndex()
	{
		return _seatIndex;
	}

	public bool IsBusy()
	{
		return _numAsyncOps > 0;
	}

	public StatusType GetStatusType()
	{
		if (_frontDoor.ConnectionState == FrontDoorConnectionAWS.FrontDoorConnectionState.Disconnected)
		{
			return StatusType.NotConnected;
		}
		if (!_frontDoor.Connected)
		{
			return StatusType.Connecting;
		}
		if (_course == null || _course.CurrentEventModule == PlayerEventModule.Join)
		{
			return StatusType.NotInEvent;
		}
		if (_course.CurrentEventModule != PlayerEventModule.HumanDraft)
		{
			return StatusType.Done;
		}
		if (_botState == BotState.ReceivedReadyReq)
		{
			return StatusType.NotReady;
		}
		if (_botState == BotState.WaitingForOthersToReady)
		{
			return StatusType.WaitingForReady;
		}
		if (_botState == BotState.WaitingForPod)
		{
			return StatusType.WaitingForPod;
		}
		if (_botState == BotState.Picking)
		{
			return StatusType.Picking;
		}
		if (_botState == BotState.PickingCompleted)
		{
			return StatusType.CompleteDraft;
		}
		if (_botState == BotState.CardPoolGranted)
		{
			return StatusType.ConfirmCardPool;
		}
		if (_botState == BotState.Completed)
		{
			return StatusType.Done;
		}
		if (_botState == BotState.WaitingForPack)
		{
			return StatusType.WaitingForPack;
		}
		return StatusType.NotInEvent;
	}

	public string GetStatusString()
	{
		if (!_frontDoor.Connected)
		{
			return _frontDoor.ConnectionState.ToString();
		}
		if (_course == null)
		{
			return "Not in event";
		}
		if (_course.CurrentEventModule != PlayerEventModule.HumanDraft)
		{
			return _course.CurrentEventModule.ToString();
		}
		if (_botState == BotState.ReceivedReadyReq)
		{
			return "Need to ready up";
		}
		if (_botState == BotState.WaitingForOthersToReady)
		{
			return $"Ready. Waiting for others ({_numPlayersReady}/{_podCapacity} ready).";
		}
		if (_botState == BotState.WaitingForPod)
		{
			return $"In Pod - {_numPlayersInPod}/{_podCapacity} joined.";
		}
		if (_botState == BotState.Picking)
		{
			string text = ((_numCardsToPick > 1) ? $" (picking {_numCardsToPick} cards)" : "");
			return $"Picking - Pack {_packNumber} - Pick {_pickNumber}{text} ({_secondsLeft}s left)";
		}
		if (_botState == BotState.WaitingForPack)
		{
			return "Waiting for pack to arrive";
		}
		return "Not joined to pod queue";
	}

	public DraftBot(string email, string password, EnvironmentDescription environment, EventInfoV3 evt, Wizards.Arena.Client.Logging.ILogger logger)
	{
		_email = email;
		_password = password;
		_environment = environment;
		_logger = logger;
		_event = evt;
		FDCConnectionConfig config = new FDCConnectionConfig
		{
			ApplicationVersion = Application.version,
			InactivityTimeoutMs = MDNPlayerPrefs.InactivityTimeoutMs,
			IsEditor = Application.isEditor,
			ClientInfo = new BIClientInfo
			{
				DeviceId = SystemInfo.deviceUniqueIdentifier,
				DeviceModel = SystemInfo.deviceModel,
				OS = SystemInfo.operatingSystem,
				Platform = PlatformUtils.GetClientPlatform()
			},
			MessageCountLimit = 30
		};
		_accountClient = new WizardsAccountsClient();
		_accountClient.SetCredentials(environment);
		_frontDoor = new FrontDoorConnectionAWS(config, RecordHistoryUtils.ShouldRecordHistory, _logger, "Draft Bot: " + email);
		_accountClient = new WizardsAccountsClient();
		_frontDoor.OnMsg_PodQueueNotification += OnDraftPodNotificationReceived;
		_frontDoor.OnMsg_DraftNotification += OnDraftNotification;
		_cosmeticsProvider = new CosmeticsProvider(new AwsCosmeticsServiceWrapper(_frontDoor));
		_deckDataProvider = new DeckDataProvider(new AwsDeckServiceWrapper(_frontDoor));
		_inventoryServiceWrapper = new AwsInventoryServiceWrapper(_frontDoor, _cosmeticsProvider, _deckDataProvider);
		_eventsServiceWrapper = new AwsEventServiceWrapper(_frontDoor, _inventoryServiceWrapper);
	}

	public Promise<AccountInformation> ConnectToFrontdoor(int depth = 0)
	{
		_numAsyncOps++;
		return _accountClient.Debug_LogIn_Familiar(_email, _password).ThenOnMainThreadIfSuccess(delegate(AccountInformation accountInfo)
		{
			_accountInformation = accountInfo;
			FDCConnectionParams parameters = new FDCConnectionParams
			{
				Host = _environment.fdHost,
				Port = _environment.fdPort,
				SessionTicket = _accountInformation.Credentials.Jwt,
				ClientVersion = Application.version,
				IsDebugAccount = true,
				AcceptsPolicy = () => true
			};
			_frontDoor.OnConnected += OnConnectedToFrontdoor;
			_frontDoor.Connect(parameters);
		}).IfError(delegate(Promise<AccountInformation> p)
		{
			_frontDoor.OnConnected -= OnConnectedToFrontdoor;
			_numAsyncOps--;
			if (p.Error.Exception != null)
			{
				PromiseExtensions.Logger.Error($"Connection failed for {_email}\nException: {p.Error.Exception}");
			}
			else
			{
				AccountError accountError = WASUtils.ToAccountError(p.Error);
				PromiseExtensions.Logger.Error("Connection failed for " + _email + "\nWASError: " + accountError?.ErrorMessage);
				if (accountError != null && accountError.ErrorType == AccountError.ErrorTypes.UpdateRequired)
				{
					_accountClient.UpdateToken = accountError.UpdateToken;
					string dob = new DateTime(1999, 1, 1).ToString("yyyy-MM-dd");
					return _accountClient.UpdateParentalConsent("US", dob).Then(delegate(Promise<string> promise)
					{
						if (promise.Successful && depth < 2)
						{
							return ConnectToFrontdoor(depth + 1);
						}
						PromiseExtensions.Logger.Error("Failed to update parental consent for " + _email + " using US and " + dob);
						return new SimplePromise<AccountInformation>(promise.Error);
					});
				}
			}
			return new SimplePromise<AccountInformation>(p.Error);
		}, allowRecovery: true);
	}

	public void OnConnectedToFrontdoor()
	{
		_numAsyncOps--;
		_frontDoor.OnConnected -= OnConnectedToFrontdoor;
		PAPA.StartGlobalCoroutine(RunStartHook());
	}

	private IEnumerator RunStartHook()
	{
		yield return _frontDoor.SendMessage<StartHookResponse>(CmdType.StartHook, new StartHookReq()).IfError(delegate(Error e)
		{
			PromiseExtensions.Logger.Error($"{_email} Starthook Response failed! {e}");
		}).AsCoroutine();
	}

	public void Destroy()
	{
		_frontDoor?.Close(TcpConnectionCloseType.NormalClosure, "DraftBot.Destroy");
	}

	private IEnumerator RefreshEventState()
	{
		_numAsyncOps++;
		_course = null;
		Promise<ICourseInfoWrapper> promise = _eventsServiceWrapper.GetEventCourse(_event.InternalEventName).IfSuccess(delegate(Promise<ICourseInfoWrapper> p)
		{
			_course = p.Result;
			_draftId = _course?.DraftId;
		});
		yield return promise.AsCoroutine();
		_numAsyncOps--;
	}

	private List<EventEntryFee> GetAvailableEntryFees()
	{
		List<EventEntryFee> list = new List<EventEntryFee>();
		foreach (EventEntryFee entryFee in _event.EntryFees)
		{
			if (!entryFee.MaxUses.HasValue)
			{
				list.Add(entryFee);
				continue;
			}
			int num = 0;
			if (_event.PastEntries != null && _event.PastEntries.TryGetValue(entryFee.CurrencyType, out var value))
			{
				num = value.Quantity;
			}
			if (num < entryFee.MaxUses)
			{
				list.Add(entryFee);
			}
		}
		return list;
	}

	public IEnumerator GetIntoEvent(bool isRetry = false)
	{
		yield return RefreshEventState();
		_numAsyncOps++;
		if (_course == null || _course.CurrentEventModule == PlayerEventModule.Join)
		{
			IFDPromise<CurrencyQtys> currencyPromise = _frontDoor.GetPlayerCurrencies();
			yield return currencyPromise.Coroutine_Wait();
			CurrencyQtys playerCurrencies = currencyPromise.Result;
			List<EventEntryFee> availableEntryFees = GetAvailableEntryFees();
			EventEntryFee eventEntryFee = availableEntryFees.FirstOrDefault((EventEntryFee entryFee) => GetPlayerOwnedCurrencyQty(playerCurrencies, entryFee) >= entryFee.Quantity);
			if (eventEntryFee == null && availableEntryFees.Count > 0)
			{
				_numAsyncOps--;
				SimpleLog.LogError(_email + " does not have a way to pay entry fee for event.");
				yield break;
			}
			yield return _eventsServiceWrapper.JoinAndPay(_event.InternalEventName, (eventEntryFee?.CurrencyType ?? Wizards.Arena.Enums.Store.PurchaseCurrency.None).ToEventEntryCurrency(), eventEntryFee?.Quantity ?? 0, eventEntryFee?.ReferenceId, "").Then(delegate(Promise<ICourseInfoWrapper> p)
			{
				_course = p.Result;
			}).IfSuccess(delegate
			{
				_botState = BotState.NotInPod;
			})
				.IfError(delegate(Error e)
				{
					if (e.Code == 5015)
					{
						if (_course.CurrentEventModule != PlayerEventModule.HumanDraft)
						{
							_botState = BotState.Completed;
						}
					}
					else
					{
						PromiseExtensions.Logger.Error($"{_email} failed to join event. (Error: {e})");
					}
				})
				.AsCoroutine();
		}
		yield return RefreshEventState();
		if (_course == null)
		{
			_numAsyncOps--;
			yield break;
		}
		if (_course.CurrentEventModule == PlayerEventModule.HumanDraft && _course.DraftId == null)
		{
			_botState = BotState.NotInPod;
		}
		if (_course.CurrentEventModule == PlayerEventModule.HumanDraft && !string.IsNullOrWhiteSpace(_course.DraftId))
		{
			yield return _eventsServiceWrapper.TryRejoinHumanDraft(_course.InternalEventName).IfSuccess(delegate(Promise<ICourseInfoWrapper> p)
			{
				_course = p.Result;
				_botState = ((_course.DraftId != null) ? BotState.WaitingForPack : BotState.NotInPod);
			}).IfError((Action<Error>)delegate
			{
				_botState = BotState.NotInPod;
			})
				.AsCoroutine();
		}
		if (_botState == BotState.NotInPod && _course.CurrentEventModule == PlayerEventModule.HumanDraft)
		{
			yield return _eventsServiceWrapper.JoinNewPodQueue(_event.InternalEventName).IfSuccess(delegate
			{
				_botState = BotState.WaitingForPod;
			}).IfError(delegate(Error e)
			{
				PromiseExtensions.Logger.Error($"{_email} failed to join Queue. (Error: {e.Code})");
			})
				.AsCoroutine();
		}
		_numAsyncOps--;
	}

	public IEnumerator BecomeReady()
	{
		_numAsyncOps++;
		IFDPromise<ClientPlayerCourseV2> readyPromise = _frontDoor.ReadyDraft(_event.InternalEventName, _draftId);
		yield return readyPromise.Coroutine_Wait();
		if (readyPromise.Successful)
		{
			if (_botState == BotState.ReceivedReadyReq)
			{
				_botState = BotState.WaitingForOthersToReady;
			}
		}
		else
		{
			SimpleLog.LogError($"{_email} failed to ready up for draft (Error: {readyPromise.ErrorCode})");
		}
		_numAsyncOps--;
	}

	public IEnumerator CancelPod()
	{
		_numAsyncOps++;
		yield return _eventsServiceWrapper.DropFromAllPodQueues().IfSuccess(delegate
		{
			_botState = BotState.NotInPod;
			_podCapacity = 0;
			_numPlayersInPod = 0;
		}).IfError(delegate(Error e)
		{
			PromiseExtensions.Logger.Error($"{_email} failed to drop from pods (Error: {e.Code})");
		});
		_numAsyncOps--;
	}

	public IEnumerator PickFirstInList()
	{
		_numAsyncOps++;
		_botState = BotState.WaitingForPack;
		IFDPromise<PickResponse> pickPromise = _frontDoor.MakeHumanDraftPick(_draftId, _cardsInCurrentPack.Take(_numCardsToPick).ToList(), _packNumber, _pickNumber);
		yield return pickPromise.Coroutine_Wait();
		if (pickPromise.Successful)
		{
			if (pickPromise.Result.IsPickingCompleted)
			{
				_botState = BotState.PickingCompleted;
			}
		}
		else
		{
			_botState = BotState.Picking;
			SimpleLog.LogError($"{_email} failed to make pick (Error: {pickPromise.ErrorCode})");
		}
		_numAsyncOps--;
	}

	public IEnumerator CompleteDraft()
	{
		_numAsyncOps++;
		IFDPromise<ClientPlayerCourseV2> completePromise = _frontDoor.CompleteDraft(_event.InternalEventName, isBotDraft: false);
		yield return completePromise.Coroutine_Wait();
		if (completePromise.Successful)
		{
			_botState = BotState.CardPoolGranted;
		}
		else
		{
			SimpleLog.LogError($"{_email} failed to complete draft (Error: {completePromise.ErrorCode})");
		}
		_numAsyncOps--;
	}

	public IEnumerator ConfirmCardPool()
	{
		_numAsyncOps++;
		IFDPromise<ClientPlayerCourseV2> confirmPromise = _frontDoor.ConfirmDraftCardPoolGrant(_event.InternalEventName);
		yield return confirmPromise.Coroutine_Wait();
		if (confirmPromise.Successful)
		{
			_botState = BotState.Completed;
		}
		else
		{
			SimpleLog.LogError($"{_email} failed to confirm card pool (Error: {confirmPromise.ErrorCode})");
		}
		_numAsyncOps--;
	}

	public IEnumerator Drop()
	{
		if (_course == null)
		{
			yield break;
		}
		_numAsyncOps++;
		if (string.IsNullOrWhiteSpace(_course.DraftId))
		{
			yield return _eventsServiceWrapper.DropFromAllPodQueues().IfSuccess(delegate
			{
				_botState = BotState.NotInPod;
				_podCapacity = 0;
				_numPlayersInPod = 0;
			}).AsCoroutine();
		}
		yield return _eventsServiceWrapper.DropFromEvent(_course.InternalEventName).IfSuccess(delegate
		{
			_botState = BotState.NotInPod;
			_podCapacity = 0;
			_numPlayersInPod = 0;
		}).IfError(delegate(Error e)
		{
			PromiseExtensions.Logger.Error($"{_email} failed to drop event: {e}");
		})
			.AsCoroutine();
		yield return RefreshEventState();
		_numAsyncOps--;
	}

	public void Update()
	{
		_frontDoor?.ProcessMessages();
	}

	private void OnDraftPodNotificationReceived(PodQueueNotification notification)
	{
		Debug.Log("Got notif: " + JsonConvert.SerializeObject(notification));
		_numPlayersInPod = notification.NumInPod;
		_podCapacity = notification.PodCapacity;
	}

	private void OnDraftNotification(DraftNotification notification)
	{
		Debug.Log(_email + " got notif: " + JsonConvert.SerializeObject(notification));
		if (notification.Type == EDraftNotificationType.Close)
		{
			_botState = BotState.NotInPod;
			_podCapacity = 0;
			_numPlayersInPod = 0;
			_draftId = null;
			return;
		}
		if (notification.Type == EDraftNotificationType.ReadyReq)
		{
			_draftId = notification.DraftId;
			_botState = BotState.ReceivedReadyReq;
		}
		if (notification.Type == EDraftNotificationType.ReadyUpdate)
		{
			_numPlayersReady = notification.NumReady;
		}
		if (notification.Type == EDraftNotificationType.TableInfo)
		{
			string screenName = _accountInformation?.DisplayName;
			_seatIndex = notification.TableInfo.Players.FindIndex((PlayerInfo pi) => pi.ScreenName == screenName);
		}
		if (notification.Type == EDraftNotificationType.PickReq || (notification.Type == EDraftNotificationType.TableInfo && notification.PickInfo != null))
		{
			_packNumber = notification.PickInfo.SelfPack;
			_pickNumber = notification.PickInfo.SelfPick;
			_numCardsToPick = notification.PickInfo.NumCardsToPick;
			_secondsLeft = notification.PickInfo.TimeoutSec;
			_cardsInCurrentPack = notification.PickInfo.PackCards;
			List<int> cardsInCurrentPack = _cardsInCurrentPack;
			_botState = ((cardsInCurrentPack != null && cardsInCurrentPack.Count > 0) ? BotState.Picking : BotState.WaitingForPack);
		}
	}

	private int GetPlayerOwnedCurrencyQty(CurrencyQtys playerCurrency, EventEntryFee entryFee)
	{
		switch (entryFee.CurrencyType)
		{
		case Wizards.Arena.Enums.Store.PurchaseCurrency.Gold:
			return playerCurrency.Gold;
		case Wizards.Arena.Enums.Store.PurchaseCurrency.Gem:
			return playerCurrency.Gems;
		case Wizards.Arena.Enums.Store.PurchaseCurrency.SealedToken:
			return playerCurrency.SealedToken;
		case Wizards.Arena.Enums.Store.PurchaseCurrency.DraftToken:
			return playerCurrency.DraftToken;
		case Wizards.Arena.Enums.Store.PurchaseCurrency.CustomToken:
		{
			if (playerCurrency.CustomTokens != null && playerCurrency.CustomTokens.TryGetValue(entryFee.ReferenceId, out var value))
			{
				return value;
			}
			return 0;
		}
		case Wizards.Arena.Enums.Store.PurchaseCurrency.None:
			return 0;
		default:
			SimpleLog.LogError(string.Format("{0}: Invalid currency type passed: {1}", "GetPlayerOwnedCurrencyQty", entryFee.CurrencyType));
			return -1;
		}
	}
}
