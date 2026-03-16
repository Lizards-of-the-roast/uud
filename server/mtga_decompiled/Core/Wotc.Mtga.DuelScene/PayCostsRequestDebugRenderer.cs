using System.Linq;
using System.Text.RegularExpressions;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class PayCostsRequestDebugRenderer : BaseUserRequestDebugRenderer<PayCostsRequest>
{
	private readonly MtgGameState _gameState;

	private readonly ICardDatabaseAdapter _cardDatabase;

	public PayCostsRequestDebugRenderer(PayCostsRequest request, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
		: base(request)
	{
		_gameState = gameState;
		_cardDatabase = cardDatabase;
	}

	public override void Render()
	{
		if (_request.PaymentActions != null)
		{
			foreach (Action action in _request.PaymentActions.Actions)
			{
				MtgCardInstance cardById = _gameState.GetCardById(action.InstanceId);
				if (cardById != null)
				{
					string text = action.ActionType.ToString();
					text = text + " " + _cardDatabase.GreLocProvider.GetLocalizedText(cardById.TitleId);
					if (action.AbilityGrpId != 0)
					{
						bool flag = false;
						string abilityTextByCardAbilityGrpId = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(cardById.GrpId, action.AbilityGrpId, cardById.Abilities.Select((AbilityPrintingData o) => o.Id));
						abilityTextByCardAbilityGrpId = Regex.Replace(abilityTextByCardAbilityGrpId, "<[^>]*>", string.Empty);
						if (abilityTextByCardAbilityGrpId.Length > 20)
						{
							flag = true;
							abilityTextByCardAbilityGrpId = abilityTextByCardAbilityGrpId.Remove(0, 20);
						}
						text = text + " \"" + abilityTextByCardAbilityGrpId + (flag ? "...\"" : "\"");
					}
					if (GUILayout.Button(text))
					{
						_request.PaymentActions.SubmitAction(action);
					}
				}
			}
			return;
		}
		GUILayout.Label("Request not implemented.  Be brave and do it.  I believe in you.");
	}
}
