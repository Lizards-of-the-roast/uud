using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public class SimpleGameStateConstruction
{
	public delegate bool DoesDamage(MtgCardInstance card);

	public MtgGameState StateForReference;

	private DeckHeuristic _deckHeuristic;

	private NPEDirector _npeDirector;

	private IAbilityDataProvider _abilityDataProvider;

	private int AttackerLifetotal;

	private int DefenderLifetotal;

	private HashSet<uint> AttackerCreatures = new HashSet<uint>();

	private HashSet<uint> DefenderCreatures = new HashSet<uint>();

	private Dictionary<uint, int> AssignedDamage = new Dictionary<uint, int>();

	private Dictionary<uint, int> AppliedDamageTotal = new Dictionary<uint, int>();

	private HashSet<uint> DiedInCombat = new HashSet<uint>();

	private Dictionary<uint, MtgCardInstance> idToModifiedCreatureInstance = new Dictionary<uint, MtgCardInstance>();

	public DeckHeuristic DeckHeuristic => _deckHeuristic;

	private SimpleGameStateConstruction(SimpleGameStateConstruction toCopy)
	{
		StateForReference = toCopy.StateForReference;
		_deckHeuristic = toCopy._deckHeuristic;
		_npeDirector = toCopy._npeDirector;
		_abilityDataProvider = toCopy._abilityDataProvider;
		AttackerLifetotal = toCopy.AttackerLifetotal;
		DefenderLifetotal = toCopy.DefenderLifetotal;
		foreach (uint attackerCreature in toCopy.AttackerCreatures)
		{
			AttackerCreatures.Add(attackerCreature);
		}
		foreach (uint defenderCreature in toCopy.DefenderCreatures)
		{
			DefenderCreatures.Add(defenderCreature);
		}
	}

	public SimpleGameStateConstruction(DeckHeuristic deckHeuristic, uint defenderId, MtgGameState stateForReference, IAbilityDataProvider abilityDataProvider, NPEDirector npeDirector = null)
	{
		_deckHeuristic = deckHeuristic;
		_npeDirector = npeDirector;
		_abilityDataProvider = abilityDataProvider;
		StateForReference = stateForReference;
		if (stateForReference.LocalPlayer.InstanceId == defenderId)
		{
			DefenderLifetotal = stateForReference.LocalPlayer.LifeTotal;
			AttackerLifetotal = stateForReference.Opponent.LifeTotal;
		}
		else
		{
			DefenderLifetotal = stateForReference.Opponent.LifeTotal;
			AttackerLifetotal = stateForReference.LocalPlayer.LifeTotal;
		}
		foreach (uint cardId in stateForReference.Battlefield.CardIds)
		{
			MtgCardInstance cardById = stateForReference.GetCardById(cardId);
			if (cardById.CardTypes.Contains(CardType.Creature))
			{
				if (cardById.Controller.InstanceId == defenderId)
				{
					DefenderCreatures.Add(cardId);
				}
				else
				{
					AttackerCreatures.Add(cardId);
				}
			}
		}
	}

	private static bool DoesDamageInFirstStrikeDamageStep(MtgCardInstance card)
	{
		if (!card.Abilities.Exists((AbilityPrintingData x) => x.Id == 6))
		{
			return card.Abilities.Exists((AbilityPrintingData x) => x.Id == 3);
		}
		return true;
	}

	private static bool DoesDamageInNormalDamageStep(MtgCardInstance card)
	{
		if (card.Abilities.Exists((AbilityPrintingData x) => x.Id == 6))
		{
			return card.Abilities.Exists((AbilityPrintingData x) => x.Id == 3);
		}
		return true;
	}

	private void AssignDamage(uint id, int damageAmount)
	{
		if (!AssignedDamage.ContainsKey(id))
		{
			AssignedDamage[id] = 0;
		}
		AssignedDamage[id] += damageAmount;
	}

	private void ApplyDamageAndMarkForDeath(uint attackerId, List<uint> blockers)
	{
		ApplyDamageAndMarkForDeath(attackerId);
		foreach (uint blocker in blockers)
		{
			ApplyDamageAndMarkForDeath(blocker);
		}
	}

	private MtgCardInstance GetCardById(uint id)
	{
		MtgCardInstance result = StateForReference.GetCardById(id);
		if (idToModifiedCreatureInstance.ContainsKey(id))
		{
			result = idToModifiedCreatureInstance[id];
		}
		return result;
	}

	private void ApplyDamageAndMarkForDeath(uint id)
	{
		MtgCardInstance cardById = GetCardById(id);
		int value = 0;
		if (AssignedDamage.TryGetValue(id, out value))
		{
			AssignedDamage[id] = 0;
		}
		if (AppliedDamageTotal.ContainsKey(id))
		{
			AppliedDamageTotal[id] += value;
		}
		else
		{
			AppliedDamageTotal[id] = value;
		}
		if (0 >= cardById.Toughness.Value - cardById.Damage - AppliedDamageTotal[id])
		{
			DiedInCombat.Add(id);
		}
	}

	public SimpleGameStateConstruction GetResultantGameStateFromPotentialCombatPacket(uint attackerId, List<uint> blockers, Dictionary<uint, Dictionary<string, object>> creatureToModifications)
	{
		SimpleGameStateConstruction simpleGameStateConstruction = new SimpleGameStateConstruction(this);
		simpleGameStateConstruction.ApplyModifications(creatureToModifications);
		simpleGameStateConstruction.ApplyCombatPacket(attackerId, blockers, DoesDamageInFirstStrikeDamageStep);
		simpleGameStateConstruction.ApplyDamageAndMarkForDeath(attackerId, blockers);
		simpleGameStateConstruction.ApplyCombatPacket(attackerId, blockers, DoesDamageInNormalDamageStep);
		simpleGameStateConstruction.ApplyDamageAndMarkForDeath(attackerId, blockers);
		simpleGameStateConstruction.PurgeDead();
		return simpleGameStateConstruction;
	}

	private void ApplyModifications(Dictionary<uint, Dictionary<string, object>> creatureToModifications)
	{
		if (creatureToModifications == null)
		{
			return;
		}
		foreach (KeyValuePair<uint, Dictionary<string, object>> creatureToModification in creatureToModifications)
		{
			MtgCardInstance cardById = StateForReference.GetCardById(creatureToModification.Key);
			Dictionary<string, object> value = creatureToModification.Value;
			if (value.ContainsKey("Power") && value.ContainsKey("Toughness"))
			{
				cardById.Power = new StringBackedInt(cardById.Power.Value + (int)value["Power"]);
				cardById.Toughness = new StringBackedInt(cardById.Toughness.Value + (int)value["Toughness"]);
			}
			if (value.ContainsKey("GainAbility"))
			{
				foreach (uint item in (HashSet<uint>)value["GainAbility"])
				{
					AbilityPrintingData abilityPrintingById = _abilityDataProvider.GetAbilityPrintingById(item);
					cardById.Abilities.Add(abilityPrintingById);
				}
			}
			idToModifiedCreatureInstance[creatureToModification.Key] = cardById;
		}
	}

	public SimpleGameStateConstruction GetResultantGameStateFromFullConfiguration(Dictionary<uint, FinalizedCombatPacket> fullyConsideredCombatConfiguration)
	{
		SimpleGameStateConstruction simpleGameStateConstruction = new SimpleGameStateConstruction(this);
		foreach (FinalizedCombatPacket value in fullyConsideredCombatConfiguration.Values)
		{
			simpleGameStateConstruction.ApplyModifications(value.CreatureToModificationsToBeApplied);
		}
		foreach (FinalizedCombatPacket value2 in fullyConsideredCombatConfiguration.Values)
		{
			simpleGameStateConstruction.ApplyCombatPacket(value2.AttackerId, value2.BlockerOrdering, DoesDamageInFirstStrikeDamageStep);
		}
		foreach (FinalizedCombatPacket value3 in fullyConsideredCombatConfiguration.Values)
		{
			simpleGameStateConstruction.ApplyDamageAndMarkForDeath(value3.AttackerId, value3.BlockerOrdering);
		}
		foreach (FinalizedCombatPacket value4 in fullyConsideredCombatConfiguration.Values)
		{
			simpleGameStateConstruction.ApplyCombatPacket(value4.AttackerId, value4.BlockerOrdering, DoesDamageInNormalDamageStep);
		}
		foreach (FinalizedCombatPacket value5 in fullyConsideredCombatConfiguration.Values)
		{
			simpleGameStateConstruction.ApplyDamageAndMarkForDeath(value5.AttackerId, value5.BlockerOrdering);
		}
		simpleGameStateConstruction.PurgeDead();
		return simpleGameStateConstruction;
	}

	private void PurgeDead()
	{
		AttackerCreatures = new HashSet<uint>(AttackerCreatures.Except(DiedInCombat));
		DefenderCreatures = new HashSet<uint>(DefenderCreatures.Except(DiedInCombat));
	}

	private void ApplyCombatPacket(uint attackerId, List<uint> blockers, DoesDamage doesDamageThisStep)
	{
		if (DiedInCombat.Contains(attackerId))
		{
			return;
		}
		MtgCardInstance cardById = GetCardById(attackerId);
		int num = Math.Max(0, cardById.Power.Value);
		bool flag = AIUtilities.HasLifelink(cardById);
		if (blockers.Count == 0)
		{
			if (doesDamageThisStep(cardById))
			{
				DefenderLifetotal -= num;
				if (flag)
				{
					AttackerLifetotal += num;
				}
			}
			return;
		}
		int num2 = 0;
		List<MtgCardInstance> list = new List<MtgCardInstance>();
		foreach (uint blocker in blockers)
		{
			if (DiedInCombat.Contains(blocker))
			{
				continue;
			}
			MtgCardInstance cardById2 = GetCardById(blocker);
			if (doesDamageThisStep(cardById2))
			{
				int num3 = Math.Max(0, cardById2.Power.Value);
				num2 = ((num3 <= 0 || !AIUtilities.HasDeathtouch(cardById2)) ? (num2 + num3) : (num2 + 100000));
				if (AIUtilities.HasLifelink(cardById2))
				{
					DefenderLifetotal += num3;
				}
			}
			list.Add(cardById2);
		}
		AssignDamage(attackerId, num2);
		if (!doesDamageThisStep(cardById))
		{
			return;
		}
		if (list.Count() > 0 && flag)
		{
			AttackerLifetotal += num;
		}
		int num4 = num;
		foreach (MtgCardInstance item in list)
		{
			MtgCardInstance cardById3 = GetCardById(item.InstanceId);
			int num5 = cardById3.Toughness.Value - (int)cardById3.Damage;
			if (num4 > 0 && AIUtilities.HasDeathtouch(cardById))
			{
				AssignDamage(cardById3.InstanceId, 100000);
				num4--;
			}
			else if (num5 <= num4)
			{
				AssignDamage(cardById3.InstanceId, num5);
				num4 -= num5;
			}
			else
			{
				AssignDamage(cardById3.InstanceId, num4);
				num4 = 0;
			}
		}
	}

	public float ScoreChange(SimpleGameStateConstruction projectedGameState)
	{
		return (float)_deckHeuristic._creaturesToLifeValueRatio._creatures * (projectedGameState.CreatureBasedScore() - CreatureBasedScore()) + (float)_deckHeuristic._creaturesToLifeValueRatio._life * (projectedGameState.LifetotalBasedScore() - LifetotalBasedScore());
	}

	private float CreatureBasedScore()
	{
		float num = 0f;
		foreach (uint defenderCreature in DefenderCreatures)
		{
			MtgCardInstance cardById = StateForReference.GetCardById(defenderCreature);
			num += _deckHeuristic.ScoreCard(cardById, _npeDirector);
		}
		foreach (uint attackerCreature in AttackerCreatures)
		{
			MtgCardInstance cardById2 = StateForReference.GetCardById(attackerCreature);
			num -= _deckHeuristic.ScoreCard(cardById2, _npeDirector);
		}
		return num;
	}

	private float LifetotalBasedScore()
	{
		float num = 0f;
		if (DefenderLifetotal <= 0)
		{
			num -= 10000f;
		}
		if (AttackerLifetotal <= 0)
		{
			num += 10000f;
		}
		num += (float)DefenderLifetotal;
		return num - (float)AttackerLifetotal;
	}
}
