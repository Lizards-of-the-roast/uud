using HasbroGo.Helpers;
using HasbroGo.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HasbroGo;

public class Bootstrap : MonoBehaviour
{
	[Header("Core Dependencies")]
	[SerializeField]
	private GameObject[] dependencyPrefabsToInstantiate;

	[Header("Scene Data")]
	[SerializeField]
	private string sceneToLoadAfterBootstrap;

	private void Awake()
	{
		GameObject[] array = dependencyPrefabsToInstantiate;
		for (int i = 0; i < array.Length; i++)
		{
			Object.Instantiate(array[i]);
		}
		if (string.IsNullOrEmpty(sceneToLoadAfterBootstrap))
		{
			LogHelper.Logger.Log(LogLevel.Error, "Bootstrap prefab is missing a scene to load after completion. ");
		}
		else
		{
			SceneManager.LoadScene(sceneToLoadAfterBootstrap);
		}
	}
}
