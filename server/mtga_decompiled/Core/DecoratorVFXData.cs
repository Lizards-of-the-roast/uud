using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;

public class DecoratorVFXData
{
	public float StartTime = 1f;

	public float CleanUpAfterSeconds = 4f;

	public List<AltAssetReference<GameObject>> Prefabs = new List<AltAssetReference<GameObject>>(1);
}
