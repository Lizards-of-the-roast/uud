using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.AssignDamage;

public class MtgDamageAssignment
{
	public readonly uint InstanceId;

	public uint AssignedDamage;

	public readonly uint LethalDamage;

	public readonly uint MaxDamage;

	public readonly bool IsAttackQuarry;

	public readonly bool IsPlayerInstance;

	public readonly bool IsBlockingAttacker;

	public bool HasLethalDamageAssigned => AssignedDamage >= LethalDamage;

	public MtgDamageAssignment(uint instanceId, uint assignedDamage, uint lethalDamage, uint maxDamage, bool isAttackQuarry, bool isPlayerInstance, bool isBlockingAttacker)
	{
		InstanceId = instanceId;
		AssignedDamage = assignedDamage;
		LethalDamage = lethalDamage;
		MaxDamage = maxDamage;
		IsAttackQuarry = isAttackQuarry;
		IsPlayerInstance = isPlayerInstance;
		IsBlockingAttacker = isBlockingAttacker;
	}

	public DamageAssignment ToDamageAssignment()
	{
		return new DamageAssignment
		{
			InstanceId = InstanceId,
			AssignedDamage = AssignedDamage,
			MinDamage = LethalDamage,
			MaxDamage = MaxDamage
		};
	}
}
