using System;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullPendingEffectController : IPendingEffectController, IDisposable
{
	public static readonly IPendingEffectController Default = new NullPendingEffectController();

	public void AddPendingEffect(PendingEffectData pendingEffect, MtgCardInstance Affector, MtgPlayer player)
	{
	}

	public void RemovePendingEffect(PendingEffectData pendingEffect)
	{
	}

	public void Dispose()
	{
	}
}
