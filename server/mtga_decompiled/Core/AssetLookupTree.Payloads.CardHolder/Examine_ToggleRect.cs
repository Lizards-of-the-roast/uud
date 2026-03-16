using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.CardHolder;

public class Examine_ToggleRect : IPayload
{
	public Vector3 Position;

	public float Width;

	public IEnumerable<string> GetFilePaths()
	{
		return Array.Empty<string>();
	}
}
