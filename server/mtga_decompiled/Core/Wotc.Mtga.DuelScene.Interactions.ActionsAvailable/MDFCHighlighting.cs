using System.Collections.Generic;
using GreClient.Rules;
using WorkflowVisuals;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class MDFCHighlighting
{
	public static void SetHighlights(in Highlights highlights, IReadOnlyList<Action> actions, MtgCardInstance cardInstance)
	{
		if (cardInstance.TryGetBackInstance(out var backInstance) && TryGetActionToHighlight(actions, out var toHighlight))
		{
			highlights.IdToHighlightType_User[backInstance.InstanceId] = Highlighting.HighlightForAction(toHighlight, backInstance);
		}
	}

	private static bool TryGetActionToHighlight(IEnumerable<Action> actions, out Action toHighlight)
	{
		if (actions == null)
		{
			toHighlight = null;
			return false;
		}
		foreach (Action action in actions)
		{
			if (action.IsMDFCAction() && action.CanAffordToCast())
			{
				toHighlight = action;
				return true;
			}
		}
		toHighlight = null;
		return false;
	}
}
