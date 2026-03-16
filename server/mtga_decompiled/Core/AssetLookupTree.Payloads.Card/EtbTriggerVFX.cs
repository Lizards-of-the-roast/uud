using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public class EtbTriggerVFX : ILayeredPayload, IPayload
{
	public List<VfxData> VfxDatas = new List<VfxData>();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		int i = 0;
		while (i < VfxDatas.Count)
		{
			int num;
			for (int j = 0; j < VfxDatas[i].PrefabData.AllPrefabs.Count; j = num)
			{
				yield return VfxDatas[i].PrefabData.AllPrefabs[j].RelativePath;
				num = j + 1;
			}
			num = i + 1;
			i = num;
		}
	}
}
