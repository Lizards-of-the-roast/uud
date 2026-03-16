using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public interface IUXEventGrouper
{
	void GroupEvents(in int startIdx, ref List<UXEvent> events);
}
