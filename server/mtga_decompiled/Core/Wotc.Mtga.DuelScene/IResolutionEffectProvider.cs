namespace Wotc.Mtga.DuelScene;

public interface IResolutionEffectProvider
{
	ObservableReference<ResolutionEffectModel> ResolutionEffect { get; }
}
