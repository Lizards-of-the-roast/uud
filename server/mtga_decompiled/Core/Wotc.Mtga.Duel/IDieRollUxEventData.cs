using AssetLookupTree;
using Wotc.Mtga.DuelScene.VFX;

namespace Wotc.Mtga.Duel;

public interface IDieRollUxEventData
{
	IDiceView InstantiateDiceView(GREPlayerNum controller, IVfxProvider vfxProvider, AssetLookupSystem assetLookupSystem);
}
