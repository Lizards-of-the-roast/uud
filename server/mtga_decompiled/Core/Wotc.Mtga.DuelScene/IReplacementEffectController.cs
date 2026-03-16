using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IReplacementEffectController
{
	void TryAddReplacementEffect(ReplacementEffectData data);

	void UpdateReplacementEffect(ReplacementEffectData data);

	void RemoveReplacementEffect(ReplacementEffectData data);
}
