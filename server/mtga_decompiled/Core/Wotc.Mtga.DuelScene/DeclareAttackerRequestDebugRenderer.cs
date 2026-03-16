using System.Collections.Generic;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class DeclareAttackerRequestDebugRenderer : BaseUserRequestDebugRenderer<DeclareAttackerRequest>
{
	private MtgGameState _gameState;

	private ICardDatabaseAdapter _cardDatabase;

	public DeclareAttackerRequestDebugRenderer(DeclareAttackerRequest declareAttackerRequest, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
		: base(declareAttackerRequest)
	{
		_gameState = gameState;
		_cardDatabase = cardDatabase;
	}

	public override void Render()
	{
		if (GUILayout.Button("Submit Attackers"))
		{
			_request.SubmitAttackers();
		}
		if (GUILayout.Button("All Attack"))
		{
			_request.DeclareAllAttackers(_request.Attackers[0].LegalDamageRecipients[0]);
		}
		if (GUILayout.Button("Cancel Attack"))
		{
			List<Attacker> list = new List<Attacker>();
			foreach (Attacker declaredAttacker in _request.DeclaredAttackers)
			{
				if (declaredAttacker.SelectedDamageRecipient != null)
				{
					declaredAttacker.SelectedDamageRecipient = null;
					list.Add(declaredAttacker);
				}
			}
			if (list.Count > 0)
			{
				_request.UpdateAttacker(list.ToArray());
			}
		}
		HashSet<uint> hashSet = new HashSet<uint>();
		foreach (Attacker attacker in _request.Attackers)
		{
			MtgCardInstance cardById = _gameState.GetCardById(attacker.AttackerInstanceId);
			if (!_request.DeclaredAttackers.Exists((Attacker x) => x.AttackerInstanceId == attacker.AttackerInstanceId))
			{
				string text = "Declare" + ((attacker.AlternativeGrpId != 0) ? " (EXERT)" : string.Empty);
				foreach (DamageRecipient legalDamageRecipient in attacker.LegalDamageRecipients)
				{
					switch (legalDamageRecipient.Type)
					{
					case DamageRecType.PlanesWalker:
					{
						MtgCardInstance cardById2 = _gameState.GetCardById(legalDamageRecipient.PlaneswalkerInstanceId);
						if (GUILayout.Button($"{_cardDatabase.GreLocProvider.GetLocalizedText(cardById.TitleId)} [{cardById.InstanceId}] {text} --> {_cardDatabase.GreLocProvider.GetLocalizedText(cardById2.TitleId)}, [{cardById2.InstanceId}]"))
						{
							attacker.SelectedDamageRecipient = legalDamageRecipient;
							_request.UpdateAttacker(attacker);
						}
						break;
					}
					case DamageRecType.Player:
						if (GUILayout.Button($"{_cardDatabase.GreLocProvider.GetLocalizedText(cardById.TitleId)} [{cardById.InstanceId}] {text} --> Player [{legalDamageRecipient.PlayerSystemSeatId}]"))
						{
							attacker.SelectedDamageRecipient = legalDamageRecipient;
							_request.UpdateAttacker(attacker);
						}
						break;
					}
				}
			}
			else if (!hashSet.Contains(attacker.AttackerInstanceId))
			{
				hashSet.Add(attacker.AttackerInstanceId);
				if (GUILayout.Button($"{_cardDatabase.GreLocProvider.GetLocalizedText(cardById.TitleId)} [{cardById.InstanceId}] Undeclare"))
				{
					attacker.SelectedDamageRecipient = null;
					_request.UpdateAttacker(attacker);
				}
			}
		}
	}
}
