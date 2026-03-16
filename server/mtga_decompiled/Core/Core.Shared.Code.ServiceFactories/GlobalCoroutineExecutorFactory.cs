using UnityEngine;

namespace Core.Shared.Code.ServiceFactories;

public class GlobalCoroutineExecutorFactory
{
	public static GlobalCoroutineExecutor Create()
	{
		GameObject gameObject = new GameObject("CoroutineExecutor");
		Object.DontDestroyOnLoad(gameObject);
		return gameObject.AddComponent<GlobalCoroutineExecutor>();
	}
}
