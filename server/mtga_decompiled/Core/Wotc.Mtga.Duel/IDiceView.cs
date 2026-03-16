using System;
using System.Collections.Generic;
using AssetLookupTree;
using GreClient.Rules;
using Wotc.Mtga.DuelScene.VFX;

namespace Wotc.Mtga.Duel;

public interface IDiceView : IDisposable
{
	event Action<IDiceView> RollsCommencedHandlers;

	event Action<IDiceView> RollsCompletedHandlers;

	event Action<IDiceView> KeepAndIgnoresCommencedHandlers;

	event Action<IDiceView> KeepAndIgnoresCompletedHandlers;

	void Initialize(GREPlayerNum controller, IVfxProvider vfxProvider, AssetLookupSystem assetLookupSystem);

	void Roll(IReadOnlyList<DieRollResultData> dieRolls);

	void KeepAndIgnore();
}
