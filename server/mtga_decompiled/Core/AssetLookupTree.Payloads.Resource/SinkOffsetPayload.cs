using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Resource;

public class SinkOffsetPayload : IPayload
{
	public Vector3 Offset = Vector3.zero;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
