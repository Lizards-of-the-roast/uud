using System;
using System.Collections.Generic;
using UnityEngine;

public class AssetCache<T> : IDisposable where T : UnityEngine.Object
{
	private readonly Dictionary<string, T> _cache = new Dictionary<string, T>();

	private readonly AssetLoader.AssetTracker<T> _tracker = new AssetLoader.AssetTracker<T>(typeof(T).FullName + " Cache");

	public T Get(string path)
	{
		if (_cache.ContainsKey(path))
		{
			T val = _cache[path];
			if (val != null)
			{
				return val;
			}
		}
		T val2 = _tracker.Acquire(path);
		_cache[path] = val2;
		return val2;
	}

	public void Dispose()
	{
		_cache.Clear();
		_tracker.Cleanup();
	}
}
