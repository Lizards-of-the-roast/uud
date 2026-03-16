using AssetLookupTree;

namespace Wotc.Mtga.Wrapper.Draft;

internal interface ITableDraftPopupView
{
	void InitTable(IDraftPod draftPod, BustVisualData[] bustVisualDatas, AssetLookupSystem assetLookupSystem);

	void UpdateTable(bool passDirectionIsLeft, PlayerBoosterVisualData[] boosterVisualDatas);
}
