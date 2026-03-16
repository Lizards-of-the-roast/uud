using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class NullBattlefieldDataProvider : IBattlefieldDataProvider
{
	public static readonly IBattlefieldDataProvider Default = new NullBattlefieldDataProvider();

	public IReadOnlyList<BattlefieldData> GetAllBattlefields()
	{
		return Array.Empty<BattlefieldData>();
	}

	public BattlefieldData? GetBattlefieldByName(string name)
	{
		return null;
	}

	public string GetDefaultBattlefield()
	{
		return "LGS";
	}
}
