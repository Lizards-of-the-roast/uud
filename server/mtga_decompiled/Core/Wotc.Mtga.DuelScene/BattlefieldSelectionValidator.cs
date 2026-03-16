using System.Collections.Generic;
using GreClient.Network;
using Wotc.Mtga.DuelScene.UI.DEBUG;

namespace Wotc.Mtga.DuelScene;

public class BattlefieldSelectionValidator : IMatchConfigValidator
{
	private readonly IBattlefieldDataProvider _battlefieldDataProvider;

	public BattlefieldSelectionValidator(IBattlefieldDataProvider battlefieldDataProvider)
	{
		_battlefieldDataProvider = battlefieldDataProvider;
	}

	public IEnumerable<(IMatchConfigValidator.Result resultType, string reason)> GetResults(MatchConfig matchConfig)
	{
		string battlefieldSelection = matchConfig.BattlefieldSelection;
		BattlefieldData? battlefieldByName = _battlefieldDataProvider.GetBattlefieldByName(battlefieldSelection);
		if (!battlefieldByName.HasValue)
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: "Battlefield " + battlefieldSelection + " is not registered in the asset lookup tree - choose another one.");
		}
		else if (!AssetLoader.HaveAsset(battlefieldByName.Value.ScenePath))
		{
			yield return (resultType: IMatchConfigValidator.Result.Error, reason: "Battlefield " + battlefieldSelection + " is not found in the current asset bundles - choose another one.");
		}
	}
}
