using System;
using System.Collections.Generic;
using UnityEngine;

public class CardLayout_Fan : ICardLayout
{
	public float Radius = 45f;

	public float OverlapOffset;

	public float OverlapRotation = 5f;

	public float VerticalOffset;

	public float ZOffset;

	public float TiltRatio = 0.5f;

	public float MaxDeltaAngle = 5f;

	public float TotalDeltaAngle = 30f;

	public float PivotPercentage = 0.5f;

	public readonly List<int> ArtificialSpacers = new List<int>();

	private int _cardCount;

	private float _deltaAngle;

	private Vector3 _pivot = Vector3.up;

	private Vector3 _topLeft = Vector3.left;

	private Vector3 _topRight = Vector3.right;

	protected virtual Vector3 GetCardPositionFromIndex(int i, int count, Vector3 pivot, float deltaAngle, float angleAdjust)
	{
		float num = (float)i - (float)(count - 1) * PivotPercentage;
		float f = num * deltaAngle * (MathF.PI / 180f) - angleAdjust;
		float num2 = Mathf.Cos(f);
		float num3 = Mathf.Sin(f);
		Vector3 result = pivot + Vector3.up * (num2 * Radius + num * VerticalOffset) + Vector3.right * num3 * Radius + Vector3.forward * OverlapOffset * i;
		result.z += ZOffset;
		return result;
	}

	protected virtual Quaternion GetCardRotationFromIndex(int i, int count, float deltaAngle, float angleAdjust)
	{
		Quaternion identity = Quaternion.identity;
		float num = ((float)i - (float)(count - 1) * PivotPercentage) * deltaAngle - angleAdjust;
		return identity * Quaternion.AngleAxis(OverlapRotation, Vector3.up) * Quaternion.AngleAxis(-1f * num * TiltRatio, Vector3.forward);
	}

	public virtual void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		_cardCount = allCardViews.Count + ArtificialSpacers.Count;
		_pivot = center - Vector3.up * Radius;
		_deltaAngle = ((_cardCount == 0) ? 0f : Mathf.Min(TotalDeltaAngle / (float)_cardCount, MaxDeltaAngle));
		float num = (float)(-(_cardCount - 1)) * PivotPercentage * _deltaAngle * (MathF.PI / 180f);
		float f = num - MathF.PI / 2f;
		_topLeft = GetCardPositionFromIndex(0, _cardCount, _pivot, _deltaAngle, 0f) + Vector3.up * 0.7f * Mathf.Cos(num) + Vector3.right * 0.7f * Mathf.Sin(num) + Vector3.up * 0.5f * Mathf.Cos(f) + Vector3.right * 0.5f * Mathf.Sin(f);
		float num2 = (float)(_cardCount - 1) * PivotPercentage * _deltaAngle * (MathF.PI / 180f);
		float f2 = num2 + MathF.PI / 2f;
		_topRight = GetCardPositionFromIndex(_cardCount - 1, _cardCount, _pivot, _deltaAngle, 0f) + Vector3.up * 0.7f * Mathf.Cos(num2) + Vector3.right * 0.7f * Mathf.Sin(num2) + Vector3.up * 0.5f * Mathf.Cos(f2) + Vector3.right * 0.5f * Mathf.Sin(f2);
		float num3 = Mathf.Atan2(_topLeft.y - _topRight.y, _topRight.x - _topRight.x);
		_pivot = center - Vector3.up * Radius * Mathf.Cos(num3) + Vector3.right * Radius * Mathf.Sin(num3);
		int num4 = 0;
		for (int i = 0; i < _cardCount; i++)
		{
			if (!ArtificialSpacers.Contains(i))
			{
				CardLayoutData item = new CardLayoutData
				{
					Card = allCardViews[num4],
					Position = GetCardPositionFromIndex(i, _cardCount, _pivot, _deltaAngle, num3),
					Rotation = GetCardRotationFromIndex(i, _cardCount, _deltaAngle, num3)
				};
				allData.Add(item);
				num4++;
			}
		}
	}
}
