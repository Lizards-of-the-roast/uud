using GreClient.Rules;
using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class ChooseStartingPlayerRequestDebugRenderer : BaseUserRequestDebugRenderer<ChooseStartingPlayerRequest>
{
	private MtgGameState _gameState;

	public ChooseStartingPlayerRequestDebugRenderer(ChooseStartingPlayerRequest chooseStartingPlayerRequest, MtgGameState gameState)
		: base(chooseStartingPlayerRequest)
	{
		_gameState = gameState;
	}

	public override void Render()
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Play"))
		{
			_request.ChooseStartingPlayer(_gameState.LocalPlayer.InstanceId);
		}
		if (GUILayout.Button("Draw"))
		{
			_request.ChooseStartingPlayer(_gameState.Opponent.InstanceId);
		}
		GUILayout.EndHorizontal();
	}
}
