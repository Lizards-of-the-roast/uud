using System.Collections.Generic;
using AssetLookupTree.Payloads.Helpers;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class InfoHanger : ILayeredPayload, IPayload
{
	public enum TextFormatting
	{
		None,
		RemoveLoyaltyValue
	}

	public readonly AltAssetReference<Sprite> BadgeRef = new AltAssetReference<Sprite>();

	public ClientOrGreLocKey HeaderLocKey = new ClientOrGreLocKey();

	public ClientOrGreLocKey BodyLocKey = new ClientOrGreLocKey();

	public ClientOrGreLocKey AddendumLocKey = new ClientOrGreLocKey();

	public TextFormatting Formatting;

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return BadgeRef.RelativePath;
	}
}
