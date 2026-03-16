using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullGameStatePlaybackController : IGameStatePlaybackController
{
	public static readonly IGameStatePlaybackController Default = new NullGameStatePlaybackController();

	public void StartPlayback(MtgGameState gameState)
	{
	}

	public void CompletePlayback(MtgGameState gameState)
	{
	}
}
