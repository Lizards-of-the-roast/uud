using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectTargets;

public interface IAutoTargetingSolution
{
	bool TryAutoTarget(TargetSelection targetSelection, TargetSource targetSource, out Target target);
}
