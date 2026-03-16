using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Player.Counters;

public class IncrementVfx : IPayload
{
	public VfxData VfxData = new VfxData();

	public IEnumerable<string> GetFilePaths()
	{
		int i = 0;
		while (i < VfxData.PrefabData.AllPrefabs.Count)
		{
			yield return VfxData.PrefabData.AllPrefabs[i].RelativePath;
			int num = i + 1;
			i = num;
		}
	}
}
