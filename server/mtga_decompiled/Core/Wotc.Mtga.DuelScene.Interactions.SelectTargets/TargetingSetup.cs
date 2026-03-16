using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectTargets;

public static class TargetingSetup
{
	public const uint MAX_STACK_IDX_DEPTH = 4u;

	public static BrowserType BrowserTypeForTargetSelection(TargetSelection targetSelection, IReadOnlyCollection<ZoneType> associatedZones, MtgZone stack)
	{
		if (associatedZones == null || targetSelection == null || stack == null)
		{
			return BrowserType.None;
		}
		if (associatedZones.Count == 0 || associatedZones.Contains(ZoneType.Battlefield) || associatedZones.Contains(ZoneType.None))
		{
			return BrowserType.NonBrowser;
		}
		if (associatedZones.Count > 1 || associatedZones.Contains(ZoneType.Graveyard) || associatedZones.Contains(ZoneType.Exile))
		{
			return BrowserType.MultiZone;
		}
		if (associatedZones.Contains(ZoneType.Stack) && (MultipleStackTargets(targetSelection) || TargetIsTooDeepInStack(targetSelection, stack)))
		{
			return BrowserType.Stack;
		}
		return BrowserType.NonBrowser;
	}

	private static bool MultipleStackTargets(TargetSelection targetSelection)
	{
		return targetSelection.Targets.Count > 1;
	}

	private static bool TargetIsTooDeepInStack(TargetSelection targetSelection, MtgZone stack)
	{
		foreach (Target target in targetSelection.Targets)
		{
			if ((long)stack.CardIds.IndexOf(target.TargetInstanceId) > 4L)
			{
				return true;
			}
		}
		return false;
	}
}
