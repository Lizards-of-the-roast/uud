using GreClient.Rules;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class AuraColdSelectionWarning : ISelectionConfirmation
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly IEntityNameProvider<uint> _entityNameProvider;

	private readonly IClientLocProvider _clientLocProvider;

	public AuraColdSelectionWarning(IGameStateProvider gameStateProvider, IEntityNameProvider<uint> entityNameProvider, IClientLocProvider clientLocProvider)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_entityNameProvider = entityNameProvider ?? NullIdNameProvider.Default;
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public string GetConfirmationText(HighlightType highlight, IEntityView entityView, SelectNRequest request)
	{
		uint sourceId = request.SourceId;
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		if (highlight == HighlightType.Cold && mtgGameState.TryGetCard(entityView.InstanceId, out var _) && mtgGameState.TryGetCard(sourceId, out var card2) && card2.Subtypes.Contains(SubType.Aura))
		{
			return _clientLocProvider.GetLocalizedText("DuelScene/Warning/AttachAura_ColdHighlightWarning", ("aura", _entityNameProvider.GetName(sourceId)), ("target", _entityNameProvider.GetName(entityView.InstanceId)));
		}
		return null;
	}
}
