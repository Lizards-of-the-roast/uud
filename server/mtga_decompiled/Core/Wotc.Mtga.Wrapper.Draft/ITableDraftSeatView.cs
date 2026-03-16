using System.Collections.Generic;

namespace Wotc.Mtga.Wrapper.Draft;

internal interface ITableDraftSeatView
{
	List<IDraftBoosterView> DraftBoosterViews { get; }

	void SetBustVisualData(BustVisualData bustVisualData);

	void AddDraftBoosterViews(IDraftBoosterView[] draftBoosterViews);
}
