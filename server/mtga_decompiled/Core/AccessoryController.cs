using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.Platforms;
using Wotc.Mtga;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Providers;
using Wotc.Mtgo.Gre.External.Messaging;

public class AccessoryController : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	public enum AccessoryInteraction
	{
		None,
		Fidget,
		HoverEnter,
		HoverExit,
		ColorChange,
		Mute
	}

	public const string INTERACTION_CATEGORY = "AccessoryInteraction";

	public bool DebugMode;

	[HideInInspector]
	public bool debugScene;

	public float HoverWarmup = 0.3f;

	[SerializeField]
	protected float _opponentClickCooldown = 3f;

	[SerializeField]
	protected float _localClickCooldown;

	[FormerlySerializedAs("_petClickCollider")]
	[SerializeField]
	protected Collider clickCollider;

	[FormerlySerializedAs("_petHoverCollider")]
	[SerializeField]
	protected Collider hoverCollider;

	[SerializeField]
	protected Collider phaseChangeCollider;

	[SerializeField]
	private Collider _variantChangeCollider;

	[SerializeField]
	protected RuntimeAnimatorController OpponentSideAnimController;

	[Header("Local Pet Settings")]
	public bool overrideLocalTransforms;

	[DisableIf("overrideLocalTransforms")]
	[SerializeField]
	protected Vector3 localPosition;

	[DisableIf("overrideLocalTransforms")]
	[SerializeField]
	protected Vector3 localRotation;

	[Header("Opponent Pet Settings")]
	[Space]
	public bool MirrorOnOpponentSide;

	[Space]
	public bool overrideOpponentTransforms;

	[DisableIf("overrideOpponentTransforms")]
	[SerializeField]
	protected Vector3 opponentPosition;

	[DisableIf("overrideOpponentTransforms")]
	[SerializeField]
	protected Vector3 opponentRotation;

	[Header("Handheld Settings")]
	public bool overrideTransformsOnHandheld;

	[DisableIf("overrideTransformsOnHandheld")]
	[SerializeField]
	protected GameObject handheldTransformsGO;

	[DisableIf("overrideTransformsOnHandheld")]
	[SerializeField]
	protected Vector3 localPositionHandheld;

	[DisableIf("overrideTransformsOnHandheld")]
	[SerializeField]
	protected Vector3 opponentPositionHandheld;

	[Space]
	[Space]
	[Space]
	[SerializeField]
	protected UnityEvent _gameStartEvent;

	[SerializeField]
	protected UnityEvent changeItemClickEvent;

	[FormerlySerializedAs("_petClickEvent")]
	[SerializeField]
	protected UnityEvent clickEvent;

	[FormerlySerializedAs("_petOpponentClickEvent")]
	[SerializeField]
	protected UnityEvent opponentClickEvent;

	[FormerlySerializedAs("_petHoverEnterEvent")]
	[SerializeField]
	protected UnityEvent hoverEnterEvent;

	[FormerlySerializedAs("_petHoverExitEvent")]
	[SerializeField]
	protected UnityEvent hoverExitEvent;

	[Header("Fidget")]
	[HideInInspector]
	[SerializeField]
	protected float _minTimeToFidget = 20f;

	[HideInInspector]
	[SerializeField]
	protected float _maxTimeToFidget = 45f;

	[FormerlySerializedAs("_petIdleFidgetEvent")]
	[InspectorUnityEventRange("_minTimeToFidget", "_maxTimeToFidget")]
	[SerializeField]
	protected UnityEvent _petFidgetEvent;

	[Header("Twitch")]
	[HideInInspector]
	[SerializeField]
	protected float _minTimeToTwitch = 15f;

	[HideInInspector]
	[SerializeField]
	protected float _maxTimeToTwitch = 25f;

	[FormerlySerializedAs("_petIdleFidgetsEvent")]
	[InspectorUnityEventRange("_minTimeToTwitch", "_maxTimeToTwitch")]
	[SerializeField]
	protected UnityEvent _petTwitchEvent;

	[SerializeField]
	protected UnityEvent _petVictoryEvent;

	[SerializeField]
	protected UnityEvent _petDefeatEvent;

	[SerializeField]
	protected UnityEvent _petReactGoodLargeEvent;

	[SerializeField]
	protected UnityEvent _petReactGoodMediumEvent;

	[SerializeField]
	protected UnityEvent _petReactBadLargeEvent;

	[SerializeField]
	protected UnityEvent _petReactBadMediumEvent;

	[SerializeField]
	protected UnityEvent _muteEvent;

	[SerializeField]
	protected UnityEvent _unmuteEvent;

	[SerializeField]
	protected UnityEvent _localEmoteEvent;

	[SerializeField]
	protected UnityEvent _opponentEmoteEvent;

	[SerializeField]
	protected UnityEvent _localReactGraveyardEvent;

	[SerializeField]
	protected UnityEvent _opponentReactGraveyardEvent;

	[Header("Local Pet Roping")]
	[SerializeField]
	protected UnityEvent _localRopingStartEvent;

	[SerializeField]
	protected UnityEvent _localRopingEndEvent;

	[Header("Opponent Pet Roping")]
	[SerializeField]
	protected UnityEvent _opponentRopingStartEvent;

	[SerializeField]
	protected UnityEvent _opponentRopingEndEvent;

	[Header("Ability Reactions")]
	[SerializeField]
	protected AbilityType _abilityType;

	[SerializeField]
	protected AbilityWord _abilityWord;

	[SerializeField]
	protected uint _abilityGrpId;

	[SerializeField]
	protected Designation _designationAdded;

	[SerializeField]
	protected UnityEvent _abilityTriggeredEventOpponent;

	[SerializeField]
	protected UnityEvent _abilityTriggeredEventLocal;

	[Header("Game Exit")]
	[SerializeField]
	protected UnityEvent _gameExitEvent;

	public GREPlayerNum _ownerPlayerNum;

	protected GameManager _gameManager;

	protected CosmeticsProvider _cosmetics;

	private UIMessageHandler _uiMessageHandler;

	private DuelSceneLogger _dsLogger;

	protected bool _hovering;

	protected float _boredom = 60f;

	protected float _restlessness = 10f;

	protected float _reactionTime;

	protected bool _readyToReact = true;

	protected float _colorChangeClickCooldown;

	protected float _clickNearCooldown;

	protected float _localClickCooldownCache;

	protected float _opponentClickCooldownCache = 3f;

	protected bool _warmedUp;

	protected bool startedTimer;

	protected Animator _animator;

	protected bool _globalMuted;

	protected bool _playerMuted;

	protected AccessoryPeriodicTimerManager periodicTimer;

	protected AccessoryCooldownTimerManager cooldownTimer;

	protected List<ITickable> _tickables = new List<ITickable>();

	[HideInInspector]
	public bool _turnOffTimers;

	public Collider ClickCollider => clickCollider;

	public Collider VariantChangeCollider => _variantChangeCollider;

	protected bool _muted
	{
		get
		{
			if (!_globalMuted)
			{
				return _playerMuted;
			}
			return true;
		}
	}

	public virtual void Init(GameManager gameManager, GREPlayerNum playerNum, CosmeticsProvider cosmetics = null, ClientPetSelection petSelection = null)
	{
		_gameManager = gameManager;
		_ownerPlayerNum = playerNum;
		_cosmetics = cosmetics;
		startedTimer = false;
		_localClickCooldownCache = _localClickCooldown;
		_opponentClickCooldownCache = _opponentClickCooldown;
		CombatAnimationPlayer.DamageDealtByCard += OnDamageDealtByCard;
		GameEndUXEvent.GameEndEventExecuted += OnGameEndEventExecuted;
		if ((bool)gameManager)
		{
			_dsLogger = gameManager.Logger;
		}
		if ((bool)_gameManager)
		{
			_gameManager.UXEventQueue.EventExecutionCommenced += OnEventExecutionCommenced;
			_gameManager.UXEventQueue.EventExecutionCompleted += OnEventExecutionCompleted;
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
		else
		{
			if (overrideLocalTransforms)
			{
				SetTransforms(localPosition, localRotation);
			}
			if (overrideTransformsOnHandheld && PlatformUtils.IsHandheld())
			{
				GameObject gameObject = base.transform.Find("HandheldTransforms").gameObject;
				if (gameObject != null)
				{
					gameObject.transform.localPosition = localPositionHandheld;
				}
				else
				{
					Debug.Log("No child with the name  <color=red>'HandheldTransforms' </color>attached");
				}
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

	private static bool UseLogger()
	{
		return true;
	}

	protected void UpdateUIMessageHandler()
	{
		if (_gameManager != null)
		{
			_uiMessageHandler = _gameManager.UIMessageHandler;
			if (_uiMessageHandler != null)
			{
				_uiMessageHandler.GenericEventReceived += OnGenericEventReceived;
			}
		}
	}

	public virtual void Update()
	{
		if (_muted)
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
		if ((bool)_gameManager)
		{
			if (_gameManager.TimerManager.IsLocalPlayerTimeWarningVisible() && !startedTimer)
			{
				HandleRopingStartEvent();
			}
			if (!_gameManager.TimerManager.IsLocalPlayerTimeWarningVisible() && startedTimer)
			{
				HandleRopingEndEvent();
			}
		}
	}

	protected void OnEventExecutionCommenced(UXEvent uxEventCommenced)
	{
		if (uxEventCommenced is UXEventUpdatePhase)
		{
			_gameStartEvent.Invoke();
		}
	}

	protected void OnEventExecutionCompleted(UXEvent uxEventComplete)
	{
		if (uxEventComplete is ZoneTransferGroup)
		{
			foreach (ZoneTransferUXEvent zoneTransfer in ((ZoneTransferGroup)uxEventComplete)._zoneTransfers)
			{
				OnZoneTransferExecuted(zoneTransfer);
			}
		}
		if (uxEventComplete is ResolutionEventEndedUXEvent resolutionEventEndedUXEvent)
		{
			bool valueOrDefault = resolutionEventEndedUXEvent.Instigator?.Owner?.ControlledByLocalPlayer == true;
			bool num = resolutionEventEndedUXEvent.AbilityPrinting?.ReferencedAbilityTypes.Contains(_abilityType) ?? false;
			AbilityPrintingData abilityPrinting = resolutionEventEndedUXEvent.AbilityPrinting;
			bool flag = abilityPrinting != null && abilityPrinting.AbilityWord != AbilityWord.None && resolutionEventEndedUXEvent.AbilityPrinting.AbilityWord.Equals(_abilityWord);
			AbilityPrintingData abilityPrinting2 = resolutionEventEndedUXEvent.AbilityPrinting;
			bool flag2 = abilityPrinting2 != null && abilityPrinting2.Id != 0 && resolutionEventEndedUXEvent.AbilityPrinting.Id.Equals(_abilityGrpId);
			bool flag3 = resolutionEventEndedUXEvent.CardPrinting?.Abilities.Exists(_abilityType, (AbilityPrintingData ability, AbilityType abilityType) => ability.ReferencedAbilityTypes.Contains(abilityType)) ?? false;
			bool flag4 = (num || flag3 || flag || flag2) && valueOrDefault && _ownerPlayerNum == GREPlayerNum.LocalPlayer;
			bool flag5 = (num || flag3 || flag || flag2) && !valueOrDefault && _ownerPlayerNum == GREPlayerNum.Opponent;
			if (flag4)
			{
				if (!cooldownTimer.ReadyToReact)
				{
					return;
				}
				_abilityTriggeredEventLocal.Invoke();
				periodicTimer.SetDelay_Short();
				cooldownTimer.SetTimer_Reaction();
			}
			if (flag5)
			{
				if (!cooldownTimer.ReadyToReact)
				{
					return;
				}
				_abilityTriggeredEventOpponent.Invoke();
				periodicTimer.SetDelay_Short();
				cooldownTimer.SetTimer_Reaction();
			}
		}
		if (uxEventComplete is AddDesignationUXEvent addDesignationUXEvent && _designationAdded != Designation.None && addDesignationUXEvent.Designation.Type == _designationAdded && _gameManager.CurrentGameState.TryGetPlayer(addDesignationUXEvent.Designation.AffectedId, out var player))
		{
			bool num2 = player.IsLocalPlayer && _ownerPlayerNum == GREPlayerNum.LocalPlayer;
			bool flag6 = !player.IsLocalPlayer && _ownerPlayerNum == GREPlayerNum.Opponent;
			if (num2)
			{
				_abilityTriggeredEventLocal.Invoke();
				periodicTimer.SetDelay_Short();
				cooldownTimer.SetTimer_Reaction();
			}
			if (flag6)
			{
				_abilityTriggeredEventOpponent.Invoke();
				periodicTimer.SetDelay_Short();
				cooldownTimer.SetTimer_Reaction();
			}
		}
	}

	public void HandleMuteOn()
	{
		_muteEvent.Invoke();
	}

	public void HandleMuteOff()
	{
		_unmuteEvent.Invoke();
		periodicTimer.SetDelay_Short();
	}

	public virtual void HandleFidget()
	{
		_petFidgetEvent.Invoke();
	}

	public virtual void HandleIdleFidget()
	{
		_petTwitchEvent.Invoke();
	}

	public virtual void HandleRopingStartEvent()
	{
		if (!_muted)
		{
			startedTimer = true;
			if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
			{
				_localRopingStartEvent?.Invoke();
			}
			else
			{
				_opponentRopingStartEvent?.Invoke();
			}
		}
	}

	public virtual void HandleRopingEndEvent()
	{
		if (!_muted && startedTimer)
		{
			startedTimer = false;
			if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
			{
				_localRopingEndEvent?.Invoke();
			}
			else
			{
				_opponentRopingEndEvent?.Invoke();
			}
		}
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (clickCollider != null && eventData.hovered.Exists((GameObject x) => x == clickCollider.gameObject))
		{
			HandleClick();
		}
		if (phaseChangeCollider != null && eventData.hovered.Exists((GameObject x) => x == phaseChangeCollider.gameObject))
		{
			HandleVariantChange();
		}
	}

	public virtual void HandleClick()
	{
		if (_muted)
		{
			return;
		}
		if (_ownerPlayerNum == GREPlayerNum.LocalPlayer)
		{
			if (cooldownTimer.ReadyLocalClick)
			{
				clickEvent.Invoke();
				periodicTimer.SetDelay_Long();
				cooldownTimer.SetTimer_LocalClick();
				LogInteraction(AccessoryInteraction.Fidget);
			}
		}
		else if (_ownerPlayerNum == GREPlayerNum.Opponent && cooldownTimer.ReadyOpponentClick)
		{
			opponentClickEvent.Invoke();
			cooldownTimer.SetTimer_OpponentClick();
			periodicTimer.SetDelay_Long();
		}
	}

	public virtual void HandleVariantChange()
	{
		if (!_muted && _ownerPlayerNum == GREPlayerNum.LocalPlayer)
		{
			changeItemClickEvent.Invoke();
		}
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		if (hoverCollider != null)
		{
			HandleHoverExit();
		}
	}

	public virtual void HandleHoverEnter()
	{
		if (!_hovering && !_muted)
		{
			hoverEnterEvent.Invoke();
			_hovering = true;
			periodicTimer.SetTimer_Boredom();
			SendInteraction(AccessoryInteraction.HoverEnter);
			LogInteraction(AccessoryInteraction.HoverEnter);
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		if (hoverCollider != null && eventData.hovered.Exists(hoverCollider, (GameObject x, Collider hoverCollider) => x == hoverCollider.gameObject))
		{
			HandleHoverEnter();
		}
	}

	public virtual void HandleHoverExit()
	{
		if (!_muted)
		{
			hoverExitEvent.Invoke();
			_hovering = false;
			_warmedUp = false;
			periodicTimer.SetTimer_Boredom();
			SendInteraction(AccessoryInteraction.HoverExit);
			LogInteraction(AccessoryInteraction.HoverExit);
		}
	}

	public virtual void HandleHoverEnter(GREPlayerNum inputSource)
	{
	}

	public virtual void HandleHoverExit(GREPlayerNum inputSource)
	{
	}

	public virtual void HandleLocalEmote()
	{
		if (!_muted && cooldownTimer.ReadyToReact)
		{
			_localEmoteEvent.Invoke();
			periodicTimer.SetDelay_Short();
			cooldownTimer.SetTimer_Reaction();
		}
	}

	public virtual void HandleOpponentEmote()
	{
		if (!_muted && cooldownTimer.ReadyToReact)
		{
			_opponentEmoteEvent.Invoke();
			periodicTimer.SetDelay_Short();
			cooldownTimer.SetTimer_Reaction();
		}
	}

	public virtual void HandleVictory()
	{
		if (!_muted)
		{
			_petVictoryEvent.Invoke();
		}
	}

	public virtual void HandleDefeat()
	{
		if (!_muted)
		{
			_petDefeatEvent.Invoke();
		}
	}

	protected void ResetBoredom()
	{
		_boredom = Random.Range(_minTimeToFidget, _maxTimeToFidget);
	}

	protected void ResetRestlessness()
	{
		_restlessness = Random.Range(_minTimeToTwitch, _maxTimeToTwitch);
	}

	protected void ResetReactionTime()
	{
		_reactionTime = 5f;
	}

	public virtual void HandleHype(float hypeAmount)
	{
		if (!_muted && cooldownTimer.ReadyToReact && hypeAmount != 0f)
		{
			if (hypeAmount >= 1f)
			{
				_petReactGoodLargeEvent.Invoke();
				periodicTimer.SetDelay_Medium();
				cooldownTimer.SetTimer_Reaction();
			}
			else if (hypeAmount > 0f)
			{
				_petReactGoodMediumEvent.Invoke();
				periodicTimer.SetDelay_Medium();
				cooldownTimer.SetTimer_Reaction();
			}
			else if (hypeAmount <= -1f)
			{
				_petReactBadLargeEvent.Invoke();
				periodicTimer.SetDelay_Medium();
				cooldownTimer.SetTimer_Reaction();
			}
			else
			{
				_petReactBadMediumEvent.Invoke();
				periodicTimer.SetDelay_Medium();
				cooldownTimer.SetTimer_Reaction();
			}
		}
	}

	public virtual void HandleGameStart()
	{
		_gameStartEvent.Invoke();
		if (cooldownTimer != null)
		{
			cooldownTimer.SetTimer_Reaction();
		}
		ResetAllTriggers();
	}

	public virtual void HandleGameExit()
	{
		_gameExitEvent.Invoke();
	}

	protected void OnDamageDealtByCard(MtgCardInstance affector, int amount)
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

	protected void OnZoneTransferExecuted(ZoneTransferUXEvent zoneTransferUXEvent)
	{
		if (zoneTransferUXEvent.Instigator == null || !_readyToReact)
		{
			return;
		}
		if (zoneTransferUXEvent.FromZone.Type == ZoneType.Battlefield && zoneTransferUXEvent.ToZone.Type == ZoneType.Graveyard)
		{
			if (zoneTransferUXEvent.ToZone.Owner.IsLocalPlayer && _ownerPlayerNum == GREPlayerNum.LocalPlayer)
			{
				_localReactGraveyardEvent.Invoke();
				periodicTimer.SetDelay_Medium();
				cooldownTimer.SetTimer_Reaction();
			}
			if (!zoneTransferUXEvent.ToZone.Owner.IsLocalPlayer && _ownerPlayerNum == GREPlayerNum.Opponent)
			{
				_opponentReactGraveyardEvent.Invoke();
				periodicTimer.SetDelay_Medium();
				cooldownTimer.SetTimer_Reaction();
			}
		}
		else
		{
			float num = GetHypeFromZoneTransferReason(zoneTransferUXEvent.Reason);
			MtgCardInstance instigator = zoneTransferUXEvent.Instigator;
			bool? obj;
			if (instigator == null)
			{
				obj = null;
			}
			else
			{
				MtgPlayer controller = instigator.Controller;
				obj = ((controller != null) ? new bool?(!controller.IsLocalPlayer) : ((bool?)null));
			}
			bool? flag = obj;
			if (flag == true)
			{
				num *= -1f;
			}
			if (_ownerPlayerNum == GREPlayerNum.Opponent)
			{
				num *= -1f;
			}
			HandleHype(num);
		}
		ResetReactionTime();
	}

	protected float GetHypeFromZoneTransferReason(ZoneTransferReason reason)
	{
		switch (reason)
		{
		case ZoneTransferReason.Resolve:
			return 0f;
		case ZoneTransferReason.Countered:
			return 1f;
		case ZoneTransferReason.Destroy:
		case ZoneTransferReason.Damage:
		case ZoneTransferReason.Deathtouch:
			return 0.5f;
		case ZoneTransferReason.Exile:
		case ZoneTransferReason.Bounce:
			return 0.5f;
		default:
			return 0f;
		}
	}

	protected float GetHypeFromDamageAmount(int damage)
	{
		return (float)damage / 5f;
	}

	protected void OnGameEndEventExecuted(GameEndUXEvent gameEndUxEvent)
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
			HandleGameExit();
		}
	}

	protected virtual void OnGenericEventReceived(string category, string payload)
	{
	}

	protected void LogInteraction(AccessoryInteraction item)
	{
		if (!Application.isEditor)
		{
			_dsLogger?.OnAccessoryInteraction(_ownerPlayerNum, item);
		}
	}

	protected void SendInteraction(AccessoryInteraction item)
	{
		if ((GREPlayerNum.LocalPlayer == _ownerPlayerNum || !DebugMode) && _gameManager != null)
		{
			_gameManager?.UIMessageHandler?.TrySendGenericEvent("AccessoryInteraction", item.ToString());
		}
	}

	public void SetGlobalMuted(bool globalMuted)
	{
		_globalMuted = globalMuted;
	}

	public virtual void OnGlobalMuteChanged(bool globalMuted)
	{
		if (_ownerPlayerNum != GREPlayerNum.LocalPlayer)
		{
			bool muted = _muted;
			_globalMuted = globalMuted;
			bool muted2 = _muted;
			if (muted != muted2)
			{
				(_muted ? _muteEvent : _unmuteEvent)?.Invoke();
			}
		}
	}

	public void SubscribeToMuteEvents()
	{
		if ((bool)_gameManager && _ownerPlayerNum == GREPlayerNum.Opponent && _gameManager.Context.TryGet<IEntityDialogControllerProvider>(out var result) && result.TryGetDialogControllerByPlayerType(GREPlayerNum.Opponent, out var dialogController))
		{
			dialogController.IsMutedUpdated += OnGlobalMuteChanged;
		}
	}

	public void SetPlayerMuted(bool playerMuted)
	{
		_playerMuted = playerMuted;
	}

	public virtual void OnPlayerMuteChanged(bool playerMuted)
	{
		if (_ownerPlayerNum != GREPlayerNum.LocalPlayer)
		{
			bool muted = _muted;
			_playerMuted = playerMuted;
			bool muted2 = _muted;
			if (muted != muted2)
			{
				(_muted ? _muteEvent : _unmuteEvent)?.Invoke();
			}
		}
	}

	public virtual void Cleanup()
	{
		GameEndUXEvent.GameEndEventExecuted -= OnGameEndEventExecuted;
		if (_uiMessageHandler != null)
		{
			_uiMessageHandler.GenericEventReceived -= OnGenericEventReceived;
		}
		CombatAnimationPlayer.DamageDealtByCard -= OnDamageDealtByCard;
		GameEndUXEvent.GameEndEventExecuted -= OnGameEndEventExecuted;
		if ((bool)_gameManager)
		{
			_gameManager.UXEventQueue.EventExecutionCommenced -= OnEventExecutionCommenced;
			if ((bool)_gameManager && _gameManager.ViewManager != null)
			{
				GREPlayerNum ownerPlayerNum = _ownerPlayerNum;
				if (ownerPlayerNum != GREPlayerNum.LocalPlayer && ownerPlayerNum == GREPlayerNum.Opponent)
				{
					if (!_gameManager.Context.TryGet<IEntityDialogControllerProvider>(out var result) || !result.TryGetDialogControllerByPlayerType(GREPlayerNum.Opponent, out var dialogController))
					{
						return;
					}
					dialogController.IsMutedUpdated -= OnGlobalMuteChanged;
				}
			}
		}
		_gameManager = null;
	}

	protected void OnDestroy()
	{
		Cleanup();
	}

	public void SetOpponentAnimController()
	{
		if ((bool)OpponentSideAnimController)
		{
			RuntimeAnimatorController opponentSideAnimController = OpponentSideAnimController;
			GetComponentInChildren<Animator>().runtimeAnimatorController = opponentSideAnimController;
		}
	}

	public void GetAnimationController()
	{
		_animator = GetComponentInChildren<Animator>();
	}

	public void SetTransforms(Vector3 _targetPosition, Vector3? _targetRotation = null)
	{
		base.transform.localPosition = _targetPosition;
		if (_targetRotation.HasValue)
		{
			base.transform.localRotation = Quaternion.Euler(_targetRotation.Value);
		}
	}

	public void MirrorTransform()
	{
		base.transform.localScale = Vector3.Scale(base.transform.localScale, new Vector3(1f, 1f, -1f));
		if (overrideLocalTransforms)
		{
			base.transform.localRotation = Quaternion.Euler(360f - localRotation.x, 360f - localRotation.y, localRotation.z);
		}
	}

	public void ResetAllTriggers()
	{
		GetAnimationController();
		AnimatorControllerParameter[] parameters = _animator.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.type == AnimatorControllerParameterType.Trigger && animatorControllerParameter.name != "Game_Start")
			{
				_animator.ResetTrigger(animatorControllerParameter.name);
			}
		}
	}

	public void AssignClickCollider(GameObject _gameObject)
	{
		clickCollider = _gameObject.GetComponent<Collider>();
	}

	public void SetOpponentTransform(Vector3? translation = null, Vector3? rotation = null)
	{
		if (translation.HasValue)
		{
			opponentPosition = new Vector3(translation.Value.x, translation.Value.y, translation.Value.z);
		}
		if (rotation.HasValue)
		{
			opponentRotation = new Vector3(rotation.Value.x, rotation.Value.y, rotation.Value.z);
		}
	}
}
