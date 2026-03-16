using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullDelayedTriggerController : IDelayedTriggerController
{
	public static readonly IDelayedTriggerController Default = new NullDelayedTriggerController();

	public void AddDelayedTrigger(MtgCardInstance delayedTrigger)
	{
	}

	public void UpdateDelayedTrigger(MtgCardInstance delayedTrigger)
	{
	}

	public void RemoveDelayedTrigger(uint delayedTriggerId)
	{
	}
}
