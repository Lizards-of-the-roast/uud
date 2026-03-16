using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Text;

public class TextAssetPayload : IPayload
{
	public AltAssetReference<TextAsset> TextAssetReference = new AltAssetReference<TextAsset>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return TextAssetReference.RelativePath;
	}
}
