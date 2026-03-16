using System.Collections.Generic;

public class FinalizedCombatPacket
{
	public uint AttackerId;

	public List<uint> BlockerOrdering;

	public Dictionary<uint, Dictionary<string, object>> CreatureToModificationsToBeApplied;

	public FinalizedCombatPacket(uint id, List<uint> ordering)
	{
		AttackerId = id;
		BlockerOrdering = ordering;
		CreatureToModificationsToBeApplied = new Dictionary<uint, Dictionary<string, object>>();
	}
}
