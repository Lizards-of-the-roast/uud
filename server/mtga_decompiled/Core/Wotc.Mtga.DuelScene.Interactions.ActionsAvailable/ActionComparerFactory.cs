namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public static class ActionComparerFactory
{
	public static IModalActionComparer CreateModalActionComparer()
	{
		return new ActionComparerAggregate(new SpecialActionComparer(), new ActivateActionComparer(), new MDFCAltCastComparer(), new PrototypeCastComparer(), new AdventureComparer(), new OmenComparer(), new AltGrpIdComparer(), new CastLeftComparer(), new CastMDFCComparer(), new PlayMDFCComparer(), new RelativeAbilityComparer());
	}
}
