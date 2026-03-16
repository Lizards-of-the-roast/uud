using Wotc.Mtga.Events;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class OmniscienceUtil
{
	public const uint OMNISCIENCE_CARD_ID = 69761u;

	public const uint OMNISCIENCE_ABILITY_ID = 19205u;

	public static bool EventHasEmblem(IPlayerEvent playerEvent)
	{
		return playerEvent?.EventUXInfo?.EventComponentData?.EmblemDisplay?.EmblemIDs?.Contains(69761u) == true;
	}
}
