using GreClient.Rules;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.Interactions.SelectN;

public class BirthingRitualSacrificeSelectionWarning : ISelectionConfirmation
{
	private const uint ABILITY_ID_BIRTHING_RITUAL = 172351u;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IResolutionEffectProvider _resolutionEffectProvider;

	private readonly IClientLocProvider _clientLocProvider;

	public BirthingRitualSacrificeSelectionWarning(IGameStateProvider gameStateProvider, IResolutionEffectProvider resolutionEffectProvider, IClientLocProvider clientLocProvider)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_resolutionEffectProvider = resolutionEffectProvider ?? NullResolutionEffectProvider.Default;
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
	}

	public string GetConfirmationText(HighlightType highlightType, IEntityView entityView, SelectNRequest request)
	{
		if (highlightType != HighlightType.Cold)
		{
			return null;
		}
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		ResolutionEffectModel activeResolutionData = _resolutionEffectProvider.ResolutionEffect;
		if (!mtgGameState.TryGetCard(request.SourceId, out var card) || card.TitleId != 760437)
		{
			return null;
		}
		if (!IsResolvingAbilityBirthingRitual(activeResolutionData))
		{
			return null;
		}
		return _clientLocProvider.GetLocalizedText("DuelScene/Selection_BirthingRitual_ColdHighlightWarning");
	}

	private static bool IsResolvingAbilityBirthingRitual(ResolutionEffectModel activeResolutionData)
	{
		if (activeResolutionData == null)
		{
			return false;
		}
		return activeResolutionData.AbilityPrinting?.Id == 172351;
	}
}
