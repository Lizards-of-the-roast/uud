using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

public class ActionsAvailableRequestNPEHandler : ActionsAvailableRequestRandomHandler
{
	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly NPE_Game _game;

	private readonly MtgGameState _gameState;

	private bool _shouldFreestyle;

	public ActionsAvailableRequestNPEHandler(ActionsAvailableRequest request, ICardDatabaseAdapter cardDatabase, NPE_Game game, MtgGameState gameState, Random random)
		: base(request, random)
	{
		_cardDatabase = cardDatabase;
		_game = game;
		_gameState = gameState;
	}

	public override void HandleRequest()
	{
		if (_game._freestylingAllowances.TryGetValue(_gameState.GameWideTurn, out var value))
		{
			_shouldFreestyle = value;
		}
		if (_request.OriginalMessage.AllowCancel == AllowCancel.Continue)
		{
			_request.Cancel();
			return;
		}
		Wotc.Mtgo.Gre.External.Messaging.Action action = _request.Actions.Find((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.ActionType == ActionType.Play);
		if (action != null)
		{
			_request.SubmitAction(action);
			return;
		}
		List<Wotc.Mtgo.Gre.External.Messaging.Action> list = _request.Actions.FindAll((Wotc.Mtgo.Gre.External.Messaging.Action a) => ActionsAvailableRequest.IsCastAction(a) && a.ActionType != ActionType.Play);
		List<Wotc.Mtgo.Gre.External.Messaging.Action> list2 = _request.Actions.FindAll((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.ActionType == ActionType.Activate);
		foreach (uint asapGRPID in _game._requestedAsapPlays)
		{
			Wotc.Mtgo.Gre.External.Messaging.Action action2 = list.Find((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.GrpId == asapGRPID && CanAffordAction(x));
			if (action2 != null)
			{
				_request.SubmitAction(action2);
				return;
			}
		}
		if (_game._requestedPlays.TryGetValue(_gameState.GameWideTurn, out var value2) && value2.TryGetValue(_gameState.CurrentPhase, out var value3) && value3.TryGetValue(_gameState.CurrentStep, out var value4) && value4.Count > 0)
		{
			ScriptedAction scriptedAction = value4.Peek();
			if (scriptedAction is TryCastAction)
			{
				TryCastAction tryCastAction = scriptedAction as TryCastAction;
				foreach (Wotc.Mtgo.Gre.External.Messaging.Action item in list)
				{
					if (tryCastAction.CardToCast == item.GrpId && CanAffordAction(item))
					{
						value4.Dequeue();
						_request.SubmitAction(item);
						return;
					}
				}
			}
			if (scriptedAction is TryActivateAction tryActivateAction)
			{
				foreach (Wotc.Mtgo.Gre.External.Messaging.Action item2 in list2)
				{
					MtgCardInstance cardById = _gameState.GetCardById(item2.InstanceId);
					if (_cardDatabase.GreLocProvider.GetLocalizedText(cardById.TitleId).Equals(tryActivateAction.CardToActivate) && !cardById.HasSummoningSickness && CanAffordAction(item2))
					{
						value4.Dequeue();
						_request.SubmitAction(item2);
						return;
					}
				}
			}
			if (scriptedAction is TryTOCAction)
			{
				uint tOC_GRPID = (scriptedAction as TryTOCAction).TOC_GRPID;
				foreach (Wotc.Mtgo.Gre.External.Messaging.Action item3 in _request.Actions.FindAll((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.GrpId == 2))
				{
					if (item3.AbilityGrpId == tOC_GRPID)
					{
						value4.Dequeue();
						_request.SubmitAction(item3);
						return;
					}
				}
			}
		}
		if (_shouldFreestyle)
		{
			foreach (Wotc.Mtgo.Gre.External.Messaging.Action item4 in list)
			{
				if (CanAffordAction(item4))
				{
					_request.SubmitAction(item4);
					return;
				}
			}
		}
		if (_request.CanPass)
		{
			_request.SubmitPass();
		}
	}

	private bool CanAffordAction(Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		bool flag = false;
		ICollection<AutoTapAction> collection = action.AutoTapActions();
		foreach (AutoTapAction item in collection)
		{
			AbilityPrintingRecord abilityRecordById = _cardDatabase.AbilityDataProvider.GetAbilityRecordById(item.AbilityGrpId);
			string localizedText = _cardDatabase.GreLocProvider.GetLocalizedText(abilityRecordById.TextId, "en-US", formatted: false);
			if (localizedText != null && localizedText.Contains("Pay"))
			{
				flag = true;
			}
		}
		if (_game.CastForFree || action.ManaCost.Count == 0 || collection.Count > 0)
		{
			return !flag;
		}
		return false;
	}
}
