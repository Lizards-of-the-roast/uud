using AssetLookupTree;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftHeaderViewMock : IDraftHeaderView
{
	public void InitDraftState(StaticDraftStateVisualData staticDraftStateVisualData, AssetLookupSystem assetLookupSystem)
	{
	}

	public void UpdateDraftState(DynamicDraftStateVisualData dynamicDraftStateVisualData)
	{
	}

	public void AddBoosterViewToPool(IDraftBoosterView boosterView)
	{
	}

	public IDraftBoosterView CreateBoosterView(CollationMapping boosterId, SeatLocationId seatLocationId)
	{
		DraftBoosterViewMock draftBoosterViewMock = new DraftBoosterViewMock();
		draftBoosterViewMock.SetBoosterData(boosterId);
		return draftBoosterViewMock;
	}
}
