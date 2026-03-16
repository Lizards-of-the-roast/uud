using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.AssignDamage;

public readonly struct MtgDamageAssigner
{
	public readonly uint InstanceId;

	public readonly uint TotalDamage;

	public readonly bool CanIgnoreBlockers;

	public readonly bool AttackerHasTrample;

	public readonly uint AttackQuarryId;

	public readonly bool AttackQuarryIsPlayer;

	public readonly bool CanAssignDamageToAttackQuarry;

	public readonly bool AttackQuarryIsBlockingAttacker;

	public readonly bool DamageAffectedByReplacements;

	public MtgDamageAssigner(uint instanceId, uint totalDamage, bool canIgnoreBlockers, bool attackerHasTrample, uint attackQuarryId, bool attackQuarryIsPlayer, bool canAssignDamageToAttackQuarry, bool attackQuarryIsBlockingAttacker, bool damageAffectedByReplacements)
	{
		InstanceId = instanceId;
		TotalDamage = totalDamage;
		CanIgnoreBlockers = canIgnoreBlockers;
		AttackerHasTrample = attackerHasTrample;
		AttackQuarryId = attackQuarryId;
		AttackQuarryIsPlayer = attackQuarryIsPlayer;
		CanAssignDamageToAttackQuarry = canAssignDamageToAttackQuarry;
		AttackQuarryIsBlockingAttacker = attackQuarryIsBlockingAttacker;
		DamageAffectedByReplacements = damageAffectedByReplacements;
	}

	public static MtgDamageAssigner Create(DamageAssigner greDamageAssigner, MtgGameState gameState)
	{
		uint instanceId = greDamageAssigner.InstanceId;
		uint num = 0u;
		bool attackQuarryIsPlayer = false;
		bool canAssignDamageToAttackQuarry = false;
		bool attackQuarryIsBlockingAttacker = false;
		bool attackerHasTrample = false;
		if (gameState.TryGetCard(instanceId, out var card))
		{
			attackerHasTrample = card.Abilities.Exists((AbilityPrintingData x) => x.Id == 14);
			num = card.AttackTargetId;
			attackQuarryIsPlayer = gameState.TryGetPlayer(num, out var _);
			canAssignDamageToAttackQuarry = GetCanAssignDamageToQuarry(num, greDamageAssigner.Assignments);
			attackQuarryIsBlockingAttacker = GetAttackQuarryIsBlockingAttacker(instanceId, num, gameState);
		}
		return new MtgDamageAssigner(instanceId, greDamageAssigner.TotalDamage, greDamageAssigner.CanIgnoreBlockers, attackerHasTrample, num, attackQuarryIsPlayer, canAssignDamageToAttackQuarry, attackQuarryIsBlockingAttacker, greDamageAssigner.DamageAffectedByReplacements);
	}

	private static bool GetCanAssignDamageToQuarry(uint attackQuarryId, IEnumerable<DamageAssignment> assignments)
	{
		return assignments.Exists(attackQuarryId, (DamageAssignment assignment, uint quarryId) => assignment.InstanceId == quarryId);
	}

	private static bool GetAttackQuarryIsBlockingAttacker(uint attackerId, uint attackQuarryId, MtgGameState gameState)
	{
		if (gameState.TryGetCard(attackQuarryId, out var card))
		{
			return card.BlockingIds.Exists(attackerId, (uint x, uint blockedId) => x == blockedId);
		}
		return false;
	}

	public DamageAssigner ToDamageAssigner(IReadOnlyList<MtgDamageAssignment> damageAssignments)
	{
		DamageAssigner damageAssigner = new DamageAssigner
		{
			InstanceId = InstanceId,
			CanIgnoreBlockers = CanIgnoreBlockers,
			TotalDamage = TotalDamage
		};
		foreach (MtgDamageAssignment damageAssignment in damageAssignments)
		{
			damageAssigner.Assignments.Add(damageAssignment.ToDamageAssignment());
		}
		return damageAssigner;
	}

	public bool CanSubmit(IReadOnlyList<MtgDamageAssignment> assignments)
	{
		if (!AllDamageAssigned(TotalDamage, assignments))
		{
			return false;
		}
		if (!CanAssignDamageToAttackQuarry || AttackQuarryIsBlockingAttacker || (!CanIgnoreBlockers && !AttackerHasTrample))
		{
			return true;
		}
		if (CanIgnoreBlockers && AttackerHasTrample)
		{
			if (!CanSubmitCanIgnoreBlockers(assignments))
			{
				return CanSubmitTrample(assignments);
			}
			return true;
		}
		if (CanIgnoreBlockers)
		{
			return CanSubmitCanIgnoreBlockers(assignments);
		}
		if (AttackerHasTrample)
		{
			return CanSubmitTrample(assignments);
		}
		return true;
	}

	private static bool CanSubmitCanIgnoreBlockers(IReadOnlyList<MtgDamageAssignment> assignments)
	{
		bool num = assignments.Exists((MtgDamageAssignment x) => !x.IsAttackQuarry && x.AssignedDamage != 0);
		bool flag = assignments.Exists((MtgDamageAssignment x) => x.IsAttackQuarry && x.AssignedDamage != 0);
		return num != flag;
	}

	private static bool CanSubmitTrample(IReadOnlyList<MtgDamageAssignment> assignments)
	{
		if (assignments.Exists((MtgDamageAssignment x) => x.IsAttackQuarry && x.AssignedDamage == 0))
		{
			return true;
		}
		return !assignments.Exists((MtgDamageAssignment x) => !x.IsAttackQuarry && x.AssignedDamage < x.LethalDamage);
	}

	private static bool AllDamageAssigned(uint totalDamage, IReadOnlyList<MtgDamageAssignment> assignments)
	{
		uint num = 0u;
		foreach (MtgDamageAssignment assignment in assignments)
		{
			num += assignment.AssignedDamage;
		}
		return num == totalDamage;
	}
}
