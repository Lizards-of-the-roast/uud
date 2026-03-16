using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public interface IBattlefieldDataProvider
{
	IReadOnlyList<BattlefieldData> GetAllBattlefields();

	BattlefieldData? GetBattlefieldByName(string name);

	string GetDefaultBattlefield();
}
