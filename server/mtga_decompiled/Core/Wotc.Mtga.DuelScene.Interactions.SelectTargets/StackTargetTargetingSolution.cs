using System.Linq;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectTargets;

public class StackTargetTargetingSolution : IAutoTargetingSolution
{
	public bool TryAutoTarget(TargetSelection targetSelection, TargetSource targetSource, out Target target)
	{
		target = null;
		if (targetSelection.MinTargets != targetSelection.MaxTargets)
		{
			return false;
		}
		if (targetSelection.Targets.Count != targetSelection.MinTargets)
		{
			return false;
		}
		if (targetSelection.Targets.Exists((Target t) => t.LegalAction != SelectAction.Select || t.Highlight == Wotc.Mtgo.Gre.External.Messaging.HighlightType.Cold))
		{
			return false;
		}
		target = targetSelection.Targets.FirstOrDefault((Target x) => x.LegalAction == SelectAction.Select);
		return target != null;
	}
}
