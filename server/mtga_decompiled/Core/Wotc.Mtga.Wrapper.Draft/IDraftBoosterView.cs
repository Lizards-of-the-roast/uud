using System;

namespace Wotc.Mtga.Wrapper.Draft;

public interface IDraftBoosterView
{
	CollationMapping CollationId { get; }

	bool PassDirectionIsLeft { get; }

	IDraftBoosterView SetBoosterData(CollationMapping collationId);

	IDraftBoosterView PassBooster(bool passDirecitonLeft, Action<IDraftBoosterView> onFinishedAnimation = null);

	IDraftBoosterView UpdateActive(bool isActive);
}
