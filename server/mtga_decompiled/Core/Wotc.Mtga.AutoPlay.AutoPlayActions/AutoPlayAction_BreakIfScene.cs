using UnityEngine.SceneManagement;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.AutoPlay.AutoPlayActions;

public class AutoPlayAction_BreakIfScene : AutoPlayAction
{
	private string _sceneName;

	protected override void OnInitialize(in string[] parameters, int index)
	{
		_sceneName = AutoPlayAction.FromParameter(in parameters, index + 1);
		Timeout = AutoPlayAction.FromParameter(in parameters, index + 2)?.IntoFloat() ?? Timeout;
	}

	protected override void OnUpdate()
	{
		if (!base.IsComplete)
		{
			Scene sceneByName = SceneManager.GetSceneByName(_sceneName);
			if (sceneByName.IsValid() && sceneByName.isLoaded)
			{
				Break("BreakIfScene: Breaking subactions, " + sceneByName.name + " is loaded.");
			}
			else
			{
				Complete("BreakIfScene: Continue, " + _sceneName + " not loaded");
			}
		}
	}
}
