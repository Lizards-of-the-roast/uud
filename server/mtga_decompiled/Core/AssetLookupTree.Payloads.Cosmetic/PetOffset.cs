using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Cosmetic;

public class PetOffset : IPayload
{
	public OffsetData Offset = new OffsetData();

	public bool OverrideMobileBattlefieldDimensions;

	public Vector2 MobileBattlefieldDimensions;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
