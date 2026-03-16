using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Wizards.GeneralUtilities.Object_Pooling_Scroll_Rect;
using Wizards.Mtga;

namespace Core.Meta.MainNavigation.Achievements;

public class PopulateSetSelection : MonoBehaviour, IRecyclableScrollRectDataSource
{
	[SerializeField]
	private RecyclableScrollRect _scrollRect;

	[SerializeField]
	private AchievementGroupsController _achievementGroupsController;

	private IClientAchievement _achievementToInitTo;

	private ScrollRect _achievementGroupScrollRect;

	private IAchievementManager _achievementManager;

	private void Awake()
	{
		_achievementManager = Pantry.Get<IAchievementManager>();
	}

	private void Start()
	{
		_scrollRect.Initialize(this);
		SelectSet();
	}

	private void OnEnable()
	{
		if (_scrollRect.content.childCount != 0)
		{
			_scrollRect.verticalNormalizedPosition = 0f;
			SelectSet();
		}
	}

	private void SelectSet()
	{
		if (_achievementToInitTo == null)
		{
			_scrollRect.content.GetChild(0).GetComponent<AchievementSetItem>().SelectSet(overrideCurrentlySelectedCheck: true);
			return;
		}
		AchievementDeeplinkingCalculations achievementDeeplinkingCalculations = new AchievementDeeplinkingCalculations();
		foreach (Transform item in _scrollRect.content)
		{
			AchievementSetItem component = item.GetComponent<AchievementSetItem>();
			if (!(component == null))
			{
				achievementDeeplinkingCalculations = component.SelectSetIfItsTheRightAchievement(_achievementToInitTo);
				if (achievementDeeplinkingCalculations.CheckIfAnimationDone != null && achievementDeeplinkingCalculations.YdistanceOfTargetCard != 0f && achievementDeeplinkingCalculations.GetTotalYdistance != null && achievementDeeplinkingCalculations.GetYDistanceOfTargetCardInImmediateParent != null)
				{
					break;
				}
			}
		}
		StartCoroutine(snapScrollRectToSpecificAchievement(achievementDeeplinkingCalculations));
		_achievementToInitTo = null;
	}

	public void InitToSpecificAchievement(IClientAchievement achievement, ScrollRect achievementsGroupScrollRect)
	{
		_achievementToInitTo = achievement;
		_achievementGroupScrollRect = achievementsGroupScrollRect;
	}

	private IEnumerator snapScrollRectToSpecificAchievement(AchievementDeeplinkingCalculations calculations)
	{
		yield return new WaitUntil(calculations.CheckIfAnimationDone);
		float num = calculations.GetYDistanceOfTargetCardInImmediateParent();
		calculations.YdistanceOfTargetCard += num;
		float num2 = calculations.GetTotalYdistance();
		float y = 1f - calculations.YdistanceOfTargetCard / (num2 - (float)Screen.height);
		_achievementGroupScrollRect.normalizedPosition = new Vector2(0f, y);
	}

	public int GetItemCount()
	{
		return _achievementManager.AchievementSets.Count() + 1;
	}

	public void SetCell(ICell cell, int index)
	{
		AchievementSetItem achievementSetItem = cell as AchievementSetItem;
		if (!(achievementSetItem == null))
		{
			achievementSetItem.ConfigureCell((index == 0) ? null : _achievementManager.AchievementSets.ElementAt(index - 1), _achievementGroupsController);
		}
	}
}
