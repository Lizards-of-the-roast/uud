using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Assets;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class DuelSceneDebugLauncher : MonoBehaviour
{
	[SerializeField]
	private Button _exitButton;

	private void Awake()
	{
		_exitButton.onClick.AddListener(ExitScene);
	}

	private void OnDestroy()
	{
		_exitButton.onClick.RemoveListener(ExitScene);
	}

	private void ExitScene()
	{
		PAPA objOfType2;
		if (TryFindObjectOfType<Bootstrap>(out var objOfType))
		{
			objOfType.RestartGame("Exit Debug Launcher");
		}
		else if (TryFindObjectOfType<PAPA>(out objOfType2))
		{
			objOfType2.ShutdownImmediate();
			Scenes.LoadScene("Bootstrap");
		}
	}

	private static bool TryFindObjectOfType<T>(out T objOfType) where T : MonoBehaviour
	{
		objOfType = Object.FindObjectOfType<T>();
		return objOfType != null;
	}
}
