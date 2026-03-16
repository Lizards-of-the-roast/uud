using System.Linq;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectTargets;

public class TriggeredAbilityTargetingSolution : IAutoTargetingSolution
{
	public bool TryAutoTarget(TargetSelection targetSelection, TargetSource targetSource, out Target target)
	{
		target = null;
		int count = targetSelection.Targets.Count;
		if (count != targetSelection.MinTargets)
		{
			return false;
		}
		if (count != targetSelection.MaxTargets)
		{
			return false;
		}
		target = targetSelection.Targets.FirstOrDefault((Target x) => x.LegalAction == SelectAction.Select);
		return target != null;
	}
}
