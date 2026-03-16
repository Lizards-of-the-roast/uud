using AssetLookupTree;
using UnityEngine;

namespace Core.Meta.MainNavigation.EventPageV2;

public class ContainerProxy : MonoBehaviour
{
	public Transform Root;

	private Transform _instance;

	public T SetInstance<T>(AltAssetReference<T> assetReference) where T : Component
	{
		Clear();
		if (assetReference == null)
		{
			return null;
		}
		T val = AssetLoader.Instantiate(assetReference, Root);
		_instance = val.transform;
		return val;
	}

	private void Clear()
	{
		if (_instance != null)
		{
			Object.Destroy(_instance.gameObject);
			_instance = null;
		}
	}
}
