using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.ZoneTransfer;

public class CreateCard_OriginOffsets : IPayload
{
	public CardHolderType HolderType = CardHolderType.None;

	public Vector3 PositionOffset = Vector3.zero;

	public Vector3 RotationOffset = Vector3.zero;

	public Vector3 ScaleMultiplier = Vector3.one;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
