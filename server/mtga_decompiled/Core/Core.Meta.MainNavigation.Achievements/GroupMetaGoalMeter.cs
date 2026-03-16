using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.GeneralUtilities;
using Wizards.GeneralUtilities.Extensions;
using Wotc.Mtga.Extensions;

namespace Core.Meta.MainNavigation.Achievements;

public class GroupMetaGoalMeter : UIBehaviour
{
	private enum RewardBubbleState
	{
		UnClaimed,
		Claimable,
		Claimed
	}

	private static readonly int _rewardBubbleState = Animator.StringToHash("AchievementBubble");

	[SerializeField]
	private GameObject _milestoneRewardBubbleIndicator;

	[SerializeField]
	private GameObject _noRewardMilestoneIndicator;

	[SerializeField]
	private RectTransform _milestoneParent;

	[SerializeField]
	private Image _meterFillBar;

	[SerializeField]
	[Range(0f, 1f)]
	[AdditionalInformation("The minimum fill amount for the meter when there is no achievements assigned.")]
	private float _minimumFillAmount;

	[SerializeField]
	[AnimationCurveExpanded]
	[AnimationCurveRange(0f, 0f, float.PositiveInfinity, 1f, false)]
	private AnimationCurve _fillAnimation = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Coroutine _layoutWait;

	private Coroutine _meterFillingCoroutine;

	private List<RectTransform> _upcomingRewardBubbles;

	private IClientAchievementGroup _achievementGroup;

	protected override void OnEnable()
	{
		if (_achievementGroup == null)
		{
			return;
		}
		foreach (IClientAchievement achievement in _achievementGroup.Achievements)
		{
			achievement.OnRewardClaimed += UpdateUI;
		}
		UpdateUI();
	}

	private void PopulateMeterWithMilestones()
	{
		if (_achievementGroup == null)
		{
			return;
		}
		_milestoneParent.DestroyChildren(immediate: true);
		for (int i = 1; i <= _achievementGroup.Achievements.Count; i++)
		{
			(int, IAchievementGroupReward) groupReward;
			bool flag = _achievementGroup.TryGetAchievementGroupReward(i, out groupReward);
			AchievementGroupMeterMilestone component = Object.Instantiate(flag ? _milestoneRewardBubbleIndicator : _noRewardMilestoneIndicator, _milestoneParent).GetComponent<AchievementGroupMeterMilestone>();
			if (component == null)
			{
				Debug.LogError("There was no achievement milestone script found! This should always be present.", component);
				break;
			}
			component.SetRewardItem(groupReward.Item2, i);
			if (!flag)
			{
				continue;
			}
			Animator animator = component.Animator;
			if (!(animator == null))
			{
				if (_achievementGroup.ClaimedAchievementCount >= i)
				{
					animator.SetInteger(_rewardBubbleState, 2);
				}
				else if (_achievementGroup.ClaimedAchievementCount + _achievementGroup.ClaimableAchievementCount >= i)
				{
					animator.SetInteger(_rewardBubbleState, 1);
				}
				else
				{
					animator.SetInteger(_rewardBubbleState, 0);
				}
			}
		}
	}

	internal void AssignAchievementGroup(IClientAchievementGroup achievementGroup)
	{
		if (_achievementGroup != null)
		{
			foreach (IClientAchievement achievement in _achievementGroup.Achievements)
			{
				achievement.OnRewardClaimed -= UpdateUI;
			}
		}
		_achievementGroup = achievementGroup;
		PopulateMeterWithMilestones();
	}

	public void UpdateUI()
	{
		if (_achievementGroup != null)
		{
			if (_layoutWait != null)
			{
				StopCoroutine(_layoutWait);
			}
			_layoutWait = StartCoroutine(ForceLayoutAndWaitForEndOfFrame());
		}
	}

	private IEnumerator ForceLayoutAndWaitForEndOfFrame()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.gameObject.transform);
		yield return new WaitForEndOfFrame();
		SetAchievementBarProgress();
	}

	private void SetAchievementBarProgress()
	{
		if (_milestoneParent.childCount == 0 || _achievementGroup.ClaimedAchievementCount <= 0)
		{
			_meterFillBar.fillAmount = 0f;
			return;
		}
		float startX = _milestoneParent.rect.width * _meterFillBar.fillAmount;
		float x = ((RectTransform)_milestoneParent.GetChild(_achievementGroup.ClaimedAchievementCount - 1).transform).anchoredPosition.x;
		_upcomingRewardBubbles = GetRewardsInRange(startX, x);
		if (_meterFillingCoroutine != null)
		{
			StopCoroutine(_meterFillingCoroutine);
		}
		_meterFillingCoroutine = StartCoroutine(FillTheMeter(x / _milestoneParent.rect.width));
	}

	private IEnumerator FillTheMeter(float endFillAmount)
	{
		float startingValue = _meterFillBar.fillAmount;
		float runningTime = 0f;
		while (_meterFillBar.fillAmount < endFillAmount)
		{
			runningTime += Time.deltaTime;
			_meterFillBar.fillAmount = Mathf.Lerp(startingValue, endFillAmount, runningTime / _fillAnimation.AnimationTime()) * _fillAnimation.Evaluate(runningTime);
			CheckRewardBubbles();
			yield return null;
		}
		_meterFillBar.fillAmount = endFillAmount;
		MeterAnimationComplete();
	}

	private List<RectTransform> GetRewardsInRange(float startX, float endX)
	{
		List<RectTransform> list = new List<RectTransform>();
		for (int i = 0; i < _milestoneParent.childCount; i++)
		{
			RectTransform rectTransform = _milestoneParent.GetChild(i) as RectTransform;
			if (rectTransform != null && rectTransform.anchoredPosition.x > startX && rectTransform.anchoredPosition.x <= endX)
			{
				list.Add(rectTransform);
			}
		}
		return list;
	}

	private void CheckRewardBubbles()
	{
		float currentMeterX = _milestoneParent.rect.width * _meterFillBar.fillAmount;
		foreach (RectTransform upcomingRewardBubble in _upcomingRewardBubbles)
		{
			if (currentMeterX >= upcomingRewardBubble.anchoredPosition.x)
			{
				Animator animator = upcomingRewardBubble.GetComponent<AchievementGroupMeterMilestone>().Animator;
				if (animator != null)
				{
					animator.SetInteger(_rewardBubbleState, 2);
				}
			}
		}
		_upcomingRewardBubbles.RemoveAll((RectTransform bubble) => currentMeterX >= bubble.anchoredPosition.x);
	}

	private void MeterAnimationComplete()
	{
		_upcomingRewardBubbles.Clear();
	}

	protected override void OnDisable()
	{
		if (_layoutWait != null)
		{
			StopCoroutine(_layoutWait);
		}
		_layoutWait = null;
		if (_meterFillingCoroutine != null)
		{
			StopCoroutine(_meterFillingCoroutine);
		}
		_meterFillingCoroutine = null;
		if (_achievementGroup == null)
		{
			return;
		}
		foreach (IClientAchievement achievement in _achievementGroup.Achievements)
		{
			achievement.OnRewardClaimed -= UpdateUI;
		}
	}

	protected override void OnDestroy()
	{
		if (_achievementGroup == null)
		{
			return;
		}
		foreach (IClientAchievement achievement in _achievementGroup.Achievements)
		{
			achievement.OnRewardClaimed -= UpdateUI;
		}
	}
}
