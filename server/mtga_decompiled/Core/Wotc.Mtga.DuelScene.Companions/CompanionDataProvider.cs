using GreClient.Rules;
using Wizards.Mtga.Inventory;

namespace Wotc.Mtga.DuelScene.Companions;

public class CompanionDataProvider : ICompanionDataProvider
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly IPlayerInfoProvider _playerInfoProvider;

	public CompanionDataProvider(IGameStateProvider gameStateProvider, IPlayerInfoProvider playerInfoProvider)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_playerInfoProvider = playerInfoProvider ?? NullPlayerInfoProvider.Default;
	}

	public CompanionData GetCompanionDataForPlayer(uint instanceId)
	{
		if (!((MtgGameState)_gameStateProvider.CurrentGameState).TryGetPlayer(instanceId, out var player))
		{
			return default(CompanionData);
		}
		if (!_playerInfoProvider.TryGetPlayerInfo(instanceId, out var result))
		{
			return default(CompanionData);
		}
		ClientPetSelection petSelection = result.PetSelection;
		if (petSelection == null)
		{
			return default(CompanionData);
		}
		return new CompanionData(petSelection.name, petSelection.variant, player.ClientPlayerEnum);
	}
}
