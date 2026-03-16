using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullReplacementEffectController : IReplacementEffectController
{
	public static readonly IReplacementEffectController Default = new NullReplacementEffectController();

	public void RemoveReplacementEffect(ReplacementEffectData data)
	{
	}

	public void TryAddReplacementEffect(ReplacementEffectData data)
	{
	}

	public void UpdateReplacementEffect(ReplacementEffectData data)
	{
	}
}
