using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.MainNavigation.RewardTrack;
using Core.Meta.MainNavigation.Achievements;
using Core.Meta.Quests;
using GreClient.CardData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class ObjectiveBubble : MonoBehaviour
{
	public enum PromotionalDisplayType
	{
		TourQuestI,
		TourQuestII,
		TourQuestIII,
		GenericPromotional,
		NotPromotional
	}

	[SerializeField]
	private TMP_Text _winsText;

	[SerializeField]
	private Localize _progressText;

	[SerializeField]
	protected Localize _mainImageText;

	[SerializeField]
	private Localize _sidebarText;

	[SerializeField]
	private Image _primaryImage;

	[SerializeField]
	private Image _secondaryImage;

	[SerializeField]
	private Image _tertiaryImage;

	[SerializeField]
	protected Animator _animator;

	[SerializeField]
	private Animator _buttonAnimator;

	[SerializeField]
	protected Image _progressFillImage;

	[SerializeField]
	private Image _greyProgressFillImage;

	[SerializeField]
	protected NotificationPopup _notificationPopup;

	[SerializeField]
	protected GameObject _popupParent;

	[SerializeField]
	private GameObject _locationIndicator;

	[SerializeField]
	protected GameObject _textShadow;

	[SerializeField]
	private GameObject _promotionVFX;

	[SerializeField]
	private GameObject _sparkyVFX;

	[SerializeField]
	private GameObject _sparkyHighlight;

	[SerializeField]
	private List<Sprite> _promotionalIconTypes;

	[SerializeField]
	private Image _promotionalIcon;

	[SerializeField]
	private Localize _eppLevelLabel;

	[SerializeField]
	private Image _premiumBPWreath;

	[SerializeField]
	private Image _unownedRewardTease;

	[SerializeField]
	private Animator _levelUp;

	[SerializeField]
	private GameObject _questChangeVFX;

	[SerializeField]
	private GameObject _favoriteAchievementTag;

	private AssetLoader.AssetTracker<Sprite> _primaryImageSpriteTracker;

	private AssetLoader.AssetTracker<Sprite> _secondaryImageSpriteTracker;

	private AssetLoader.AssetTracker<Sprite> _tertiaryImageSpriteTracker;

	public int Reference_startProgress;

	public int Reference_endProgress;

	public int Reference_curGoalProgress;

	public int Reference_nextGoalProgress;

	public string Reference_rewardContext;

	public bool IsDeepLink;

	public Client_QuestData Reference_questData;

	public List<ClientInventoryUpdateReportItem> Reference_invUpdate;

	public ProgressionTrackLevel Reference_levelData;

	public IClientAchievement Achievement;

	private bool _popupEnabled = true;

	private bool _hardPreventPopups;

	private bool _clickable = true;

	protected NotificationPopup.PopupData _popupData = new NotificationPopup.PopupData();

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	private float _countStartingTimeSeconds;

	private float _countdownTime;

	private bool _timerUpdating;

	private bool _isPopupActive;

	public CanvasGroup CanvasGroup { get; protected set; }

	public bool ISBPBubble => Reference_rewardContext == MasteryPassConstants.MASTERY_CONTEXT_BP;

	public string WinsText
	{
		get
		{
			return _winsText?.text ?? "";
		}
		set
		{
			if ((bool)_winsText)
			{
				_winsText.text = value;
			}
		}
	}

	public virtual bool Tickable
	{
		get
		{
			if (Reference_curGoalProgress > Reference_startProgress)
			{
				return Reference_endProgress > Reference_startProgress;
			}
			return false;
		}
	}

	public virtual bool Clickable
	{
		get
		{
			if (Reference_questData != null)
			{
				if (Reference_questData.CanSwap)
				{
					return Reference_questData.EndingProgress < Reference_questData.Goal;
				}
				return false;
			}
			if (Reference_levelData != null)
			{
				return true;
			}
			if (IsDeepLink)
			{
				return true;
			}
			return _clickable;
		}
	}

	public bool IsPopupActive
	{
		get
		{
			return _isPopupActive;
		}
		protected set
		{
			_isPopupActive = value && _popupEnabled;
			if (_isPopupActive)
			{
				if (_notificationPopup == null)
				{
					if (this.InitPopupCallback != null)
					{
						this.InitPopupCallback(this, _cardDatabase, _cardViewBuilder);
					}
				}
				else if (this.RefreshPopupCallback != null)
				{
					this.RefreshPopupCallback(this, _cardDatabase, _cardViewBuilder);
				}
				SetPopupPosition();
				_popupData.ApplyData(_notificationPopup);
			}
			_notificationPopup?.gameObject.UpdateActive(_isPopupActive);
		}
	}

	public event Action<ObjectiveBubble, CardDatabase, CardViewBuilder> InitPopupCallback;

	public event Action<ObjectiveBubble, CardDatabase, CardViewBuilder> RefreshPopupCallback;

	public event Action<ObjectiveBubble> Click;

	public event Action<ObjectiveBubble> PopupEnabled;

	public event Action<ObjectiveBubble> PopupDisabled;

	public event Action TimerCompleted;

	public void Init(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_popupData.RefreshButtonOnClickListener = ToggleMobilePopup;
		IsPopupActive = false;
		_mainImageText.SetText("MainNav/General/Empty_String");
		CanvasGroup = GetComponent<CanvasGroup>();
		if (PlatformUtils.IsHandheld())
		{
			_locationIndicator.GetComponent<Image>().enabled = false;
		}
	}

	private void Update()
	{
		if (_timerUpdating)
		{
			UpdateTimerDisplay();
		}
		if (CustomInputModule.GetTouchCount() > 0 && (CustomInputModule.PointerWasPressedThisFrame() || CustomInputModule.PointerWasReleasedThisFrame()))
		{
			Vector2 pointerPosition = CustomInputModule.GetPointerPosition();
			if (Physics.Raycast(Camera.main.ScreenPointToRay(pointerPosition), out var hitInfo) && hitInfo.transform == base.transform)
			{
				ToggleMobilePopup();
			}
		}
	}

	public void InitializeTimer(Sprite mainSprite, NotificationPopupReward rewardPrefab, float timeRemainingSeconds)
	{
		_primaryImage.sprite = mainSprite;
		_greyProgressFillImage.fillAmount = 1f;
		_progressFillImage.fillAmount = 0f;
		_popupData.HeaderString1 = "MainNav/Quest/Quest_Wait_Text";
		_popupData.HeaderString2 = "MainNav/General/Empty_String";
		_popupData.FooterString = "MainNav/Popups/QuestRewardPopupDetailsForWaiting";
		_popupData.RefreshButtonString = "MainNav/Popups/QuestRewardPopupDetailsForWaiting";
		_popupData.SetPopup3dObject(rewardPrefab, null, null);
		_textShadow.SetActive(value: false);
		SetTimer(timeRemainingSeconds);
	}

	public void SetTimer(float timeRemainingSeconds)
	{
		_countdownTime = timeRemainingSeconds;
		_countStartingTimeSeconds = Time.time;
		_timerUpdating = true;
		UpdateTimerDisplay();
	}

	private void UpdateTimerDisplay()
	{
		float num = Time.time - _countStartingTimeSeconds;
		float val = _countdownTime - num;
		val = Math.Max(val, 0f);
		if (IsPopupActive)
		{
			float num2 = val;
			int num3 = (int)Math.Floor(num2 / 3600f);
			float num4 = (float)num3 * 3600f;
			float num5 = num2 - num4;
			int num6 = (int)Math.Floor(num5 / 60f);
			float num7 = (float)num6 * 60f;
			int num8 = (int)Math.Floor(num5 - num7);
			string unlocalizedProgressText = "(" + num3 + ":" + num6.ToString("00.") + ":" + num8.ToString("00.") + ")";
			_notificationPopup.SetUnlocalizedProgressText(unlocalizedProgressText);
		}
		if (val <= 0f)
		{
			if (_countdownTime > 0f)
			{
				StartCoroutine(TimerCompleteYield());
			}
			_timerUpdating = false;
		}
	}

	private IEnumerator TimerCompleteYield()
	{
		yield return new WaitForSeconds(0.1f);
		this.TimerCompleted?.Invoke();
	}

	public virtual void Tick(Action<RewardObjectiveContext> onTickedCallback)
	{
		if (!Tickable)
		{
			return;
		}
		bool num = Reference_curGoalProgress != 0;
		bool flag = num && Reference_endProgress >= Reference_curGoalProgress;
		if (num)
		{
			SetRadialFill((float)Reference_endProgress / (float)Reference_curGoalProgress);
			MTGALocalizedString xOfYLocString = ContentControllerObjectives.GetXOfYLocString(Reference_endProgress.ToString(), Reference_curGoalProgress.ToString());
			if (Reference_levelData == null || !Reference_levelData.IsProgressionComplete)
			{
				SetProgressText(xOfYLocString);
			}
		}
		if (flag)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_quest_complete, base.gameObject);
			PlayCompletePulse();
		}
		else
		{
			bool doLevelUp = false;
			if (ISBPBubble)
			{
				LevelChange levelChange = Pantry.Get<SetMasteryDataProvider>().GetAndRemoveCachedBpLevelChanges()?.FirstOrDefault();
				if (levelChange != null)
				{
					doLevelUp = levelChange.Level.Index < Reference_levelData.Index;
				}
			}
			PlayProgressPulse(doLevelUp);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_progress_poof, base.gameObject);
		}
		onTickedCallback?.Invoke(new RewardObjectiveContext
		{
			contextString = Reference_rewardContext,
			questData = Reference_questData,
			ClientInventoryUpdateReportItem = Reference_invUpdate
		});
	}

	public void SetSidebarVisible(bool visible)
	{
		if (PlatformUtils.IsHandheld())
		{
			_animator.SetTrigger("NoDescription");
		}
		else
		{
			_animator.SetTrigger((!visible) ? "NoDescription" : "Description");
		}
	}

	public void SetActivePromotionVFX(bool val)
	{
		if (_promotionVFX != null)
		{
			_promotionVFX.UpdateActive(val);
		}
		if (_sparkyVFX != null)
		{
			_sparkyVFX.UpdateActive(val);
		}
	}

	public void SetQuestChangeVFX(bool val)
	{
		if (_questChangeVFX != null)
		{
			_questChangeVFX.UpdateActive(val);
		}
	}

	public void SetSparkyHighlightBeacon(string BeaconName)
	{
		if (_sparkyHighlight != null)
		{
			SceneObjectBeacon sceneObjectBeacon = _sparkyHighlight.AddComponent<SceneObjectBeacon>();
			sceneObjectBeacon.BeaconName = BeaconName;
			sceneObjectBeacon.InitializeBeacon();
		}
	}

	public void SetMasteryLevelLabel(ProgressionTrackLevel level)
	{
		int rawLevel = level.RawLevel;
		_eppLevelLabel?.SetText("MainNav/General/Simple_Number", new Dictionary<string, string> { 
		{
			"number",
			rawLevel.ToString()
		} });
	}

	public void SetPromotionalIcon(PromotionalDisplayType promoType)
	{
		_promotionalIcon.sprite = _promotionalIconTypes[(int)promoType];
	}

	public void SetSidebarDescription(MTGALocalizedString desc)
	{
		_sidebarText.SetText(desc);
	}

	public void SetSegmentedFill(int numSegments)
	{
		_animator.SetInteger("Segments", numSegments);
	}

	public void SetRadialFill(float fill)
	{
		_progressFillImage.fillAmount = fill;
	}

	public void SetProgressText(MTGALocalizedString text)
	{
		_progressText.SetText(text);
		_popupData.ProgressString = text;
	}

	public void SetFooterText(MTGALocalizedString text)
	{
		_popupData.FooterString = text;
	}

	public void SetPopupDescription(MTGALocalizedString desc)
	{
		_popupData.DescriptionString = desc;
	}

	public void SetPopupRefreshButtonText(MTGALocalizedString desc)
	{
		_popupData.RefreshButtonString = desc;
	}

	public void SetLocked(bool value)
	{
		_popupData.IsLocked = value;
	}

	public void SetReward(RewardDisplayData reward)
	{
		if (reward == null)
		{
			return;
		}
		SetUnOwnedRewardOverlay(active: false);
		SetLocked(value: false);
		InitializeObjectiveImage(_primaryImage, ref _primaryImageSpriteTracker, "ObjectiveBubblePrimarySprite", reward.Thumbnail1Path);
		InitializeObjectiveImage(_secondaryImage, ref _secondaryImageSpriteTracker, "ObjectiveBubbleSecondarySprite", reward.Thumbnail2Path);
		InitializeObjectiveImage(_tertiaryImage, ref _tertiaryImageSpriteTracker, "ObjectiveBubbleTertiaryImageSprite", reward.Thumbnail3Path);
		if (reward.Quantity > 1)
		{
			_mainImageText.SetText("MainNav/General/Simple_Number", new Dictionary<string, string> { 
			{
				"number",
				reward.Quantity.ToString()
			} });
			_mainImageText.gameObject.SetActive(value: true);
			_textShadow.SetActive(value: true);
		}
		else
		{
			_mainImageText.SetText("MainNav/General/Empty_String");
			_mainImageText.gameObject.SetActive(value: false);
			_textShadow.SetActive(value: false);
		}
		if (RewardDisplayData.TryParseCard(reward.ReferenceID, out var grpId, out var _))
		{
			CardPrintingData cardPrintingById = WrapperController.Instance.CardDatabase.CardDataProvider.GetCardPrintingById(grpId);
			if (cardPrintingById != null)
			{
				if (reward.MainText.Parameters == null)
				{
					reward.MainText.Parameters = new Dictionary<string, string>();
				}
				reward.MainText.Parameters["cardName"] = WrapperController.Instance.CardDatabase.GreLocProvider.GetLocalizedText(cardPrintingById.TitleId);
			}
		}
		_popupData.HeaderString1 = "MainNav/EventRewards/Reward";
		_popupData.HeaderString2 = reward.MainText;
		if (!string.IsNullOrEmpty(reward.Popup3dObjectPath))
		{
			_popupData.SetPopupDisplayData(reward);
		}
		else
		{
			_popupData.SetPopup3dObject("", null, null);
		}
	}

	public void SetOnTrack(bool isOnTrack)
	{
		_locationIndicator.SetActive(isOnTrack);
	}

	public void SetClickable(bool clickable)
	{
		_clickable = clickable;
	}

	public void SetRefreshHover(bool showRefreshIcon)
	{
		_buttonAnimator.SetBool("Refresh", showRefreshIcon);
	}

	public void SetUpAsCompletedFinalLevel()
	{
		SetProgressText("MainNav/General/Complete");
		ToComplete();
		_popupData.HeaderString1 = "MainNav/General/Empty_String";
		_popupData.HeaderString2 = "MainNav/General/Empty_String";
		_primaryImage.gameObject.SetActive(value: false);
		_secondaryImage.gameObject.SetActive(value: false);
		_tertiaryImage.gameObject.SetActive(value: false);
		_mainImageText.SetText("MainNav/General/Empty_String");
		_mainImageText.gameObject.SetActive(value: false);
		_textShadow.SetActive(value: false);
		_hardPreventPopups = true;
	}

	public void SetPopup(NotificationPopup notificationPopup)
	{
		_notificationPopup = notificationPopup;
	}

	public void SetPopupEnabledIfAllowed(bool popupEnabled)
	{
		_popupEnabled = popupEnabled;
		if (_hardPreventPopups)
		{
			_popupEnabled = false;
		}
		IsPopupActive = _isPopupActive;
	}

	public void ToggleMobilePopup()
	{
		if (IsPopupActive)
		{
			CloseMobilePopup();
			this.PopupDisabled?.Invoke(this);
			OnClick();
		}
		else
		{
			IsPopupActive = true;
			PopupRefreshButtonOveride();
			this.PopupEnabled?.Invoke(this);
			_buttonAnimator.SetTrigger("MouseOver", value: true);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_rollover_a, base.gameObject);
		}
	}

	protected virtual void PopupRefreshButtonOveride()
	{
		_popupData.RefreshButtonActive = Clickable;
	}

	public void SetPopupPosition()
	{
		_popupData.WorldPos = _popupParent.GetComponent<RectTransform>().position;
		if ((bool)_notificationPopup)
		{
			_popupData.ApplyData(_notificationPopup);
		}
	}

	public void PopupRefreshSafeArea(RectTransform safeArea, Camera camera)
	{
		_popupData.SetSafeArea(safeArea, camera);
		SetPopupPosition();
	}

	public void CloseMobilePopup()
	{
		IsPopupActive = false;
		SetRefreshHover(showRefreshIcon: false);
		_buttonAnimator.SetTrigger("MouseOver", value: false);
		_buttonAnimator.SetTrigger("Up");
	}

	public void ActivatePopup()
	{
		IsPopupActive = true;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_quest_rollover_a, base.gameObject);
	}

	public void DeactivatePopup()
	{
		IsPopupActive = false;
	}

	public void OnClick()
	{
		if (_clickable)
		{
			this.Click?.Invoke(this);
		}
	}

	public void SetUnOwnedRewardOverlay(bool active)
	{
		if (_unownedRewardTease != null)
		{
			_unownedRewardTease.gameObject.SetActive(active);
		}
		Color color = new Color(1f, 1f, 1f, 0.35f);
		if (_primaryImage != null)
		{
			_primaryImage.color = (active ? color : Color.white);
		}
		if (_secondaryImage != null)
		{
			_secondaryImage.color = (active ? color : Color.white);
		}
		if (_tertiaryImage != null)
		{
			_tertiaryImage.color = (active ? color : Color.white);
		}
	}

	public void SetDimmed(bool dimmed, bool showProgress = true)
	{
		_animator.SetTrigger("Reset", !dimmed);
		_animator.SetTrigger("Lock", dimmed);
		if (!dimmed && showProgress && base.gameObject.activeInHierarchy)
		{
			StartCoroutine(Highlight());
		}
	}

	private IEnumerator Highlight()
	{
		yield return new WaitForSeconds(0.5f);
		_animator.SetTrigger("Reset", value: false);
		_animator.SetTrigger("toHighlight");
	}

	public void PlayCompletePulse()
	{
		_animator.SetBool("Completed", value: true);
		_animator.SetTrigger("PulseComplete", value: true);
	}

	public void ToHighlight()
	{
		_animator.SetTrigger("Reset");
		_animator.SetTrigger("toHighlight");
	}

	public void ToDim()
	{
		_animator.SetTrigger("Reset", value: false);
		_animator.SetTrigger("Lock", value: true);
	}

	public void ToComplete()
	{
		_animator.SetTrigger("Reset");
		_animator.SetBool("Completed", value: true);
		_animator.SetTrigger("toComplete", value: true);
	}

	public void PlayProgressPulse(bool doLevelUp = false)
	{
		_animator.SetBool("PulseProgress", value: true);
		if (_levelUp != null && doLevelUp)
		{
			_levelUp.SetTrigger("LevelUp", value: true);
		}
	}

	public void SetInactive(bool inactive)
	{
		_animator.SetTrigger("Inactive", inactive);
	}

	public Vector3 GetIndicatorCanvasPosition()
	{
		Vector3[] array = new Vector3[4];
		_locationIndicator.GetComponent<RectTransform>().GetWorldCorners(array);
		return array[0];
	}

	public Vector3 GetIndicatorWorldPosition()
	{
		return _locationIndicator.transform.position;
	}

	public void ActivatePremiumWreath(bool active)
	{
		if (_premiumBPWreath != null)
		{
			_premiumBPWreath.gameObject.SetActive(active);
		}
	}

	public void SetFavoriteAchievement(bool active)
	{
		if (_favoriteAchievementTag != null)
		{
			_favoriteAchievementTag.SetActive(active);
		}
	}

	public void OnDestroy()
	{
		AssetLoaderUtils.CleanupImage(_primaryImage, _primaryImageSpriteTracker);
		AssetLoaderUtils.CleanupImage(_secondaryImage, _secondaryImageSpriteTracker);
		AssetLoaderUtils.CleanupImage(_tertiaryImage, _tertiaryImageSpriteTracker);
	}

	private static void InitializeObjectiveImage(Image image, ref AssetLoader.AssetTracker<Sprite> spriteTracker, string key, string thumbnailPath)
	{
		if (image != null)
		{
			if (spriteTracker == null)
			{
				spriteTracker = new AssetLoader.AssetTracker<Sprite>(key);
			}
			AssetLoaderUtils.TrySetSpriteAndLogOnError(image, spriteTracker, thumbnailPath, "Couldn't set " + key + " bubble icon. Please ensure the asset exists and is place in the ServerRewards folder.");
			image.gameObject.SetActive(!string.IsNullOrEmpty(thumbnailPath));
		}
	}
}
