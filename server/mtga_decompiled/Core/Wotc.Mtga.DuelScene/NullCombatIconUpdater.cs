using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullCombatIconUpdater : ICombatIconUpdater
{
	public static readonly ICombatIconUpdater Default = new NullCombatIconUpdater();

	public void UpdateCombatIcons(IEnumerable<DuelScene_CDC> allCards)
	{
	}
}
