using System.Collections.Generic;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.Interactions.AssignDamage;

public static class MtgDamageAssignmentExtensions
{
	public static bool CanTransferDamage(this MtgDamageAssignment assignment, MtgDamageAssigner assigner, IReadOnlyList<MtgDamageAssignment> allAssignments)
	{
		for (int num = allAssignments.Count - 1; num >= 0; num--)
		{
			MtgDamageAssignment mtgDamageAssignment = allAssignments[num];
			if (mtgDamageAssignment != assignment && mtgDamageAssignment.CanDecrement(assigner, allAssignments))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanDecrement(this MtgDamageAssignment assignment, MtgDamageAssigner assigner, IReadOnlyList<MtgDamageAssignment> allAssignments)
	{
		if (assignment.AssignedDamage == 0)
		{
			return false;
		}
		if (!assigner.CanAssignDamageToAttackQuarry || assigner.AttackQuarryIsBlockingAttacker || assignment.IsAttackQuarry)
		{
			return true;
		}
		if (assigner.AttackerHasTrample)
		{
			if (assignment.AssignedDamage <= assignment.LethalDamage)
			{
				return allAssignments.Exists(assignment, (MtgDamageAssignment x, MtgDamageAssignment y) => x != y && x.IsAttackQuarry && x.AssignedDamage == 0);
			}
			return true;
		}
		return true;
	}

	public static IncrementAction GetIncrementAction(this MtgDamageAssignment assignment, MtgDamageAssigner assigner, IReadOnlyList<MtgDamageAssignment> allAssignments)
	{
		if (assignment.MaxDamage == assignment.AssignedDamage)
		{
			return IncrementAction.None;
		}
		if (assigner.CanAssignDamageToAttackQuarry && !assigner.AttackQuarryIsBlockingAttacker)
		{
			if (assigner.CanIgnoreBlockers && assigner.AttackerHasTrample)
			{
				UseUnassignedOrTryTransfer(assigner, assignment, allAssignments);
			}
			else if (assigner.CanIgnoreBlockers)
			{
				if (assignment.IsAttackQuarry)
				{
					return IncrementAction.RedistributeAllFromBlockers;
				}
				if (allAssignments.Exists((MtgDamageAssignment x) => x.IsAttackQuarry && x.AssignedDamage != 0))
				{
					return IncrementAction.RedistributeAllFromQuarry;
				}
			}
			else
			{
				bool flag = false;
				bool flag2 = true;
				foreach (MtgDamageAssignment allAssignment in allAssignments)
				{
					if (allAssignment.IsAttackQuarry)
					{
						flag = allAssignment.AssignedDamage != 0;
					}
					else
					{
						flag2 &= allAssignment.HasLethalDamageAssigned;
					}
				}
				if (!flag2 && (assignment.IsAttackQuarry || flag))
				{
					return IncrementAction.None;
				}
			}
		}
		return UseUnassignedOrTryTransfer(assigner, assignment, allAssignments);
	}

	private static IncrementAction UseUnassignedOrTryTransfer(MtgDamageAssigner assigner, MtgDamageAssignment assignment, IReadOnlyList<MtgDamageAssignment> allAssignments)
	{
		if (!HasUnassignedDamage(assigner.TotalDamage, allAssignments))
		{
			if (!assignment.CanTransferDamage(assigner, allAssignments))
			{
				return IncrementAction.None;
			}
			return IncrementAction.Increment_Transfer;
		}
		return IncrementAction.Increment_UnassignedDamage;
	}

	private static bool HasUnassignedDamage(uint totalDamage, IEnumerable<MtgDamageAssignment> assignments)
	{
		uint num = 0u;
		foreach (MtgDamageAssignment assignment in assignments)
		{
			uint assignedDamage = assignment.AssignedDamage;
			num += assignedDamage;
		}
		return num < totalDamage;
	}
}
