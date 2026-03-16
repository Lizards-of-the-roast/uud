using System.Collections.Generic;
using Wotc.Mtga.Duel;

namespace AssetLookupTree.Payloads.GeneralEffect;

public class DieViewPrefab : IPayload
{
	public AltAssetReference<DieView> PrefabRef = new AltAssetReference<DieView>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return PrefabRef.RelativePath;
	}
}
