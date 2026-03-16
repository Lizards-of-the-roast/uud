using AssetLookupTree;

namespace Wotc.Mtga.Wrapper.Draft;

public interface IDraftHeaderView
{
	void InitDraftState(StaticDraftStateVisualData staticDraftStateVisualData, AssetLookupSystem assetLookupSystem);

	void UpdateDraftState(DynamicDraftStateVisualData dynamicDraftStateVisualData);

	void AddBoosterViewToPool(IDraftBoosterView boosterView);

	IDraftBoosterView CreateBoosterView(CollationMapping boosterId, SeatLocationId parent);
}
