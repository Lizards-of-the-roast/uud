using UnityEngine;
using UnityEngine.SceneManagement;

namespace HasbroGo;

public class SceneLoader : MonoBehaviour
{
	public void LoadScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
}
