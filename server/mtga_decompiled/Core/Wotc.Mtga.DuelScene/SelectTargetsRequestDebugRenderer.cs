using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SelectTargetsRequestDebugRenderer : BaseUserRequestDebugRenderer<SelectTargetsRequest>
{
	private readonly MtgGameState _gameState;

	private readonly ICardDatabaseAdapter _cardDatabase;

	public SelectTargetsRequestDebugRenderer(SelectTargetsRequest selectTargetsRequest, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
		: base(selectTargetsRequest)
	{
		_gameState = gameState;
		_cardDatabase = cardDatabase;
	}

	public override void Render()
	{
		GUI.color = (_request.TargetSelections.TrueForAll((TargetSelection x) => x.CanSubmit()) ? UnityEngine.Color.green : UnityEngine.Color.white);
		if (GUILayout.Button("Submit Targets"))
		{
			_request.SubmitTargets();
		}
		GUI.color = UnityEngine.Color.white;
		GUILayout.Space(10f);
		for (int num = 0; num < _request.TargetSelections.Count; num++)
		{
			TargetSelection targetSelection = _request.TargetSelections[num];
			GUI.color = ((targetSelection.CanSubmit() && targetSelection.IsAtCapacity()) ? UnityEngine.Color.green : UnityEngine.Color.white);
			GUI.color = ((targetSelection.CanSubmit() && !targetSelection.IsAtCapacity()) ? UnityEngine.Color.green : UnityEngine.Color.white);
			foreach (Target target in targetSelection.Targets)
			{
				if (_gameState.TryGetEntity(target.TargetInstanceId, out var mtgEntity))
				{
					string text = ((target.LegalAction == SelectAction.Select) ? "Target" : "Un-Target");
					string text2 = string.Empty;
					if (mtgEntity is MtgPlayer mtgPlayer)
					{
						text2 = (mtgPlayer.IsLocalPlayer ? "LocalPlayer" : "Opponent");
					}
					else if (mtgEntity is MtgCardInstance mtgCardInstance)
					{
						text2 = _cardDatabase.GreLocProvider.GetLocalizedText(mtgCardInstance.TitleId);
					}
					GUILayout.BeginHorizontal();
					if (GUILayout.Button(text))
					{
						_request.UpdateTarget(target, targetSelection.TargetIdx);
					}
					GUILayout.Label(text2);
					GUILayout.EndHorizontal();
				}
				else
				{
					GUILayout.Label($"Instance Id of {target.TargetInstanceId} not found in current state");
				}
			}
			GUI.color = UnityEngine.Color.white;
			GUI.contentColor = UnityEngine.Color.white;
			if (num < _request.TargetSelections.Count - 1)
			{
				GUILayout.Space(5f);
			}
		}
	}
}
