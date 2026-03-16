using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public interface IActionProcessor
{
	void HandleActions(IEntityView entity, List<GreInteraction> actions);
}
