using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class PlayerNameProvider : IEntityNameProvider<MtgPlayer>
{
	private readonly IPlayerInfoProvider _playerInfoProvider;

	public PlayerNameProvider(IPlayerInfoProvider playerInfoProvider)
	{
		_playerInfoProvider = playerInfoProvider ?? NullPlayerInfoProvider.Default;
	}

	public string GetName(MtgPlayer player, bool formatted = true)
	{
		return _playerInfoProvider.GetScreenNameForPlayer(player.InstanceId);
	}
}
