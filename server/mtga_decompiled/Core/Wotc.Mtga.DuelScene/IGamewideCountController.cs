using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IGamewideCountController
{
	void AddGamewideCount(GamewideCountData gamewideCountData);

	void UpdateGamewideCount(GamewideCountData gameWideHistoryCount);

	void RemoveGamewideCount(GamewideCountData gamewideCountData);
}
