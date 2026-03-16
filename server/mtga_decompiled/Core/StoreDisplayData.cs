using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "ScriptableObject/Store Display Data", fileName = "StoreDisplayData")]
public class StoreDisplayData : ScriptableObject
{
	public List<SkuDisplay> Bundles;

	public void CopyFrom(StoreDisplayData other)
	{
		Bundles = other.Bundles.Select((SkuDisplay item) => new SkuDisplay(item)).ToList();
	}
}
