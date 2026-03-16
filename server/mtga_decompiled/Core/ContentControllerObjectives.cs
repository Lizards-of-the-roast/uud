using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core.Shared.Code;
using Core.Code.Input;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Achievements;
using Core.Meta.MainNavigation.Objectives.Utils;
using Core.Meta.Quests;
using MTGA.KeyboardManager;
using UnityEngine;
using UnityEngine.Serialization;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Arena.Enums.Event;
using Wizards.Arena.Enums.UILayout;
using Wizards.MDN;
using Wizards.MDN.Services.Models.PlayerInventory.CampaignGraph;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wizards.Unification.Models.Graph;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;

public class ContentControllerObjectives : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	private const int questRefreshHour = 9;

	private const int dailyRewardRefreshHour = 9;

	private const int weeklyRewardRefreshHour = 9;

	private const DayOfWeek weeklyRewardRefreshDay = DayOfWeek.Sunday;

	private const int maximumQuestsAtOnce = 3;

	public const int MaximumNodeGraphObjectives = 1;

	[SerializeField]
	private float _screenBuffer;

	[SerializeField]
	private GameObject ObjectivesParent;

	[FormerlySerializedAs("NotificationPrefab")]
	[SerializeField]
	private ObjectiveBubble DefaultBubblePrefab;

	[SerializeField]
	private ObjectiveBubble AchievementBubblePrefab;

	[FormerlySerializedAs("battlePassNotificationPrefab")]
	[SerializeField]
	private ObjectiveBubble BattlePassBubblePrefab;

	[FormerlySerializedAs("colorMasteryObjectiveBubblePrefab")]
	[SerializeField]
	private ColorMasteryObjectiveBubble ColorMasteryBubblePrefab;

	[SerializeField]
	private ObjectiveBubble NPEBubblePrefab;

	[SerializeField]
	private Animator _objectivesAnimator;

	[SerializeField]
	private Sprite _hourglassGraphic;

	[SerializeField]
	private Sprite _darkenedDeckboxGraphic;

	[SerializeField]
	private NotificationPopup _notificationPopupPrefab;

	[SerializeField]
	private NotificationPopup _achievementNotificationPopupPrefab;

	[SerializeField]
	private Transform _popupParent;

	[SerializeField]
	private NotificationPopupReward _tomorrowDeckBoxPrefab;

	[SerializeField]
	private NotificationPopupReward _hourglassPrefab;

	[SerializeField]
	private float _totalBarTime = 20f;

	[SerializeField]
	private ObjectiveProgressBar _objectiveBarSparkTrack;

	[SerializeField]
	private GameObject _objectiveBar;

	[SerializeField]
	private float _initialSpeed = 0.2f;

	[SerializeField]
	private float _acceleration = 0.5f;

	[SerializeField]
	private GameObject _raycastblocker;

	[SerializeField]
	private RectTransform _safeArea;

	private NotificationPopup _achievementNotificationPopup;

	private NotificationPopup _notificationPopup;

	private List<ObjectiveBubble> _bubbles = new List<ObjectiveBubble>();

	private List<GameObject> _otherRewardObjects = new List<GameObject>();

	private GameObject _timerGameObject;

	private ObjectiveBubble _weeklyRewardsBubble;

	private ObjectiveBubble _dailyRewardsBubble;

	private float _elapsedTime;

	private float _currentSpeed;

	private bool _interactable = true;

	private int _tickIndex;

	private int _animationEndIndex;

	private DateTime _timestamp;

	private Action _refreshQuestDataAndRebuildDisplay;

	private bool _isAnimatorSetShowing = true;

	private RectTransform _objectiveRectTrans;

	private RectTransform _rectTransform;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private CardMaterialBuilder _cardMaterialBuilder;

	private CampaignGraphManager _campaignGraphManager;

	private IAccountClient _accountClient;

	private IAchievementManager _achievementManager;

	private float _prevScreenWidth;

	private float _prevRectTransWidth;

	public bool IsAnimating { get; private set; }

	public bool IsVisible
	{
		get
		{
			if (base.gameObject.activeInHierarchy)
			{
				return _objectivesAnimator.GetCurrentAnimatorStateInfo(0).IsName("Objectives_Intro");
			}
			return false;
		}
	}

	public bool AnimatorShowing => _isAnimatorSetShowing;

	private RectTransform ObjectivesRectTrans
	{
		get
		{
			if (!_objectiveRectTrans)
			{
				_objectiveRectTrans = ObjectivesParent.GetComponent<RectTransform>();
			}
			return _objectiveRectTrans;
		}
	}

	private RectTransform RectTransform
	{
		get
		{
			if (!_rectTransform)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public PriorityLevelEnum Priority => PriorityLevelEnum.Wrapper;

	public bool IsPopupActive
	{
		get
		{
			foreach (ObjectiveBubble bubble in _bubbles)
			{
				if (bubble.IsPopupActive)
				{
					return true;
				}
			}
			return false;
		}
	}

	public event Action OnBarFinishedAnimating;

	public event Action<Guid> OnQuestSwapClicked;

	public event Action<RewardObjectiveContext> OnRewardTicked;

	public void Initialize(KeyboardManager keyboardManager, IActionSystem actionSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder, IAccountClient accountClient)
	{
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_cardMaterialBuilder = cardMaterialBuilder;
		_campaignGraphManager = Pantry.Get<CampaignGraphManager>();
		_accountClient = accountClient;
		_achievementManager = Pantry.Get<IAchievementManager>();
	}

	public void Hide()
	{
		_keyboardManager?.Unsubscribe(this);
		_interactable = true;
		ClearAllBubbles();
		_objectiveBarSparkTrack.EnableSpark(isEnabled: false);
		base.gameObject.SetActive(value: false);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_stop, _objectiveBarSparkTrack.gameObject);
		if (_raycastblocker != null)
		{
			_raycastblocker.SetActive(value: false);
		}
	}

	private void DestroyBubble(ObjectiveBubble bubble)
	{
		bubble.CloseMobilePopup();
		bubble.Click -= SwapQuest;
		bubble.Click -= OpenBattlePassTrack;
		bubble.PopupEnabled -= OnPopupActive;
		bubble.PopupDisabled -= OnPopupDeactivate;
		_bubbles.Remove(bubble);
		UnityEngine.Object.Destroy(bubble.gameObject);
	}

	private void ClearAllBubbles()
	{
		CloseAllObjectivePopups();
		for (int num = _bubbles.Count - 1; num >= 0; num--)
		{
			DestroyBubble(_bubbles[num]);
		}
		_weeklyRewardsBubble = null;
		_dailyRewardsBubble = null;
		_bubbles.Clear();
		for (int num2 = _otherRewardObjects.Count - 1; num2 >= 0; num2--)
		{
			UnityEngine.Object.Destroy(_otherRewardObjects[num2]);
		}
		_otherRewardObjects.Clear();
	}

	private void GoToAchievement(ObjectiveBubble achievementBubble)
	{
		IClientAchievement achievement = achievementBubble.Achievement;
		SceneLoader.GetSceneLoader().GoToAchievementsScene("fromObjectiveTrackerBar", achievement);
	}

	private void SwapQuest(ObjectiveBubble notificationToSwap)
	{
		this.OnQuestSwapClicked?.Invoke(notificationToSwap.Reference_questData.Id);
	}

	private void OpenBattlePassTrack(ObjectiveBubble bpBubble)
	{
		if (SceneLoader.GetSceneLoader().CurrentContentType == NavContentType.Home)
		{
			ProgressionTrackPageContext trackPageContext = new ProgressionTrackPageContext(Pantry.Get<SetMasteryDataProvider>()?.CurrentBpName, NavContentType.Home, NavContentType.Home);
			SceneLoader.GetSceneLoader().GoToProgressionTrackScene(trackPageContext, "From EPP Objective Circle");
		}
	}

	public async Task ShowQuestBar(List<Client_QuestData> quests, List<RewardDisplayData> questRewards, RewardDisplayData dailyReward, RewardDisplayData weeklyReward, int dailyWins, int curDailyGoal, int weeklyWins, int curWeeklyGoal, bool doUpdateBarAnimation, int numWinsInMatch, bool dailyWeeklyWinsEnabled, Action RefreshQuestDataAndRebuildDisplay = null)
	{
		base.gameObject.SetActive(value: true);
		ClearAllBubbles();
		_refreshQuestDataAndRebuildDisplay = RefreshQuestDataAndRebuildDisplay;
		_timestamp = ServerGameTime.GameTime;
		_keyboardManager?.Subscribe(this);
		for (int num = quests.Count - 1; num > -1; num--)
		{
			Client_QuestData client_QuestData = quests[num];
			if (client_QuestData.QuestType == QuestTypeEnum.Onboarding)
			{
				_ = questRewards[num];
				quests.RemoveAt(num);
				questRewards.RemoveAt(num);
			}
		}
		int count = 3 - quests.Count - 1;
		foreach (IClientAchievement questTrackerAchievement in _achievementManager.GetQuestTrackerAchievements(count))
		{
			CreateBubble_Achievement(questTrackerAchievement);
		}
		if (quests.Count < 3)
		{
			_timerGameObject = CreateBubble_QuestTomorrow().gameObject;
		}
		for (int i = 0; i < quests.Count; i++)
		{
			Client_QuestData client_QuestData2 = quests[i];
			RewardDisplayData rewardDisplayData = questRewards[i];
			CreateBubble_Quest(client_QuestData2, rewardDisplayData, "Quest" + client_QuestData2.QuestType, DefaultBubblePrefab, i);
		}
		int numWinsInMatch2 = ((dailyWeeklyWinsEnabled && doUpdateBarAnimation) ? numWinsInMatch : 0);
		if (dailyReward != null)
		{
			_dailyRewardsBubble = CreateBubble_DailyRewards(dailyReward, dailyWins, curDailyGoal, numWinsInMatch2, WrapperController.Instance.PostMatchClientUpdate?.dailyWinUpdates);
		}
		if (weeklyReward != null)
		{
			_weeklyRewardsBubble = CreateBubble_WeeklyRewards(weeklyReward, weeklyWins, curWeeklyGoal, numWinsInMatch2, WrapperController.Instance.PostMatchClientUpdate?.weeklyWinUpdates);
		}
		CampaignGraphUpdate[] array = WrapperController.Instance.PostMatchClientUpdate?.campaignGraphUpdates;
		if (array != null)
		{
			List<ClientInventoryUpdateReportItem> ts = new List<ClientInventoryUpdateReportItem>();
			CampaignGraphUpdate[] array2 = array;
			foreach (CampaignGraphUpdate campaignGraphUpdate in array2)
			{
				if (campaignGraphUpdate.inventoryUpdateReportItem != null)
				{
					ts.Add(campaignGraphUpdate.inventoryUpdateReportItem);
				}
			}
			WrapperController.Instance.PostMatchClientUpdate.campaignGraphUpdates = null;
			SceneLoader.GetSceneLoader().GetRewardsContentController().AddAndDisplayRewardsCoroutine(ts, Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/Rewards_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Rewards/EventRewards/ClaimPrizeButton"));
		}
		if (await WrapperController.Instance.SparkyTourState.PVPGamesLocked())
		{
			foreach (ObjectiveBubble bubble in _bubbles)
			{
				bubble.gameObject.SetActive(value: false);
			}
		}
		IClientAchievement clientAchievement = _achievementManager.FavoriteAchievements.FirstOrDefault((IClientAchievement achievement) => !achievement.IsClaimed);
		IReadOnlyDictionary<string, ClientGraphDefinition> definitions;
		if (clientAchievement != null)
		{
			CreateBubble_Achievement(clientAchievement);
		}
		else if (_campaignGraphManager.TryGetDefinitions(out definitions))
		{
			CreateNodeGraphBubbles(definitions);
		}
		SetMasteryDataProvider setMasteryDataProvider = Pantry.Get<SetMasteryDataProvider>();
		if (setMasteryDataProvider != null && !setMasteryDataProvider.FailedInitializing && !string.IsNullOrEmpty(setMasteryDataProvider.CurrentBpName))
		{
			string currentBpName = setMasteryDataProvider.CurrentBpName;
			CurrentProgressionSummary currentProgressionSummary = setMasteryDataProvider.GetCurrentProgressionSummary(currentBpName, _cardDatabase, _cardMaterialBuilder);
			CreateBubble_BattlePass(currentProgressionSummary).SetInactive(setMasteryDataProvider.HasTrackExpired(currentBpName));
		}
		else
		{
			_weeklyRewardsBubble?.gameObject.SetActive(value: false);
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_appear, base.gameObject);
		_objectiveBar.transform.SetAsFirstSibling();
		if (doUpdateBarAnimation)
		{
			PlayQuestSparkAnimation();
			SetDailyWeeklyStatus(dailyWeeklyWinsEnabled);
		}
		else
		{
			UpdateBubbleInteractability();
		}
	}

	public void ReplaceQuest(Guid oldId, Client_QuestData newQuest, RewardDisplayData newReward)
	{
		ObjectiveBubble objectiveBubble = _bubbles.FirstOrDefault((ObjectiveBubble x) => x.Reference_questData != null && x.Reference_questData.Id == oldId);
		int siblingIndex = objectiveBubble.transform.GetSiblingIndex();
		DestroyBubble(objectiveBubble);
		ObjectiveBubble objectiveBubble2 = CreateBubble_Quest(newQuest, newReward, "Quest" + newQuest.QuestType, DefaultBubblePrefab, siblingIndex);
		objectiveBubble2.transform.SetSiblingIndex(siblingIndex);
		foreach (ObjectiveBubble bubble in _bubbles)
		{
			if (bubble.Reference_questData != null)
			{
				bubble.Reference_questData.CanSwap = false;
				bubble.SetFooterText("MainNav/General/Empty_String");
			}
		}
		UpdateBubbleInteractability();
		objectiveBubble2.PlayProgressPulse();
	}

	public void SetInteractable(bool interactable)
	{
		_interactable = interactable;
		UpdateBubbleInteractability();
	}

	private void UpdateBubbleInteractability()
	{
		foreach (ObjectiveBubble bubble in _bubbles)
		{
			bool flag = _interactable && !IsAnimating && bubble.Clickable;
			bool refreshHover = flag && bubble.Reference_levelData == null;
			bubble.SetClickable(flag || bubble.IsDeepLink);
			if (bubble.Achievement == null)
			{
				bubble.SetRefreshHover(refreshHover);
			}
			bubble.SetPopupEnabledIfAllowed(_interactable && !IsAnimating);
		}
	}

	private void PlayQuestSparkAnimation()
	{
		_animationEndIndex = 0;
		IsAnimating = true;
		_elapsedTime = 0f;
		_currentSpeed = _initialSpeed;
		_tickIndex = 0;
		_objectiveBarSparkTrack.EnableSpark(isEnabled: true);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_start, _objectiveBarSparkTrack.gameObject);
		AudioManager.PostStopEvent(WwiseEvents.sfx_ui_main_quest_main_bar_start.EventName, _objectiveBarSparkTrack.gameObject, 5f);
		UpdateBubbleInteractability();
	}

	private void PlayEventPrizeAnimation(int rewardBubbleIndex)
	{
		_animationEndIndex = rewardBubbleIndex;
		IsAnimating = true;
		AudioManager.PostStopEvent(WwiseEvents.sfx_ui_main_quest_main_bar_start.EventName, _objectiveBarSparkTrack.gameObject, 5f);
		_elapsedTime = GetBarPercentForBubble(rewardBubbleIndex - 1) * _totalBarTime;
		_currentSpeed = _initialSpeed;
		_tickIndex = 0;
		_objectiveBarSparkTrack.EnableSpark(isEnabled: true);
		UpdateBubbleInteractability();
	}

	private void SetEventPrizeBarPosition(int numWins)
	{
		IsAnimating = false;
		float barPercentForBubble = GetBarPercentForBubble(numWins);
		_objectiveBarSparkTrack.SetPct(barPercentForBubble);
		_objectiveBarSparkTrack.EnableSpark(isEnabled: true);
	}

	private float GetBarPercentForBubble(int index)
	{
		if (_bubbles.Count == 0)
		{
			return 0f;
		}
		index = Mathf.Clamp(index, 0, _bubbles.Count - 1);
		ObjectiveBubble objectiveBubble = _bubbles[index];
		Vector3[] array = new Vector3[4];
		_objectiveBarSparkTrack.GetComponent<RectTransform>().GetWorldCorners(array);
		float x = objectiveBubble.GetIndicatorCanvasPosition().x;
		float x2 = array[0].x;
		float x3 = array[2].x;
		return (x - x2) / (x3 - x2);
	}

	private void Update()
	{
		if (_timestamp.Hour < 9 && ServerGameTime.GameTime.Hour >= 9)
		{
			_timestamp = ServerGameTime.GameTime;
			RebuildAllBubbles();
			return;
		}
		if (_prevScreenWidth != RectTransform.rect.width || _prevRectTransWidth != ObjectivesRectTrans.rect.width)
		{
			_prevScreenWidth = RectTransform.rect.width;
			_prevRectTransWidth = ObjectivesRectTrans.rect.width;
			float num = RectTransform.rect.width - _screenBuffer;
			float num2 = 1f;
			if (num < ObjectivesRectTrans.rect.width)
			{
				num2 = num / ObjectivesRectTrans.rect.width;
				ObjectivesRectTrans.localScale = new Vector3(num2, num2, num2);
			}
			else
			{
				ObjectivesRectTrans.localScale = Vector3.one;
			}
			if (PlatformUtils.IsHandheld())
			{
				ObjectivesRectTrans.anchoredPosition = new Vector2(0f - _screenBuffer * num2, ObjectivesRectTrans.anchoredPosition.y);
			}
		}
		if (!IsAnimating)
		{
			return;
		}
		_currentSpeed += _acceleration * Time.deltaTime;
		float value = _elapsedTime / _totalBarTime;
		value = Mathf.Clamp(value, 0f, 1f);
		_elapsedTime += Time.deltaTime * _currentSpeed;
		float value2 = _elapsedTime / _totalBarTime;
		value2 = Mathf.Clamp(value2, 0f, 1f);
		_objectiveBarSparkTrack.SetPct(value2);
		float sparkXPos = _objectiveBarSparkTrack.GetSparkXPos();
		bool flag = false;
		if (_tickIndex < _bubbles.Count)
		{
			ObjectiveBubble objectiveBubble = _bubbles[_tickIndex];
			float x = objectiveBubble.GetIndicatorWorldPosition().x;
			if (sparkXPos >= x)
			{
				_tickIndex++;
				objectiveBubble.Tick(this.OnRewardTicked);
				if (_animationEndIndex != 0 && _bubbles[_animationEndIndex] == objectiveBubble)
				{
					flag = true;
					_bubbles[_animationEndIndex - 1].SetDimmed(dimmed: true);
					_bubbles[_animationEndIndex].SetDimmed(dimmed: false);
				}
			}
		}
		if (flag)
		{
			IsAnimating = false;
			this.OnBarFinishedAnimating?.Invoke();
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_stop, _objectiveBarSparkTrack.gameObject);
			UpdateBubbleInteractability();
		}
		else if (value2 >= 1f && value < 1f)
		{
			IsAnimating = false;
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_main_bar_stop, _objectiveBarSparkTrack.gameObject);
			_objectiveBarSparkTrack.EnableSpark(isEnabled: false);
			this.OnBarFinishedAnimating?.Invoke();
			SetDailyWeeklyStatus(enabled: true);
			UpdateBubbleInteractability();
		}
	}

	private void Awake()
	{
		_isAnimatorSetShowing = _objectivesAnimator.GetBool("Showing");
	}

	private void OnEnable()
	{
		if (IsVisible != _isAnimatorSetShowing)
		{
			if (_isAnimatorSetShowing)
			{
				AnimateIntro();
			}
			else
			{
				AnimateOutro();
			}
		}
	}

	private void OnDisable()
	{
		AudioManager.ExecuteActionOnEvent(WwiseEvents.sfx_ui_main_quest_main_bar_start.EventName, AkActionOnEventType.AkActionOnEventType_Stop, _objectiveBarSparkTrack.gameObject);
		SetDailyWeeklyStatus(enabled: true);
	}

	private void OnDestroy()
	{
		_keyboardManager?.Unsubscribe(this);
	}

	public void SetBubblesToFinalState()
	{
		AudioManager.PostStopEvent(WwiseEvents.sfx_ui_main_quest_main_bar_start.EventName, _objectiveBarSparkTrack.gameObject);
		foreach (ObjectiveBubble bubble in _bubbles)
		{
			SetBubbleToFinalState(bubble);
		}
	}

	private void SetBubbleToFinalState(ObjectiveBubble bubble)
	{
		if (bubble.Reference_nextGoalProgress > bubble.Reference_curGoalProgress && bubble.Reference_endProgress > bubble.Reference_startProgress)
		{
			bool num = bubble.Reference_nextGoalProgress != 0;
			bool flag = num && bubble.Reference_endProgress == bubble.Reference_nextGoalProgress;
			if (num)
			{
				bubble.SetRadialFill((float)bubble.Reference_endProgress / (float)bubble.Reference_nextGoalProgress);
				MTGALocalizedString xOfYLocString = GetXOfYLocString(bubble.Reference_endProgress.ToString(), bubble.Reference_nextGoalProgress.ToString());
				bubble.SetProgressText(xOfYLocString);
			}
			if (flag)
			{
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_quest_complete, bubble.gameObject);
				bubble.PlayCompletePulse();
			}
			else
			{
				bubble.PlayProgressPulse();
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_progress_poof, base.gameObject);
			}
		}
	}

	public void ResetAnimations()
	{
		_isAnimatorSetShowing = true;
	}

	public void AnimateOutro()
	{
		_isAnimatorSetShowing = false;
		_objectivesAnimator.SetBool("Showing", value: false);
		foreach (ObjectiveBubble bubble in _bubbles)
		{
			bubble.SetActivePromotionVFX(val: false);
		}
	}

	public void AnimateIntro()
	{
		_isAnimatorSetShowing = true;
		_objectivesAnimator.SetBool("Showing", value: true);
		foreach (ObjectiveBubble bubble in _bubbles)
		{
			bubble.SetActivePromotionVFX(val: true);
		}
	}

	[ContextMenu("Disable Historic Queue Blocker")]
	private void TEST_DisableHistoricQueueBlocker()
	{
		SetDailyWeeklyStatus(enabled: true);
	}

	public bool SetDailyWeeklyStatus(bool enabled)
	{
		bool valueOrDefault = _dailyRewardsBubble?.gameObject?.activeSelf == true;
		bool valueOrDefault2 = _weeklyRewardsBubble?.gameObject?.activeSelf == true;
		if (valueOrDefault)
		{
			_dailyRewardsBubble.CanvasGroup.alpha = (enabled ? 1f : 0.2f);
			_dailyRewardsBubble.CanvasGroup.interactable = enabled;
			_dailyRewardsBubble.CanvasGroup.blocksRaycasts = enabled;
		}
		if (valueOrDefault2)
		{
			_weeklyRewardsBubble.CanvasGroup.alpha = (enabled ? 1f : 0.2f);
			_weeklyRewardsBubble.CanvasGroup.interactable = enabled;
			_weeklyRewardsBubble.CanvasGroup.blocksRaycasts = enabled;
		}
		return valueOrDefault || valueOrDefault2;
	}

	private ObjectiveBubble CreateBubble_Quest(Client_QuestData questData, RewardDisplayData rewardDisplayData, string name, ObjectiveBubble prefab, int index = 0)
	{
		ObjectiveBubble objectiveBubble = CreateBubble(name, prefab);
		if (rewardDisplayData.MainText.Parameters == null)
		{
			rewardDisplayData.MainText.Parameters = new Dictionary<string, string>();
		}
		rewardDisplayData.MainText.Parameters["number2"] = "500";
		MTGALocalizedString mTGALocalizedString = questData.LocKey + "_Text";
		mTGALocalizedString.Parameters = new Dictionary<string, string> { 
		{
			"quantity",
			questData.Goal.ToString()
		} };
		objectiveBubble.SetRadialFill((float)questData.StartingProgress / (float)questData.Goal);
		MTGALocalizedString xOfYLocString = GetXOfYLocString(questData.StartingProgress.ToString(), questData.Goal.ToString());
		objectiveBubble.SetProgressText(xOfYLocString);
		objectiveBubble.SetReward(rewardDisplayData);
		objectiveBubble.SetSidebarVisible(visible: true);
		objectiveBubble.SetSidebarDescription(mTGALocalizedString);
		objectiveBubble.SetPopupDescription(mTGALocalizedString);
		objectiveBubble.SetSparkyHighlightBeacon($"SparkyHightlight_NormalQuest{index}");
		if (questData.EndingProgress == questData.Goal)
		{
			objectiveBubble.SetFooterText("MainNav/General/Complete");
		}
		else if (questData.CanSwap)
		{
			objectiveBubble.SetFooterText("MainNav/Quest/Quest_Swap_Text");
			objectiveBubble.SetPopupRefreshButtonText("MainNav/Quest/Quest_Swap_Text");
		}
		else
		{
			objectiveBubble.SetFooterText("MainNav/General/Empty_String");
		}
		objectiveBubble.Reference_startProgress = questData.StartingProgress;
		objectiveBubble.Reference_endProgress = questData.EndingProgress;
		objectiveBubble.Reference_curGoalProgress = questData.Goal;
		Guid id = questData.Id;
		objectiveBubble.Reference_rewardContext = "Quest.Completed." + id.ToString();
		objectiveBubble.Reference_questData = questData;
		QuestData questData2 = WrapperController.Instance.PostMatchClientUpdate?.questUpdate.FirstOrDefault((QuestData x) => x != null && x.inventoryUpdate?.context?.sourceId != null && x.inventoryUpdate.context.sourceId == questData.Id.ToString());
		if (questData2 != null)
		{
			Client_QuestData client_QuestData = new Client_QuestData(questData2);
			objectiveBubble.Reference_invUpdate = new List<ClientInventoryUpdateReportItem> { client_QuestData.InventoryUpdate };
		}
		objectiveBubble.Click += SwapQuest;
		return objectiveBubble;
	}

	private ObjectiveBubble CreateBubble_DailyRewards(RewardDisplayData reward, int wins, int curGoal, int numWinsInMatch, PeriodicRewardsTrackUpdate update)
	{
		ObjectiveBubble objectiveBubble = CreateBubble("Daily", DefaultBubblePrefab);
		int reference_startProgress = update?.PreviousSequenceId ?? (wins - numWinsInMatch);
		int num = update?.CurrentSequenceId ?? wins;
		objectiveBubble.SetReward(reward);
		objectiveBubble.SetFooterText("MainNav/EventRewards/Earning_Win_Rewards");
		objectiveBubble.SetPopupDescription("MainNav/EventRewards/Daily_Wins_Title");
		objectiveBubble.SetSegmentedFill(curGoal);
		objectiveBubble.SetRadialFill((float)num / (float)curGoal);
		objectiveBubble.SetSparkyHighlightBeacon("SparkyHightlight_DailyReward");
		if (num < curGoal)
		{
			MTGALocalizedString xOfYLocString = GetXOfYLocString(num.ToString(), curGoal.ToString());
			objectiveBubble.SetProgressText(xOfYLocString);
		}
		else
		{
			float timer = CalculatDailyTimeRemaining();
			objectiveBubble.SetTimer(timer);
			objectiveBubble.TimerCompleted += RebuildAllBubbles;
			objectiveBubble.ToComplete();
		}
		objectiveBubble.Reference_startProgress = reference_startProgress;
		objectiveBubble.Reference_endProgress = num;
		objectiveBubble.Reference_curGoalProgress = curGoal;
		objectiveBubble.Reference_rewardContext = "PlayerReward.OnMatchCompletedDaily";
		objectiveBubble.Reference_invUpdate = update?.InventoryUpdates;
		return objectiveBubble;
	}

	private ObjectiveBubble CreateBubble_WeeklyRewards(RewardDisplayData reward, int wins, int curGoal, int numWinsInMatch, PeriodicRewardsTrackUpdate update)
	{
		ObjectiveBubble objectiveBubble = CreateBubble("Weekly", DefaultBubblePrefab);
		int reference_startProgress = update?.PreviousSequenceId ?? (wins - numWinsInMatch);
		int num = update?.CurrentSequenceId ?? wins;
		if (reward.MainText.Parameters == null)
		{
			reward.MainText.Parameters = new Dictionary<string, string>();
		}
		reward.MainText.Parameters["count"] = "250";
		objectiveBubble.SetReward(reward);
		objectiveBubble.SetFooterText("MainNav/EventRewards/Earning_Win_Rewards");
		objectiveBubble.SetPopupDescription("MainNav/EventRewards/Weekly_Wins_Title");
		objectiveBubble.SetSegmentedFill(curGoal);
		objectiveBubble.SetRadialFill((float)num / (float)curGoal);
		objectiveBubble.SetSparkyHighlightBeacon("SparkyHightlight_WeeklyReward");
		if (num < curGoal)
		{
			MTGALocalizedString xOfYLocString = GetXOfYLocString(num.ToString(), curGoal.ToString());
			objectiveBubble.SetProgressText(xOfYLocString);
		}
		else
		{
			float timer = CalculateWeeklyTimeRemaining();
			objectiveBubble.SetTimer(timer);
			objectiveBubble.TimerCompleted += RebuildAllBubbles;
			objectiveBubble.ToComplete();
		}
		objectiveBubble.Reference_startProgress = reference_startProgress;
		objectiveBubble.Reference_endProgress = num;
		objectiveBubble.Reference_curGoalProgress = curGoal;
		objectiveBubble.Reference_rewardContext = "PlayerReward.OnMatchCompletedWeekly";
		objectiveBubble.Reference_invUpdate = update?.InventoryUpdates;
		return objectiveBubble;
	}

	private ObjectiveBubble CreateBubble_BattlePass(CurrentProgressionSummary progressionSummary)
	{
		ObjectiveBubble objectiveBubble = CreateBubble("BattlePass", BattlePassBubblePrefab);
		objectiveBubble.Reference_rewardContext = MasteryPassConstants.MASTERY_CONTEXT_BP;
		objectiveBubble.name += " - Level";
		if (progressionSummary.LevelInfo.IsProgressionComplete)
		{
			objectiveBubble.SetUpAsCompletedFinalLevel();
		}
		else
		{
			MTGALocalizedString mTGALocalizedString = "EPP/Objective/XPFraction";
			mTGALocalizedString.Parameters = new Dictionary<string, string>
			{
				{
					"value1",
					progressionSummary.LevelInfo.EXPProgressIfIsCurrent.ToString()
				},
				{
					"value2",
					progressionSummary.LevelInfo.ServerLevel.xpToComplete.ToString()
				}
			};
			objectiveBubble.SetProgressText(mTGALocalizedString);
			objectiveBubble.ActivatePremiumWreath(progressionSummary.Tier > 0);
			objectiveBubble.SetReward(progressionSummary.CurrentReward);
		}
		objectiveBubble.SetRadialFill((float)progressionSummary.LevelInfo.EXPProgressIfIsCurrent / (float)progressionSummary.LevelInfo.ServerLevel.xpToComplete);
		objectiveBubble.SetSidebarVisible(visible: false);
		objectiveBubble.SetPopupDescription("EPP/Objective/ClickToView");
		objectiveBubble.SetFooterText("EPP/Level/XPNextLevel");
		objectiveBubble.Reference_levelData = progressionSummary.LevelInfo;
		objectiveBubble.SetMasteryLevelLabel(progressionSummary.LevelInfo);
		objectiveBubble.SetPopupRefreshButtonText("MainNav/Quest/MasteryPass");
		PlayerTrackUpdate playerTrackUpdate = WrapperController.Instance.PostMatchClientUpdate?.battlePassUpdate;
		if (playerTrackUpdate != null)
		{
			objectiveBubble.Reference_invUpdate = new List<ClientInventoryUpdateReportItem>();
			if (playerTrackUpdate?.trackDiff?.inventoryUpdates != null)
			{
				objectiveBubble.Reference_invUpdate.AddRange(playerTrackUpdate?.trackDiff?.inventoryUpdates);
			}
			if (playerTrackUpdate?.rewardWebDiff?.inventoryUpdates != null)
			{
				objectiveBubble.Reference_invUpdate.AddRange(playerTrackUpdate?.rewardWebDiff?.inventoryUpdates);
			}
		}
		objectiveBubble.Reference_endProgress = progressionSummary.LevelInfo.EXPProgressIfIsCurrent;
		objectiveBubble.Reference_curGoalProgress = progressionSummary.LevelInfo.ServerLevel.xpToComplete;
		objectiveBubble.Click += OpenBattlePassTrack;
		objectiveBubble.SetUnOwnedRewardOverlay(progressionSummary.ShouldTease);
		objectiveBubble.SetLocked(progressionSummary.ShouldTease);
		return objectiveBubble;
	}

	private ObjectiveBubble CreateBubble_QuestTomorrow()
	{
		ObjectiveBubble objectiveBubble = CreateBubble("Timer", DefaultBubblePrefab);
		float timeRemainingSeconds = CalculatQuestRefreshTimeRemaining();
		objectiveBubble.InitializeTimer(_hourglassGraphic, _hourglassPrefab, timeRemainingSeconds);
		objectiveBubble.TimerCompleted += RebuildAllBubbles;
		objectiveBubble.SetPopupDescription("MainNav/General/Empty_String");
		_timerGameObject = objectiveBubble.gameObject;
		return objectiveBubble;
	}

	private ObjectiveBubble CreateBubble_Achievement(IClientAchievement achievement)
	{
		ObjectiveBubble objectiveBubble = CreateBubble("Achievement", AchievementBubblePrefab);
		RewardDisplayData rewardDisplayData = TempRewardTranslation.ChestDescriptionToDisplayData(achievement.Reward.RewardChestDescription, _cardDatabase.CardDataProvider, _cardMaterialBuilder);
		rewardDisplayData.ReferenceID = achievement.Reward.RewardChestDescription.referenceId;
		objectiveBubble.SetReward(rewardDisplayData);
		objectiveBubble.SetProgressText(GetXOfYLocString(achievement.CurrentCount.ToString(), achievement.MaxCount.ToString()));
		objectiveBubble.SetSidebarVisible(!achievement.IsFavorite);
		objectiveBubble.SetSidebarDescription(achievement.TitleLocalizationKey);
		objectiveBubble.SetPopupDescription(achievement.DescriptionLocalizationKey);
		objectiveBubble.SetRadialFill((float)achievement.CurrentCount / (float)achievement.MaxCount);
		objectiveBubble.SetRefreshHover(showRefreshIcon: false);
		objectiveBubble.SetPopupRefreshButtonText("MainNav/Quest/ViewAchievement");
		if (!string.IsNullOrEmpty(achievement.ParentheticalTextLocalizationKey))
		{
			objectiveBubble.SetFooterText(achievement.ParentheticalTextLocalizationKey);
		}
		else
		{
			objectiveBubble.SetFooterText("EMPTY_NO_SPACE");
		}
		objectiveBubble.SetFavoriteAchievement(achievement.IsFavorite);
		if (achievement.IsClaimable)
		{
			objectiveBubble.PlayCompletePulse();
		}
		objectiveBubble.Achievement = achievement;
		objectiveBubble.Click += GoToAchievement;
		objectiveBubble.IsDeepLink = true;
		return objectiveBubble;
	}

	private List<ObjectiveBubble> CreateNodeGraphBubbles(IReadOnlyDictionary<string, ClientGraphDefinition> graphs)
	{
		List<ObjectiveBubble> list = new List<ObjectiveBubble>();
		List<ClientNodeDefinition> list2 = new List<ClientNodeDefinition>();
		Dictionary<string, ClientCampaignGraphState> dictionary = new Dictionary<string, ClientCampaignGraphState>();
		Dictionary<string, ClientGraphDefinition> dictionary2 = new Dictionary<string, ClientGraphDefinition>();
		foreach (KeyValuePair<string, ClientGraphDefinition> graph in graphs)
		{
			graph.Deconstruct(out var _, out var value);
			ClientGraphDefinition clientGraphDefinition = value;
			IEnumerable<ClientNodeDefinition> objectiveBubbleNodes = clientGraphDefinition.GetObjectiveBubbleNodes();
			if (!objectiveBubbleNodes.Any())
			{
				continue;
			}
			ClientCampaignGraphState state = _campaignGraphManager.GetState(clientGraphDefinition);
			foreach (ClientNodeDefinition item in objectiveBubbleNodes)
			{
				if (!state.NodeStates.TryGetValue(item.Id, out var value2) || value2.Status != Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Available)
				{
					continue;
				}
				string text = item.UXInfo?.ObjectiveBubbleUXInfo?.RewardNodeId;
				if (!string.IsNullOrEmpty(text) && text != item.Id && state.NodeStates.TryGetValue(text, out var value3) && value3.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed)
				{
					continue;
				}
				int num = 0;
				foreach (ClientNodeDefinition item2 in list2)
				{
					if (ContentControllerObjectivesUtils.DoesObjectiveNodeHaveHigherDynamicPriority(item, item2, state))
					{
						break;
					}
					num++;
				}
				list2.Insert(num, item);
				dictionary.Add(item.Id, state);
				dictionary2.Add(item.Id, clientGraphDefinition);
			}
			if (list2.Count > 0 && SceneLoader.GetSceneLoader().CurrentContentType == NavContentType.Home)
			{
				if (list2[0].Id != MDNPlayerPrefs.GetNPEObjectiveLastSeen(_accountClient.AccountInformation.AccountID))
				{
					ColorMasteryBubblePrefab.SetQuestChangeVFX(val: true);
					NPEBubblePrefab.SetQuestChangeVFX(val: true);
					MDNPlayerPrefs.SetNPEObjectiveLastSeen(_accountClient.AccountInformation.AccountID, list2[0].Id);
				}
				else
				{
					ColorMasteryBubblePrefab.SetQuestChangeVFX(val: false);
					NPEBubblePrefab.SetQuestChangeVFX(val: false);
				}
			}
		}
		for (int i = 0; i < Math.Min(list2.Count, 1); i++)
		{
			ClientNodeDefinition clientNodeDefinition = list2[i];
			ObjectiveBubble nodeGraphObjectiveBubblePrefab = DefaultBubblePrefab;
			if (clientNodeDefinition.UXInfo.ObjectiveBubbleUXInfo.PrefabReferenceId == "Objective_Progress_ColorMastery")
			{
				nodeGraphObjectiveBubblePrefab = ColorMasteryBubblePrefab;
			}
			else if (clientNodeDefinition.UXInfo.ObjectiveBubbleUXInfo.PrefabReferenceId == "Objective_NPEQuest")
			{
				nodeGraphObjectiveBubblePrefab = NPEBubblePrefab;
			}
			list.Add(CreateNodeGraphBubble(clientNodeDefinition, dictionary2[clientNodeDefinition.Id], dictionary[clientNodeDefinition.Id], nodeGraphObjectiveBubblePrefab));
		}
		return list;
	}

	private ObjectiveBubble CreateNodeGraphBubble(ClientNodeDefinition objectiveNode, ClientGraphDefinition graphDefinition, ClientCampaignGraphState graphState, ObjectiveBubble nodeGraphObjectiveBubblePrefab)
	{
		ClientObjectiveBubbleUXInfo objectiveBubbleUXInfo = objectiveNode.UXInfo.ObjectiveBubbleUXInfo;
		ClientNodeState clientNodeState = graphState.NodeStates[objectiveNode.Id];
		ObjectiveBubble objectiveBubble = CreateBubble(objectiveNode.Id, nodeGraphObjectiveBubblePrefab);
		int progress = objectiveNode.GetProgress(clientNodeState);
		int radialThreshold = objectiveBubbleUXInfo.RadialThreshold;
		ClientChestDescription chest = null;
		if (objectiveNode.TryGetRewardChest(graphDefinition, graphState, out chest))
		{
			objectiveBubble.Reference_questData = objectiveNode.GenerateClient_QuestData(clientNodeState, progress, radialThreshold, chest);
		}
		if (objectiveNode.GetObjectiveBubbleRewardDisplayData(chest, out var rewardDisplayData))
		{
			objectiveBubble.SetReward(rewardDisplayData);
			objectiveBubble.SetPopupDescription(rewardDisplayData.SecondaryText ?? ((MTGALocalizedString)"MainNav/General/Empty_String"));
			objectiveBubble.SetFooterText(objectiveBubbleUXInfo.PopupUXInfo?.FooterLocKey ?? "MainNav/General/Empty_String");
			objectiveBubble.SetPopupRefreshButtonText(objectiveBubbleUXInfo.PopupUXInfo?.RefreshButtonLocKey ?? "MainNav/General/Empty_String");
		}
		SetNodeGraphObjectiveBubbleClickAction(objectiveBubble, objectiveNode);
		objectiveBubble.IsDeepLink = objectiveNode.UXInfo.ObjectiveBubbleUXInfo.DeepLinkType != Wizards.Arena.Enums.UILayout.ObjectiveBubbleDeepLinkType.None;
		objectiveBubble.Reference_startProgress = progress;
		objectiveBubble.Reference_endProgress = progress;
		objectiveBubble.Reference_curGoalProgress = radialThreshold;
		Client_QuestData reference_questData = objectiveBubble.Reference_questData;
		if (reference_questData != null)
		{
			MTGALocalizedString xOfYLocString = GetXOfYLocString(reference_questData.EndingProgress.ToString(), reference_questData.Goal.ToString());
			objectiveBubble.SetProgressText(xOfYLocString);
		}
		objectiveBubble.SetRadialFill((float)progress / (float)radialThreshold);
		objectiveBubble.SetSidebarVisible(visible: false);
		objectiveBubble.enabled = true;
		return objectiveBubble;
	}

	private static void SetNodeGraphObjectiveBubbleClickAction(ObjectiveBubble bubble, ClientNodeDefinition objectiveNode)
	{
		bool flag = true;
		switch (objectiveNode.UXInfo.ObjectiveBubbleUXInfo.DeepLinkType)
		{
		case Wizards.Arena.Enums.UILayout.ObjectiveBubbleDeepLinkType.EventLandingPage:
			bubble.Click += delegate
			{
				EventContext eventContext = WrapperController.Instance.EventManager.GetEventContext(objectiveNode.UXInfo.ObjectiveBubbleUXInfo.DeepLinkReferenceId);
				SceneLoader.GetSceneLoader().GoToEventScreen(eventContext);
			};
			break;
		case Wizards.Arena.Enums.UILayout.ObjectiveBubbleDeepLinkType.EventPlayBlade:
			bubble.Click += delegate
			{
				SceneLoader.GetSceneLoader().ShowPlayBladeAndSelect(objectiveNode.UXInfo.ObjectiveBubbleUXInfo.DeepLinkReferenceId);
			};
			break;
		case Wizards.Arena.Enums.UILayout.ObjectiveBubbleDeepLinkType.SetMasteryLandingPage:
			bubble.Click += delegate
			{
				ProgressionTrackPageContext trackPageContext = new ProgressionTrackPageContext(objectiveNode.UXInfo.ObjectiveBubbleUXInfo.DeepLinkReferenceId, NavContentType.Home, NavContentType.Home);
				SceneLoader.GetSceneLoader().GoToProgressionTrackScene(trackPageContext, "From Objective Bubble");
			};
			break;
		case Wizards.Arena.Enums.UILayout.ObjectiveBubbleDeepLinkType.TaggedEventLandingPage:
			bubble.Click += delegate
			{
				if (Enum.TryParse<EventTag>(objectiveNode.UXInfo.ObjectiveBubbleUXInfo.DeepLinkReferenceId, out var tag))
				{
					EventContext eventContext = (from ec in WrapperController.Instance.EventManager.EventContexts
						where ec.PlayerEvent.EventInfo.EventTags != null && ec.PlayerEvent.EventInfo.EventTags.Contains(tag)
						orderby ec.PlayerEvent.EventInfo.ClosedTime
						select ec).FirstOrDefault();
					if (eventContext != null)
					{
						SceneLoader.GetSceneLoader().GoToEventScreen(eventContext);
					}
				}
			};
			break;
		default:
			flag = false;
			break;
		}
		if (bubble.Reference_questData != null)
		{
			bubble.Reference_questData.CanSwap = flag;
		}
		bubble.SetClickable(flag);
	}

	public void SetPopupOnBubble(ObjectiveBubble bubble, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		RefreshPopupOnBubble(bubble, cardDatabase, cardViewBuilder);
	}

	public void RefreshPopupOnBubble(ObjectiveBubble bubble, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		if (bubble.Achievement != null)
		{
			if (_achievementNotificationPopup == null)
			{
				_achievementNotificationPopup = UnityEngine.Object.Instantiate(_achievementNotificationPopupPrefab, _popupParent);
				_achievementNotificationPopup.Init(cardDatabase, cardViewBuilder);
				_achievementNotificationPopup.gameObject.SetActive(value: false);
			}
			bubble.SetPopup(_achievementNotificationPopup);
		}
		else
		{
			if (_notificationPopup == null)
			{
				_notificationPopup = UnityEngine.Object.Instantiate(_notificationPopupPrefab, _popupParent);
				_notificationPopup.Init(cardDatabase, cardViewBuilder);
				_notificationPopup.gameObject.SetActive(value: false);
			}
			bubble.SetPopup(_notificationPopup);
		}
		bubble.PopupRefreshSafeArea(_safeArea, CurrentCamera.Value);
	}

	private ObjectiveBubble CreateBubble(string name, ObjectiveBubble prefab, bool enabled = true)
	{
		ObjectiveBubble objectiveBubble = UnityEngine.Object.Instantiate(prefab, ObjectivesParent.transform);
		objectiveBubble.enabled = enabled;
		objectiveBubble.Init(_cardDatabase, _cardViewBuilder);
		objectiveBubble.name = objectiveBubble.name + " - " + name;
		objectiveBubble.InitPopupCallback += SetPopupOnBubble;
		objectiveBubble.RefreshPopupCallback += RefreshPopupOnBubble;
		_bubbles.Add(objectiveBubble);
		if (PlatformUtils.IsHandheld())
		{
			objectiveBubble.PopupEnabled += OnPopupActive;
			objectiveBubble.PopupDisabled += OnPopupDeactivate;
		}
		return objectiveBubble;
	}

	public void RebuildAllBubbles()
	{
		ClearAllBubbles();
		if (_refreshQuestDataAndRebuildDisplay != null)
		{
			_refreshQuestDataAndRebuildDisplay();
			_refreshQuestDataAndRebuildDisplay = null;
		}
	}

	private static float CalculatQuestRefreshTimeRemaining()
	{
		DateTime gameTime = ServerGameTime.GameTime;
		DateTime dateTime = new DateTime(gameTime.Year, gameTime.Month, gameTime.Day, 9, 0, 0);
		if (gameTime.Hour >= 9)
		{
			dateTime = dateTime.AddDays(1.0);
		}
		return (float)(dateTime - gameTime).TotalSeconds;
	}

	private static float CalculatDailyTimeRemaining()
	{
		DateTime gameTime = ServerGameTime.GameTime;
		DateTime dateTime = new DateTime(gameTime.Year, gameTime.Month, gameTime.Day, 9, 0, 0);
		if (gameTime.Hour >= 9)
		{
			dateTime = dateTime.AddDays(1.0);
		}
		return (float)(dateTime - gameTime).TotalSeconds;
	}

	private static float CalculateWeeklyTimeRemaining()
	{
		DateTime gameTime = ServerGameTime.GameTime;
		DateTime dateTime = new DateTime(gameTime.Year, gameTime.Month, gameTime.Day, 9, 0, 0);
		int num = 0 - gameTime.DayOfWeek;
		if (num < 0)
		{
			num += 7;
		}
		if (num == 0 && gameTime.Hour >= 9)
		{
			num = 7;
		}
		dateTime = dateTime.AddDays(num);
		return (float)(dateTime - gameTime).TotalSeconds;
	}

	public static MTGALocalizedString GetXOfYLocString(string x, string y)
	{
		MTGALocalizedString mTGALocalizedString = "MainNav/General/X_Of_Y";
		mTGALocalizedString.Parameters = new Dictionary<string, string>
		{
			{ "x", x },
			{ "y", y }
		};
		return mTGALocalizedString;
	}

	public void OnPopupActive(ObjectiveBubble bubble)
	{
		_actionSystem.PushFocus(this);
		if (_raycastblocker != null)
		{
			_raycastblocker.transform.SetSiblingIndex(0);
			_raycastblocker.SetActive(bubble.IsPopupActive);
		}
		bubble.SetDimmed(dimmed: false);
		bubble.PopupRefreshSafeArea(_safeArea, CurrentCamera.Value);
		CloseAllObjectivePopups(bubble);
	}

	public void OnPopupDeactivate(ObjectiveBubble bubble)
	{
		if (_raycastblocker != null)
		{
			_raycastblocker.SetActive(value: false);
		}
		_actionSystem.PopFocus(this);
		CloseAllObjectivePopups(bubble);
	}

	public void CloseAllObjectivePopups()
	{
		CloseAllObjectivePopups(null);
	}

	public void CloseAllObjectivePopups(ObjectiveBubble exceptThisOne)
	{
		for (int i = 0; i < _bubbles.Count; i++)
		{
			if (!(_bubbles[i] == exceptThisOne))
			{
				if (_bubbles[i].IsPopupActive)
				{
					_actionSystem.PopFocus(this);
					_bubbles[i].CloseMobilePopup();
				}
				_bubbles[i].SetDimmed(dimmed: false);
			}
		}
		if (exceptThisOne == null && _raycastblocker != null)
		{
			_raycastblocker.SetActive(value: false);
		}
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape && PlatformUtils.IsHandheld() && IsPopupActive)
		{
			CloseAllObjectivePopups();
			return true;
		}
		return false;
	}

	public void OnBack(ActionContext context)
	{
		if (PlatformUtils.IsHandheld())
		{
			CloseAllObjectivePopups();
		}
		else
		{
			context.Used = false;
		}
	}
}
