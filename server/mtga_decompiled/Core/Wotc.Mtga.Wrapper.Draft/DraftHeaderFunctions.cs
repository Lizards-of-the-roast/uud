using System.Collections.Generic;

namespace Wotc.Mtga.Wrapper.Draft;

public static class DraftHeaderFunctions
{
	public static List<IDraftBoosterView> UpdateSeatBoosters(IDraftHeaderView stateView, List<IDraftBoosterView> currentBoosterViewList, CollationMapping[] newBoosterStateData, bool passDirectionIsLeft, SeatLocationId boosterParentId)
	{
		List<IDraftBoosterView> list = new List<IDraftBoosterView>();
		int i = 0;
		while (i < newBoosterStateData.Length)
		{
			IDraftBoosterView draftBoosterView = currentBoosterViewList.Find((IDraftBoosterView view) => view.CollationId == newBoosterStateData[i]);
			if (draftBoosterView == null)
			{
				list.Add(stateView.CreateBoosterView(newBoosterStateData[i], boosterParentId));
			}
			else
			{
				list.Add(draftBoosterView);
				currentBoosterViewList.Remove(draftBoosterView);
			}
			int num = i + 1;
			i = num;
		}
		for (int num2 = 0; num2 < currentBoosterViewList.Count; num2++)
		{
			currentBoosterViewList[num2].PassBooster(passDirectionIsLeft, stateView.AddBoosterViewToPool);
		}
		return list;
	}
}
