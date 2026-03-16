using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface ICombatIconUpdater
{
	void UpdateCombatIcons(IEnumerable<DuelScene_CDC> allCards);
}
