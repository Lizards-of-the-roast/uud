using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullSuspendLikeController : ISuspendLikeController
{
	public static readonly ISuspendLikeController Default = new NullSuspendLikeController();

	public void AddSuspendLikeData(SuspendLikeData data)
	{
	}

	public void UpdateSuspendLikeData(SuspendLikeData data)
	{
	}

	public void RemoveSuspendLikeData(uint id)
	{
	}
}
