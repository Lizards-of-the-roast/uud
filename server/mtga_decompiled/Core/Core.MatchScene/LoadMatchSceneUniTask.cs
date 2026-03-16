using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Wizards.Mtga.Assets;

namespace Core.MatchScene;

public class LoadMatchSceneUniTask
{
	private MatchSceneManager.MatchSceneInitData initData;

	public LoadMatchSceneUniTask(MatchSceneManager.MatchSceneInitData initData)
	{
		this.initData = initData;
	}

	public async UniTask Load(CancellationToken token)
	{
		await Scenes.LoadSceneAsync("MatchScene", initData.loadMode).WithCancellation(token);
		Scene sceneByName = SceneManager.GetSceneByName("MatchScene");
		SceneManager.SetActiveScene(sceneByName);
		sceneByName.GetSceneComponent<MatchSceneManager>().Initialize(initData);
	}
}
