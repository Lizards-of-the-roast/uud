using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullGamewideController : IGamewideCountController
{
	public static readonly IGamewideCountController Default = new NullGamewideController();

	public void AddGamewideCount(GamewideCountData gamewideCountData)
	{
	}

	public void UpdateGamewideCount(GamewideCountData gameWideHistoryCount)
	{
	}

	public void RemoveGamewideCount(GamewideCountData gamewideCountData)
	{
	}
}
