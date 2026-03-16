using GreClient.Rules;

public class ChooseLocalPlayerRequestHandler : BaseUserRequestHandler<ChooseStartingPlayerRequest>
{
	private readonly MtgGameState _gameState;

	public ChooseLocalPlayerRequestHandler(ChooseStartingPlayerRequest request, MtgGameState gameState)
		: base(request)
	{
		_gameState = gameState;
	}

	public override void HandleRequest()
	{
		_request.ChooseStartingPlayer(_gameState.LocalPlayer.InstanceId);
	}
}
