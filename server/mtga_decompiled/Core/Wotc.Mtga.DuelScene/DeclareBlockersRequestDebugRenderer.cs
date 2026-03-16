using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class DeclareBlockersRequestDebugRenderer : BaseUserRequestDebugRenderer<DeclareBlockersRequest>
{
	private MtgGameState _gameState;

	private ICardDatabaseAdapter _cardDatabase;

	public DeclareBlockersRequestDebugRenderer(DeclareBlockersRequest declareBlockersRequest, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
		: base(declareBlockersRequest)
	{
		_gameState = gameState;
		_cardDatabase = cardDatabase;
	}

	public override void Render()
	{
		if (GUILayout.Button("Submit Blockers"))
		{
			_request.SubmitBlockers();
		}
		foreach (Blocker allBlocker in _request.AllBlockers)
		{
			MtgCardInstance cardById = _gameState.GetCardById(allBlocker.BlockerInstanceId);
			GUILayout.Label($"{_cardDatabase.GreLocProvider.GetLocalizedText(cardById.TitleId)} [{cardById.InstanceId}]");
			if (allBlocker.SelectedAttackerInstanceIds.Count < allBlocker.MaxAttackers)
			{
				for (int i = 0; i < allBlocker.AttackerInstanceIds.Count; i++)
				{
					MtgCardInstance cardById2 = _gameState.GetCardById(allBlocker.AttackerInstanceIds[i]);
					if (GUILayout.Button($"{_cardDatabase.GreLocProvider.GetLocalizedText(cardById2.TitleId)} [{cardById2.InstanceId}]" + " BLOCK"))
					{
						allBlocker.SelectedAttackerInstanceIds.Add(allBlocker.AttackerInstanceIds[i]);
						_request.UpdateBlockers(allBlocker);
					}
				}
			}
			for (int j = 0; j < allBlocker.SelectedAttackerInstanceIds.Count; j++)
			{
				MtgCardInstance cardById3 = _gameState.GetCardById(allBlocker.SelectedAttackerInstanceIds[j]);
				if (GUILayout.Button($"{_cardDatabase.GreLocProvider.GetLocalizedText(cardById3.TitleId)} [{cardById3.InstanceId}]" + "UN-BLOCK"))
				{
					allBlocker.SelectedAttackerInstanceIds.Clear();
					_request.UpdateBlockers(allBlocker);
				}
			}
		}
	}
}
