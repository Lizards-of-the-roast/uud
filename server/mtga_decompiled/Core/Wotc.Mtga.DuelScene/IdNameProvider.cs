using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class IdNameProvider : IEntityNameProvider<uint>
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly IEntityNameProvider<MtgEntity> _entityNameProvider;

	public IdNameProvider(IEntityNameProvider<MtgEntity> entityNameProvider, IGameStateProvider gameStateProvider)
	{
		_entityNameProvider = entityNameProvider ?? NullEntityNameProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
	}

	public string GetName(uint entityId, bool formatted = true)
	{
		if (!TryGetEntity(entityId, out var mtgEntity))
		{
			return string.Empty;
		}
		return _entityNameProvider.GetName(mtgEntity, formatted);
	}

	private bool TryGetEntity(uint instanceId, out MtgEntity mtgEntity)
	{
		MtgGameState mtgGameState = _gameStateProvider.LatestGameState;
		MtgGameState mtgGameState2 = _gameStateProvider.CurrentGameState;
		if (mtgGameState != null && mtgGameState.TryGetEntity(instanceId, out mtgEntity))
		{
			return true;
		}
		if (mtgGameState2 != null && mtgGameState2.TryGetEntity(instanceId, out mtgEntity))
		{
			return true;
		}
		mtgEntity = null;
		return false;
	}
}
