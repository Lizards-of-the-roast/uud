using System;
using System.Collections.Generic;
using UnityEngine;

public class CardLayout_HalfFan : ICardLayout
{
	public float Radius = 10f;

	public float OverlapOffset = 0.2f;

	public float OverlapRotation;

	public float TiltRatio = 1f;

	public float TotalDeltaAngle = 30f;

	public float PivotPercentage = 1f;

	public float AdditionalRotation;

	public CardHolderType HolderType = CardHolderType.Stack;

	public uint TargetingSourceId { get; set; }

	private Vector3 GetCardPositionFromIndex(int i, int count, Vector3 pivot, float deltaAngle)
	{
		float f = ((float)i - (float)(count - 1) * PivotPercentage) * deltaAngle * (MathF.PI / 180f);
		float num = Mathf.Cos(f);
		float num2 = Mathf.Sin(f);
		return pivot + Vector3.up * (num * Radius) + Vector3.right * num2 * Radius + Vector3.forward * OverlapOffset * (count - 1 - i);
	}

	private Quaternion GetCardRotationFromIndex(int i, int count, float deltaAngle)
	{
		Quaternion identity = Quaternion.identity;
		float num = ((float)i - (float)(count - 1) * PivotPercentage) * deltaAngle;
		return identity * Quaternion.AngleAxis(num * TiltRatio, Vector3.back) * Quaternion.AngleAxis(OverlapRotation, Vector3.up);
	}

	public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		int num = -1;
		for (int i = 0; i < allCardViews.Count; i++)
		{
			if (allCardViews[i].InstanceId == TargetingSourceId)
			{
				num = i;
				break;
			}
		}
		int num2 = ((-1 < num && num < allCardViews.Count - 1) ? allCardViews.Count : 0);
		float deltaAngle = 0f;
		if (allCardViews.Count > 0)
		{
			deltaAngle = TotalDeltaAngle / (float)(allCardViews.Count + num2);
		}
		Vector3 pivot = center - Vector3.up * Radius;
		for (int j = 0; j < allCardViews.Count; j++)
		{
			int num3 = ((-1 < num && num < j) ? num2 : 0);
			CardLayoutData cardLayoutData = new CardLayoutData();
			cardLayoutData.Card = allCardViews[j];
			cardLayoutData.Position = GetCardPositionFromIndex(j + num3, allCardViews.Count + num2, pivot, deltaAngle);
			cardLayoutData.Rotation = GetCardRotationFromIndex(j + num3, allCardViews.Count + num2, deltaAngle);
			allData.Add(cardLayoutData);
		}
	}
}
