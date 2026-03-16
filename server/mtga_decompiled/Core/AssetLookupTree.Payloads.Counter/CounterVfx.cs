using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Counter;

public abstract class CounterVfx : IPayload
{
	public VfxPrefabData PrefabData = new VfxPrefabData();

	public VfxData CardVFX = new VfxData();

	public IEnumerable<string> GetFilePaths()
	{
		int i = 0;
		while (i < PrefabData.AllPrefabs.Count)
		{
			yield return PrefabData.AllPrefabs[i].RelativePath;
			int num = i + 1;
			i = num;
		}
		i = 0;
		while (i < CardVFX.PrefabData.AllPrefabs.Count)
		{
			yield return CardVFX.PrefabData.AllPrefabs[i].RelativePath;
			int num = i + 1;
			i = num;
		}
	}
}
