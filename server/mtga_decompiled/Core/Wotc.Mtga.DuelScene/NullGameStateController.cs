using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullGameStateController : IGameStateController
{
	public static readonly IGameStateController Default = new NullGameStateController();

	public void SetCurrentGameState(MtgGameState gameState)
	{
	}

	public void SetLatestGameState(MtgGameState gameState)
	{
	}
}
