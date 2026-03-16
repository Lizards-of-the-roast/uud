using System.Collections.Generic;

namespace AssetLookupTree.Payloads.ZoneTransfer;

public class SustainVFX : IPayload
{
	public VfxPrefabData PrefabData = new VfxPrefabData();

	public IEnumerable<string> GetFilePaths()
	{
		int i = 0;
		while (i < PrefabData.AllPrefabs.Count)
		{
			yield return PrefabData.AllPrefabs[i].RelativePath;
			int num = i + 1;
			i = num;
		}
	}
}
