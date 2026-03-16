using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Combat;

public class VFXHit : IPayload
{
	public CombatVFXData VfxData = new CombatVFXData();

	public IEnumerable<string> GetFilePaths()
	{
		int i = 0;
		while (i < VfxData.Prefabs.Count)
		{
			yield return VfxData.Prefabs[i].RelativePath;
			int num = i + 1;
			i = num;
		}
	}
}
