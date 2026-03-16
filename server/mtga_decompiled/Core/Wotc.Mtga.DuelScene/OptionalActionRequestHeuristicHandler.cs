using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class OptionalActionRequestHeuristicHandler : BaseUserRequestHandler<OptionalActionMessageRequest>
{
	private readonly DeckHeuristic _heuristic;

	private readonly MtgGameState _gameState;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly BaseUserRequestHandler<OptionalActionMessageRequest> _fallback;

	public OptionalActionRequestHeuristicHandler(OptionalActionMessageRequest request, DeckHeuristic heuristic, MtgGameState gameState, ICardDatabaseAdapter cardDatabase, BaseUserRequestHandler<OptionalActionMessageRequest> fallback)
		: base(request)
	{
		_heuristic = heuristic;
		_gameState = gameState;
		_cardDatabase = cardDatabase;
		_fallback = fallback;
	}

	public override void HandleRequest()
	{
		if (HasHeuristicForTopCardOnStack())
		{
			_request.SubmitResponse(OptionResponse.AllowYes);
		}
		else
		{
			_fallback.HandleRequest();
		}
	}

	private bool HasHeuristicForTopCardOnStack()
	{
		MtgCardInstance topCardOnStack = _gameState.GetTopCardOnStack();
		if (topCardOnStack == null)
		{
			return false;
		}
		uint id = ((topCardOnStack.ObjectSourceGrpId != 0) ? topCardOnStack.ObjectSourceGrpId : topCardOnStack.BaseGrpId);
		if (_cardDatabase.CardDataProvider.TryGetCardPrintingById(id, out var card))
		{
			return _heuristic.CardHeuristicExists(card.TitleId, _cardDatabase);
		}
		return false;
	}
}
