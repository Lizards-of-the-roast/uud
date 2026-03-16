using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class NullActionProcessor : IActionProcessor
{
	public static readonly IActionProcessor Default = new NullActionProcessor();

	public void HandleActions(IEntityView entity, List<GreInteraction> actions)
	{
	}
}
