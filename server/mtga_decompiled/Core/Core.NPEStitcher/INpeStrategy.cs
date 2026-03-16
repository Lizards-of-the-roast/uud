using System;
using System.Collections;
using Wizards.Arena.Promises;

namespace Core.NPEStitcher;

public interface INpeStrategy
{
	bool Initialized { get; }

	NpeModuleState State { get; }

	bool Available { get; }

	bool TutorialRequired { get; }

	int NextGameNumber { get; }

	IEnumerator Refresh();

	void Join(Action<bool, Error> onComplete);

	void PlayMatch();

	void SkipTutorial(Action onComplete);

	void ClaimRewards(Action onComplete);

	void ReplayTutorial(Action onComplete);
}
