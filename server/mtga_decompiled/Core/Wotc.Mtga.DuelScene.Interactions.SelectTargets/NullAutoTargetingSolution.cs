using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectTargets;

public class NullAutoTargetingSolution : IAutoTargetingSolution
{
	public static readonly IAutoTargetingSolution Default = new NullAutoTargetingSolution();

	public bool TryAutoTarget(TargetSelection targetSelection, TargetSource targetSource, out Target target)
	{
		target = null;
		return false;
	}
}
