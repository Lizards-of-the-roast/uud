using System;
using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

[Serializable]
public class TargetingColorPayload : IPayload
{
	public Color Color;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
