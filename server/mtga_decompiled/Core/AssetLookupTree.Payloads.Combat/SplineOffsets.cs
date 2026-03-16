using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Combat;

public class SplineOffsets : IPayload
{
	public Vector3 EndOffset;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
