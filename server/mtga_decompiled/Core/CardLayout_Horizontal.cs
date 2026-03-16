using System.Collections.Generic;
using UnityEngine;

public class CardLayout_Horizontal : ICardLayout
{
	private Vector3 _spacing = new Vector3(1.5f, 0f, 0f);

	private Vector3 _scale = Vector3.one;

	public Vector3 Spacing
	{
		get
		{
			return _spacing;
		}
		set
		{
			_spacing = value;
		}
	}

	public Vector3 Scale
	{
		get
		{
			return _scale;
		}
		set
		{
			_scale = value;
		}
	}

	public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		Vector3 position = -0.5f * Spacing * (allCardViews.Count - 1) + center;
		foreach (DuelScene_CDC allCardView in allCardViews)
		{
			allData.Add(new CardLayoutData
			{
				Card = allCardView,
				Position = position,
				Scale = Scale
			});
			position += Spacing;
		}
	}
}
