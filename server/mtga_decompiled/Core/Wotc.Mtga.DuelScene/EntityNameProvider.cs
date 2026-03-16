using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class EntityNameProvider : IEntityNameProvider<MtgEntity>
{
	private readonly IEntityNameProvider<MtgCardInstance> _cardNameProvider;

	private readonly IEntityNameProvider<MtgPlayer> _playerNameProvider;

	public EntityNameProvider(IEntityNameProvider<MtgCardInstance> cardNameProvider, IEntityNameProvider<MtgPlayer> playerNameProvider)
	{
		_cardNameProvider = cardNameProvider ?? NullCardNameProvider.Default;
		_playerNameProvider = playerNameProvider ?? NullPlayerNameProvider.Default;
	}

	public string GetName(MtgEntity entity, bool formatted = true)
	{
		if (!(entity is MtgCardInstance entity2))
		{
			if (entity is MtgPlayer entity3)
			{
				return _playerNameProvider.GetName(entity3, formatted);
			}
			return string.Empty;
		}
		return _cardNameProvider.GetName(entity2, formatted);
	}
}
