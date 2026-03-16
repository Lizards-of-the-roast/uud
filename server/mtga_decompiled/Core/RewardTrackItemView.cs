using System;
using System.Collections.Generic;
using Core.MainNavigation.RewardTrack;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class RewardTrackItemView : MonoBehaviour
{
	[HideInInspector]
	public NotificationPopup Hanger;

	[HideInInspector]
	public CustomButton ClickShield;

	[HideInInspector]
	public SetMasteryDataProvider MasteryPassProvider;

	[SerializeField]
	private List<RewardIcon> _rewardIcons;

	[SerializeField]
	private Localize _levelText;

	[SerializeField]
	private Image _repeatableLevelImage;

	[SerializeField]
	private Animator stateAnimator;

	private static readonly int StateComplete = Animator.StringToHash("State_Complete");

	private static readonly int TentPole = Animator.StringToHash("TentPole");

	private static readonly int StateLocked = Animator.StringToHash("State_Locked");

	private static readonly int StateCurrentLevel = Animator.StringToHash("State_CurrentLevel");

	private static readonly int StateNextLevel = Animator.StringToHash("State_NextLevel");

	private static readonly int RenewalPanel = Animator.StringToHash("RenewalPanel");

	private static readonly int RenewalIntro = Animator.StringToHash("RenewalIntro");

	private RewardDisplayData[] _rewards;

	private bool _completed;

	private bool _isTentPoleItem;

	private bool _isPremiumTierLocked;

	private bool _isCurrentLevel;

	private bool _isNextLevel;

	private int _level;

	private bool _isRepeatableLevel;

	private bool _isHoverable;

	private bool hasEnhancements;

	private GameObject _customPrefab;

	private GameObject _customPrefabInstance;

	public bool Completed
	{
		get
		{
			return _completed;
		}
		set
		{
			_completed = value;
			if (stateAnimator.isActiveAndEnabled)
			{
				stateAnimator.SetBool(StateComplete, _completed);
			}
		}
	}

	public bool IsTentPoleItem
	{
		get
		{
			return _isTentPoleItem;
		}
		set
		{
			_isTentPoleItem = value;
			if (stateAnimator.isActiveAndEnabled)
			{
				stateAnimator.SetBool(TentPole, _isTentPoleItem);
			}
		}
	}

	public bool IsPremiumTierLocked
	{
		get
		{
			return _isPremiumTierLocked;
		}
		set
		{
			_isPremiumTierLocked = value;
			if (stateAnimator.isActiveAndEnabled)
			{
				stateAnimator.SetBool(StateLocked, _isPremiumTierLocked);
			}
		}
	}

	public bool IsCurrentLevel
	{
		get
		{
			return _isCurrentLevel;
		}
		set
		{
			_isCurrentLevel = value;
			if (stateAnimator.isActiveAndEnabled)
			{
				stateAnimator.SetBool(StateCurrentLevel, _isCurrentLevel);
			}
		}
	}

	public bool IsNextLevel
	{
		get
		{
			return _isNextLevel;
		}
		set
		{
			_isNextLevel = value;
			if (stateAnimator.isActiveAndEnabled)
			{
				stateAnimator.SetBool(StateNextLevel, _isNextLevel);
			}
		}
	}

	public bool IsRepeatableLevel
	{
		get
		{
			return _isRepeatableLevel;
		}
		set
		{
			_isRepeatableLevel = value;
			_levelText.gameObject.UpdateActive(!_isRepeatableLevel);
			if (_repeatableLevelImage != null)
			{
				_repeatableLevelImage.gameObject.UpdateActive(_isRepeatableLevel);
			}
		}
	}

	public RectTransform Rect => base.gameObject.transform as RectTransform;

	public event Action<int, int> Click;

	private void OnEnable()
	{
		stateAnimator.SetBool(StateComplete, _completed);
		stateAnimator.SetBool(TentPole, _isTentPoleItem);
		stateAnimator.SetBool(StateLocked, _isPremiumTierLocked);
		stateAnimator.SetBool(StateCurrentLevel, _isCurrentLevel);
		stateAnimator.SetBool(StateNextLevel, _isNextLevel);
	}

	public void SetLevel(int index)
	{
		_level = index;
		_levelText.gameObject.UpdateActive(!IsRepeatableLevel);
		_levelText.SetText("MainNav/General/Simple_Number", new Dictionary<string, string> { 
		{
			"number",
			(index + 1).ToString()
		} });
		if (_repeatableLevelImage != null)
		{
			_repeatableLevelImage.gameObject.UpdateActive(IsRepeatableLevel);
		}
	}

	public void SetRewards(RewardDisplayData[] rewards, GameObject customPrefab = null, bool showRenewal = false, bool playIntro = false)
	{
		_rewards = rewards;
		hasEnhancements = showRenewal && rewards?.Length > _rewardIcons.Count && rewards[2] != null;
		stateAnimator.SetBool(RenewalPanel, !_completed && hasEnhancements);
		stateAnimator.SetBool(RenewalIntro, !_completed && hasEnhancements && playIntro);
		for (int i = 0; i < _rewardIcons.Count; i++)
		{
			RewardIcon rewardIcon = _rewardIcons[i];
			if (i < rewards?.Length)
			{
				rewardIcon.gameObject.UpdateActive((hasEnhancements && i == 0) ? (rewards[2] != null) : (rewards[i] != null));
				rewardIcon.SetReward((hasEnhancements && i == 0) ? rewards[2] : rewards[i]);
				rewardIcon.Popup = Hanger;
				rewardIcon.RewardTrackItemView = this;
				rewardIcon.MasteryPassProvider = MasteryPassProvider;
				if (ClickShield != null)
				{
					rewardIcon.ClickShield = ClickShield;
				}
			}
			else
			{
				rewardIcon.gameObject.UpdateActive(active: false);
			}
		}
		if (customPrefab != null)
		{
			_rewardIcons[1].gameObject.UpdateActive(active: false);
			if (_customPrefabInstance == null)
			{
				_customPrefabInstance = UnityEngine.Object.Instantiate(customPrefab, _rewardIcons[1].transform.parent);
				_customPrefabInstance.transform.SetAsFirstSibling();
			}
			else
			{
				_customPrefabInstance.UpdateActive(active: true);
			}
		}
		else if (_customPrefabInstance != null)
		{
			_customPrefabInstance.UpdateActive(active: false);
		}
	}

	public void ToggleAnimHover(string AnimString)
	{
		stateAnimator.SetTrigger(AnimString, !stateAnimator.GetBool(AnimString));
	}

	public void SetHover(bool enable)
	{
		_isHoverable = enable;
	}

	public void Button_OnPointerEnter(int tier)
	{
		if (_isHoverable && _rewards != null && tier < _rewards.Length && IsRewardsValid(tier))
		{
			_rewardIcons[tier].ActivatePopup();
		}
	}

	public void Button_OnPointerExit(int tier)
	{
		if ((!_isHoverable || (_rewards != null && tier < _rewards.Length)) && IsRewardsValid(tier))
		{
			_rewardIcons[tier].DeactivatePopup();
		}
	}

	public void Button_OnPointerClick(int tier)
	{
		if (hasEnhancements && tier == 0)
		{
			tier = 2;
		}
		this.Click?.Invoke(tier, _level);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
	}

	public void Button_ToggleMobilePopup(int tier)
	{
		if (_rewards != null && tier < _rewards.Length && IsRewardsValid(tier))
		{
			_rewardIcons[tier].ToggleMobilePopup();
		}
	}

	private bool IsRewardsValid(int tier)
	{
		if (!hasEnhancements || tier != 0)
		{
			return _rewards[tier] != null;
		}
		return _rewards[2] != null;
	}
}
