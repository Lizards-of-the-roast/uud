using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Event;

public class VersusLetterboxPayload : IPayload
{
	public Color Color;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
