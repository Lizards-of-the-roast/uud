using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class AvatarViewManager : IAvatarViewManager, IAvatarViewProvider, IAvatarViewController
{
	private readonly GameManager _gameManager;

	private readonly MutableAvatarViewProvider _provider;

	private readonly IAvatarBuilder _avatarBuilder;

	private readonly IAvatarLayout _avatarLayout;

	private readonly IPlayerInfoProvider _playerInfoProvider;

	public AvatarViewManager(MutableAvatarViewProvider provider, IAvatarBuilder avatarBuilder, IAvatarLayout avatarLayout, IPlayerInfoProvider playerInfoProvider, GameManager gameManager)
	{
		_provider = provider ?? new MutableAvatarViewProvider();
		_avatarBuilder = avatarBuilder ?? NullAvatarBuilder.Default;
		_avatarLayout = avatarLayout ?? NullAvatarLayout.Default;
		_playerInfoProvider = playerInfoProvider ?? NullPlayerInfoProvider.Default;
		_gameManager = gameManager;
	}

	public IEnumerable<DuelScene_AvatarView> GetAllAvatars()
	{
		return _provider.GetAllAvatars();
	}

	public DuelScene_AvatarView GetAvatarById(uint id)
	{
		return _provider.GetAvatarById(id);
	}

	public DuelScene_AvatarView GetAvatarByPlayerSide(GREPlayerNum playerType)
	{
		return _provider.GetAvatarByPlayerSide(playerType);
	}

	public DuelScene_AvatarView CreateAvatarView(MtgPlayer player)
	{
		if (player == null)
		{
			return null;
		}
		DuelScene_AvatarView duelScene_AvatarView = _avatarBuilder.Create(player);
		if (duelScene_AvatarView == null)
		{
			return null;
		}
		string portraitId = _playerInfoProvider.GetPlayerInfo(player.InstanceId)?.AvatarSelection ?? string.Empty;
		duelScene_AvatarView.Init(_gameManager, player, portraitId);
		duelScene_AvatarView.SetLifeTotal(player.LifeTotal);
		duelScene_AvatarView.UpdateCounters(player.Counters);
		_provider.AvatarViewsById.Add(player.InstanceId, duelScene_AvatarView);
		GREPlayerNum clientPlayerEnum = player.ClientPlayerEnum;
		if (!_provider.AvatarViewsByEnum.ContainsKey(clientPlayerEnum))
		{
			_provider.AvatarViewsByEnum.Add(clientPlayerEnum, duelScene_AvatarView);
		}
		_provider.AllAvatars.Add(duelScene_AvatarView);
		_avatarLayout.LayoutAvatars(_provider.AllAvatars);
		return duelScene_AvatarView;
	}

	public bool DeleteAvatar(uint playerId)
	{
		if (!_provider.TryGetAvatarById(playerId, out var avatar))
		{
			return false;
		}
		_provider.AllAvatars.Remove(avatar);
		_provider.AvatarViewsById.Remove(playerId);
		_avatarLayout.LayoutAvatars(_provider.AllAvatars);
		return _avatarBuilder.Destroy(avatar);
	}
}
