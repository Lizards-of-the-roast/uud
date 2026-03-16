using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Player.PlayerRankSprites;
using AssetLookupTree.Payloads.Prefab;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using Wizards.Mtga.Format;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class MatchEndScene : MonoBehaviour
{
	[SerializeField]
	private Transform _matchEndParent;

	private bool _isDraw;

	[SerializeField]
	private Transform _canvasPopup;

	[SerializeField]
	private Camera _overlayCamera;

	private MatchEndDisplay _targetElement;

	private EventTrigger _viewBattlefieldButton;

	private EventTrigger _leaveMatchButton;

	private EventTrigger _exitMatchBackgroundButton;

	private MatchManager _matchManager;

	private AssetLookupSystem _assetLookupSystem;

	private static readonly int WaitingForData = Animator.StringToHash("WaitingForData");

	private static readonly int RankDraw = Animator.StringToHash("RankDraw");

	private static readonly int Rank = Animator.StringToHash("Rank");

	private static readonly int BadgeChange = Animator.StringToHash("BadgeChange");

	public GameEndSurvey SurveyUI { get; private set; }

	private IClientLocProvider _locManager
	{
		get
		{
			if (Languages.ActiveLocProvider != null)
			{
				return Languages.ActiveLocProvider;
			}
			return null;
		}
	}

	public event System.Action EndOfMatchAnimationsCompleted;

	public event System.Action ExitMatchCompleted;

	public event System.Action EndOfMatchControlsEnabled;

	public void Init(MatchManager.GameResult result, string avatar, AssetLookupSystem assetLookupSystem, NPEState npeState, MatchManager matchManager)
	{
		_matchManager = matchManager;
		_assetLookupSystem = assetLookupSystem;
		string prefabPath = assetLookupSystem.GetPrefabPath<GameEndSurveyPrefab, GameEndSurvey>();
		SurveyUI = AssetLoader.Instantiate<GameEndSurvey>(prefabPath, _canvasPopup);
		_targetElement = getTargetElement(result);
		_targetElement.Anim.SetBool(WaitingForData, value: true);
		_targetElement.SetAvatar(ProfileUtilities.GetAvatarFullImagePath(assetLookupSystem, (npeState.ActiveNPEGame != null) ? "Avatar_Basic_NPE" : avatar));
		_targetElement.RootObj.UpdateActive(active: true);
		SetupButtons(_targetElement);
		MatchEndDisplay getTargetElement(MatchManager.GameResult resultData)
		{
			if (resultData.Result != ResultType.WinLoss)
			{
				MatchEndDisplay result2 = AssetLoader.Instantiate<MatchEndDisplay>(assetLookupSystem.GetPrefabPath<DrawMatchEndDisplayPrefab, MatchEndDisplay>(), _matchEndParent);
				_isDraw = true;
				return result2;
			}
			if (resultData.Winner == GREPlayerNum.LocalPlayer)
			{
				return AssetLoader.Instantiate<MatchEndDisplay>(assetLookupSystem.GetPrefabPath<VictoryMatchEndDisplayPrefab, MatchEndDisplay>(), _matchEndParent);
			}
			return AssetLoader.Instantiate<MatchEndDisplay>(assetLookupSystem.GetPrefabPath<DefeatMatchEndDisplayPrefab, MatchEndDisplay>(), _matchEndParent);
		}
	}

	private void SetupButtons(MatchEndDisplay _targetElement)
	{
		_targetElement.SpawnButtons(_matchEndParent);
		_viewBattlefieldButton = _targetElement.Buttons._viewBattlefieldButton;
		_leaveMatchButton = _targetElement.Buttons._leaveMatchButton;
		_exitMatchBackgroundButton = _targetElement.Buttons._exitMatchBackgroundButton;
		EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
		triggerEvent.AddListener(OnViewBattlefieldClicked);
		_viewBattlefieldButton.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerClick,
			callback = triggerEvent
		});
		EventTrigger.TriggerEvent triggerEvent2 = new EventTrigger.TriggerEvent();
		triggerEvent2.AddListener(OnLeaveMatchClicked);
		_leaveMatchButton.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerClick,
			callback = triggerEvent2
		});
		_exitMatchBackgroundButton.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerClick,
			callback = triggerEvent2
		});
	}

	private void OnViewBattlefieldClicked(BaseEventData baseEventData)
	{
		if ((bool)_targetElement)
		{
			_targetElement.gameObject.UpdateActive(active: false);
		}
		_leaveMatchButton.gameObject.UpdateActive(active: true);
		_exitMatchBackgroundButton.gameObject.UpdateActive(active: false);
		_viewBattlefieldButton.gameObject.UpdateActive(active: false);
	}

	public void LeaveMatch()
	{
		this.ExitMatchCompleted?.Invoke();
	}

	private void OnLeaveMatchClicked(BaseEventData baseEventData)
	{
		LeaveMatch();
	}

	public void EnableEndOfMatchControls(bool enableViewBattlefieldButton = true)
	{
		_exitMatchBackgroundButton.gameObject.UpdateActive(active: true);
		_viewBattlefieldButton.gameObject.UpdateActive(enableViewBattlefieldButton);
		this.EndOfMatchControlsEnabled?.Invoke();
	}

	private void OnDestroy()
	{
		clearEventTriggers(_viewBattlefieldButton);
		clearEventTriggers(_leaveMatchButton);
		clearEventTriggers(_exitMatchBackgroundButton);
		this.EndOfMatchAnimationsCompleted = null;
		this.ExitMatchCompleted = null;
		AudioManager.ExecuteActionOnEvent(WwiseEvents.sfx_ui_epp_levelup_start.EventName, AkActionOnEventType.AkActionOnEventType_Stop, base.gameObject);
		static void clearEventTriggers(EventTrigger eventTrigger)
		{
			List<EventTrigger.Entry> triggers = eventTrigger.triggers;
			foreach (EventTrigger.Entry item in triggers)
			{
				item.callback.RemoveAllListeners();
			}
			triggers.Clear();
		}
	}

	public void ShowEndOfMatchProgress(RankProgress rankProgress, MythicRatingUpdated mythicRatingUpdated, MDNEFormatType formatType)
	{
		if (!_targetElement)
		{
			this.EndOfMatchAnimationsCompleted?.Invoke();
			return;
		}
		Animator anim = _targetElement.Anim;
		AnimationComplete_SMB animationComplete_SMB = anim.GetBehaviour<AnimationComplete_SMB>();
		animationComplete_SMB.Reset();
		animationComplete_SMB.OnAnimationComplete = delegate
		{
			animationComplete_SMB.OnAnimationComplete = null;
			this.EndOfMatchAnimationsCompleted?.Invoke();
		};
		bool flag = rankProgress != null && rankProgress.seasonOrdinal != 0;
		if (flag)
		{
			_targetElement.RankDisplay.EndGameRankProgressDisplay(_assetLookupSystem, rankProgress, FormatUtilities.IsLimited(formatType), mythicRatingUpdated);
		}
		anim.SetBool(WaitingForData, value: false);
		if (anim.ContainsParameter(RankDraw))
		{
			anim.SetBool(RankDraw, _isDraw && flag);
		}
		anim.SetBool(Rank, !_isDraw && flag);
		if (flag)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_epp_levelup_stop, base.gameObject);
			string empty = string.Empty;
			bool isConstructed = formatType == MDNEFormatType.Constructed;
			anim.SetBool(BadgeChange, _targetElement.RankDisplay.RankUp);
			if (!_targetElement.RankDisplay.RankUp)
			{
				return;
			}
			PlayerRankSprites rankSprite = RankIconUtils.GetRankSprite(_assetLookupSystem, rankProgress.newClass, rankProgress.newLevel, isConstructed);
			string newRankSprite = rankSprite?.SpriteRef.RelativePath;
			string text = rankSprite?.GemOverlayRef.RelativePath;
			string text2 = rankSprite?.RankBaseRef.RelativePath;
			_targetElement.SetNewRankSprite(newRankSprite);
			if (!string.IsNullOrEmpty(text))
			{
				_targetElement.SetGemOverlaySprite(text);
			}
			else
			{
				_targetElement.NewRankGemSprite.enabled = false;
			}
			if (_targetElement.NewRankBase != null)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					_targetElement.SetRankBaseSprite(text2);
				}
				else
				{
					_targetElement.NewRankBase.enabled = false;
				}
			}
			if (rankProgress.newClass == RankingClassType.Master || rankProgress.newClass == RankingClassType.Mythic)
			{
				_targetElement.NewRankText.text = empty + RankUtilities.GetClassDisplayName(rankProgress.newClass);
			}
			else
			{
				_targetElement.NewRankText.text = empty + Languages.ActiveLocProvider.GetLocalizedText("Rank/Rank_Tier_Tooltip", ("rankDisplayName", RankUtilities.GetClassDisplayName(rankProgress.newClass)), ("playerTier", rankProgress.newLevel.ToString()));
			}
			if (_targetElement.EventTypeText != null)
			{
				if (_matchManager != null)
				{
					SuperFormat format = _matchManager.Format;
					_targetElement.EventTypeText.text = ((format == SuperFormat.Constructed) ? _locManager.GetLocalizedText("MainNav/HomePage/EventBlade/ConstructedRank") : _locManager.GetLocalizedText("MainNav/HomePage/EventBlade/LimitedRank"));
				}
				else
				{
					_targetElement.EventTypeText.text = _locManager.GetLocalizedText("MainNav/HomePage/EventBlade/ConstructedRank");
				}
			}
		}
		else
		{
			this.EndOfMatchAnimationsCompleted?.Invoke();
		}
	}

	private void OnEnable()
	{
		if ((bool)CurrentCamera.Value)
		{
			UniversalAdditionalCameraData universalAdditionalCameraData = CurrentCamera.Value.GetUniversalAdditionalCameraData();
			if ((object)universalAdditionalCameraData != null && universalAdditionalCameraData.renderType == CameraRenderType.Base)
			{
				universalAdditionalCameraData.cameraStack.Add(_overlayCamera);
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)CurrentCamera.Value)
		{
			UniversalAdditionalCameraData universalAdditionalCameraData = CurrentCamera.Value.GetUniversalAdditionalCameraData();
			if ((object)universalAdditionalCameraData != null && universalAdditionalCameraData.renderType == CameraRenderType.Base)
			{
				universalAdditionalCameraData.cameraStack.Remove(_overlayCamera);
			}
		}
	}
}
