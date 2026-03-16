using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wizards.Mtga;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementsContentController : NavContentController
{
	[FormerlySerializedAs("groupController")]
	[SerializeField]
	private AchievementGroupsController _groupController;

	[SerializeField]
	private PopulateSetSelection _setSelectionPopulator;

	[SerializeField]
	private ScrollRect _achievementGroupScrollRect;

	private AchievementsScreenWrapperCompassGuide _compassGuide;

	public override NavContentType NavContentType => NavContentType.Achievements;

	private void Awake()
	{
		_compassGuide = Pantry.Get<WrapperCompass>().GetGuide<AchievementsScreenWrapperCompassGuide>();
	}

	protected override void Start()
	{
		base.Start();
		if (_compassGuide != null)
		{
			GoToSpecificAchievement(_compassGuide.Achievement);
		}
	}

	private void OnDisable()
	{
		_groupController.ClearAchievementsContent();
	}

	public void GoToSpecificAchievement(IClientAchievement achievement)
	{
		_setSelectionPopulator.InitToSpecificAchievement(achievement, _achievementGroupScrollRect);
	}
}
