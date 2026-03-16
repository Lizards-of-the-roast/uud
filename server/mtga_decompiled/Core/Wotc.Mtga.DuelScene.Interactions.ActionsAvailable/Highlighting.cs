using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class Highlighting
{
	public const uint ABILITY_ID_ASCENDANT_SPIRIT = 75081u;

	public static HighlightType HighlightForAction(Action action, MtgCardInstance instance)
	{
		if (action == null)
		{
			return HighlightType.None;
		}
		if (!action.CanAffordToCast())
		{
			return HighlightType.None;
		}
		if (UseHighlightOverride(action, out var highlightOverride))
		{
			return highlightOverride;
		}
		if (action.Highlight != Wotc.Mtgo.Gre.External.Messaging.HighlightType.None)
		{
			return (HighlightType)action.Highlight;
		}
		if (action.AbilityGrpId != 0 && instance.PlayWarnings.Exists((ShouldntPlayData x) => x.AbilityId == action.AbilityGrpId))
		{
			return HighlightType.Cold;
		}
		return HighlightType.Hot;
	}

	private static bool UseHighlightOverride(Action action, out HighlightType highlightOverride)
	{
		if (action == null)
		{
			highlightOverride = HighlightType.None;
			return false;
		}
		if (action.AbilityGrpId == 75081 && action.Highlight == Wotc.Mtgo.Gre.External.Messaging.HighlightType.Cold && action.ActionType == ActionType.Activate)
		{
			highlightOverride = HighlightType.None;
			return true;
		}
		highlightOverride = HighlightType.None;
		return false;
	}
}
