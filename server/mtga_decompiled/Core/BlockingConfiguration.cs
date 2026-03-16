using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

public class BlockingConfiguration
{
	public float Score;

	public Dictionary<uint, HashSet<uint>> AbleBlockerIdToBlockableAttackerIds;

	public Dictionary<uint, HashSet<uint>> AttackerIdToCoupledBlockerIds;

	public MtgGameState _stateForReference;

	public override string ToString()
	{
		string text = "BC=";
		foreach (BlockToMake item in GetListOfBlocksToMake())
		{
			text = text + "block:[" + item.BlockerId + " on " + item.AttackerId + "], ";
		}
		return text;
	}

	public bool IsValidBlockingConfiguration()
	{
		foreach (KeyValuePair<uint, HashSet<uint>> attackerIdToCoupledBlockerId in AttackerIdToCoupledBlockerIds)
		{
			uint key = attackerIdToCoupledBlockerId.Key;
			HashSet<uint> value = attackerIdToCoupledBlockerId.Value;
			if (AIUtilities.HasMenace(_stateForReference.GetCardById(key)) && value.Count() == 1)
			{
				return false;
			}
			foreach (uint item in value)
			{
				foreach (QualificationData affectedByQualification in _stateForReference.GetCardById(item).AffectedByQualifications)
				{
					if (affectedByQualification.Type == QualificationType.CantBlock)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public BlockingConfiguration(SimpleGameStateConstruction startingGameState, DeclareBlockersRequest blockRequest)
	{
		_stateForReference = startingGameState.StateForReference;
		AbleBlockerIdToBlockableAttackerIds = new Dictionary<uint, HashSet<uint>>();
		foreach (Blocker allBlocker in blockRequest.AllBlockers)
		{
			uint blockerInstanceId = allBlocker.BlockerInstanceId;
			if (!AbleBlockerIdToBlockableAttackerIds.TryGetValue(blockerInstanceId, out var value))
			{
				value = new HashSet<uint>();
				AbleBlockerIdToBlockableAttackerIds.Add(blockerInstanceId, value);
			}
			foreach (uint attackerInstanceId in allBlocker.AttackerInstanceIds)
			{
				value.Add(attackerInstanceId);
			}
		}
		AttackerIdToCoupledBlockerIds = new Dictionary<uint, HashSet<uint>>();
		foreach (uint cardId in _stateForReference.Battlefield.CardIds)
		{
			if (_stateForReference.GetCardById(cardId).AttackState == AttackState.Attacking)
			{
				AttackerIdToCoupledBlockerIds.Add(cardId, new HashSet<uint>());
			}
		}
	}

	public BlockingConfiguration(BlockingConfiguration previous)
	{
		_stateForReference = previous._stateForReference;
		AbleBlockerIdToBlockableAttackerIds = new Dictionary<uint, HashSet<uint>>();
		foreach (KeyValuePair<uint, HashSet<uint>> ableBlockerIdToBlockableAttackerId in previous.AbleBlockerIdToBlockableAttackerIds)
		{
			uint key = ableBlockerIdToBlockableAttackerId.Key;
			HashSet<uint> hashSet = new HashSet<uint>();
			foreach (uint item in ableBlockerIdToBlockableAttackerId.Value)
			{
				hashSet.Add(item);
			}
			AbleBlockerIdToBlockableAttackerIds.Add(key, hashSet);
		}
		AttackerIdToCoupledBlockerIds = new Dictionary<uint, HashSet<uint>>();
		foreach (KeyValuePair<uint, HashSet<uint>> attackerIdToCoupledBlockerId in previous.AttackerIdToCoupledBlockerIds)
		{
			uint key2 = attackerIdToCoupledBlockerId.Key;
			HashSet<uint> hashSet2 = new HashSet<uint>();
			foreach (uint item2 in attackerIdToCoupledBlockerId.Value)
			{
				hashSet2.Add(item2);
			}
			AttackerIdToCoupledBlockerIds.Add(key2, hashSet2);
		}
	}

	public BlockingConfiguration(MtgGameState stateForReference, AttackingConfiguration proposedAttack)
	{
		_stateForReference = stateForReference;
		uint instanceId = _stateForReference.Opponent.InstanceId;
		AbleBlockerIdToBlockableAttackerIds = new Dictionary<uint, HashSet<uint>>();
		foreach (uint cardId in _stateForReference.Battlefield.CardIds)
		{
			MtgCardInstance cardById = _stateForReference.GetCardById(cardId);
			if (!cardById.CardTypes.Contains(CardType.Creature) || cardById.Controller.InstanceId != instanceId || cardById.IsTapped)
			{
				continue;
			}
			HashSet<uint> hashSet = new HashSet<uint>();
			foreach (uint committedAttackerId in proposedAttack.CommittedAttackerIds)
			{
				MtgCardInstance cardById2 = _stateForReference.GetCardById(committedAttackerId);
				if (!AIUtilities.HasUnblockable(cardById2))
				{
					if (!AIUtilities.HasFlying(cardById2))
					{
						hashSet.Add(committedAttackerId);
					}
					else if (AIUtilities.HasFlying(cardById) || AIUtilities.HasReach(cardById))
					{
						hashSet.Add(committedAttackerId);
					}
				}
			}
			if (hashSet.Count > 0)
			{
				AbleBlockerIdToBlockableAttackerIds.Add(cardId, hashSet);
			}
		}
		AttackerIdToCoupledBlockerIds = new Dictionary<uint, HashSet<uint>>();
		foreach (uint committedAttackerId2 in proposedAttack.CommittedAttackerIds)
		{
			AttackerIdToCoupledBlockerIds.Add(committedAttackerId2, new HashSet<uint>());
		}
	}

	public void CoupleAttackerAndBlocker(uint attackerId, uint blockerId)
	{
		AttackerIdToCoupledBlockerIds[attackerId].Add(blockerId);
		AbleBlockerIdToBlockableAttackerIds.Remove(blockerId);
	}

	public void BlockerAbstains(uint blockerId)
	{
		AbleBlockerIdToBlockableAttackerIds.Remove(blockerId);
	}

	public List<BlockToMake> GetListOfBlocksToMake()
	{
		List<BlockToMake> list = new List<BlockToMake>();
		foreach (KeyValuePair<uint, HashSet<uint>> attackerIdToCoupledBlockerId in AttackerIdToCoupledBlockerIds)
		{
			uint key = attackerIdToCoupledBlockerId.Key;
			foreach (uint item in attackerIdToCoupledBlockerId.Value)
			{
				list.Add(new BlockToMake(item, key));
			}
		}
		return list;
	}
}
