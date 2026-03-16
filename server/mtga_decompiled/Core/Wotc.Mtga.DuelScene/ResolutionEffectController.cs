using System;

namespace Wotc.Mtga.DuelScene;

public class ResolutionEffectController : IResolutionEffectController, IResolutionEffectProvider, IDisposable
{
	public ObservableReference<ResolutionEffectModel> ResolutionEffect { get; private set; } = new ObservableReference<ResolutionEffectModel>();

	public void ResolutionStart(ResolutionEffectModel resolutionEffect)
	{
		ResolutionEffect.Value = resolutionEffect;
	}

	public void ResolutionComplete()
	{
		ResolutionEffect.Value = null;
	}

	public void Dispose()
	{
		ResolutionEffect.Dispose();
		ResolutionEffect = null;
	}
}
