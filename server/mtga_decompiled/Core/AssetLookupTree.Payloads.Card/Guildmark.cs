using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class Guildmark : IPayload
{
	public readonly AltAssetReference<Texture2D> SymbolRef = new AltAssetReference<Texture2D>();

	public readonly AltAssetReference<CardColorTable> ColorTableRef = new AltAssetReference<CardColorTable>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return SymbolRef.RelativePath;
		yield return ColorTableRef.RelativePath;
	}
}
