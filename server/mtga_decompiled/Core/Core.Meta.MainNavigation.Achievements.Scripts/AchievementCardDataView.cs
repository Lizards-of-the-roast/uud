using UnityEngine;

namespace Core.Meta.MainNavigation.Achievements.Scripts;

public abstract class AchievementCardDataView : MonoBehaviour
{
	protected IClientAchievement _achievementData;

	private IClientAchievement _lastUiUpdatedAchievementData;

	public void AssignAchievementData(IClientAchievement clientAchievement)
	{
		_achievementData = clientAchievement;
		if (base.gameObject.activeInHierarchy)
		{
			CardViewUpdate();
		}
	}

	protected virtual void OnEnable()
	{
		if (_achievementData != null && (_lastUiUpdatedAchievementData == null || _lastUiUpdatedAchievementData != _achievementData))
		{
			_lastUiUpdatedAchievementData = _achievementData;
			CardViewUpdate();
		}
	}

	protected abstract void CardViewUpdate();
}
