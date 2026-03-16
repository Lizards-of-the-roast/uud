using System;
using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class FaceDownIdProvider : IFaceDownIdProvider, IDisposable
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly Dictionary<uint, uint> _faceDownIdMapping = new Dictionary<uint, uint>();

	private uint _nextFaceDownId = 1u;

	public FaceDownIdProvider(IGameStateProvider gameStateProvider)
	{
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_gameStateProvider.CurrentGameState.ValueUpdated += UpdateCurrentGameState;
	}

	public bool TryGetFaceDownId(uint instanceId, out uint faceDownId)
	{
		return _faceDownIdMapping.TryGetValue(instanceId, out faceDownId);
	}

	private void UpdateCurrentGameState(MtgGameState gameState)
	{
		if (gameState == null)
		{
			return;
		}
		MtgZone battlefield = gameState.Battlefield;
		if (battlefield == null)
		{
			return;
		}
		foreach (MtgCardInstance visibleCard in battlefield.VisibleCards)
		{
			if ((!(visibleCard?.Controller?.ControlledByLocalPlayer)) ?? false)
			{
				if (_faceDownIdMapping.ContainsKey(visibleCard.InstanceId) && !visibleCard.FaceDownState.IsFaceDown)
				{
					_faceDownIdMapping.Remove(visibleCard.InstanceId);
				}
				else if (visibleCard.FaceDownState.IsFaceDown && visibleCard.OthersideGrpId == 0 && !_faceDownIdMapping.ContainsKey(visibleCard.InstanceId))
				{
					_faceDownIdMapping[visibleCard.InstanceId] = _nextFaceDownId;
					_nextFaceDownId++;
				}
			}
		}
	}

	public void Dispose()
	{
		_gameStateProvider.CurrentGameState.ValueUpdated -= UpdateCurrentGameState;
		_faceDownIdMapping.Clear();
	}
}
