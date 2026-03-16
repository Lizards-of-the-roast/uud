using System.Collections.Generic;

namespace AssetLookupTree.Payloads.UI.DuelScene;

public class DuelSceneUIPrefabs : IPayload
{
	public readonly AltAssetReference<PlayerNames> PlayerNamesRef = new AltAssetReference<PlayerNames>();

	public readonly AltAssetReference<ManaColorSelector> ManaColorSelectorRef = new AltAssetReference<ManaColorSelector>();

	public readonly AltAssetReference<AttackerCost> AttackerCostRef = new AltAssetReference<AttackerCost>();

	public readonly AltAssetReference<ConfirmWidget> ConfirmWidgetRef = new AltAssetReference<ConfirmWidget>();

	public readonly AltAssetReference<BattleFieldStaticElementsLayout> BattlefieldLayoutRef = new AltAssetReference<BattleFieldStaticElementsLayout>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return PlayerNamesRef.RelativePath;
		yield return ManaColorSelectorRef.RelativePath;
		yield return AttackerCostRef.RelativePath;
		yield return ConfirmWidgetRef.RelativePath;
		yield return BattlefieldLayoutRef.RelativePath;
	}
}
