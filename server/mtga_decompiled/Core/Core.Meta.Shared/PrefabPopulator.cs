using UnityEngine;

namespace Core.Meta.Shared;

public abstract class PrefabPopulator : MonoBehaviour
{
	public virtual void Awake()
	{
		Populate();
	}

	public abstract void Populate();
}
