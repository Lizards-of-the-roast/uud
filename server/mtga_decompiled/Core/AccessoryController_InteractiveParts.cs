using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Providers;

public class AccessoryController_InteractiveParts : AccessoryController, IPointerClickHandler, IEventSystemHandler
{
	public new enum AccessoryInteraction
	{
		None,
		Fidget,
		HoverEnter,
		HoverExit,
		ColorChange,
		Mute
	}

	public new const string INTERACTION_CATEGORY = "AccessoryInteraction";

	public Collider chestCollider;

	public Collider armUpperCollider;

	public Collider armLowerCollider;

	public Collider legRightCollider;

	public Collider headCollider;

	public UnityEvent chestClickEvent;

	public UnityEvent armClickEvent;

	public UnityEvent legClickEvent;

	public UnityEvent headClickEvent;

	public override void Init(GameManager gameManager, GREPlayerNum playerNum, CosmeticsProvider cosmetics = null, ClientPetSelection petSelection = null)
	{
		_gameManager = gameManager;
		_ownerPlayerNum = playerNum;
		_cosmetics = cosmetics;
		CombatAnimationPlayer.DamageDealtByCard += base.OnDamageDealtByCard;
		GameEndUXEvent.GameEndEventExecuted += base.OnGameEndEventExecuted;
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
			if (MirrorOnOpponentSide)
			{
				MirrorTransform();
			}
		}
		else if (overrideLocalTransforms)
		{
			SetTransforms(localPosition, localRotation);
		}
		OnGlobalMuteChanged(MDNPlayerPrefs.DisableEmotes);
		SubscribeToMuteEvents();
		_animator = GetComponentInChildren<Animator>();
		periodicTimer = new AccessoryPeriodicTimerManager(this, _animator, _minTimeToFidget, _maxTimeToFidget, _minTimeToTwitch, _maxTimeToTwitch);
		cooldownTimer = new AccessoryCooldownTimerManager(this, _animator, _localClickCooldownCache, _opponentClickCooldownCache);
		_tickables.Add(periodicTimer);
		_tickables.Add(cooldownTimer);
	}

	public override void HandleClick()
	{
		if (!base._muted)
		{
			if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
			{
				chestClickEvent.Invoke();
				LogInteraction(AccessoryController.AccessoryInteraction.Fidget);
			}
			else if (_ownerPlayerNum == GREPlayerNum.Opponent && !(_opponentClickCooldown > 0f))
			{
				opponentClickEvent.Invoke();
				_opponentClickCooldown = 3f;
			}
		}
	}

	public void HandleClickPart(UnityEvent evnt)
	{
		if (!base._muted)
		{
			if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
			{
				evnt.Invoke();
				LogInteraction(AccessoryController.AccessoryInteraction.Fidget);
			}
			else if (_ownerPlayerNum == GREPlayerNum.Opponent && !(_opponentClickCooldown > 0f))
			{
				opponentClickEvent.Invoke();
				_opponentClickCooldown = 3f;
			}
		}
	}

	public override void HandleHoverEnter()
	{
		if ((DebugMode || _ownerPlayerNum == GREPlayerNum.LocalPlayer) && !_hovering && !base._muted)
		{
			hoverEnterEvent.Invoke();
			_hovering = true;
			ResetBoredom();
			if (_ownerPlayerNum == GREPlayerNum.LocalPlayer && _gameManager != null)
			{
				_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", AccessoryInteraction.HoverEnter.ToString());
			}
			LogInteraction(AccessoryController.AccessoryInteraction.HoverEnter);
		}
	}

	public override void HandleHoverExit()
	{
		if ((DebugMode || GREPlayerNum.LocalPlayer == _ownerPlayerNum) && !base._muted)
		{
			hoverExitEvent.Invoke();
			_hovering = false;
			_warmedUp = false;
			ResetBoredom();
			if ((GREPlayerNum.LocalPlayer == _ownerPlayerNum || !DebugMode) && _gameManager != null)
			{
				_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", AccessoryInteraction.HoverExit.ToString());
			}
			LogInteraction(AccessoryController.AccessoryInteraction.HoverExit);
		}
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

	public override void OnPlayerMuteChanged(bool playerMuted)
	{
		if (_ownerPlayerNum != GREPlayerNum.LocalPlayer)
		{
			bool muted = base._muted;
			_playerMuted = playerMuted;
			bool muted2 = base._muted;
			if (muted != muted2)
			{
				(base._muted ? _muteEvent : _unmuteEvent)?.Invoke();
			}
		}
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (chestCollider != null && eventData.hovered.Exists((GameObject x) => x == chestCollider.gameObject))
		{
			HandleClick();
		}
		if (armUpperCollider != null && armLowerCollider != null && (eventData.rawPointerPress == armUpperCollider.gameObject || eventData.rawPointerPress == armLowerCollider.gameObject))
		{
			HandleClickPart(armClickEvent);
		}
		if (legRightCollider != null && eventData.rawPointerPress == legRightCollider.gameObject)
		{
			HandleClickPart(legClickEvent);
		}
		if (headCollider != null && eventData.rawPointerPress == headCollider.gameObject)
		{
			HandleClickPart(headClickEvent);
		}
	}
}
