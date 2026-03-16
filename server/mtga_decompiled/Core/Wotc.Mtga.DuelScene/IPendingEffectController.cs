using System;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface IPendingEffectController : IDisposable
{
	void AddPendingEffect(PendingEffectData pendingEffect, MtgCardInstance Affector, MtgPlayer player);

	void RemovePendingEffect(PendingEffectData pendingEffect);
}
