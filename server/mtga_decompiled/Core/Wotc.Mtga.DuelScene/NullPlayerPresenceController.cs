using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class NullPlayerPresenceController : IPlayerPresenceController
{
	public static readonly IPlayerPresenceController Default = new NullPlayerPresenceController();

	public void Update(IEnumerable<DuelScene_CDC> allCards)
	{
	}

	public void SetHoveredCardId(uint id)
	{
	}
}
