using System;
using GreClient.Rules;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

public class AccessoryController_Legacy : AccessoryController, IPointerClickHandler, IEventSystemHandler
{
	public new const string INTERACTION_CATEGORY = "AccessoryInteraction";

	public bool HoverOverride;

	public bool LoopHoverAnim;

	[SerializeField]
	protected Collider _colorChangeClickCollider;

	[SerializeField]
	protected Collider _clickNearCollider;

	[Space(10f)]
	[SerializeField]
	protected float _colorChangeCooldownTime = 1f;

	[SerializeField]
	protected UnityEvent _colorChangeClickEvent;

	[Space(10f)]
	[SerializeField]
	protected float _colorChangeCompleteDelay = 0.5f;

	[SerializeField]
	protected UnityEvent _colorChangeCompleteEvent;

	[SerializeField]
	protected UnityEvent _clickNearEvent;

	protected ClientPetSelection _petSelection;

	protected EventTrigger _evtTriggerHover;

	protected EventTrigger _evtTriggerClick;

	protected bool _petting;

	protected float _pettingAccumulator;

	protected float _pettingThreshold = 6f;

	protected UIMessageHandler _uiMessageHandler;

	public Collider ClickNearCollider => _clickNearCollider;

	public override void Init(GameManager gameManager, GREPlayerNum playerNum, CosmeticsProvider cosmetics = null, ClientPetSelection petSelection = null)
	{
		_gameManager = gameManager;
		_ownerPlayerNum = playerNum;
		_cosmetics = cosmetics;
		_petSelection = petSelection;
		CombatAnimationPlayer.DamageDealtByCard += OnDamageDealtByCard;
		GameEndUXEvent.GameEndEventExecuted += OnGameEndEventExecuted;
		if ((bool)_gameManager)
		{
			_gameManager.UXEventQueue.EventExecutionCommenced += base.OnEventExecutionCommenced;
			_gameManager.UXEventQueue.EventExecutionCompleted += base.OnEventExecutionCompleted;
		}
		if (_ownerPlayerNum == GREPlayerNum.Opponent)
		{
			UpdateUIMessageHandler();
			if (overrideOpponentTransforms)
			{
				SetTransforms(opponentPosition, opponentRotation);
			}
			if (overrideTransformsOnHandheld && PlatformUtils.IsHandheld())
			{
				if (handheldTransformsGO != null)
				{
					handheldTransformsGO.transform.localPosition = opponentPositionHandheld;
				}
				else
				{
					Debug.Log("No child with the name  <color=red>'HandheldTransforms' </color>attached");
				}
			}
			SetOpponentAnimController();
			if (MirrorOnOpponentSide)
			{
				MirrorTransform();
			}
		}
		if (clickCollider != null)
		{
			clickCollider.gameObject.AddComponent<EventTrigger>();
			_evtTriggerClick = clickCollider.gameObject.GetComponent<EventTrigger>();
			EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
			triggerEvent.AddListener(delegate
			{
				HandleClick();
			});
			EventTrigger.Entry item = new EventTrigger.Entry
			{
				callback = triggerEvent,
				eventID = EventTriggerType.PointerClick
			};
			_evtTriggerClick.triggers.Add(item);
		}
		if (hoverCollider != null)
		{
			hoverCollider.gameObject.AddComponent<EventTrigger>();
			_evtTriggerHover = hoverCollider.gameObject.GetComponent<EventTrigger>();
			if (HoverOverride)
			{
				EventTrigger.TriggerEvent triggerEvent2 = new EventTrigger.TriggerEvent();
				triggerEvent2.AddListener(delegate
				{
					HandleClick();
				});
				EventTrigger.Entry item2 = new EventTrigger.Entry
				{
					callback = triggerEvent2,
					eventID = EventTriggerType.PointerClick
				};
				_evtTriggerHover.triggers.Add(item2);
			}
			if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
			{
				EventTrigger.TriggerEvent triggerEvent3 = new EventTrigger.TriggerEvent();
				triggerEvent3.AddListener(delegate
				{
					HandleHoverEnter(GREPlayerNum.LocalPlayer);
				});
				EventTrigger.Entry item3 = new EventTrigger.Entry
				{
					callback = triggerEvent3,
					eventID = EventTriggerType.PointerEnter
				};
				_evtTriggerHover.triggers.Add(item3);
				EventTrigger.TriggerEvent triggerEvent4 = new EventTrigger.TriggerEvent();
				triggerEvent4.AddListener(delegate
				{
					HandleHoverExit(GREPlayerNum.LocalPlayer);
				});
				EventTrigger.Entry item4 = new EventTrigger.Entry
				{
					callback = triggerEvent4,
					eventID = EventTriggerType.PointerExit
				};
				_evtTriggerHover.triggers.Add(item4);
			}
		}
		OnGlobalMuteChanged(MDNPlayerPrefs.DisableEmotes);
		SubscribeToMuteEvents();
		_animator = GetComponentInChildren<Animator>();
		periodicTimer = new AccessoryPeriodicTimerManager(this, _animator, _minTimeToFidget, _maxTimeToFidget, _minTimeToTwitch, _maxTimeToTwitch);
		cooldownTimer = new AccessoryCooldownTimerManager(this, _animator, _localClickCooldownCache, _opponentClickCooldownCache);
		_tickables.Add(periodicTimer);
		_tickables.Add(cooldownTimer);
	}

	public override void Update()
	{
		if (base._muted)
		{
			return;
		}
		if (!_turnOffTimers)
		{
			foreach (ITickable tickable in _tickables)
			{
				tickable.Update(Time.deltaTime);
			}
		}
		if (_colorChangeClickCooldown > 0f)
		{
			_colorChangeClickCooldown -= Time.deltaTime;
			if (_colorChangeClickCooldown <= 0f)
			{
				_colorChangeCompleteEvent.Invoke();
			}
		}
		if (_ownerPlayerNum != GREPlayerNum.Opponent && _petting)
		{
			_pettingAccumulator += Time.deltaTime;
			if (_pettingAccumulator > HoverWarmup && !_warmedUp)
			{
				_warmedUp = true;
				hoverEnterEvent.Invoke();
			}
			if (_pettingAccumulator > _pettingThreshold && !LoopHoverAnim)
			{
				HandleHoverExit(_ownerPlayerNum);
			}
		}
	}

	public override void Cleanup()
	{
		GameEndUXEvent.GameEndEventExecuted -= OnGameEndEventExecuted;
		if (_uiMessageHandler != null)
		{
			_uiMessageHandler.GenericEventReceived -= OnGenericEventReceived;
		}
		CombatAnimationPlayer.DamageDealtByCard -= OnDamageDealtByCard;
		GameEndUXEvent.GameEndEventExecuted -= OnGameEndEventExecuted;
		if (_evtTriggerHover != null)
		{
			foreach (EventTrigger.Entry trigger in _evtTriggerHover.triggers)
			{
				trigger?.callback?.RemoveAllListeners();
			}
			_evtTriggerHover.triggers.Clear();
		}
		if (_evtTriggerClick != null)
		{
			foreach (EventTrigger.Entry trigger2 in _evtTriggerClick.triggers)
			{
				trigger2?.callback?.RemoveAllListeners();
			}
			_evtTriggerClick.triggers.Clear();
		}
		_gameManager = null;
	}

	protected new void OnDestroy()
	{
		Cleanup();
	}

	private void HandleColorChange(GREPlayerNum inputSource)
	{
		if (inputSource != _ownerPlayerNum)
		{
			return;
		}
		_colorChangeClickCooldown = _colorChangeCooldownTime + _colorChangeCompleteDelay;
		_pettingAccumulator = 0f;
		ResetBoredom();
		_colorChangeClickEvent.Invoke();
		if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
		{
			if (_gameManager != null)
			{
				_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", AccessoryInteraction.ColorChange.ToString());
			}
			LogInteraction(AccessoryInteraction.ColorChange);
		}
	}

	public new void HandleMuteOn()
	{
		_muteEvent.Invoke();
	}

	public new void HandleMuteOff()
	{
		_unmuteEvent.Invoke();
	}

	public override void HandleFidget()
	{
		_petFidgetEvent.Invoke();
		ResetBoredom();
	}

	public override void HandleIdleFidget()
	{
		_petTwitchEvent.Invoke();
		ResetRestlessness();
	}

	public override void HandleClick()
	{
		if (!base._muted)
		{
			if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
			{
				clickEvent.Invoke();
				LogInteraction(AccessoryInteraction.Fidget);
			}
			else if (_ownerPlayerNum == GREPlayerNum.Opponent && !(_opponentClickCooldown > 0f))
			{
				opponentClickEvent.Invoke();
				_opponentClickCooldown = 3f;
			}
		}
	}

	private void HandleClickNear()
	{
		if (_ownerPlayerNum == GREPlayerNum.LocalPlayer && !base._muted)
		{
			_clickNearEvent.Invoke();
			_clickNearCooldown = 2f;
			_pettingAccumulator = 0f;
			ResetBoredom();
			LogInteraction(AccessoryInteraction.Fidget);
		}
	}

	public override void HandleHoverEnter(GREPlayerNum inputSource)
	{
		if (inputSource == _ownerPlayerNum && !_petting && !base._muted)
		{
			_petting = true;
			_pettingAccumulator = 0f;
			ResetBoredom();
			if (inputSource == GREPlayerNum.LocalPlayer && _gameManager != null)
			{
				_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", AccessoryInteraction.HoverEnter.ToString());
			}
			LogInteraction(AccessoryInteraction.HoverEnter);
		}
	}

	public override void HandleHoverExit(GREPlayerNum inputSource)
	{
		if (inputSource == _ownerPlayerNum && !base._muted)
		{
			hoverExitEvent.Invoke();
			_petting = false;
			_pettingAccumulator = 0f;
			_warmedUp = false;
			ResetBoredom();
			if (inputSource == GREPlayerNum.LocalPlayer && _gameManager != null)
			{
				_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", AccessoryInteraction.HoverExit.ToString());
			}
			LogInteraction(AccessoryInteraction.HoverExit);
		}
	}

	public new void SetGlobalMuted(bool globalMuted)
	{
		_globalMuted = globalMuted;
	}

	public override void OnGlobalMuteChanged(bool globalMuted)
	{
		if (_ownerPlayerNum != GREPlayerNum.LocalPlayer)
		{
			bool muted = base._muted;
			_globalMuted = globalMuted;
			bool muted2 = base._muted;
			if (muted != muted2)
			{
				(base._muted ? _muteEvent : _unmuteEvent)?.Invoke();
			}
		}
	}

	public override void HandleHype(float hypeAmount)
	{
		if (!base._muted && hypeAmount != 0f)
		{
			if (hypeAmount >= 1f)
			{
				_petReactGoodLargeEvent.Invoke();
			}
			else if (hypeAmount > 0f)
			{
				_petReactGoodMediumEvent.Invoke();
			}
			else if (hypeAmount <= -1f)
			{
				_petReactBadLargeEvent.Invoke();
			}
			else
			{
				_petReactBadMediumEvent.Invoke();
			}
		}
	}

	protected new void OnDamageDealtByCard(MtgCardInstance affector, int amount)
	{
		float num = GetHypeFromDamageAmount(amount);
		if (!affector.Controller.IsLocalPlayer)
		{
			num *= -1f;
		}
		if (_ownerPlayerNum == GREPlayerNum.Opponent)
		{
			num *= -1f;
		}
		HandleHype(num);
	}

	protected new float GetHypeFromDamageAmount(int damage)
	{
		return (float)damage / 5f;
	}

	protected new void OnGameEndEventExecuted(GameEndUXEvent gameEndUxEvent)
	{
		if (gameEndUxEvent.Result == ResultType.WinLoss)
		{
			GREPlayerNum loser = gameEndUxEvent.Loser;
			if (_ownerPlayerNum == loser)
			{
				HandleDefeat();
			}
			else
			{
				HandleVictory();
			}
		}
	}

	protected override void OnGenericEventReceived(string category, string payload)
	{
		if (MDNPlayerPrefs.DisableEmotes || base._muted || _gameManager == null || !_gameManager.Context.TryGet<IEntityDialogControllerProvider>(out var result) || (result.TryGetDialogControllerByPlayerType(_ownerPlayerNum, out var dialogController) && dialogController.IsMuted()) || string.CompareOrdinal(category, "AccessoryInteraction") != 0 || !Enum.TryParse<AccessoryInteraction>(payload, ignoreCase: true, out var result2))
		{
			return;
		}
		switch (result2)
		{
		case AccessoryInteraction.HoverEnter:
			if (!_petting)
			{
				HandleHoverEnter(GREPlayerNum.Opponent);
			}
			break;
		case AccessoryInteraction.HoverExit:
			if (_petting)
			{
				HandleHoverExit(GREPlayerNum.Opponent);
			}
			break;
		case AccessoryInteraction.Fidget:
			if (!(_opponentClickCooldown > 0f))
			{
				HandleClick();
			}
			break;
		case AccessoryInteraction.ColorChange:
			if (!(_colorChangeClickCooldown > _colorChangeCooldownTime))
			{
				HandleColorChange(GREPlayerNum.Opponent);
			}
			break;
		}
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (_colorChangeClickCollider != null && eventData.pointerPressRaycast.gameObject == _colorChangeClickCollider.gameObject)
		{
			if (_colorChangeClickCooldown > _colorChangeCooldownTime)
			{
				return;
			}
			HandleColorChange(GREPlayerNum.LocalPlayer);
		}
		if (_clickNearCollider != null && eventData.rawPointerPress == _clickNearCollider.gameObject && !(_clickNearCooldown > 0f))
		{
			HandleClickNear();
		}
	}

	protected new void ResetBoredom()
	{
		_boredom = UnityEngine.Random.Range(_minTimeToFidget, _maxTimeToFidget);
	}

	protected new void ResetRestlessness()
	{
		_restlessness = UnityEngine.Random.Range(_minTimeToTwitch, _maxTimeToTwitch);
	}
}
