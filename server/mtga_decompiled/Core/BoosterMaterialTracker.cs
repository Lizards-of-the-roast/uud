using System.Collections.Generic;
using UnityEngine;

public class BoosterMaterialTracker : MonoBehaviour
{
	private readonly List<Material> _trackedMaterials = new List<Material>();

	public void TrackMaterial(Material m)
	{
		_trackedMaterials.Add(m);
	}

	public void FreeMaterials()
	{
		foreach (Material trackedMaterial in _trackedMaterials)
		{
			BoosterPayloadUtilities.FreeBoosterMaterial(trackedMaterial);
		}
		_trackedMaterials.Clear();
	}

	public void OnDestroy()
	{
		FreeMaterials();
	}
}
