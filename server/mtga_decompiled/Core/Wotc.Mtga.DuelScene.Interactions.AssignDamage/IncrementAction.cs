namespace Wotc.Mtga.DuelScene.Interactions.AssignDamage;

public enum IncrementAction
{
	None,
	Increment_UnassignedDamage,
	Increment_Transfer,
	RedistributeAllFromBlockers,
	RedistributeAllFromQuarry
}
