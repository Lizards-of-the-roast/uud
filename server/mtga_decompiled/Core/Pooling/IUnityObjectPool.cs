using UnityEngine;

namespace Pooling;

public interface IUnityObjectPool
{
	GameObject PopObject(GameObject prefab);

	GameObject PopObject(GameObject prefab, Transform parent);

	GameObject PopObject(string assetPath);

	GameObject PopObject(string assetPath, Transform parent);

	void PushObject(GameObject instance);

	void PushObject(GameObject instance, bool worldPositionStays);

	void DeferredPushObject(GameObject instance);

	void Clear();

	void Destroy();
}
