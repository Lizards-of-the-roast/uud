namespace Wotc.Mtga.DuelScene;

public interface IResolutionEffectController
{
	void ResolutionStart(ResolutionEffectModel resolutionEffect);

	void ResolutionComplete();
}
