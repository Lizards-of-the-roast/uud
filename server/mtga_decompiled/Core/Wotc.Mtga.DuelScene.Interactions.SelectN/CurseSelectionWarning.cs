using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class CurseSelectionWarning : ISelectionConfirmation
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly IClientLocProvider _clientLocProvider;

	public CurseSelectionWarning(IGameStateProvider gameStateProvider, IClientLocProvider clientLocProvider)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public string GetConfirmationText(HighlightType highlight, IEntityView entityView, SelectNRequest request)
	{
		if (highlight != HighlightType.Cold)
		{
			return null;
		}
		if (!(entityView is DuelScene_AvatarView { IsLocalPlayer: not false }))
		{
			return null;
		}
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		MtgCardInstance mtgCardInstance = null;
		if (mtgGameState.TryGetCard(request.SourceId, out var card))
		{
			mtgCardInstance = card;
		}
		else
		{
			PromptParameter promptParameter = request.Prompt.Parameters.Find("CardId", (PromptParameter pp, string str) => pp.ParameterName == str);
			if (promptParameter != null && mtgGameState.TryGetCard((uint)promptParameter.NumberValue, out var card2))
			{
				mtgCardInstance = card2;
			}
		}
		if (mtgCardInstance == null)
		{
			return null;
		}
		if (!mtgCardInstance.Subtypes.Contains(SubType.Curse) && (mtgCardInstance.Parent == null || !mtgCardInstance.Parent.Subtypes.Contains(SubType.Curse)))
		{
			return null;
		}
		return _clientLocProvider.GetLocalizedText("DuelScene/SelectTargets_LocalPlayer_Curse");
	}
}
