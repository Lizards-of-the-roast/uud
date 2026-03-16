using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLayout_HorizontalAligned : ICardLayout
{
	public Vector3 Spacing { get; set; } = new Vector3(1.5f, 0f, 0.1f);

	public bool Flipped { get; set; }

	public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		Vector3 position = center;
		foreach (DuelScene_CDC item in OrderedCArds(allCardViews))
		{
			allData.Add(new CardLayoutData
			{
				Card = item,
				Position = position
			});
			position += Spacing;
		}
	}

	private IEnumerable OrderedCArds(List<DuelScene_CDC> allCardViews)
	{
		if (Flipped)
		{
			for (int i = allCardViews.Count - 1; i >= 0; i--)
			{
				yield return allCardViews[i];
			}
		}
		else
		{
			for (int i = 0; i < allCardViews.Count; i++)
			{
				yield return allCardViews[i];
			}
		}
	}
}
