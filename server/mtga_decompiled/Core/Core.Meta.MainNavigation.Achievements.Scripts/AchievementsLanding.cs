using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Core.Meta.MainNavigation.Achievements.Scripts;

public class AchievementsLanding : MonoBehaviour
{
	[SerializeField]
	private Transform _achievementsHubParent;

	[SerializeField]
	private GameObject _achievementsHubScreen;

	public void LandingButtonClicked()
	{
		_achievementsHubParent.DestroyChildren();
		Object.Instantiate(_achievementsHubScreen, _achievementsHubParent);
	}
}
