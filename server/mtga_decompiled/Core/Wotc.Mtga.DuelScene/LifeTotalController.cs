using System;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class LifeTotalController : ILifeTotalController, IDisposable
{
	private readonly IAvatarViewProvider _avatarViewProvider;

	private readonly ISignalListen<GameStatePlaybackCompleteSignalArgs> _playbackCompleteSignal;

	private readonly ISignalDispatch<IncrementPlayerLifeSignalArgs> _incrementPlayerLifeSignal;

	public LifeTotalController(IAvatarViewProvider avatarViewProvider, ISignalListen<GameStatePlaybackCompleteSignalArgs> playbackCompleteSignal, ISignalDispatch<IncrementPlayerLifeSignalArgs> incrementPlayerLifeSignal)
	{
		_avatarViewProvider = avatarViewProvider;
		_playbackCompleteSignal = playbackCompleteSignal;
		_incrementPlayerLifeSignal = incrementPlayerLifeSignal;
		_playbackCompleteSignal.Listeners += OnGameStatePlaybackComplete;
	}

	private void OnGameStatePlaybackComplete(GameStatePlaybackCompleteSignalArgs args)
	{
		SetAllLifeTotals(args.GameState);
	}

	private void SetAllLifeTotals(MtgGameState gameState)
	{
		foreach (MtgPlayer player in gameState.Players)
		{
			if (_avatarViewProvider.TryGetAvatarById(player.InstanceId, out var avatar))
			{
				avatar.SetLifeTotal(player.LifeTotal);
			}
		}
	}

	public void IncrementPlayerLife(uint playerId, int amount)
	{
		if (_avatarViewProvider.TryGetAvatarById(playerId, out var avatar))
		{
			avatar.IncrementLifeTotal(amount);
			_incrementPlayerLifeSignal.Dispatch(new IncrementPlayerLifeSignalArgs(this, playerId, amount));
		}
	}

	public void Dispose()
	{
		_playbackCompleteSignal.Listeners -= OnGameStatePlaybackComplete;
	}
}
