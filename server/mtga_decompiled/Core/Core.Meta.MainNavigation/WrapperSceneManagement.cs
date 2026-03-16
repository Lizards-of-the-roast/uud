using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Mtga;

namespace Core.Meta.MainNavigation;

public class WrapperSceneManagement
{
	private readonly List<WrapperSceneInformation> _loadedScenes = new List<WrapperSceneInformation>();

	private readonly GlobalCoroutineExecutor _globalCoroutineExecutor = Pantry.Get<GlobalCoroutineExecutor>();

	private readonly HashSet<string> _preSceneUnloadingReservations = new HashSet<string>();

	private readonly Dictionary<string, AsyncOperation> _sceneLoadingAsyncOperations = new Dictionary<string, AsyncOperation>();

	public bool AllScenesLoaded => _loadedScenes.All((WrapperSceneInformation x) => x.LoadingStatus == SceneLoadingStatus.Loaded || x.LoadingStatus == SceneLoadingStatus.Ready);

	public bool AllScenesReady => _loadedScenes.All((WrapperSceneInformation x) => x.LoadingStatus == SceneLoadingStatus.Ready);

	public event Action<WrapperSceneInformation> OnSceneLoading;

	public event Action<WrapperSceneInformation> OnSceneLoaded;

	public event Action<WrapperSceneInformation> OnSceneReady;

	public event Action OnAllScenesLoaded;

	public event Action OnAllScenesReady;

	public static WrapperSceneManagement Create()
	{
		return new WrapperSceneManagement();
	}

	public IEnumerator UnloadWrapper(WrapperSceneLifeCycle[] lifecyclesToNotUnload = null)
	{
		List<WrapperSceneInformation> scenesToUnload = ((lifecyclesToNotUnload == null || lifecyclesToNotUnload.Length == 0) ? new List<WrapperSceneInformation>(_loadedScenes) : new List<WrapperSceneInformation>(_loadedScenes).Where((WrapperSceneInformation x) => !lifecyclesToNotUnload.Contains(x.SceneLifeCycle)).ToList());
		while (scenesToUnload.Count > 0)
		{
			WrapperSceneInformation sceneInformationToUnload = scenesToUnload[0];
			scenesToUnload.RemoveAt(0);
			yield return UnloadScene(sceneInformationToUnload);
		}
	}

	public bool IsSceneLoaded(string sceneName)
	{
		if (_loadedScenes.Exists((WrapperSceneInformation x) => x.SceneName == sceneName))
		{
			return SceneManager.GetSceneByName(sceneName).IsValid();
		}
		return false;
	}

	public void LoadScene(WrapperSceneInformation sceneInformationToLoad)
	{
		_globalCoroutineExecutor.StartGlobalCoroutine(LoadScene_Coroutine(sceneInformationToLoad));
	}

	public IEnumerator LoadScene_Coroutine(WrapperSceneInformation sceneInformationToLoad)
	{
		if (!SceneManager.GetSceneByName(sceneInformationToLoad.SceneName).IsValid() && !_preSceneUnloadingReservations.Contains(sceneInformationToLoad.SceneName) && !_sceneLoadingAsyncOperations.ContainsKey(sceneInformationToLoad.SceneName))
		{
			_preSceneUnloadingReservations.Add(sceneInformationToLoad.SceneName);
			yield return UnloadUnnecessaryScenes(sceneInformationToLoad.SceneLifeCycle);
			_loadedScenes.Add(sceneInformationToLoad);
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneInformationToLoad.SceneName, LoadSceneMode.Additive);
			if (asyncOperation == null)
			{
				Debug.LogError("Error loading the scene: " + sceneInformationToLoad.SceneName + "\nAsync scene loading information from Unity's scene loader is null!");
				yield break;
			}
			asyncOperation.allowSceneActivation = false;
			_preSceneUnloadingReservations.Remove(sceneInformationToLoad.SceneName);
			_sceneLoadingAsyncOperations.Add(sceneInformationToLoad.SceneName, asyncOperation);
			_globalCoroutineExecutor.StartGlobalCoroutine(HandleSceneLoading(sceneInformationToLoad));
			yield return asyncOperation;
		}
	}

	private IEnumerator HandleSceneLoading(WrapperSceneInformation sceneInformation)
	{
		sceneInformation.LoadingStatus = SceneLoadingStatus.Loading;
		this.OnSceneLoading?.Invoke(sceneInformation);
		while (_sceneLoadingAsyncOperations[sceneInformation.SceneName].progress < 0.9f)
		{
			yield return null;
		}
		sceneInformation.LoadingStatus = SceneLoadingStatus.Loaded;
		this.OnSceneLoaded?.Invoke(sceneInformation);
		if (_loadedScenes.All((WrapperSceneInformation x) => sceneInformation.LoadingStatus == SceneLoadingStatus.Loaded || sceneInformation.LoadingStatus == SceneLoadingStatus.Ready))
		{
			this.OnAllScenesLoaded?.Invoke();
		}
		ISceneSetupTask[] array = SceneManager.GetSceneByName(sceneInformation.SceneName).GetRootGameObjects().SelectMany((GameObject x) => x.GetComponentsInChildren<ISceneSetupTask>(includeInactive: true))
			.ToArray();
		int taskCount = array.Length;
		for (int num = 0; num < taskCount; num++)
		{
			_globalCoroutineExecutor.StartGlobalCoroutine(TaskWrapper(array[num], delegate
			{
				taskCount--;
			}));
		}
		yield return new WaitUntil(() => taskCount == 0);
		_sceneLoadingAsyncOperations[sceneInformation.SceneName].allowSceneActivation = true;
		sceneInformation.LoadingStatus = SceneLoadingStatus.Ready;
		this.OnSceneReady?.Invoke(sceneInformation);
		_sceneLoadingAsyncOperations.Remove(sceneInformation.SceneName);
		if (_loadedScenes.All((WrapperSceneInformation x) => x.LoadingStatus == SceneLoadingStatus.Ready))
		{
			this.OnAllScenesReady?.Invoke();
		}
	}

	private IEnumerator TaskWrapper(ISceneSetupTask task, Action onComplete)
	{
		yield return new WaitUntil(() => task.SetupCompleted);
		onComplete?.Invoke();
	}

	private IEnumerator UnloadScene(WrapperSceneInformation sceneInformationToUnload)
	{
		if (_loadedScenes.Exists((WrapperSceneInformation x) => x.SceneName.Equals(sceneInformationToUnload.SceneName, StringComparison.InvariantCulture)))
		{
			_loadedScenes.RemoveAll((WrapperSceneInformation x) => x.SceneName.Equals(sceneInformationToUnload.SceneName, StringComparison.InvariantCulture));
			if (SceneManager.GetSceneByName(sceneInformationToUnload.SceneName).isLoaded)
			{
				yield return SceneManager.UnloadSceneAsync(sceneInformationToUnload.SceneName);
			}
		}
	}

	public IEnumerator UnloadUnnecessaryScenes(WrapperSceneLifeCycle sceneLifecycleBeingLoaded)
	{
		WrapperSceneInformation[] array = null;
		switch (sceneLifecycleBeingLoaded)
		{
		case WrapperSceneLifeCycle.Wrapper:
			array = _loadedScenes.Where((WrapperSceneInformation x) => x.SceneLifeCycle != WrapperSceneLifeCycle.LoggedIn).ToArray();
			break;
		case WrapperSceneLifeCycle.IndividualPage:
			array = _loadedScenes.Where(delegate(WrapperSceneInformation x)
			{
				WrapperSceneLifeCycle sceneLifeCycle = x.SceneLifeCycle;
				return sceneLifeCycle == WrapperSceneLifeCycle.IndividualPage || sceneLifeCycle == WrapperSceneLifeCycle.SubPage;
			}).ToArray();
			break;
		case WrapperSceneLifeCycle.SubPage:
		case WrapperSceneLifeCycle.Temporary:
		case WrapperSceneLifeCycle.LoggedIn:
			array = null;
			break;
		default:
			Debug.LogError("Trying to populate with an unqualified lifecycle");
			break;
		}
		if (array != null)
		{
			WrapperSceneInformation[] array2 = array;
			foreach (WrapperSceneInformation sceneInformationToUnload in array2)
			{
				yield return UnloadScene(sceneInformationToUnload);
			}
		}
	}
}
