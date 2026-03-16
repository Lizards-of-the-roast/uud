using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public interface IAutoSubmitActionCalculator
{
	GreInteraction GetAutoSubmitAction(IEnumerable<GreInteraction> actions);

	bool TryGetAutoSubmitAction(IEnumerable<GreInteraction> actions, out GreInteraction toSubmit)
	{
		toSubmit = GetAutoSubmitAction(actions);
		return toSubmit != null;
	}
}
