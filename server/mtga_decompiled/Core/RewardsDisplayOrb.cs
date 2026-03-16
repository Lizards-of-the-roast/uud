using Core.MainNavigation.RewardTrack;
using UnityEngine;
using Wizards.Mtga;

public class RewardsDisplayOrb : MonoBehaviour
{
	public void OnMasteryTreeButtonPressed()
	{
		string currentBpName = Pantry.Get<SetMasteryDataProvider>().CurrentBpName;
		SceneLoader.GetSceneLoader()?.GetRewardsContentController()?.Clear();
		SceneLoader.GetSceneLoader().GoToRewardTreeScene(new RewardTreePageContext(currentBpName, null, null, NavContentType.RewardTrack));
	}
}
