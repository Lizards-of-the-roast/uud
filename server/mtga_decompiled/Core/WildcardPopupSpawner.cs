using UnityEngine;

public class WildcardPopupSpawner : MonoBehaviour
{
	public void ShowWildcardPopup()
	{
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		(sceneLoader.CurrentNavContent as HomePageContentController)?.ObjectivesPanel?.CloseAllObjectivePopups();
		sceneLoader.GetWildcardPopup().Activate(activate: true);
	}
}
