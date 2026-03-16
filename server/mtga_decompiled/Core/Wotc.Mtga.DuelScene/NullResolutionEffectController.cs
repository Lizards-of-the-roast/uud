namespace Wotc.Mtga.DuelScene;

public class NullResolutionEffectController : IResolutionEffectController
{
	public static readonly NullResolutionEffectController Default = new NullResolutionEffectController();

	public void ResolutionStart(ResolutionEffectModel resolutionEffect)
	{
	}

	public void ResolutionComplete()
	{
	}
}
