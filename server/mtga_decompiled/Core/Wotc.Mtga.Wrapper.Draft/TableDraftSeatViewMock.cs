using System.Collections.Generic;

namespace Wotc.Mtga.Wrapper.Draft;

public class TableDraftSeatViewMock : ITableDraftSeatView
{
	public BustVisualData SeatVisualData;

	public List<IDraftBoosterView> DraftBoosterViews { get; private set; } = new List<IDraftBoosterView>();

	public void SetBustVisualData(BustVisualData visualData)
	{
		SeatVisualData = visualData;
	}

	public void AddDraftBoosterViews(IDraftBoosterView[] draftBoosterViews)
	{
		DraftBoosterViews.AddRange(draftBoosterViews);
	}
}
