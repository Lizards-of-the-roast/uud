using System.Collections.Generic;
using AssetLookupTree;
using Core.Code.AssetLookupTree.AssetLookup;
using UnityEngine;
using Wizards.Mtga;

namespace Core.Meta.MainNavigation.Achievements.Scripts;

public class AchievementHubController : MonoBehaviour
{
	[SerializeField]
	private GameObject _claimableSection;

	[SerializeField]
	private Transform _claimableAchievementsSectionHolder;

	[SerializeField]
	private Transform _upNextAchievementsSectionHolder;

	private IAchievementManager _achievementManager;

	private AssetLookupSystem _lookupSystem;

	private string _groupPrefabPath;

	private string _achievementCardPrefabPath;

	private void Awake()
	{
		_achievementManager = Pantry.Get<IAchievementManager>();
		_lookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
	}

	private void Start()
	{
		_groupPrefabPath = AchievementsScreenHelperFunctions.GetPrefabPath(_lookupSystem, "AchievementGroup");
		_achievementCardPrefabPath = AchievementsScreenHelperFunctions.GetPrefabPath(_lookupSystem, "AchievementScreenCard");
		SetupNextUpSection();
	}

	private void SetupClaimableSection()
	{
		IReadOnlyList<IClientAchievementGroup> claimableAchievementGroups = _achievementManager.ClaimableAchievementGroups;
		_claimableSection.SetActive(claimableAchievementGroups.Count > 0);
		foreach (IClientAchievementGroup item in claimableAchievementGroups)
		{
			AssetLoader.Instantiate(_groupPrefabPath, _claimableAchievementsSectionHolder).GetComponent<AchievementGroupDisplay>().AssignAchievementGroup(item, onlyShowClaimable: true);
		}
	}

	private void SetupNextUpSection()
	{
		foreach (IClientAchievement upNextAchievement in _achievementManager.UpNextAchievements)
		{
			AchievementCard component = AssetLoader.Instantiate(_achievementCardPrefabPath, _upNextAchievementsSectionHolder).GetComponent<AchievementCard>();
			GameObject gameObject = component.gameObject;
			gameObject.name = gameObject.name + " (" + upNextAchievement?.Id.GraphId + "." + upNextAchievement?.Id.NodeId + ")";
			component.SetAchievementData(upNextAchievement, isUpNext: true);
		}
	}
}
