using System.Collections.Generic;

namespace Wotc.Mtga.Wrapper.Draft;

public static class TableDraftPopupFunctions
{
	public static IDraftBoosterView[] GetNewDraftBoosterViews(CollationMapping[] boostersNeededIds, Queue<IDraftBoosterView> boosterViewPool)
	{
		IDraftBoosterView[] array = new IDraftBoosterView[boostersNeededIds.Length];
		int i;
		for (i = 0; i < boostersNeededIds.Length; i++)
		{
			if (boosterViewPool.Count <= 0)
			{
				break;
			}
			array[i] = boosterViewPool.Dequeue();
			array[i].SetBoosterData(boostersNeededIds[i]);
		}
		for (; i < boostersNeededIds.Length; i++)
		{
			array[i] = null;
		}
		return array;
	}

	public static CollationMapping[] GetBoostersNeededAndReturnUnused(CollationMapping[] allBoostersIds, List<IDraftBoosterView> boosterViews, Queue<IDraftBoosterView> boosterViewPool)
	{
		List<CollationMapping> list = new List<CollationMapping>(allBoostersIds);
		int num = 0;
		while (num < boosterViews.Count)
		{
			if (list.Remove(boosterViews[num].CollationId))
			{
				num++;
				continue;
			}
			boosterViews[num].UpdateActive(isActive: false);
			boosterViewPool.Enqueue(boosterViews[num]);
			boosterViews.RemoveAt(num);
		}
		return list.ToArray();
	}
}
