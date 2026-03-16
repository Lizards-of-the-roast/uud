using System.Collections;
using System.Collections.Generic;
using MovementSystem;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Pooling;

public class UnityObjectPool : MonoBehaviour, IUnityObjectPool
{
	private class UnityPoolObject
	{
		private const uint DEFAULT_POOL_SIZE = uint.MaxValue;

		private const float DECAY_ACTIVITY_INCREMENT = 20f;

		private const float DECAY_DESTROY_INCREMENT = 3f;

		private const float DECAY_MAXIMUM = 90f;

		private readonly Stack<UnityPoolTag> _pool = new Stack<UnityPoolTag>();

		public float DecayTimeRemaining = 90f;

		public readonly uint MaxSize = uint.MaxValue;

		public Transform Root;

		public int PoolCount => _pool.Count;

		public bool PushObject(UnityPoolTag unityPoolTag)
		{
			if (_pool.Contains(unityPoolTag))
			{
				return true;
			}
			if (_pool.Count >= MaxSize)
			{
				return false;
			}
			_pool.Push(unityPoolTag);
			unityPoolTag.Pooled = true;
			unityPoolTag.Used = false;
			IncrementDecayTimer(decay: false);
			return true;
		}

		public bool PopObject(out UnityPoolTag unityPoolTag, bool decay = false)
		{
			unityPoolTag = null;
			if (_pool.Count == 0)
			{
				return false;
			}
			unityPoolTag = _pool.Pop();
			if ((object)unityPoolTag == null)
			{
				return false;
			}
			unityPoolTag.Pooled = false;
			unityPoolTag.Used = true;
			IncrementDecayTimer(decay);
			return true;
		}

		private void IncrementDecayTimer(bool decay)
		{
			DecayTimeRemaining += (decay ? 3f : 20f);
			if (DecayTimeRemaining > 90f)
			{
				DecayTimeRemaining = 90f;
			}
		}
	}

	public class UnityPoolTag : MonoBehaviour
	{
		public int PrefabTag;

		public string AssetPath = string.Empty;

		public bool Pooled;

		public bool Used;

		public bool DestroyablePool;

		private bool _isQuitting;

		private void SetQuitting()
		{
			_isQuitting = true;
		}

		private void Awake()
		{
			Application.quitting += SetQuitting;
		}

		private void OnDestroy()
		{
			Application.quitting -= SetQuitting;
			if (!_isQuitting && Application.isEditor)
			{
				if (Pooled && !DestroyablePool)
				{
					SimpleLog.LogError("Destroying object \"" + base.name + "\" while it's in the pool! This can result in NREs!\nHierarchy Path: " + base.gameObject.GetFullPath());
				}
				else
				{
					_ = Used;
				}
			}
		}
	}

	private ISplineMovementSystem _splineMovementSystem;

	private readonly Dictionary<int, UnityPoolObject> _gameObjectSourcePoolObjects = new Dictionary<int, UnityPoolObject>();

	private readonly Dictionary<string, UnityPoolObject> _assetPathSourcePoolObjects = new Dictionary<string, UnityPoolObject>();

	private bool _allowPooling = true;

	private bool _acceleratePurge;

	private bool _destroyable;

	public static UnityObjectPool CreatePool(string poolId, bool keepAlive, Transform poolParent, ISplineMovementSystem splineMovementSystem)
	{
		UnityObjectPool unityObjectPool = new GameObject("Pool_" + poolId).AddComponent<UnityObjectPool>();
		unityObjectPool.transform.SetParent(poolParent);
		unityObjectPool.gameObject.UpdateActive(active: true);
		unityObjectPool._splineMovementSystem = splineMovementSystem;
		unityObjectPool._destroyable = !keepAlive;
		if (keepAlive)
		{
			Object.DontDestroyOnLoad(unityObjectPool);
		}
		return unityObjectPool;
	}

	private void Awake()
	{
		Application.quitting += ApplicationOnQuitting;
		Application.lowMemory += ApplicationOnLowMemory;
	}

	private void OnDestroy()
	{
		Application.quitting -= ApplicationOnQuitting;
		Application.lowMemory -= ApplicationOnLowMemory;
		_allowPooling = false;
	}

	private void ApplicationOnLowMemory()
	{
		_allowPooling = false;
		_acceleratePurge = true;
	}

	private void ApplicationOnQuitting()
	{
		_allowPooling = false;
	}

	public GameObject PopObject(string assetPath)
	{
		return PopObject(assetPath, null);
	}

	public GameObject PopObject(string assetPath, Transform parent)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			return null;
		}
		if (_assetPathSourcePoolObjects.ContainsKey(assetPath))
		{
			UnityPoolObject unityPoolObject = EnsurePoolObject(assetPath);
			UnityPoolTag unityPoolTag;
			while (unityPoolObject != null && unityPoolObject.PopObject(out unityPoolTag))
			{
				if ((bool)unityPoolTag && (bool)unityPoolTag.gameObject)
				{
					if (parent != null)
					{
						unityPoolTag.transform.SetParent(parent, worldPositionStays: false);
					}
					return unityPoolTag.gameObject;
				}
				Debug.LogError("Null object in UnityObjectPool: \"" + unityPoolObject.Root.name + "\"");
			}
		}
		return CreateObjectInternal(assetPath, parent);
	}

	public GameObject PopObject(GameObject prefab)
	{
		return PopObject(prefab, null);
	}

	public GameObject PopObject(GameObject prefab, Transform parent)
	{
		if (!prefab)
		{
			return null;
		}
		int instanceID = prefab.GetInstanceID();
		if (_gameObjectSourcePoolObjects.ContainsKey(instanceID))
		{
			UnityPoolObject unityPoolObject = EnsurePoolObject(instanceID, prefab.name);
			UnityPoolTag unityPoolTag;
			while (unityPoolObject != null && unityPoolObject.PopObject(out unityPoolTag))
			{
				if ((bool)unityPoolTag && (bool)unityPoolTag.gameObject)
				{
					if (parent != null)
					{
						unityPoolTag.transform.SetParent(parent, worldPositionStays: false);
					}
					return unityPoolTag.gameObject;
				}
				Debug.LogError("Null object in UnityObjectPool: \"" + unityPoolObject.Root.name + "\"");
			}
		}
		return CreateObjectInternal(prefab, parent);
	}

	private IEnumerator Coroutine_WaitThenPushObject(GameObject instance, bool worldPositionStays)
	{
		yield return null;
		PushObject(instance, worldPositionStays);
	}

	public void PushObject(GameObject instance)
	{
		PushObject(instance, worldPositionStays: true);
	}

	public void PushObject(GameObject instance, bool worldPositionStays)
	{
		if (!instance)
		{
			return;
		}
		_splineMovementSystem?.RemovePermanentGoal(instance.transform);
		if (_allowPooling)
		{
			UnityPoolTag component = instance.GetComponent<UnityPoolTag>();
			if ((bool)component && PushObjectInternal(component, worldPositionStays))
			{
				return;
			}
		}
		DestroyObject(instance);
	}

	public void DeferredPushObject(GameObject instance)
	{
		StartCoroutine(Coroutine_WaitThenPushObject(instance, worldPositionStays: false));
	}

	public void Clear()
	{
		foreach (int key in _gameObjectSourcePoolObjects.Keys)
		{
			UnityPoolObject unityPoolObject = _gameObjectSourcePoolObjects[key];
			while (unityPoolObject.PoolCount > 0)
			{
				if (unityPoolObject.PopObject(out var unityPoolTag) && (bool)unityPoolTag)
				{
					unityPoolTag.Used = false;
					DestroyObject(unityPoolTag.gameObject);
				}
			}
			DestroyObject(unityPoolObject.Root.gameObject);
		}
		_gameObjectSourcePoolObjects.Clear();
		foreach (string key2 in _assetPathSourcePoolObjects.Keys)
		{
			UnityPoolObject unityPoolObject2 = _assetPathSourcePoolObjects[key2];
			while (unityPoolObject2.PoolCount > 0)
			{
				if (unityPoolObject2.PopObject(out var unityPoolTag2) && (bool)unityPoolTag2)
				{
					unityPoolTag2.Used = false;
					DestroyObject(unityPoolTag2.gameObject);
				}
			}
			DestroyObject(unityPoolObject2.Root.gameObject);
		}
		_assetPathSourcePoolObjects.Clear();
	}

	private void LateUpdate()
	{
		float num = Time.deltaTime * (_acceleratePurge ? 5f : 1f);
		foreach (KeyValuePair<int, UnityPoolObject> gameObjectSourcePoolObject in _gameObjectSourcePoolObjects)
		{
			if (gameObjectSourcePoolObject.Value.PoolCount != 0)
			{
				gameObjectSourcePoolObject.Value.DecayTimeRemaining -= num;
				if (!(gameObjectSourcePoolObject.Value.DecayTimeRemaining > 0f) && gameObjectSourcePoolObject.Value.PopObject(out var unityPoolTag, decay: true) && (bool)unityPoolTag)
				{
					unityPoolTag.Used = false;
					DestroyObject(unityPoolTag.gameObject);
				}
			}
		}
		foreach (KeyValuePair<string, UnityPoolObject> assetPathSourcePoolObject in _assetPathSourcePoolObjects)
		{
			if (assetPathSourcePoolObject.Value.PoolCount != 0)
			{
				assetPathSourcePoolObject.Value.DecayTimeRemaining -= num;
				if (!(assetPathSourcePoolObject.Value.DecayTimeRemaining > 0f) && assetPathSourcePoolObject.Value.PopObject(out var unityPoolTag2, decay: true) && (bool)unityPoolTag2)
				{
					unityPoolTag2.Used = false;
					DestroyObject(unityPoolTag2.gameObject);
				}
			}
		}
	}

	private GameObject CreateObjectInternal(GameObject prefab, Transform parent)
	{
		if (!prefab)
		{
			return null;
		}
		int instanceID = prefab.GetInstanceID();
		EnsurePoolObject(instanceID, prefab.name);
		GameObject gameObject = ((!(parent != null)) ? Object.Instantiate(prefab) : Object.Instantiate(prefab, parent, worldPositionStays: false));
		UnityPoolTag unityPoolTag = gameObject.AddComponent<UnityPoolTag>();
		unityPoolTag.PrefabTag = instanceID;
		unityPoolTag.DestroyablePool = _destroyable;
		return gameObject;
	}

	private GameObject CreateObjectInternal(string assetPath, Transform parent)
	{
		if (string.IsNullOrEmpty(assetPath))
		{
			return null;
		}
		EnsurePoolObject(assetPath);
		GameObject gameObject = AssetLoader.Instantiate(assetPath, parent);
		if ((bool)gameObject)
		{
			UnityPoolTag unityPoolTag = gameObject.AddComponent<UnityPoolTag>();
			unityPoolTag.AssetPath = assetPath;
			unityPoolTag.DestroyablePool = _destroyable;
		}
		else
		{
			Debug.LogErrorFormat("Failed to Intantiate asset with path {0}", assetPath);
		}
		return gameObject;
	}

	private bool PushObjectInternal(UnityPoolTag unityPoolTag, bool worldPositionStays)
	{
		UnityPoolObject unityPoolObject = EnsurePoolObject(unityPoolTag);
		if (unityPoolObject != null && unityPoolObject.PushObject(unityPoolTag))
		{
			unityPoolTag.transform.SetParent(unityPoolObject.Root, worldPositionStays);
			unityPoolTag.gameObject.UpdateActive(active: true);
			return true;
		}
		return false;
	}

	private UnityPoolObject EnsurePoolObject(UnityPoolTag unityPoolTag)
	{
		if (!unityPoolTag)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(unityPoolTag.AssetPath))
		{
			return EnsurePoolObject(unityPoolTag.AssetPath);
		}
		return EnsurePoolObject(unityPoolTag.PrefabTag, unityPoolTag.name);
	}

	private UnityPoolObject EnsurePoolObject(int instanceId, string objectName)
	{
		if (!_gameObjectSourcePoolObjects.TryGetValue(instanceId, out var value) || value == null)
		{
			value = (_gameObjectSourcePoolObjects[instanceId] = new UnityPoolObject());
		}
		if (!value.Root)
		{
			value.Root = CreateNewPoolRoot(instanceId, objectName);
			if (!value.Root)
			{
				_gameObjectSourcePoolObjects.Remove(instanceId);
				return null;
			}
		}
		return value;
		Transform CreateNewPoolRoot(int poolId, string poolName)
		{
			GameObject gameObject = new GameObject($"{poolId} ({poolName})");
			if (!gameObject)
			{
				return null;
			}
			gameObject.UpdateActive(active: false);
			gameObject.transform.SetParent(base.transform);
			return gameObject.transform;
		}
	}

	private UnityPoolObject EnsurePoolObject(string assetPath)
	{
		if (!_assetPathSourcePoolObjects.TryGetValue(assetPath, out var value) || value == null)
		{
			value = (_assetPathSourcePoolObjects[assetPath] = new UnityPoolObject());
		}
		if (!value.Root)
		{
			value.Root = CreateNewPoolRoot(assetPath);
			if (!value.Root)
			{
				_assetPathSourcePoolObjects.Remove(assetPath);
				return null;
			}
		}
		return value;
		Transform CreateNewPoolRoot(string path)
		{
			GameObject gameObject = new GameObject(path ?? "");
			if (!gameObject)
			{
				return null;
			}
			gameObject.UpdateActive(active: false);
			gameObject.transform.SetParent(base.transform);
			return gameObject.transform;
		}
	}

	public void Destroy()
	{
		if ((bool)this)
		{
			DestroyObject(base.gameObject);
		}
	}

	private void DestroyObject(GameObject instance)
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
