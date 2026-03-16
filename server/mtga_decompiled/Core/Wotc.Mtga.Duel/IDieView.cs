using System;
using AssetLookupTree;
using Wotc.Mtga.DuelScene.VFX;

namespace Wotc.Mtga.Duel;

public interface IDieView : IDisposable
{
	event Action<IDieView> RollCommencedHandlers;

	event Action<IDieView> RollCompletedHandlers;

	event Action<IDieView> KeepCommencedHandlers;

	event Action<IDieView> KeepCompletedHandlers;

	event Action<IDieView> IgnoreCommencedHandlers;

	event Action<IDieView> IgnoreCompletedHandlers;

	void Initialize(uint dieFaces, IVfxProvider vfxProvider, AssetLookupSystem assetLookupSystem);

	void Roll(uint naturalResult, int modifiedResult);

	void Keep();

	void Ignore();
}
