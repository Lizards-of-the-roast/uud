using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CardLayout_Hand : ICardLayout
{
	public float Radius = 45f;

	public float FitAngle = 30f;

	public float MaxDeltaAngle = 4.5f;

	public float YOffset = -1.5f;

	public float ZOffset;

	public float SpacingModifier;

	public AnimationCurve OverlapRotation;

	public float RotationMultiplier = 1f;

	public int MinimumCardsForFocus = 13;

	public float MaxFocusDistance = 7.5f;

	public AnimationCurve FocusProximityWeight;

	public readonly List<int> ArtificialSpacers = new List<int>();

	private bool _focusing;

	private int _focusIndex = -1;

	private int _cardCount;

	public float GetArcBaseY => Mathf.Sin(GetLeftmostAngle * (MathF.PI / 180f)) * Radius;

	public float GetLeftmostAngle => 90f + FitAngle * 0.5f;

	public float GetLeftmostX => Mathf.Cos(GetLeftmostAngle * (MathF.PI / 180f)) * Radius;

	public float GetRightmostAngle => 90f - FitAngle * 0.5f;

	public float GetRightmostmostX => Mathf.Cos(GetRightmostAngle * (MathF.PI / 180f)) * Radius;

	public CardLayout_Hand()
	{
	}

	public CardLayout_Hand(CardLayout_Hand other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(CardLayout_Hand other)
	{
		Radius = other.Radius;
		FitAngle = other.FitAngle;
		MaxDeltaAngle = other.MaxDeltaAngle;
		YOffset = other.YOffset;
		ZOffset = other.ZOffset;
		SpacingModifier = other.SpacingModifier;
		OverlapRotation = other.OverlapRotation;
		RotationMultiplier = other.RotationMultiplier;
		MinimumCardsForFocus = other.MinimumCardsForFocus;
		MaxFocusDistance = other.MaxFocusDistance;
		FocusProximityWeight = other.FocusProximityWeight;
	}

	public virtual void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		_cardCount = allCardViews.Count + ArtificialSpacers.Count;
		float num = (float)ArtificialSpacers.Count * SpacingModifier;
		float num2 = Mathf.Min(MaxDeltaAngle, FitAngle / ((float)(_cardCount - 1) + num));
		if (num2 < MaxDeltaAngle && _focusing && _focusIndex >= 0)
		{
			GenerateDataForFocus(allCardViews, ref allData, center, num2);
			return;
		}
		float num3 = 90f + num2 * ((float)(_cardCount - 1) + num) * 0.5f;
		float num4 = 0f;
		int num5 = 0;
		for (int i = 0; i < _cardCount; i++)
		{
			if (ArtificialSpacers.Contains(i))
			{
				num4 += SpacingModifier;
				continue;
			}
			float angle = (num3 - num2 * ((float)i + num4)) * (MathF.PI / 180f);
			CalcPosRotFromAngle(angle, out var pos, out var rot);
			pos += center;
			allData.Add(new CardLayoutData(allCardViews[num5], pos, Quaternion.Euler(rot)));
			num5++;
		}
	}

	public bool SetFocusPosition(Vector3? newFocusPosition)
	{
		if (newFocusPosition.HasValue)
		{
			float y = newFocusPosition.Value.y;
			_focusing = _cardCount >= MinimumCardsForFocus && y <= MaxFocusDistance;
			if (_focusing)
			{
				float num = GetRightmostmostX - GetLeftmostX;
				int num2 = Mathf.Clamp(Mathf.RoundToInt((newFocusPosition.Value.x + (0f - GetLeftmostX)) / num * (float)(_cardCount - 1)), 0, _cardCount - 1);
				if (num2 != _focusIndex)
				{
					_focusIndex = num2;
					return true;
				}
				return false;
			}
		}
		if (_focusIndex >= 0)
		{
			_focusIndex = -1;
			return true;
		}
		return false;
	}

	private void CalcPosRotFromAngle(float angle, out Vector3 pos, out Vector3 rot)
	{
		pos = new Vector3
		{
			x = Mathf.Cos(angle) * Radius,
			y = Mathf.Sin(angle) * Radius - GetArcBaseY + YOffset,
			z = ZOffset
		};
		rot = new Vector3
		{
			x = 0f,
			y = (OverlapRotation?.Evaluate(_cardCount) ?? (-7f)),
			z = Vector3.SignedAngle(Vector3.up, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)), Vector3.forward) * RotationMultiplier
		};
	}

	private void GenerateDataForFocus(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, float deltaAngle)
	{
		_focusIndex = Mathf.Min(_focusIndex, _cardCount - 1);
		Dictionary<int, DuelScene_CDC> dictionary = new Dictionary<int, DuelScene_CDC>(_cardCount);
		int num = 0;
		for (int i = 0; i < _cardCount; i++)
		{
			if (ArtificialSpacers.Contains(i))
			{
				dictionary[i] = null;
				continue;
			}
			dictionary[i] = allCardViews[num];
			num++;
		}
		int num2 = 0;
		int num3 = 0;
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>(_cardCount);
		for (int j = 0; j < _cardCount; j++)
		{
			if (j != _focusIndex)
			{
				int num4 = Mathf.Max(1, Mathf.RoundToInt(FocusProximityWeight.Evaluate(_cardCount)) - Mathf.Abs(_focusIndex - j));
				if (j < _focusIndex)
				{
					num2 += num4;
				}
				else
				{
					num3 += num4;
				}
				dictionary2[j] = num4;
			}
		}
		float num5 = GetLeftmostAngle - deltaAngle * (float)_focusIndex;
		float num6 = GetLeftmostAngle - num5;
		float num7 = num5 - GetRightmostAngle;
		float num8 = Mathf.Max(0.1f, num6 / (float)num2);
		float num9 = Mathf.Max(0.1f, num7 / (float)num3);
		float num10 = num5;
		Vector3 pos;
		Vector3 rot;
		if (dictionary.TryGetValue(_focusIndex, out var value) && value != null)
		{
			CalcPosRotFromAngle(num10 * (MathF.PI / 180f), out pos, out rot);
			pos += center;
			allData.Add(new CardLayoutData(value, pos, Quaternion.Euler(rot)));
		}
		num10 = num5;
		for (int num11 = _focusIndex - 1; num11 >= 0; num11--)
		{
			num10 += num8 * (float)dictionary2[num11];
			if (dictionary.TryGetValue(num11, out var value2) && value2 != null)
			{
				CalcPosRotFromAngle(num10 * (MathF.PI / 180f), out pos, out rot);
				pos += center;
				allData.Add(new CardLayoutData(value2, pos, Quaternion.Euler(rot)));
			}
		}
		num10 = num5;
		for (int k = _focusIndex + 1; k < _cardCount; k++)
		{
			num10 -= num9 * (float)dictionary2[k];
			if (dictionary.TryGetValue(k, out var value3) && value3 != null)
			{
				CalcPosRotFromAngle(num10 * (MathF.PI / 180f), out pos, out rot);
				pos += center;
				allData.Add(new CardLayoutData(value3, pos, Quaternion.Euler(rot)));
			}
		}
	}
}
