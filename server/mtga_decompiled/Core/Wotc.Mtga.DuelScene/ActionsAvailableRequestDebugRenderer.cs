using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class ActionsAvailableRequestDebugRenderer : BaseUserRequestDebugRenderer<ActionsAvailableRequest>
{
	private const string PASS_ACTION_TEXT = "Pass";

	private readonly MtgGameState _gameState;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly List<Action> _actions;

	private const uint TOC_GILD_GRPID = 987u;

	private const uint TOC_TAP_UNTAP_GRPID = 989u;

	private const uint TOC_DONATE_GRPID = 990u;

	private const uint TOC_BOUNCE_TO_HAND_GRPID = 991u;

	private const uint TOC_DRAWCARD_ABILITY_GRPID = 992u;

	private const uint TOC_WAIVE_COST_GRPID = 993u;

	private const uint TOC_WISH_ABILITY_GRPID = 996u;

	private const uint TOC_PUT_LIBRARY_GRPID = 997u;

	private const uint TOC_GAIN_HASTE_GRPID = 998u;

	private const uint TOC_ADD_COLORS_GRPID = 999u;

	public ActionsAvailableRequestDebugRenderer(ActionsAvailableRequest actionsAvailableRequest, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
		: base(actionsAvailableRequest)
	{
		_gameState = gameState;
		_cardDatabase = cardDatabase;
		_actions = new List<Action>(actionsAvailableRequest.Actions);
		_actions.Sort(SortActions);
	}

	public override void Render()
	{
		foreach (Action action in _actions)
		{
			if (GUILayout.Button(GetButtonTextForAction(action)))
			{
				_request.SubmitAction(action);
			}
		}
	}

	private string GetButtonTextForAction(Action action)
	{
		if (action.ActionType == ActionType.Pass)
		{
			return "Pass";
		}
		if (_gameState.TryGetCard(action.InstanceId, out var card))
		{
			string text = action.ActionType.ToString();
			if (card.TitleId != 0)
			{
				text = text + " " + _cardDatabase.GreLocProvider.GetLocalizedText(card.TitleId, null, formatted: false);
			}
			else if (card.GrpId != 0)
			{
				uint id = ((card.GrpId > 11) ? card.GrpId : card.BaseGrpId);
				CardPrintingRecord cardRecordById = _cardDatabase.CardDataProvider.GetCardRecordById(id);
				text = text + " " + _cardDatabase.GreLocProvider.GetLocalizedText(cardRecordById.TitleId, null, formatted: false);
			}
			if (action.AbilityGrpId != 0)
			{
				bool flag = false;
				string text2 = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(card.GrpId, action.AbilityGrpId, card.Abilities.Select((AbilityPrintingData o) => o.Id), 0u, null, formatted: false);
				if (text2.Length > 20)
				{
					flag = true;
					text2 = text2.Remove(0, 20);
				}
				text = text + " \"" + text2 + (flag ? "...\"" : "\"");
			}
			else if (action.AlternativeGrpId != 0)
			{
				string abilityTextByCardAbilityGrpId = _cardDatabase.AbilityTextProvider.GetAbilityTextByCardAbilityGrpId(card.GrpId, action.AlternativeGrpId, card.Abilities.Select((AbilityPrintingData o) => o.Id), 0u, null, formatted: false);
				text = text + " (" + abilityTextByCardAbilityGrpId + ")";
			}
			return text;
		}
		return action.ActionType.ToString();
	}

	private int SortActions(Action x, Action y)
	{
		int num = y.IsActionType(ActionType.Pass).CompareTo(x.IsActionType(ActionType.Pass));
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 996).CompareTo(x.AbilityGrpId == 996);
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 993).CompareTo(x.AbilityGrpId == 993);
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 987).CompareTo(x.AbilityGrpId == 987);
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 992).CompareTo(x.AbilityGrpId == 992);
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 997).CompareTo(x.AbilityGrpId == 997);
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 991).CompareTo(x.AbilityGrpId == 991);
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 998).CompareTo(x.AbilityGrpId == 998);
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 989).CompareTo(x.AbilityGrpId == 989);
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 990).CompareTo(x.AbilityGrpId == 990);
		if (num != 0)
		{
			return num;
		}
		num = (y.AbilityGrpId == 999).CompareTo(x.AbilityGrpId == 999);
		if (num != 0)
		{
			return num;
		}
		return y.IsActionType(ActionType.ActivateTest).CompareTo(x.IsActionType(ActionType.ActivateTest));
	}
}
