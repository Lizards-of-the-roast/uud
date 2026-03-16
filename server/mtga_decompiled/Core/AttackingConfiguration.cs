using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

public class AttackingConfiguration
{
	public float Score;

	public List<uint> AbleAttackerIds;

	public List<uint> CommittedAttackerIds;

	public AttackingConfiguration(DeclareAttackerRequest attackRequest)
	{
		AbleAttackerIds = new List<uint>();
		foreach (Attacker attacker in attackRequest.Attackers)
		{
			AbleAttackerIds.Add(attacker.AttackerInstanceId);
		}
		CommittedAttackerIds = new List<uint>();
	}

	public AttackingConfiguration(AttackingConfiguration previous)
	{
		AbleAttackerIds = new List<uint>();
		foreach (uint ableAttackerId in previous.AbleAttackerIds)
		{
			AbleAttackerIds.Add(ableAttackerId);
		}
		CommittedAttackerIds = new List<uint>();
		foreach (uint committedAttackerId in previous.CommittedAttackerIds)
		{
			CommittedAttackerIds.Add(committedAttackerId);
		}
	}

	public override string ToString()
	{
		string text = "AC=attacking:[";
		foreach (uint committedAttackerId in CommittedAttackerIds)
		{
			text = text + committedAttackerId + ", ";
		}
		return text + "]";
	}

	public bool TryAddHashedAttackConfiguration(HashSet<ulong> hashedAttackConfigurations, Dictionary<uint, uint> hashedAttackers, MtgGameState gameState)
	{
		ulong num = 0uL;
		foreach (uint committedAttackerId in CommittedAttackerIds)
		{
			if (hashedAttackers.TryGetValue(committedAttackerId, out var value))
			{
				num += value;
				continue;
			}
			value = HashAttackerBasedOnStats(committedAttackerId, gameState);
			hashedAttackers.Add(committedAttackerId, value);
			num += value;
		}
		if (hashedAttackConfigurations.Add(num))
		{
			return true;
		}
		return false;
	}

	public static uint HashAttackerBasedOnStats(uint attackerID, MtgGameState gameState)
	{
		gameState.VisibleCards.TryGetValue(attackerID, out var value);
		uint num = (uint)(value.Power.Value * 769);
		num += (uint)(value.Toughness.Value * 389);
		foreach (AbilityPrintingData ability in value.Abilities)
		{
			if (ability.IsEvergreen())
			{
				num += (uint)(value.Abilities.Count * (int)(ability.Id % 786433));
			}
		}
		return num;
	}
}
