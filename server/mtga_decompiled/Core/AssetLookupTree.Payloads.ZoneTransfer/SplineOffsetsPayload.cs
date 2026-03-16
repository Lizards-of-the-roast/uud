using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.ZoneTransfer;

public class SplineOffsetsPayload : IPayload
{
	public Vector3 StartOffset = Vector3.zero;

	public Vector3 EndOffset = Vector3.zero;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
