using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Card;

public abstract class MovementVFX : ILayeredPayload, IPayload
{
	public List<VfxData> VfxDatas = new List<VfxData>();

	public HashSet<string> Layers { get; } = new HashSet<string>();

	public IEnumerable<string> GetFilePaths()
	{
		foreach (VfxData vfxData in VfxDatas)
		{
			int i = 0;
			while (i < vfxData.PrefabData.AllPrefabs.Count)
			{
				yield return vfxData.PrefabData.AllPrefabs[i].RelativePath;
				int num = i + 1;
				i = num;
			}
		}
	}
}
