using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface IPlayerPresenceController
{
	void Update(IEnumerable<DuelScene_CDC> allCards);

	void SetHoveredCardId(uint id);
}
