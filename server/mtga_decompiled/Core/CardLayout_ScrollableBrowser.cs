using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CardLayout_ScrollableBrowser : ICardLayout
{
	public float ScrollPosition;

	public float FrontZOffset;

	public Vector3 CenterOffset;

	public Vector3 Scale;

	public float FrontWidth;

	public int FrontCount;

	public float FrontYOffset;

	public float FrontRotation;

	public int CompressThreshold;

	public float SideZOffset;

	public float SideRotation;

	public float SideSpacing;

	public int SideMaxToShow;

	public float SideYOffset;

	public float AngleMult;

	public float PosMult;

	public float CardTilt;

	public bool IsReversedDisplay;

	public float FrontYAdditive;

	public float SideRotationAdditive;

	public bool StackRightToLeft;

	public readonly List<int> ArtificialSpacers = new List<int>();

	public int PiledLeft { get; private set; }

	public int PiledRight { get; private set; }

	public float FrontSpacing { get; private set; }

	public CardLayout_ScrollableBrowser()
	{
		SetDefaults();
	}

	public CardLayout_ScrollableBrowser(CardLayout_ScrollableBrowser other)
	{
		CopyFrom(other);
	}

	public void CopyFrom(CardLayout_ScrollableBrowser other)
	{
		ScrollPosition = other.ScrollPosition;
		FrontZOffset = other.FrontZOffset;
		Scale = other.Scale;
		CenterOffset = other.CenterOffset;
		FrontWidth = other.FrontWidth;
		FrontCount = other.FrontCount;
		FrontYOffset = other.FrontYOffset;
		FrontRotation = other.FrontRotation;
		CompressThreshold = other.CompressThreshold;
		SideZOffset = other.SideZOffset;
		SideRotation = other.SideRotation;
		SideSpacing = other.SideSpacing;
		SideMaxToShow = other.SideMaxToShow;
		SideYOffset = other.SideYOffset;
		AngleMult = other.AngleMult;
		PosMult = other.PosMult;
		CardTilt = other.CardTilt;
		IsReversedDisplay = other.IsReversedDisplay;
		FrontYAdditive = other.FrontYAdditive;
		SideRotationAdditive = other.SideRotationAdditive;
		StackRightToLeft = other.StackRightToLeft;
	}

	public void SetDefaults()
	{
		ScrollPosition = 0f;
		FrontZOffset = 0.2f;
		Scale = Vector3.one;
		CenterOffset = Vector3.zero;
		FrontWidth = 11f;
		FrontCount = 7;
		FrontYOffset = 0.5f;
		FrontRotation = 5f;
		CompressThreshold = 0;
		SideZOffset = 0.2f;
		SideRotation = 1f;
		SideSpacing = 0.2f;
		SideMaxToShow = 7;
		SideYOffset = 0f;
		AngleMult = 1.25f;
		PosMult = 12f;
		CardTilt = 5f;
		IsReversedDisplay = false;
		FrontYAdditive = 0f;
		SideRotationAdditive = 0f;
		StackRightToLeft = false;
	}

	public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
	{
		if (allCardViews.Count == 0)
		{
			return;
		}
		center += CenterOffset;
		float y = CardTilt * -1f;
		int num = allCardViews.Count + ArtificialSpacers.Count;
		int num2 = Mathf.Clamp(num, 1, FrontCount);
		float num3 = (float)num2 / (float)FrontCount * FrontWidth;
		int num4 = Mathf.RoundToInt(ScrollPosition * (float)(num - num2));
		float num5 = 0.5f * (float)(num2 - 1);
		float num6 = num5 + (float)num4;
		float num7 = -0.5f * num3;
		FrontSpacing = 0f;
		float num8 = 0f;
		if (num < CompressThreshold)
		{
			num7 = -0.5f * ((float)num2 / 4f) * FrontWidth;
			FrontSpacing = FrontWidth / (float)num2;
		}
		else
		{
			FrontSpacing = ((num2 <= 1) ? 1f : (num3 / (float)(num2 - 1)));
			num8 = FrontRotation;
		}
		PiledLeft = 0;
		PiledRight = 0;
		if (allCardViews.Count == 1)
		{
			allData.Add(new CardLayoutData
			{
				Card = allCardViews.First(),
				Position = center,
				Rotation = rotation,
				Scale = Scale,
				IsVisibleInLayout = true
			});
			return;
		}
		int num9 = 0;
		for (int i = 0; i < num; i++)
		{
			if (ArtificialSpacers.Contains(i))
			{
				continue;
			}
			if (num9 >= allCardViews.Count)
			{
				Debug.LogFormat("CardIndex of {0} is too large, total count is {1}, slotIndex is {2}", num9, allCardViews.Count, i);
			}
			DuelScene_CDC card = allCardViews[num9];
			int num10 = (IsReversedDisplay ? (num - 1 - i) : i);
			float num11 = ((SideZOffset < 0f) ? (0f - SideZOffset) : SideZOffset);
			float num12 = num6 - (float)num10;
			int num13 = num10 - num4;
			if (num13 < 0)
			{
				float num14 = SideRotation * AngleMult;
				if (SideRotationAdditive != 0f)
				{
					float num15 = SideRotationAdditive * -1f * (float)Mathf.Max(num13, -SideMaxToShow);
					num14 += num15;
				}
				allData.Add(new CardLayoutData
				{
					Card = card,
					Position = new Vector3(num7 + center.x + SideSpacing * (float)Mathf.Max(num13, -SideMaxToShow), center.y + SideYOffset, (float)num13 * num11 * -1f + center.z),
					Rotation = rotation * Quaternion.Euler(0f, 0f, num5 * (num8 + num14)),
					Scale = Scale,
					IsVisibleInLayout = (num13 >= -SideMaxToShow)
				});
				PiledLeft++;
			}
			else if (num13 < FrontCount)
			{
				float num16 = 0f;
				num16 = ((!StackRightToLeft) ? ((float)num13 * FrontZOffset) : ((float)(FrontCount - num13) * FrontZOffset));
				num16 *= -1f;
				float num17 = FrontYOffset - Mathf.Sin(Mathf.Abs(num12) * (MathF.PI / 180f) * AngleMult) * PosMult + center.y;
				if (FrontYAdditive != 0f)
				{
					float num18 = (0f - FrontYAdditive) * (float)num / 2f + FrontYAdditive * (float)num13;
					num17 += num18;
				}
				allData.Add(new CardLayoutData
				{
					Card = card,
					Position = new Vector3(num7 + center.x + FrontSpacing * (float)num13, num17, num16 + center.z),
					Rotation = rotation * Quaternion.Euler(0f, y, num12 * num8),
					Scale = Scale,
					IsVisibleInLayout = true
				});
			}
			else
			{
				float num19 = SideRotation * AngleMult;
				if (SideRotationAdditive != 0f)
				{
					float num20 = SideRotationAdditive * (float)(num13 - (FrontCount - 1));
					num19 += num20;
				}
				allData.Add(new CardLayoutData
				{
					Card = card,
					Position = new Vector3(num7 + center.x + FrontSpacing * (float)(FrontCount - 1) + SideSpacing * (float)Mathf.Min(num13 - (FrontCount - 1), SideMaxToShow), center.y + SideYOffset, (float)(num13 - (FrontCount - 1)) * num11 + center.z),
					Rotation = rotation * Quaternion.Euler(0f, 0f, num5 * (num8 + num19) * -1f),
					Scale = Scale,
					IsVisibleInLayout = (num13 - (FrontCount - 1) <= SideMaxToShow)
				});
				PiledRight++;
			}
			num9++;
		}
	}
}
