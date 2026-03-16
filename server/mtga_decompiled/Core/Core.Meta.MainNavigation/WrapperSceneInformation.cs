namespace Core.Meta.MainNavigation;

public class WrapperSceneInformation
{
	private readonly string _sceneName;

	private readonly WrapperSceneLifeCycle _sceneLifeCycle;

	private SceneLoadingStatus _loadingStatus;

	public string SceneName => _sceneName;

	public WrapperSceneLifeCycle SceneLifeCycle => _sceneLifeCycle;

	public SceneLoadingStatus LoadingStatus
	{
		get
		{
			return _loadingStatus;
		}
		set
		{
			_loadingStatus = value;
		}
	}

	public WrapperSceneInformation(string sceneName, WrapperSceneLifeCycle sceneLifeCycle)
	{
		_sceneName = sceneName;
		_sceneLifeCycle = sceneLifeCycle;
		_loadingStatus = SceneLoadingStatus.Unknown;
	}
}
