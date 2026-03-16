using System;
using UnityEngine;

namespace AssetLookupTree;

[Serializable]
public class OffsetData
{
	public Vector3 PositionOffset;

	public Vector3 RotationOffset;

	public bool RotationIsWorld;

	public Vector3 ScaleMultiplier;

	public bool ScaleIsWorld;

	public OffsetData()
	{
		PositionOffset = Vector3.zero;
		RotationOffset = Vector3.zero;
		ScaleMultiplier = Vector3.one;
	}

	public OffsetData(Vector3? p = null, Vector3? r = null, Vector3? s = null)
	{
		PositionOffset = (p.HasValue ? p.Value : Vector3.zero);
		RotationOffset = (r.HasValue ? r.Value : Vector3.zero);
		ScaleMultiplier = (s.HasValue ? s.Value : Vector3.one);
	}
}
