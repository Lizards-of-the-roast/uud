using System;
using System.Collections;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Avatar;
using Core.Code.Promises;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Unification.Models.Draft;
using Wizards.Unification.Models.Event;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Wotc.Mtga.Wrapper.Draft;

public class TableDraftQueueContentController : NavContentController
{
	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private TableDraftQueueView _tableDraftQueueView;

	[SerializeField]
	private Image _avatarImage;

	[SerializeField]
	private float _delayBeforeDraft = 1f;

	[Header("Buttons")]
	[SerializeField]
	private CustomButton _cancelButton;

	[SerializeField]
	private CustomButton _readyButton;

	[SerializeField]
	private CustomButton _settingsButton;

	[Header("Timer Parameters")]
	[SerializeField]
	private Slider _sparkTimerSlider;

	[SerializeField]
	private TextChronometer _textChronometer;

	[SerializeField]
	private Localize _chronometerContextText;

	private AssetLoader.AssetTracker<Sprite> _avatarImageSpriteTracker;

	private int Active_BoolFlag = Animator.StringToHash("Active");

	private int Ready_BoolFlag = Animator.StringToHash("Ready");

	private int Confirmed_BoolFlag = Animator.StringToHash("Confirmed");

	private int PlayerReveal_TriggerFlag = Animator.StringToHash("PlayerReveal");

	private EventContext _eventContext;

	private Coroutine _sparkTimerCoroutine;

	private bool _isLocalPlayerReady;

	private bool _readyToShow;

	private string _draftId;

	private IEventsServiceWrapper _eventsServiceWrapper;

	private CosmeticsProvider _cosmetics;

	private NavBarController _navBarController;

	private IAccountClient _accountClient;

	private SceneLoader _sceneLoader;

	private AssetLookupSystem _assetLookupSystem;

	private SettingsMenuHost _settingsMenuHost;

	private DateTime _queueJoinedTime;

	private int _prevNumInPod;

	public override NavContentType NavContentType => NavContentType.TableDraftQueue;

	public override bool IsReadyToShow => _readyToShow;

	private void Awake()
	{
		_cancelButton.OnClick.AddListener(HandleCancelButtonClicked);
		_readyButton.OnClick.AddListener(HandleReadyButtonClicked);
		_settingsButton.OnClick.AddListener(HandleSettingsButtonClicked);
	}

	public override void Activate(bool active)
	{
		if (active)
		{
			_eventsServiceWrapper.AddPodQueueNotificationCallback(HandlePodQueueNotification);
			_eventsServiceWrapper.AddDraftNotificationCallback(HandleDraftNotification);
		}
		else
		{
			_eventsServiceWrapper.RemovePodQueueNotificationCallback(HandlePodQueueNotification);
			_eventsServiceWrapper.RemoveDraftNotificationCallback(HandleDraftNotification);
		}
	}

	public void Init(SceneLoader sceneLoader, IEventsServiceWrapper eventsServiceWrapper, CosmeticsProvider cosmetics, IAccountClient accountClient, NavBarController navBarController, AssetLookupSystem assetLookupSystem, SettingsMenuHost settingsMenuHost)
	{
		_eventsServiceWrapper = eventsServiceWrapper;
		_cosmetics = cosmetics;
		_navBarController = navBarController;
		_accountClient = accountClient;
		_sceneLoader = sceneLoader;
		_assetLookupSystem = assetLookupSystem;
		_settingsMenuHost = settingsMenuHost;
	}

	public void SetEvent(EventContext eventContext)
	{
		_eventContext = eventContext;
	}

	public override void OnBeginOpen()
	{
		_animator.SetBool(Active_BoolFlag, value: true);
		_tableDraftQueueView.InitTable(_accountClient.AccountInformation?.DisplayName, _cosmetics.PlayerAvatarSelection, _assetLookupSystem);
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.CosmeticAvatarId = _cosmetics.PlayerAvatarSelection;
		FullBodyPayload payload = _assetLookupSystem.TreeLoader.LoadTree<FullBodyPayload>().GetPayload(_assetLookupSystem.Blackboard);
		if (payload != null)
		{
			if (_avatarImageSpriteTracker == null)
			{
				_avatarImageSpriteTracker = new AssetLoader.AssetTracker<Sprite>("TebleDraftAvatarImageSprite");
			}
			AssetLoaderUtils.TrySetSprite(_avatarImage, _avatarImageSpriteTracker, payload.Reference.RelativePath);
		}
		_navBarController.gameObject.SetActive(value: false);
		_queueJoinedTime = DateTime.UtcNow;
		Reset("Draft/Draft_QueueHeader_FindTable");
		Activate(active: true);
	}

	private void Reset(string chronometerTextKey)
	{
		_prevNumInPod = 0;
		_draftId = null;
		_isLocalPlayerReady = false;
		_chronometerContextText.SetText(chronometerTextKey);
		_textChronometer.StartCountUp((float)DateTime.UtcNow.Subtract(_queueJoinedTime).TotalSeconds);
		_readyButton.Interactable = true;
		StartCoroutine(Coroutine_JoinQueue());
	}

	public override void OnBeginClose()
	{
		_readyToShow = false;
		_tableDraftQueueView.ResetTable();
		_animator.SetBool(Active_BoolFlag, value: false);
		if (_sparkTimerCoroutine != null)
		{
			StopCoroutine(_sparkTimerCoroutine);
			_sparkTimerCoroutine = null;
		}
		_navBarController.gameObject.SetActive(value: true);
		Activate(active: false);
	}

	public override void OnFinishClose()
	{
		base.OnFinishClose();
		AssetLoaderUtils.CleanupImage(_avatarImage, _avatarImageSpriteTracker);
	}

	public void HandleCancelButtonClicked()
	{
		StartCoroutine(Coroutine_DropFromQueue());
	}

	public void HandleReadyButtonClicked()
	{
		_isLocalPlayerReady = true;
		_readyButton.Interactable = false;
		ReadyToDraft();
	}

	public void HandleSettingsButtonClicked()
	{
		_settingsMenuHost.Open();
	}

	public void HandlePodQueueNotification(PodQueueNotification podQueueNotification)
	{
		if (_prevNumInPod > podQueueNotification.NumInPod)
		{
			AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
		}
		_prevNumInPod = podQueueNotification.NumInPod;
		_tableDraftQueueView.InitSeats(podQueueNotification.PodCapacity);
		_tableDraftQueueView.UpdateFilledSeats(podQueueNotification.NumInPod, podQueueNotification.PodCapacity);
		_tableDraftQueueView.UpdateWaitingMessage(podQueueNotification.NumInPod, podQueueNotification.PodCapacity);
	}

	private IEnumerator Coroutine_ResetText()
	{
		yield return new WaitForSeconds(10f);
		if (_sparkTimerCoroutine == null)
		{
			_chronometerContextText.SetText("Draft/Draft_QueueHeader_FindTable");
		}
	}

	public void HandleDraftNotification(DraftNotification draftNotification)
	{
		switch (draftNotification.Type)
		{
		case EDraftNotificationType.ReadyReq:
			_animator.SetBool(Ready_BoolFlag, value: true);
			_draftId = draftNotification.DraftId;
			if (_sparkTimerCoroutine == null)
			{
				TaskbarFlash.Flash();
				_sparkTimerCoroutine = StartCoroutine(Coroutine_SparkTimer(draftNotification.TimeoutSec));
			}
			_textChronometer.StartCountDown(draftNotification.TimeoutSec);
			_chronometerContextText.SetText("MainNav/General/Empty_String");
			break;
		case EDraftNotificationType.ReadyUpdate:
			if (!string.IsNullOrWhiteSpace(_draftId))
			{
				_tableDraftQueueView.UpdateReadySeats(draftNotification.NumReady, _isLocalPlayerReady);
			}
			break;
		case EDraftNotificationType.TableInfo:
		{
			if (string.IsNullOrWhiteSpace(_draftId))
			{
				break;
			}
			if (_sparkTimerCoroutine != null)
			{
				StopCoroutine(_sparkTimerCoroutine);
				_sparkTimerCoroutine = null;
			}
			List<KnownSeatVisualData> list = new List<KnownSeatVisualData>();
			List<PlayerInfo> players = draftNotification.TableInfo.Players;
			for (int num = 0; num < players.Count; num++)
			{
				if (num != draftNotification.TableInfo.SelfSeat)
				{
					list.Add(TableDraftQueueFunctions.GetKnownSeatVisualData(new PlayerInSeat(players[num])));
				}
			}
			AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
			_animator.SetTrigger(PlayerReveal_TriggerFlag);
			_tableDraftQueueView.RevealAllSeatIdentities(list);
			StartCoroutine(Coroutine_MoveToDraftScene(draftNotification));
			break;
		}
		case EDraftNotificationType.Close:
			if (_isLocalPlayerReady)
			{
				_tableDraftQueueView.ResetTable();
				_sparkTimerSlider.value = 0f;
				StartCoroutine(Coroutine_ResetText());
				_animator.SetBool(Ready_BoolFlag, value: false);
				_animator.SetBool(Confirmed_BoolFlag, value: false);
				Reset("Draft/RebuildingQueue");
			}
			else
			{
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("Draft/RemovedFromQueue_Title"), Languages.ActiveLocProvider.GetLocalizedText("Draft/RemovedFromQueue_Description"), delegate
				{
					_sceneLoader.GoToEventScreen(_eventContext);
				});
			}
			break;
		case EDraftNotificationType.PickReq:
			break;
		}
	}

	private IEnumerator Coroutine_SparkTimer(float totalTime)
	{
		for (float currentTime = 0f; currentTime < totalTime; currentTime += Time.deltaTime)
		{
			_sparkTimerSlider.value = 1f - currentTime / totalTime;
			yield return null;
		}
		_sparkTimerSlider.value = 0f;
		_textChronometer.StopChronometer();
		_sparkTimerCoroutine = null;
	}

	private IEnumerator Coroutine_JoinQueue()
	{
		Promise<Unit> joinPodPromise = _eventsServiceWrapper.JoinNewPodQueue(_eventContext.PlayerEvent.EventInfo.InternalEventName);
		yield return joinPodPromise.AsCoroutine();
		_readyToShow = true;
		if (!joinPodPromise.Successful && joinPodPromise.ErrorSource != ErrorSource.Debounce)
		{
			_sceneLoader.GoToEventScreen(_eventContext);
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Join_Error_Text"));
		}
	}

	private void ReadyToDraft()
	{
		if (!(_eventContext.PlayerEvent is LimitedPlayerEvent limitedPlayerEvent))
		{
			return;
		}
		limitedPlayerEvent.ReadyDraft(_draftId).ThenOnMainThread(delegate(Promise<ICourseInfoWrapper> p)
		{
			if (p.Successful)
			{
				PlayerInSeat seat = new PlayerInSeat(_accountClient.AccountInformation?.DisplayName, _cosmetics.PlayerAvatarSelection, isReady: true);
				_tableDraftQueueView.UpdateLocalSeat(TableDraftQueueFunctions.GetKnownSeatVisualData(seat));
				_animator.SetBool(Confirmed_BoolFlag, value: true);
				_chronometerContextText.SetText("Draft/Draft_QueueHeader_Confirmed");
			}
			else
			{
				_isLocalPlayerReady = true;
				_sceneLoader.GoToEventScreen(_eventContext);
				SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Join_Error_Text"));
			}
		});
	}

	private IEnumerator Coroutine_DropFromQueue()
	{
		Promise<ModuleResponse> dropPromise = _eventsServiceWrapper.DropFromAllPodQueues();
		yield return dropPromise.AsCoroutine();
		if (!dropPromise.Successful)
		{
			SystemMessageManager.Instance.ShowOk(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Network_Error_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Event_Join_Error_Text"));
		}
		_sceneLoader.GoToEventScreen(_eventContext);
	}

	private IEnumerator Coroutine_MoveToDraftScene(DraftNotification draftInfo)
	{
		yield return new WaitForSeconds(_delayBeforeDraft);
		_sceneLoader.GoToDraftScene(_eventContext);
	}
}
