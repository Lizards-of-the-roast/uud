using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class SelectReplacementRequestDebugRenderer : BaseUserRequestDebugRenderer<SelectReplacementRequest>
{
	private readonly MtgGameState _gameState;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly CancelRequestDebugRenderer _cancelRequestDebugRenderer;

	public SelectReplacementRequestDebugRenderer(SelectReplacementRequest request, MtgGameState gameState, ICardDatabaseAdapter cardDatabase)
		: base(request)
	{
		_gameState = gameState;
		_cardDatabase = cardDatabase;
		_cancelRequestDebugRenderer = new CancelRequestDebugRenderer(request);
	}

	public override void Render()
	{
		GUI.enabled = _request.CanCancel;
		_cancelRequestDebugRenderer.Render();
		GUI.enabled = true;
		foreach (ReplacementEffect replacement in _request.Replacements)
		{
			if (_gameState.TryGetEntity(replacement.ObjectInstance, out var mtgEntity) && mtgEntity is MtgCardInstance mtgCardInstance && GUILayout.Button($"{_cardDatabase.GreLocProvider.GetLocalizedText(mtgCardInstance.TitleId)} (AbilityId: {replacement.AbilityGrpId})"))
			{
				_request.SubmitReplacement(replacement);
				break;
			}
		}
	}
}
