using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

public class DeclareAttackerRequestNPEHandler : BaseUserRequestHandler<DeclareAttackerRequest>
{
	private readonly MtgGameState _gameState;

	private readonly List<uint> _attacksToMake;

	private readonly Dictionary<uint, List<uint>> _turnsToAttackWithCreature;

	public DeclareAttackerRequestNPEHandler(DeclareAttackerRequest request, MtgGameState gameState, List<uint> attacksToMake, Dictionary<uint, List<uint>> turnsToAttackWithCreature)
		: base(request)
	{
		_gameState = gameState;
		_attacksToMake = attacksToMake;
		_turnsToAttackWithCreature = turnsToAttackWithCreature;
	}

	public override void HandleRequest()
	{
		Attacker attacker;
		if (_attacksToMake.Count == 0)
		{
			if (TryGetSupplementaryAttackers(_request.Attackers, out var supplementaryAttackers))
			{
				_request.UpdateAttacker(supplementaryAttackers.ToArray());
			}
			else
			{
				_request.SubmitAttackers();
			}
		}
		else if (TryGetAttackerForId(_attacksToMake[0], out attacker))
		{
			attacker.SelectedDamageRecipient = attacker.LegalDamageRecipients[0];
			_request.UpdateAttacker(attacker);
		}
		else
		{
			_request.SubmitAttackers();
		}
	}

	private bool TryGetSupplementaryAttackers(IEnumerable<Attacker> attackers, out List<Attacker> supplementaryAttackers)
	{
		if (_turnsToAttackWithCreature.TryGetValue(_gameState.GameWideTurn, out var value))
		{
			supplementaryAttackers = new List<Attacker>();
			foreach (Attacker attacker in attackers)
			{
				MtgCardInstance cardById = _gameState.GetCardById(attacker.AttackerInstanceId);
				if (value.Contains(cardById.GrpId) && attacker.SelectedDamageRecipient == null && attacker.LegalDamageRecipients.Count > 0)
				{
					attacker.SelectedDamageRecipient = attacker.LegalDamageRecipients[0];
					value.Remove(cardById.GrpId);
					supplementaryAttackers.Add(attacker);
				}
			}
			if (supplementaryAttackers.Count > 0)
			{
				value.Clear();
				return true;
			}
		}
		supplementaryAttackers = null;
		return false;
	}

	private bool TryGetAttackerForId(uint instanceId, out Attacker attacker)
	{
		foreach (Attacker attacker2 in _request.Attackers)
		{
			if (attacker2.AttackerInstanceId == instanceId && attacker2.SelectedDamageRecipient == null)
			{
				attacker = attacker2;
				return true;
			}
		}
		attacker = null;
		return false;
	}
}
