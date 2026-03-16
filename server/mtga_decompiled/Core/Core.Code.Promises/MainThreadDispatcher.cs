using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Core.Code.Promises;

public class MainThreadDispatcher : MonoBehaviour
{
	private static MainThreadDispatcher _instance;

	private ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

	public static MainThreadDispatcher Instance => _instance;

	private void Awake()
	{
		if (_instance != null)
		{
			Debug.LogWarning("Tried to perform double-initialization of MainThreadDispatcher: ignoring second instance.");
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			_instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void Update()
	{
		Action result;
		while (_actions.TryDequeue(out result))
		{
			result?.Invoke();
		}
	}

	public void Add(Action action)
	{
		_actions.Enqueue(action);
	}

	public void Shutdown()
	{
		_actions.Clear();
		_instance = null;
		UnityEngine.Object.Destroy(this);
	}

	public static void Dispatch(Action action)
	{
		Instance.Add(action);
	}
}
