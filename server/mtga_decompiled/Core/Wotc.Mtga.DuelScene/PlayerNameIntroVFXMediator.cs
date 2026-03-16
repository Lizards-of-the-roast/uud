using System;

namespace Wotc.Mtga.DuelScene;

public class PlayerNameIntroVFXMediator : IDisposable
{
	private readonly IDuelSceneStateProvider _duelSceneStateProvider;

	private readonly UIManager _uiManager;

	private bool _vfxPlayed;

	public PlayerNameIntroVFXMediator(IDuelSceneStateProvider duelSceneStateProvider, UIManager uiManager)
	{
		_duelSceneStateProvider = duelSceneStateProvider;
		_uiManager = uiManager;
		_duelSceneStateProvider.CurrentState.ValueUpdated += OnDuelSceneStateUpdated;
	}

	private void OnDuelSceneStateUpdated(DuelSceneState duelSceneState)
	{
		if (!_vfxPlayed && CanPlayVfx(duelSceneState))
		{
			PlayVfx();
		}
	}

	private bool CanPlayVfx(DuelSceneState duelSceneState)
	{
		if (duelSceneState.MatchScene_SubScene == MatchSceneManager.SubScene.DuelScene && !duelSceneState.MatchScene_SubSceneTransitioning)
		{
			return duelSceneState.FirstGameStatePlaybackComplete;
		}
		return false;
	}

	private void PlayVfx()
	{
		_vfxPlayed = true;
		_uiManager.PlayerNames.PlayIntroVFX();
	}

	public void Dispose()
	{
		_duelSceneStateProvider.CurrentState.ValueUpdated -= OnDuelSceneStateUpdated;
	}
}
