using Core.BI;
using Cysharp.Threading.Tasks;
using Wizards.Mtga;
using Wizards.Mtga.Assets;

namespace Core.Code.SceneUtils;

public class LoadSceneUniTask
{
	private string _scene;

	public LoadSceneUniTask(string scene)
	{
		_scene = scene;
	}

	public async UniTask Load()
	{
		BIEventType.SceneLoadStart.SendWithDefaults(("Scene", _scene));
		await Scenes.LoadSceneAsync(_scene);
		BIEventType.SceneLoadEnd.SendWithDefaults(("Scene", _scene));
	}
}
