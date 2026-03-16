using System.Collections.Generic;

namespace AssetLookupTree.Payloads.CardHolder;

public class CardHolder_StaticVfx : IPayload
{
	public VfxPrefabData VfxPrefabData = new VfxPrefabData();

	public OffsetData OffsetData = new OffsetData();

	public IEnumerable<string> GetFilePaths()
	{
		int i = 0;
		while (i < VfxPrefabData.AllPrefabs.Count)
		{
			yield return VfxPrefabData.AllPrefabs[i].RelativePath;
			int num = i + 1;
			i = num;
		}
	}
}
