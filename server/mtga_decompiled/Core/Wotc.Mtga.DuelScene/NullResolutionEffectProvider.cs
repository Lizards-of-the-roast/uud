namespace Wotc.Mtga.DuelScene;

public class NullResolutionEffectProvider : IResolutionEffectProvider
{
	public static readonly NullResolutionEffectProvider Default = new NullResolutionEffectProvider();

	public ObservableReference<ResolutionEffectModel> ResolutionEffect => null;
}
