using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface ISuspendLikeController
{
	void AddSuspendLikeData(SuspendLikeData data);

	void UpdateSuspendLikeData(SuspendLikeData data);

	void RemoveSuspendLikeData(uint id);
}
