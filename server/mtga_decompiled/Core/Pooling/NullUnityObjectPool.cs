using System.Collections;
using UnityEngine;

namespace Pooling;

public sealed class NullUnityObjectPool : IUnityObjectPool
{
	public static readonly IUnityObjectPool Default = new NullUnityObjectPool();

	GameObject IUnityObjectPool.PopObject(GameObject prefab)
	{
		if (!prefab)
		{
			return null;
		}
		return Object.Instantiate(prefab);
	}

	GameObject IUnityObjectPool.PopObject(GameObject prefab, Transform parent)
	{
		if (!prefab)
		{
			return null;
		}
		return Object.Instantiate(prefab, parent);
	}

	GameObject IUnityObjectPool.PopObject(string assetPath)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			return null;
		}
		return AssetLoader.Instantiate(assetPath);
	}

	GameObject IUnityObjectPool.PopObject(string assetPath, Transform parent)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			return null;
		}
		return AssetLoader.Instantiate(assetPath, parent);
	}

	public void PushObject(GameObject instance)
	{
		if ((bool)instance)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(instance);
			}
			else
			{
				Object.DestroyImmediate(instance);
			}
		}
	}

	public void PushObject(GameObject instance, bool worldPositionStays)
	{
		PushObject(instance);
	}

	private IEnumerator Coroutine_WaitThenPushObject(GameObject instance)
	{
		yield return null;
		PushObject(instance);
	}

	public void DeferredPushObject(GameObject instance)
	{
		if (Application.isPlaying)
		{
			PAPA.StartGlobalCoroutine(Coroutine_WaitThenPushObject(instance));
		}
		else
		{
			PushObject(instance);
		}
	}

	public void Clear()
	{
	}

	public void Destroy()
	{
	}
}
