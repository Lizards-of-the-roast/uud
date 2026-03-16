using System.Collections.Generic;

namespace AssetLookupTree.Payloads.UI.NPE;

public class NPECharacterLibraryPayload : IPayload
{
	public readonly AltAssetReference<CharacterLibrary> CharacterLibraryRef = new AltAssetReference<CharacterLibrary>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return CharacterLibraryRef.RelativePath;
	}
}
