using System;
using UnityEngine;
using UnityEngine.UI;

public class RadialLayout : LayoutGroup
{
	public float fDistance;

	[Range(0f, 360f)]
	public float StartAngle;

	public float OffsetAngle = 30f;

	protected override void OnEnable()
	{
		base.OnEnable();
		CalculateRadial();
	}

	public override void SetLayoutHorizontal()
	{
	}

	public override void SetLayoutVertical()
	{
	}

	public override void CalculateLayoutInputVertical()
	{
		CalculateRadial();
	}

	public override void CalculateLayoutInputHorizontal()
	{
		CalculateRadial();
	}

	private void CalculateRadial()
	{
		m_Tracker.Clear();
		int childCount = base.transform.childCount;
		int num = 0;
		for (int i = 0; i < childCount; i++)
		{
			if (base.transform.GetChild(i).gameObject.activeSelf)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		float num2 = ConvertToAngle(StartAngle - OffsetAngle * (float)(num - 1) * 0.5f);
		for (int num3 = childCount - 1; num3 >= 0; num3--)
		{
			if (base.transform.GetChild(num3).gameObject.activeSelf)
			{
				RectTransform rectTransform = (RectTransform)base.transform.GetChild(num3);
				if (rectTransform != null)
				{
					m_Tracker.Add(this, rectTransform, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.Pivot);
					Vector3 vector = new Vector3(Mathf.Cos(num2 * (MathF.PI / 180f)), Mathf.Sin(num2 * (MathF.PI / 180f)), 0f);
					rectTransform.localPosition = vector * fDistance;
					Vector2 vector2 = (rectTransform.pivot = new Vector2(0.5f, 0.5f));
					Vector2 anchorMin = (rectTransform.anchorMax = vector2);
					rectTransform.anchorMin = anchorMin;
					num2 += OffsetAngle;
				}
			}
		}
	}

	private float ConvertToAngle(float f)
	{
		float num = f % 360f;
		if (num < 0f)
		{
			num += 360f;
			return ConvertToAngle(num);
		}
		return num;
	}
}
