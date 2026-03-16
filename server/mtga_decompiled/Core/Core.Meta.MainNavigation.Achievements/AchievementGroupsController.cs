using System.Collections.Generic;
using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga;
using Wotc.Mtga.Extensions;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementGroupsController : MonoBehaviour
{
	[SerializeField]
	private RectTransform _groupParent;

	[SerializeField]
	private GameObject _achievementPrefabHub;

	private IEnumerable<IClientAchievementGroup> _achievementGroups;

	private string _hubPrefabPath;

	private string _groupPrefabPath;

	private List<GameObject> _achievementGroupDisplays;

	private void Awake()
	{
		AssetLookupSystem assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_hubPrefabPath = AchievementsScreenHelperFunctions.GetPrefabPath(assetLookupSystem, "AchievementHub");
		_groupPrefabPath = AchievementsScreenHelperFunctions.GetPrefabPath(assetLookupSystem, "AchievementGroup");
	}

	public AchievementDeeplinkingCalculations InitializeWithDeeplinkedAchievement(IClientAchievement achievement)
	{
		AchievementDeeplinkingCalculations achievementDeeplinkingCalculations = new AchievementDeeplinkingCalculations();
		achievementDeeplinkingCalculations.GetTotalYdistance = () => GetComponent<RectTransform>().rect.height + (float)GetComponent<VerticalLayoutGroup>().padding.top;
		foreach (GameObject achievementGroupDisplay in _achievementGroupDisplays)
		{
			AchievementGroupDisplay component = achievementGroupDisplay.GetComponent<AchievementGroupDisplay>();
			achievementDeeplinkingCalculations.YdistanceOfTargetCard += achievementGroupDisplay.GetComponent<RectTransform>().rect.height;
			if (achievement.AchievementGroup.GroupId.Equals(component.GetAchievementGroup().GroupId))
			{
				achievementDeeplinkingCalculations = component.ShowDeeplinkedAchievement(achievement, achievementDeeplinkingCalculations);
				break;
			}
		}
		return achievementDeeplinkingCalculations;
	}

	public void InitializeWithSet(IEnumerable<IClientAchievementGroup> achievementGroups)
	{
		_achievementGroups = achievementGroups;
		if (_achievementGroups == null)
		{
			_groupParent.DestroyChildren();
			AssetLoader.Instantiate(_hubPrefabPath, _groupParent);
		}
		else
		{
			PopulateGroups();
		}
	}

	private void PopulateGroups()
	{
		_groupParent.DestroyChildren();
		_achievementGroupDisplays = new List<GameObject>();
		foreach (IClientAchievementGroup achievementGroup in _achievementGroups)
		{
			GameObject gameObject = AssetLoader.Instantiate(_groupPrefabPath, _groupParent);
			_achievementGroupDisplays.Add(gameObject);
			gameObject.GetComponent<AchievementGroupDisplay>().AssignAchievementGroup(achievementGroup);
		}
	}

	public void ClearAchievementsContent()
	{
		_groupParent.DestroyChildren();
	}
}
