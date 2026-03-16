using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IDelayedTriggerController
{
	void AddDelayedTrigger(MtgCardInstance delayedTrigger);

	void UpdateDelayedTrigger(MtgCardInstance delayedTrigger);

	void RemoveDelayedTrigger(uint delayedTriggerId);
}
